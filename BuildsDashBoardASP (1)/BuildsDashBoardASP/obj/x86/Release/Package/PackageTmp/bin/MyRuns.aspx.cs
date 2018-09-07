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
using System.Security.Principal;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.Lab.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
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
    public partial class MyRuns : System.Web.UI.Page
    {
        const string BUDDYBUILDBVT = "[LabMan2.0][BVT][AdsApps][MT-RefreshOnly][Apps]";
        const string LABMANCIBVT = "[LabMan2.0][BVT][AdsApps][MT][Apps]";
        const string LABMANFFTP = "X64[LabMan][FFTP][AdsApps][MT][Apps]Advertiser";
        const string TFS_SERVER_URL = "http://adsgroupvstf:8080/tfs/adsgroup";
        const string TFSPROJECT = "AdsApps";
        const string TEST_REASON_FAILURE = "Test(s) failed or aborted";
        const string SETUP_REASON_FAILURE = "Setup or deployment failed";
        static int requestCounter;
        static List<string> buildNames;
        //static List<IBuildDetail> myBuilds;
        //bool hasBackground;
        int planId;
        string user;
        static IDictionary<string, List<TestRunSummary>> PlantestRuns;
        static IDictionary<string, string> AllEnvDictionary;
        delegate IDictionary<string, List<TestRunSummary>> GetTestRunsDelegate(int planId);
        delegate List<LabEnvironment> GetAllLabsDelegate();
        static IDictionary<string, List<IBuildDetail>> buildsByBuildDef;
        //static IDictionary<string, List<ITestRun>> PlantestRuns;
        BuildUtility bu;

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
            Response.Write(BuildHTMLUtility.getFooter());
        }

        private void Initialize()
        {
            buildNames = BuildUtility.getBuildDefinitions();
            requestCounter = 0;
            //hasBackground = true;
            //AdsApps_FFTP_MT_Full = new List<IBuildDetail>();
            //AdsApps_FFTP_MT_Refresh = new List<IBuildDetail>();
            //AdsApps_BVT_MT_Full = new List<IBuildDetail>();
            //AdsApps_BVT_MT_Refresh = new List<IBuildDetail>();
            //AdsApps_BVT_Buddy_MT_RefreshOnly = new List<IBuildDetail>();
            //LabMan_2_0_Buddy_AdsApps_RME_5 = new List<IBuildDetail>();
            //LabMan_2_0_BVT_AdsApps_RME_5 = new List<IBuildDetail>();
            //LabMan_2_0_BVT_AdsApps_RME_10 = new List<IBuildDetail>();
            //myBuilds = new List<IBuildDetail>();
            buildsByBuildDef = new Dictionary<string, List<IBuildDetail>>();

            //lockThis = new System.Object();
            bu = new BuildUtility();
            user = System.Web.HttpContext.Current.User.Identity.Name.ToLower();
            PlantestRuns = new Dictionary<string, List<TestRunSummary>>();
            planId = 3047;
            AllEnvDictionary = new Dictionary<string, string>();
        }

        private string GetHtml()
        {

            //////////////////////
            //get the test runs
            GetTestRunsAsync();
            //get the lab environments asynchronously
            GetLabEnvironmentsAsync();
            //get the builds asynchronously
            GetBuildsAsync();

            ///////////////////
            StringBuilder sbDays = new StringBuilder();

            //sbDays.Append(getHtmlHeader());



            //wait for the calls to finish
            //Session["TeamBuilds"] = TeamBuilds; 
            while (requestCounter > 0)
            {

            }

            //display builds for each build definition
            /////////////////////////
            if (buildsByBuildDef.ContainsKey("[AdsApps][BVT][MT-Refresh]"))
                sbDays.Append(BuildHTMLUtility.getBuildsHtmlByDefinition(buildsByBuildDef["[AdsApps][BVT][MT-Refresh]"], "[AdsApps][BVT][MT-Refresh]", PlantestRuns, AllEnvDictionary));
            //sbDays.Append(BuildHTMLUtility.getBuildsHtmlByDefinition(buildsByBuildDef["[AdsApps][BVT][MT-Refresh]"], "[AdsApps][BVT][MT-Refresh]"));
            if (buildsByBuildDef.ContainsKey("[AdsApps][BVT][MT-Full]"))
                sbDays.Append(BuildHTMLUtility.getBuildsHtmlByDefinition(buildsByBuildDef["[AdsApps][BVT][MT-Full]"], "[AdsApps][BVT][MT-Full]", PlantestRuns, AllEnvDictionary));
            if (buildsByBuildDef.ContainsKey("[AdsApps][FFTP][MT-Refresh]"))
                sbDays.Append(BuildHTMLUtility.getBuildsHtmlByDefinition(buildsByBuildDef["[AdsApps][FFTP][MT-Refresh]"], "[AdsApps][FFTP][MT-Refresh]", PlantestRuns, AllEnvDictionary));
            if (buildsByBuildDef.ContainsKey("[AdsApps][FFTP][MT-Full]"))
                sbDays.Append(BuildHTMLUtility.getBuildsHtmlByDefinition(buildsByBuildDef["[AdsApps][FFTP][MT-Full]"], "[AdsApps][FFTP][MT-Full]", PlantestRuns, AllEnvDictionary));
            if (buildsByBuildDef.ContainsKey("[AdsApps][BVT-Buddy][MT-RefreshOnly]"))
                sbDays.Append(BuildHTMLUtility.getBuildsHtmlByDefinition(buildsByBuildDef["[AdsApps][BVT-Buddy][MT-RefreshOnly]"], "[AdsApps][BVT-Buddy][MT-RefreshOnly]", PlantestRuns, AllEnvDictionary));
            if (buildsByBuildDef.ContainsKey("[LabMan2.0][Buddy][AdsApps][RME]-5"))
                sbDays.Append(BuildHTMLUtility.getBuildsHtmlByDefinition(buildsByBuildDef["[LabMan2.0][Buddy][AdsApps][RME]-5"], "[LabMan2.0][Buddy][AdsApps][RME]-5", PlantestRuns, AllEnvDictionary));
            if (buildsByBuildDef.ContainsKey("[LabMan2.0][BVT][AdsApps][RME]-5"))
                sbDays.Append(BuildHTMLUtility.getBuildsHtmlByDefinition(buildsByBuildDef["[LabMan2.0][BVT][AdsApps][RME]-5"], "[LabMan2.0][BVT][AdsApps][RME]-5", PlantestRuns, AllEnvDictionary));
            if (buildsByBuildDef.ContainsKey("[LabMan2.0][BVT][AdsApps][RME]-10"))
                sbDays.Append(BuildHTMLUtility.getBuildsHtmlByDefinition(buildsByBuildDef["[LabMan2.0][BVT][AdsApps][RME]-10"], "[LabMan2.0][BVT][AdsApps][RME]-10", PlantestRuns, AllEnvDictionary));

            return sbDays.ToString();
        }

        private string getHtmlHeader()
        {
            StringBuilder sbDays = new StringBuilder();
            sbDays.Append(BuildHTMLUtility.GetHeader());
            int lastXHours;
            if (!int.TryParse(ConfigurationManager.AppSettings["lastXHours"], out lastXHours))
                lastXHours = 24;
            sbDays.Append(string.Format("<h2>My Test Runs ( Last {1} hours ) - {0}</h2>", user, lastXHours.ToString()));
            return sbDays.ToString();
        }

        private void GetTestRunsAsync()
        {
            //get the test runs
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
                        //buildDetailSpec.MaxBuildsPerDefinition = 5;
                        buildDetailSpec.QueryOrder = BuildQueryOrder.FinishTimeDescending;
                        buildDetailSpec.RequestedFor = user;
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

        //static string GetBuildsTableHeader()
        //{
        //    StringBuilder sbDays = new StringBuilder();
        //    sbDays.Append(string.Format("<div id='{0}' class='{1}'>", "mybuilds", "table_font_size"));
        //    sbDays.Append(string.Format("<table cellpadding='{0}'  style='{1}'>", 3, "FONT-SIZE: 13px"));
        //    sbDays.Append("<tbody>");
        //    return sbDays.ToString();
        //}
    }
}