using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Web.UI;
//using System.Web.UI.WebControls;
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
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.LabMan;
using Microsoft.LabMan.Lab;
using System.Configuration;
using System.Runtime.Remoting.Messaging;
using TFSUtility;
using HTMLUtility;

namespace Alert
{

    public class AlertHandler
    {
        const string TFS_SERVER_URL = "http://adsgroupvstf:8080/tfs/adsgroup";
        const string TFSPROJECT = "AdsApps";
        const string TEST_REASON_FAILURE = "Test(s) failed or aborted";
        const string SETUP_REASON_FAILURE = "Setup or deployment failed";
        //bool hasBackground;
        int planId;
        static int requestCounter;
        static List<string> buildNames;
        static IDictionary<string, string> friendlyNamesDict;

        delegate IDictionary<string, List<TestRunSummary>> GetTestRunsDelegate(int planId);
        delegate List<LabEnvironment> GetLabsDelegate(TagDetail td);

        static IDictionary<string, List<IBuildDetail>> buildsByBuildDef;

        static IDictionary<string, List<TestRunSummary>> PlantestRuns;
        static IDictionary<string, string> AllEnvDictionary;
        delegate List<LabEnvironment> GetAllLabsDelegate();

        public AlertHandler()
        {
            //_tfsUrl = ConfigurationManager.AppSettings["TfsUrl"];
            //if (!Int32.TryParse(ConfigurationManager.AppSettings["DBCommandTimeInSeconds"], out _sqlCommandTimeoutInSeconds))
            //    _sqlCommandTimeoutInSeconds = 120;
        }

        public void ProcessPoolAlerts()
        {

            StringBuilder sbDays = new StringBuilder();
            Initialize();

            List<string> warningAddress = getWarwingAddress();
            List<string> alertAddress = getAlertAddress();

            string alertSubject = null;
            string warningSubject = "Warning: Test pools less than the threshold ";

            var templateList = BuildUtility.getPoolNames();

            getEnvAsync(templateList);

            //wait for the calls to finish
            while (requestCounter > 0)
            {

            }

            List<TagDetail> warningList = getWarningPools( templateList );
            List<TagDetail> alertList = getAlertPools( templateList );
            
            ////////////////////////////////////////
            
            foreach (var t in templateList)
            {
                var template = new List<TagDetail>();
                template.Add(t);

                alertList = getAlertPools(template);
                warningList = getWarningPools(template);

                if (warningList != null)
                {
                    if (warningList.Count > 0)
                    {
                        //sbDays.Append((getHtmlHeader()));
                        warningSubject = string.Format( "[AdsApps][TestRuns][Warning] Less than {0} machines in {1}", MainSettings.Default.WarningMaxRange.ToString(), t.DisplayName);

                        BuildUtility.sendEmail(EnvironmentHTMLUtility.getPoolSummaryHtml(warningList), MainSettings.Default.AlertFrom, warningAddress, warningSubject, MainSettings.Default.smtpserver);
                    }
                }

                if (alertList != null)
                {
                    if (alertList.Count > 0)
                    {
                        //sbDays.Append((getHtmlHeader()));
                        alertSubject = string.Format("[AdsApps][TestRuns][Alert] Less than {0} machines in {1}", MainSettings.Default.WarningMaxRange.ToString(), t.DisplayName);

                        BuildUtility.sendEmail(EnvironmentHTMLUtility.getPoolSummaryHtml(alertList), MainSettings.Default.AlertFrom, warningAddress, alertSubject, MainSettings.Default.smtpserver);
                    }
                }
            }
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

        private List<TagDetail> getWarningPools(IList<TagDetail> templateList)
        {
            List<TagDetail> pools = new List<TagDetail>();

            foreach (var td in templateList)
            {
                if ( isWarning( td.ReadyLabs.Count ))
                    pools.Add(td);
            }

            return pools;
        }

        private bool isWarning(int p)
        {
            int WarningMinRange;
            if (!int.TryParse(MainSettings.Default.WarningMinRange, out WarningMinRange))
                WarningMinRange = 1;

            int WarningMaxRange;
            if (!int.TryParse(MainSettings.Default.WarningMaxRange, out WarningMaxRange))
                WarningMaxRange = 5;

                return (p <= WarningMaxRange && p >= WarningMinRange);
        }

        private bool isAlert(int p)
        {
            int AlertMinRange;
            if (!int.TryParse(MainSettings.Default.AlertMinRange, out AlertMinRange))
                AlertMinRange = 0;

            int AlertMaxRange;
            if (!int.TryParse(MainSettings.Default.AlertMaxRange, out AlertMaxRange))
                AlertMaxRange = 0;

            return (p <= AlertMaxRange && p >= AlertMinRange);
        }

        private List<TagDetail> getAlertPools(IList<TagDetail> templateList)
        {
            List<TagDetail> pools = new List<TagDetail>();

            foreach (var td in templateList)
            {
                if ( isAlert(td.ReadyLabs.Count) )
                    pools.Add(td);
            }

            return pools;
        }

        private List<string> getAlertAddress()
        {
            string[] address = MainSettings.Default.AlertTo.Split(',');
            return address.ToList();
        }

        private List<string> getWarwingAddress()
        {
            string[] address = MainSettings.Default.AlertTo.Split(',');
            return address.ToList(); 
        }

        public void ProcessBuildAlerts()
        {
            StringBuilder sbDays = new StringBuilder();

            string alertSubject = null;
            string warningSubject = null;

            List<string> warningAddress = getWarwingAddress();
            List<string> alertAddress = getAlertAddress();

            Initialize();
            //get the test runs
            GetTestRunsAsync();
            //get the lab environments asynchronously
            GetLabEnvironmentsAsync();

            GetBuildsAsync();

            waitAllAsyncCalls();

            List<IBuildDetail> builds = null;

            foreach (var def in buildNames)
            {
                sbDays.Clear();
                 builds = getAlerts(buildsByBuildDef[def],def);

                 if (builds != null)
                 {
                     if (builds.Count > 0)
                     {
                         //sbDays.Append((getHtmlHeader()));
                         try
                         {
                             sbDays.Append(BuildHTMLUtility.getBuildsHtmlByDefinition(builds, def, PlantestRuns, AllEnvDictionary));
                         }
                         catch (Exception exc)
                         {
                             throw;
                         }

                         //Response.Write(BuildHTMLUtility.getFooter());
                         //sbDays.Append(BuildHTMLUtility.getFooter());
                         if (BuildUtility.isFFTP(def))
                             alertSubject = string.Format("[AdsApps][TestRuns][Alert] Last 1 test runs failed for {0}", def);
                         else
                             alertSubject = string.Format("[AdsApps][TestRuns][Alert] Last 2 test runs failed for {0}", def);

                         BuildUtility.sendEmail(sbDays.ToString(), MainSettings.Default.AlertFrom, alertAddress, alertSubject, MainSettings.Default.smtpserver);
                     }
                 }
            }
        }

        private void waitAllAsyncCalls()
        {
            while (requestCounter > 0)
            {

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

        private void GetTestRunsAsync()
        {
            GetTestRunsDelegate dlgt = new GetTestRunsDelegate(BuildUtility.GetTestRuns);
            AsyncCallback cb = new AsyncCallback(ProcessTestRunsCallBack);
            Interlocked.Increment(ref requestCounter);
            IAsyncResult ar = dlgt.BeginInvoke(planId, cb, dlgt);
        }



        public static System.Collections.Generic.List<IBuildDetail> getAlerts(System.Collections.Generic.List<IBuildDetail> list,string def)
        {
            var builds = list.OrderByDescending(bd => bd.StartTime).ToList();
            builds = getBuildForAlert(builds, def);

            if (builds.All(bd => bd.Status != BuildStatus.PartiallySucceeded && bd.Status != BuildStatus.Succeeded))
            {
                return builds;
            }
            return null;
        }

        private static System.Collections.Generic.List<IBuildDetail> getBuildForAlert(List<IBuildDetail> builds, string def)
        {
            if( BuildUtility.isFFTP(def) )
                return builds.Take(1).ToList();
            else
                return builds.Take(2).ToList();
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
                throw;
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
        private string getHtmlHeader()
        {
            StringBuilder sbDays = new StringBuilder();
            sbDays.Append(BuildHTMLUtility.GetHeader());
            return sbDays.ToString();
        }

        private void Initialize()
        {
            buildsByBuildDef = new Dictionary<string, List<IBuildDetail>>();
            friendlyNamesDict = BuildUtility.getEnvironmentPoolFriendlyNames();

            buildNames = BuildUtility.getBuildDefinitions();
            requestCounter = 0;
            PlantestRuns = new Dictionary<string, List<TestRunSummary>>();
            planId = 3047;
            AllEnvDictionary = new Dictionary<string, string>();
            //_hasBackground = true;
        }

        private string GetHtml()
        {

            //BuildUtility bu = new BuildUtility();
            StringBuilder sbDays = new StringBuilder();

            var templateList = BuildUtility.getPoolNames();

            getEnvAsync(templateList);

            //wait for the calls to finish
            while (requestCounter > 0)
            {

            }

            //display pool environments summary
            sbDays.Append(EnvironmentHTMLUtility.GetSummaryTableHeader());
            foreach (var td in templateList)
            {
                sbDays.Append(td.SummaryHtml);
            }
            sbDays.Append(EnvironmentHTMLUtility.GetSummaryTableFooter());

            //display pool environments details
            foreach (var td in templateList)
            {
                sbDays.Append(td.Html);
            }
            return sbDays.ToString();
        }

        private void getEnvAsync(IList<TagDetail> templateList)
        {
            foreach (var tag in templateList)
            {
                GetLabsDelegate dlgt = new GetLabsDelegate(BuildUtility.GetLabEnvs);
                AsyncCallback cb = new AsyncCallback(ProcessLabEnvsCallBack);
                Interlocked.Increment(ref requestCounter);
                object state = tag;
                IAsyncResult ar = dlgt.BeginInvoke(tag, cb, tag);
            }
        }




        static void ProcessLabEnvsCallBack(IAsyncResult result)
        {
            TagDetail tagDetail = (TagDetail)result.AsyncState;
            GetLabsDelegate dlgt = (GetLabsDelegate)((AsyncResult)result).AsyncDelegate;
            using (var connection = TeamProjectCollectionConnection.FromFullUri(new Uri(TFS_SERVER_URL)))
            {
                LabService labService = connection.TeamProjectCollection.GetService<LabService>();
                try
                {
                    //var buildServer = (IBuildServer)tfs.GetService(typeof(IBuildServer));

                    List<LabEnvironment> labs = dlgt.EndInvoke(result);
                    // populate the tagdetail object
                    tagDetail.Total = labs.Count;
                    tagDetail.InUseLabs = labs.Where(env => env.InUseMarker != null).ToList();
                    tagDetail.ReadyLabs = BuildUtility.GetFreeLabEnv(labs).OrderByDescending(bd => bd.CreationTime).ToList();
                    if (tagDetail.ReadyLabs != null)
                    {
                        tagDetail.SummaryHtml = EnvironmentHTMLUtility.GetTagDetailSummaryHtml(tagDetail, true, friendlyNamesDict);
                        if (tagDetail.ReadyLabs.Count > 0)
                        {
                            StringBuilder sbDays = new StringBuilder();
                            if (friendlyNamesDict.ContainsKey(tagDetail.DisplayName))
                            {
                                //sbDays.Append(string.Format("<h2 style='{0}'>{1} {2} {3} {4} {5}</h2>", "FONT-SIZE: 13px", friendlyNamesDict[tagDetail.DisplayName], "&nbsp;&nbsp;&nbsp;", tagDetail.DisplayName, "&nbsp;&nbsp;&nbsp;", GetTagSummary(tagDetail)));
                                sbDays.Append(string.Format("<h2 style='{0}'>{1}</h2>", "FONT-SIZE: 13px", friendlyNamesDict[tagDetail.DisplayName]));
                            }
                            //sbDays.Append(GetSummaryHtml(tagDetail));
                            sbDays.Append(EnvironmentHTMLUtility.GetTableHeader());
                            bool hasBackground1 = true;
                            foreach (var lab in tagDetail.ReadyLabs)
                            {
                                sbDays.Append(EnvironmentHTMLUtility.GetLabHtml(lab, hasBackground1));
                                hasBackground1 = !hasBackground1;
                            }
                            sbDays.Append(EnvironmentHTMLUtility.GetTableFooter());
                            tagDetail.Html = sbDays.ToString();
                        }
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
    }
}
