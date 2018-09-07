using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BuildsUtility;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.Lab.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Security.Principal;

using System.Text;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.LabMan;
using Microsoft.LabMan.Lab;
using System.Runtime.Remoting.Messaging;
using TFSUtility;
using HTMLUtility;


namespace BuildsDashBoardASP
{
    public partial class TeamRuns : System.Web.UI.Page
    {
        const string TFS_SERVER_URL = "http://adsgroupvstf:8080/tfs/adsgroup";
        const string TFSPROJECT = "AdsApps";
        const string TEST_REASON_FAILURE = "Test(s) failed or aborted";
        const string SETUP_REASON_FAILURE = "Setup or deployment failed";
        int planId;
        static int requestCounter;
        static List<string> buildNames;
        static IDictionary<string, List<TestRunSummary>> PlantestRuns;
        static IDictionary<string, string> AllEnvDictionary;
        delegate IDictionary<string, List<TestRunSummary>> GetTestRunsDelegate(int planId);
        delegate List<LabEnvironment> GetAllLabsDelegate();
        BuildUtility bu;
        static IDictionary<string, List<IBuildDetail>> buildsByBuildDef;
        protected void Page_Load(object sender, EventArgs e)
        {
            Initialize();

            Response.Write(getHtmlHeader());
            try
            {
                Response.Write(GetHtml());
            }
            catch (Exception exc)
            {
                
                Response.Redirect("Error.aspx");
            }

            //Response.Write(BuildHTMLUtility.getFooter());
            Response.Write(BuildHTMLUtility.getFooter());
        }

        private string GetHtml()
        {

            //BuildUtility bu = new BuildUtility();
            StringBuilder sbDays = new StringBuilder();
            //get the header 
            //sbDays.Append(getHtmlHeader());

            //get the test runs
            GetTestRunsAsync();
            //get the lab environments asynchronously
            GetLabEnvironmentsAsync();
            //get the builds asynchronously
            GetBuildsAsync();

            //wait for the calls to finish
            while (requestCounter > 0)
            {

            }

            //display builds for each build definition

            foreach (var def in buildNames)
            {
                if (buildsByBuildDef.ContainsKey(def))
                    sbDays.Append(BuildHTMLUtility.getBuildsHtmlByDefinition(buildsByBuildDef[def], def, PlantestRuns, AllEnvDictionary));
            }

            return sbDays.ToString();
        }

        private void Initialize()
        {
            buildNames = BuildUtility.getBuildDefinitions();
            requestCounter = 0;
            buildsByBuildDef = new Dictionary<string, List<IBuildDetail>>();

            bu = new BuildUtility();
            PlantestRuns = new Dictionary<string, List<TestRunSummary>>();
            planId = 3047;
            AllEnvDictionary = new Dictionary<string, string>();
        }

        private void GetTestRunsAsync()
        {
            GetTestRunsDelegate dlgt = new GetTestRunsDelegate(BuildUtility.GetTestRuns);
            AsyncCallback cb = new AsyncCallback(ProcessTestRunsCallBack);
            Interlocked.Increment(ref requestCounter);
            IAsyncResult ar = dlgt.BeginInvoke(planId, cb, dlgt);
        }

        private void GetBuildsAsync()
        {
            try
            {


                using (TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(TFS_SERVER_URL)))
                {
                    var buildServer = (IBuildServer)tfs.GetService(typeof(IBuildServer));

                    AsyncCallback callBack = new AsyncCallback(ProcessBuildsInformation);
                    foreach (string buildDef in buildNames)
                    {
                        Interlocked.Increment(ref requestCounter);
                        IBuildDetailSpec buildDetailSpec = buildServer.CreateBuildDetailSpec(
                            TFSPROJECT,
                            buildDef);
                        //buildDetailSpec.MaxBuildsPerDefinition = 10;
                        buildDetailSpec.QueryOrder = BuildQueryOrder.FinishTimeDescending;
                        //read from web.config
                        int lastXHours;
                        if (!int.TryParse(ConfigurationManager.AppSettings["lastXHours"], out lastXHours))
                            lastXHours = 24;
                        buildDetailSpec.MinFinishTime = DateTime.Now.AddHours(-1 * lastXHours);
                        buildDetailSpec.MaxFinishTime = DateTime.Now;
                        buildDetailSpec.Status = BuildStatus.All;
                        buildDetailSpec.InformationTypes = null;
                        IBuildDetailSpec[] buildSpecs = new IBuildDetailSpec[] { buildDetailSpec };
                        object state = buildDef;
                        buildServer.BeginQueryBuilds(buildSpecs, callBack, state);
                    }
                }
            }
            catch (Exception e)
            {
                throw ;
            }
        }

        private void GetLabEnvironmentsAsync()
        {
            GetAllLabsDelegate dlgt = new GetAllLabsDelegate(BuildUtility.GetAllLabEnvs);
            AsyncCallback cb = new AsyncCallback(ProcessAllLabEnvsCallBack);
            Interlocked.Increment(ref requestCounter);
            //object state = tag;
            IAsyncResult ar = dlgt.BeginInvoke(cb, null);
        }

        static void ProcessAllLabEnvsCallBack(IAsyncResult result)
        {
            //TagDetail tagDetail = (TagDetail)result.AsyncState;
            GetAllLabsDelegate dlgt = (GetAllLabsDelegate)((AsyncResult)result).AsyncDelegate;
            using (var connection = TeamProjectCollectionConnection.FromFullUri(new Uri(TFS_SERVER_URL)))
            {
                LabService labService = connection.TeamProjectCollection.GetService<LabService>();
                try
                {
                    //var buildServer = (IBuildServer)tfs.GetService(typeof(IBuildServer));

                    List<LabEnvironment> labs = dlgt.EndInvoke(result);
                    // populate the tagdetail object
                    foreach (var lab in labs)
                    {
                        AllEnvDictionary.Add(lab.LabGuid.ToString(), lab.Name);
                    }
                }
                catch (Exception e)
                {
                    throw;
                }
                finally
                {
                    // Decrement the request counter in a thread-safe manner.
                    Interlocked.Decrement(ref requestCounter);
                }
            }
        }

        private string getHtmlHeader()
        {
            StringBuilder sbDays = new StringBuilder();
            sbDays.Append(BuildHTMLUtility.GetHeader());
            //get the hours from config
            int lastXHours;
            if (!int.TryParse(ConfigurationManager.AppSettings["lastXHours"], out lastXHours))
                lastXHours = 24;
            sbDays.Append(string.Format("<h2>Team Test Runs ( Last {0} hours )</h2>", lastXHours.ToString()));
            return sbDays.ToString();
        }

        static void ProcessTestRunsCallBack(IAsyncResult result)
        {
            GetTestRunsDelegate dlgt = (GetTestRunsDelegate)result.AsyncState;
            try
            {
                PlantestRuns = dlgt.EndInvoke(result);
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                // Decrement the request counter in a thread-safe manner.
                Interlocked.Decrement(ref requestCounter);
            }
        }

        private string GetRunStatsHtml(dynamic anonymousType)
        {
            return "";
        }
        static void ProcessBuildsInformation(IAsyncResult result)
        {
            // Get the state object associated with this request.
            using (TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(TFS_SERVER_URL)))
            {
                var buildServer = (IBuildServer)tfs.GetService(typeof(IBuildServer));
                string buildDef = (string)result.AsyncState;
                try
                {
                    // Get the results and store them in the state object.
                    // IPHostEntry host = Dns.EndGetHostEntry(result);
                    List<IBuildDetail> builds = buildServer.EndQueryBuilds(result)[0].Builds.ToList();
                    builds = builds.OrderByDescending(bd => bd.StartTime).ToList();
                    buildsByBuildDef.Add(buildDef, builds);
                    //sbDays.Append(getBuildsHtmlByDefinition(buildsByBuildDef["[AdsApps][BVT][MT-Refresh]"], "[AdsApps][BVT][MT-Refresh]"));
                    //request.buildDetails = builds;
                }
                catch (Exception e)
                {
                    throw;
                    //navigate to error page
                    //Response.Redirect("Error.aspx");
                }
                finally
                {
                    // Decrement the request counter in a thread-safe manner.
                    Interlocked.Decrement(ref requestCounter);
                }
            }
        }
        //private List<ITestRun> GetTestRunByBuild(IBuildDetail b, List<ITestRun> testRuns)
        //{
        //    List<ITestRun> runs;
        //    if (testRuns != null)
        //    {
        //        foreach (ITestRun r in runs)
        //        {
        //            if (r.BuildNumber.Equals(b.BuildNumber) )
        //        }
        //    }

        //}
    }
}