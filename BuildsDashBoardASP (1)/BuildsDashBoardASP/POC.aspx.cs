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

namespace BuildsDashBoardASP
{
    public partial class POC : System.Web.UI.Page
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
        static IDictionary<string, BuildDefinitionStat> buildDefStatDictionary;

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

            Response.Write(getFooter());
        }

        private string GetHtml()
        {

            //BuildUtility bu = new BuildUtility();
            StringBuilder sbDays = new StringBuilder();
            //get the header 
            //sbDays.Append(getHtmlHeader());

            //get the test runs
            //GetTestRunsAsync();
            //get the lab environments asynchronously
            //GetLabEnvironmentsAsync();
            //get the builds asynchronously
            //DateTime timeBefore;
            //DateTime timeAfter;
           
            //timeBefore = DateTime.Now;
            foreach (var bd in buildNames)
            {
                buildDefStatDictionary.Add(bd, new BuildDefinitionStat() { 
                    Name = bd,
                    PassedTests = 0, 
                    FailedTests = 0, 
                    PartiallySucceededTests = 0, 
                    TotalTests = 0 });
            }

            GetBuildsAsync();
            
            //wait for the calls to finish
            while (requestCounter > 0)
            {

            }

            //timeAfter = DateTime.Now;
            //TimeSpan timeDur = timeAfter.Subtract(timeBefore);
            //sbDays.Append(string.Format("{0}:{1}:{2}", timeDur.Hours.ToString(), timeDur.Minutes.ToString(), timeDur.Seconds.ToString()));
            //display builds for each build definition

            sbDays.Append(getBuildDefStats(buildDefStatDictionary));

            //foreach (var bd in buildNames)
            //{
            //    sbDays.Append(getBuildStatHtml(buildDefStatDictionary[bd]));
            //}

            //if (buildsByBuildDef.ContainsKey("[AdsApps][BVT][MT-Refresh]"))
            //    sbDays.Append(getBuildsHtmlByDefinition(buildsByBuildDef["[AdsApps][BVT][MT-Refresh]"], "[AdsApps][BVT][MT-Refresh]"));
            //if (buildsByBuildDef.ContainsKey("[AdsApps][BVT][MT-Full]"))
            //    sbDays.Append(getBuildsHtmlByDefinition(buildsByBuildDef["[AdsApps][BVT][MT-Full]"], "[AdsApps][BVT][MT-Full]"));
            //if (buildsByBuildDef.ContainsKey("[AdsApps][FFTP][MT-Refresh]"))
            //    sbDays.Append(getBuildsHtmlByDefinition(buildsByBuildDef["[AdsApps][FFTP][MT-Refresh]"], "[AdsApps][FFTP][MT-Refresh]"));
            //if (buildsByBuildDef.ContainsKey("[AdsApps][FFTP][MT-Full]"))
            //    sbDays.Append(getBuildsHtmlByDefinition(buildsByBuildDef["[AdsApps][FFTP][MT-Full]"], "[AdsApps][FFTP][MT-Full]"));
            //if (buildsByBuildDef.ContainsKey("[AdsApps][BVT-Buddy][MT-RefreshOnly]"))
            //    sbDays.Append(getBuildsHtmlByDefinition(buildsByBuildDef["[AdsApps][BVT-Buddy][MT-RefreshOnly]"], "[AdsApps][BVT-Buddy][MT-RefreshOnly]"));
            //if (buildsByBuildDef.ContainsKey("[LabMan2.0][Buddy][AdsApps][RME]-5"))
            //    sbDays.Append(getBuildsHtmlByDefinition(buildsByBuildDef["[LabMan2.0][Buddy][AdsApps][RME]-5"], "[LabMan2.0][Buddy][AdsApps][RME]-5"));
            //if (buildsByBuildDef.ContainsKey("[LabMan2.0][BVT][AdsApps][RME]-5"))
            //    sbDays.Append(getBuildsHtmlByDefinition(buildsByBuildDef["[LabMan2.0][BVT][AdsApps][RME]-5"], "[LabMan2.0][BVT][AdsApps][RME]-5"));
            //if (buildsByBuildDef.ContainsKey("[LabMan2.0][BVT][AdsApps][RME]-10"))
            //    sbDays.Append(getBuildsHtmlByDefinition(buildsByBuildDef["[LabMan2.0][BVT][AdsApps][RME]-10"], "[LabMan2.0][BVT][AdsApps][RME]-10"));

            return sbDays.ToString();
        }

        public static string getBuildDefStats(IDictionary<string, BuildDefinitionStat> builds)
        {

            StringBuilder sbDays = new StringBuilder();
            if (builds == null)
                return "";
            if (builds.Count > 0)
            {
                sbDays.Append(getBuildHtmlHeader());

                StringBuilder sbHtml = new StringBuilder();
                bool hasBackground = true;
                foreach (KeyValuePair<string, BuildDefinitionStat> b in builds)
                {
                            sbHtml.Append(GetBuildHtml(b.Value, hasBackground));
                            hasBackground = !hasBackground;
                }
                if (string.IsNullOrEmpty(sbHtml.ToString()))
                    return "</tbody></table></div>";
                sbDays.Append(sbHtml.ToString());
                sbDays.Append("</tbody></table></div>");
            }
            return sbDays.ToString();
        }

        public static string getBuildStatHtml(BuildDefinitionStat buildDefinitionStat)
        {
            return "";
        }

        private void Initialize()
        {
            //buildNames = new List<string>();
            //FFTP
            buildNames = BuildUtility.getBuildDefinitions();
            requestCounter = 0;
            buildsByBuildDef = new Dictionary<string, List<IBuildDetail>>();

            bu = new BuildUtility();
            PlantestRuns = new Dictionary<string, List<TestRunSummary>>();
            planId = 3047;
            AllEnvDictionary = new Dictionary<string, string>();
            buildDefStatDictionary = new Dictionary<string, BuildDefinitionStat>();
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
                            TFSPROJECT
                            );
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
                throw e;
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

        private string getFooter()
        {
            StringBuilder sbDays = new StringBuilder();
            sbDays.Append("</td>");
            sbDays.Append("</tr>");
            sbDays.Append("</tbody>");
            sbDays.Append("</table>");
            return sbDays.ToString();
        }
        private string getHtmlHeader()
        {
            StringBuilder sbDays = new StringBuilder();
            sbDays.Append(BuildUtility.GetHeader());
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

        //static string getBuildsHtmlByDefinition(List<IBuildDetail> builds, string definition)

        //{
        //    StringBuilder sbDays = new StringBuilder();
        //    if (builds == null)
        //        return "";
        //    if (builds.Count > 0)
        //    {
        //        sbDays.Append(string.Format("<h2 style='{0}'>{1}</h2>", "FONT-SIZE: 13px", GetBuildName(definition)));
        //        sbDays.Append(getBuildHtmlHeader());

        //        StringBuilder sbHtml = new StringBuilder();
        //        bool hasBackground = true;
        //        foreach (IBuildDetail b in builds)
        //        {
        //            if (isRme(b.BuildNumber))
        //            {
        //                sbHtml.Append(GetBuildHtmlForRME(b, hasBackground));
        //                hasBackground = !hasBackground;
        //            }
        //            else
        //                if (isAdsAppsBuild(b))
        //                {
        //                    sbHtml.Append(GetBuildHtml(b, hasBackground));
        //                    hasBackground = !hasBackground;
        //                }
        //        }
        //        if (string.IsNullOrEmpty(sbHtml.ToString()))
        //            return "</tbody></table></div>";
        //        sbDays.Append(sbHtml.ToString());
        //        sbDays.Append("</tbody></table></div>");
        //    }
        //    return sbDays.ToString();
        //}
        //static string getBuildsHtmlByDefinition(List<IBuildDetail> builds,string definition)
        //{
        //    StringBuilder sbDays = new StringBuilder();
        //    if (builds == null)
        //        return "";
        //    if (builds.Count > 0)
        //    {
        //        sbDays.Append(string.Format("<h2 style='{0}'>{1}</h2>", "FONT-SIZE: 13px", GetBuildName(definition)));
        //        sbDays.Append(getBuildHtmlHeader());
        //        //builds = builds.OrderByDescending(bd => bd.StartTime).ToList();
        //        //AdsApps_FFTP_MT_Full.Sort(delegate(IBuildDetail x, IBuildDetail y)
        //        //{
        //        //    if (x.FinishTime == null && y.FinishTime == null) return 0;
        //        //    else if (x.FinishTime == null) return -1;
        //        //    else if (y.FinishTime == null) return 1;
        //        //    else return x.FinishTime.CompareTo(y.FinishTime);
        //        //});
        //        bool hasBackground = true;
        //        foreach (IBuildDetail b in builds)
        //        {
        //            if (isRme(b.BuildNumber))
        //            {
        //                sbDays.Append(GetBuildHtmlForRME(b, hasBackground));
        //                hasBackground = !hasBackground;
        //            }
        //            else
        //                if (!isOwnerSame(b))
        //                {
        //                    sbDays.Append(GetBuildHtml(b, hasBackground));
        //                    hasBackground = !hasBackground;
        //                }
        //        }
        //        sbDays.Append("</tbody></table></div>");
        //    }
        //    return sbDays.ToString();
        //}

        static bool isAdsAppsBuild(IBuildDetail bd)
        {
            return !(bd.RequestedFor.Equals(bd.RequestedBy) && bd.RequestedFor.Equals("adslabtfsmgmt"));
        }
        static bool isRme(string buildNumber)
        {
            return buildNumber.Contains("[RME]");
        }

        static string getBuildHtmlHeader()
        {
            StringBuilder sbDays = new StringBuilder();

            sbDays.Append(GetBuildsTableHeader());


            sbDays.Append("<tr>");
            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 200));
            sbDays.Append("Build Definition");
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 50));
            sbDays.Append("Total");
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 50));
            sbDays.Append("Passed");
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 100));
            sbDays.Append("Partially Succeeded");
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 50));
            sbDays.Append("Failed");
            sbDays.Append("</td>");
            //sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 100));
            //sbDays.Append("Pass %");
            //sbDays.Append("</td>");
            //sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 100));
            //sbDays.Append("Failed/Total");
            //sbDays.Append("</td>");
            //sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 100));
            //sbDays.Append("Build Duration");
            //sbDays.Append("</td>");
            //sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 200));
            //sbDays.Append("Owner");
            //sbDays.Append("</td>");
            //sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 200));
            //sbDays.Append("Environment Name");
            //sbDays.Append("</td>")


            //sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 200));
            //sbDays.Append("Start Time");
            //sbDays.Append("</td>");
            //sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 200));
            //sbDays.Append("End Time");
            //sbDays.Append("</td>");

            //sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 100));
            //sbDays.Append("Test Duration");
            //sbDays.Append("</td>");
            //////////////////////
            sbDays.Append("</tr>");

            return sbDays.ToString();
        }

        static string GetBuildsTableHeader()
        {
            StringBuilder sbDays = new StringBuilder();
            sbDays.Append(string.Format("<div id='{0}' class='{1}'>", "mybuilds", "table_font_size"));
            sbDays.Append(string.Format("<table cellpadding='{0}'  style='{1}'>", 3, "FONT-SIZE: 13px"));
            sbDays.Append("<tbody>");
            return sbDays.ToString();
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

                    foreach (var b in builds)
                    {
                        if (buildDefStatDictionary.ContainsKey(b.BuildDefinition.Name))
                        {
                            if (b.Status == BuildStatus.Succeeded)
                                buildDefStatDictionary[b.BuildDefinition.Name].PassedTests++;
                            else if (b.Status == BuildStatus.PartiallySucceeded)
                                buildDefStatDictionary[b.BuildDefinition.Name].PartiallySucceededTests++;
                            if (b.Status == BuildStatus.Failed)
                                buildDefStatDictionary[b.BuildDefinition.Name].FailedTests++;
                            buildDefStatDictionary[b.BuildDefinition.Name].TotalTests++;
                        }

                    }
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

        static string GetBuildHtml(BuildDefinitionStat b, bool hasBackground)
        {
            StringBuilder sbDays = new StringBuilder();
            //List<ITestRun> testRuns;
            //IDictionary<string, object> buildStats = BuildUtility.GetBuildStatisticsForHomePage(b, testRuns);
            //string buildEnvName = BuildUtility.GetBuildEnvInfosForHomePage(b, testRuns, AllEnvDictionary);
            //BODY
            string rowClass="targetSuccess";
            //rowClass = BuildUtility.getRowClass(b, buildStats);

            if (hasBackground)
                sbDays.Append(string.Format("<tr style='{0}' class='{1}'>", "background-color:#ddd", rowClass));
            else
                sbDays.Append(string.Format("<tr class='{0}'>", rowClass));

            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 300));
            sbDays.Append(b.Name);
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 50));
            sbDays.Append(b.TotalTests.ToString());
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 50));
            sbDays.Append(b.PassedTests.ToString());
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 100));
            sbDays.Append(b.PartiallySucceededTests.ToString());
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 50));
            sbDays.Append(b.FailedTests.ToString());
            sbDays.Append("</td>");
          
            //sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 100));
            //if (b.Status == BuildStatus.Succeeded || b.Status == BuildStatus.PartiallySucceeded)
            //    sbDays.Append(string.Format("{0}%", Convert.ToDouble(buildStats["passPerc"]).ToString("0.00")));
            //sbDays.Append("</td>");
            //sbDays.Append(string.Format("<td align='{0}'>", "center"));
            //if (b.Status == BuildStatus.Succeeded || b.Status == BuildStatus.PartiallySucceeded)
            //    sbDays.Append(string.Format("{0}/{1}", ((int)(buildStats["totalFailed"])).ToString(), ((int)(buildStats["totalTests"])).ToString()));
            //sbDays.Append("</td>");
            //sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 100));
            ////display build duration only if finish time is > than start time
            //if (b.FinishTime.CompareTo(b.StartTime) > 0)
            //{
            //    if (b.Status != BuildStatus.InProgress && b.Status != BuildStatus.None && b.Status != BuildStatus.NotStarted)
            //    {
            //        TimeSpan timeDur = b.FinishTime.Subtract(b.StartTime);
            //        sbDays.Append(string.Format("{0}:{1}:{2}", timeDur.Hours.ToString(), timeDur.Minutes.ToString(), timeDur.Seconds.ToString()));
            //    }
            //}
            //sbDays.Append("</td>");
            //sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 200));
            //sbDays.Append(b.RequestedFor);
            //sbDays.Append("</td>");
            //sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 200));
            //if (b.Status == BuildStatus.Succeeded || b.Status == BuildStatus.PartiallySucceeded)
            //{
            //    if (!string.IsNullOrEmpty(buildEnvName))
            //        sbDays.Append(buildEnvName);
            //}
            //sbDays.Append("</td>");
            ////sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 200));
            ////if (b.Status == BuildStatus.PartiallySucceeded)
            ////{
            ////    sbDays.Append(TEST_REASON_FAILURE);
            ////}
            ////else if (b.Status == BuildStatus.Failed)
            ////{
            ////    sbDays.Append(SETUP_REASON_FAILURE);
            ////}
            ////sbDays.Append("</td>");
            /////COMMENTED FOR PERFORMANCE ANALYSIS
            //sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 200));
            //sbDays.Append(b.StartTime);
            //sbDays.Append("</td>");
            //sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 200));
            //if (b.Status != BuildStatus.InProgress && b.Status != BuildStatus.None && b.Status != BuildStatus.NotStarted)
            //    sbDays.Append(b.FinishTime);
            //sbDays.Append("</td>");

            ///COMMENTED FOR PERFORMANCE ANALYSIS
            //sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center",100));
            //TimeSpan duration =(TimeSpan)buildStats["duration"];
            //if (b.Status == BuildStatus.Succeeded || b.Status == BuildStatus.PartiallySucceeded)
            //sbDays.Append(string.Format("{0}:{1}:{2}", duration.Hours.ToString(), duration.Minutes.ToString(), duration.Seconds.ToString()));
            //sbDays.Append("</td>");

            ////////////////////////////////
            sbDays.Append("</tr>");
            return sbDays.ToString();
        }

        static string GetBuildHtmlForRME(IBuildDetail b, bool hasBackground)
        {
            StringBuilder sbDays = new StringBuilder();
            //List<ITestRun> testRuns;
            List<TestRunSummary> testRuns;
            if (PlantestRuns.ContainsKey(b.BuildNumber))
                testRuns = PlantestRuns[b.BuildNumber];
            else
                testRuns = null;
            IDictionary<string, object> buildStats = BuildUtility.GetBuildStatisticsForHomePage(b, testRuns);
            string buildEnvName = BuildUtility.GetBuildEnvInfosForHomePage(b, testRuns, AllEnvDictionary);
            //BODY
            string rowClass;
            rowClass = BuildUtility.getRowClass(b, buildStats);

            if (hasBackground)
                sbDays.Append(string.Format("<tr style='{0}' class='{1}'>", "background-color:#ddd", rowClass));
            else
                sbDays.Append(string.Format("<tr class='{0}'>", rowClass));
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 50));
            //\\asgdrops\AdsApps\AdsApps_vnext\Rolling\17.02.1.20141006.3347047\retail\amd64\App\Test
            sbDays.Append(string.Format("<a href='{0}'>{1}</a>", string.Format("http://adsgroupvstf:8080/tfs/AdsGroup/AdsApps/_build#buildUri={0}&_a=summary", b.Uri), b.BuildNumber.Substring(b.BuildNumber.IndexOf("_") + 1)));
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 100));
            sbDays.Append(string.Format("<a href='{0}'>{1}</a>", "BuildDetail.aspx?name=" + b.BuildNumber, b.Status));
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 100));
            //if (b.Status == BuildStatus.Succeeded || b.Status == BuildStatus.PartiallySucceeded)
            //    sbDays.Append(string.Format("{0}%", Convert.ToDouble(buildStats["passPerc"]).ToString("0.00")));
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}'>", "right"));
            //if (b.Status == BuildStatus.Succeeded || b.Status == BuildStatus.PartiallySucceeded)
            //    sbDays.Append(string.Format("{0}/{1}", ((int)(buildStats["totalFailed"])).ToString(), ((int)(buildStats["totalTests"])).ToString()));
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 100));
            if (b.FinishTime.CompareTo(b.StartTime) > 0)
            {
                if (b.Status != BuildStatus.InProgress && b.Status != BuildStatus.None && b.Status != BuildStatus.NotStarted)
                {
                    TimeSpan timeDur = b.FinishTime.Subtract(b.StartTime);
                    sbDays.Append(string.Format("{0}:{1}:{2}", timeDur.Hours.ToString(), timeDur.Minutes.ToString(), timeDur.Seconds.ToString()));
                }
            }
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 200));
            sbDays.Append(b.RequestedFor);
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 200));
            if (b.Status == BuildStatus.Succeeded || b.Status == BuildStatus.PartiallySucceeded)
            {
                if (!string.IsNullOrEmpty(buildEnvName))
                    sbDays.Append(buildEnvName);
            }
            sbDays.Append("</td>");
            //sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 200));
            //if (b.Status == BuildStatus.PartiallySucceeded)
            //{
            //    sbDays.Append(TEST_REASON_FAILURE);
            //}
            //else if (b.Status == BuildStatus.Failed)
            //{
            //    sbDays.Append(SETUP_REASON_FAILURE);
            //}
            //sbDays.Append("</td>");
            ///COMMENTED FOR PERFORMANCE ANALYSIS
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 200));
            sbDays.Append(b.StartTime);
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 200));
            if (b.Status != BuildStatus.InProgress && b.Status != BuildStatus.None && b.Status != BuildStatus.NotStarted)
                sbDays.Append(b.FinishTime);
            sbDays.Append("</td>");

            ///COMMENTED FOR PERFORMANCE ANALYSIS
            //sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center",100));
            //TimeSpan duration =(TimeSpan)buildStats["duration"];
            //if (b.Status == BuildStatus.Succeeded || b.Status == BuildStatus.PartiallySucceeded)
            //sbDays.Append(string.Format("{0}:{1}:{2}", duration.Hours.ToString(), duration.Minutes.ToString(), duration.Seconds.ToString()));
            //sbDays.Append("</td>");

            ////////////////////////////////
            sbDays.Append("</tr>");
            return sbDays.ToString();
        }

        static string GetBuildName(string bn)
        {
            if (bn.Equals("[AdsApps][FFTP][MT-Full]"))
                return "Regression test runs with full deployment &nbsp;&nbsp;&nbsp; [AdsApps][FFTP][MT-Full]";
            else if (bn.Equals("[AdsApps][FFTP][MT-Refresh]"))
                return "Regression test runs with MT refresh deployment &nbsp;&nbsp;&nbsp; [AdsApps][FFTP][MT-Refresh]";
            else if (bn.Equals("[AdsApps][BVT][MT-Full]"))
                return "BVT test runs with full deployment &nbsp;&nbsp;&nbsp; [AdsApps][BVT][MT-Full]";
            else if (bn.Equals("[AdsApps][BVT][MT-Refresh]"))
                return "BVT test runs with MT refresh deployment &nbsp;&nbsp;&nbsp; [AdsApps][BVT][MT-Refresh]";
            else if (bn.Equals("[AdsApps][BVT-Buddy][MT-RefreshOnly]"))
                return "BVT test runs with buddy build and MT refresh deployment &nbsp;&nbsp;&nbsp; [AdsApps][BVT-Buddy][MT-RefreshOnly]";
            else if (bn.Equals("[LabMan2.0][Buddy][AdsApps][RME]-5"))
                return "Producer for precreated environments for buddy builds &nbsp;&nbsp;&nbsp; [LabMan2.0][Buddy][AdsApps][RME]-5";
            else if (bn.Equals("[LabMan2.0][BVT][AdsApps][RME]-5"))
                return "Producer for pre-created environments for CI BVTs &nbsp;&nbsp;&nbsp; [LabMan2.0][BVT][AdsApps][RME]-5";
            else if (bn.Equals("[LabMan2.0][BVT][AdsApps][RME]-10"))
                return "Producer for pre-created environments for FFTP &nbsp;&nbsp;&nbsp; [LabMan2.0][BVT][AdsApps][RME]-10";
            else
                return "UnKnown";
        }

        private List<IBuildDetail> GetBuildByName(Dictionary<string, List<IBuildDetail>> builds, string buildName)
        {
            if (builds != null)
                if (builds.ContainsKey(buildName))
                {
                    List<IBuildDetail> b = builds[buildName];
                    return b;
                }
                else
                    return null;
            else
                return null;
        }
    }
}