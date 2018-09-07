using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Build.Client;
using System.Security.Principal;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.Lab.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.Framework.Client;
using System.Configuration;
using System.Web;

using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.LabMan;
using Microsoft.LabMan.Lab;
using System.Text;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Configuration;
using System.Runtime.Remoting.Messaging;
using System.Net.Mail;
//using Microsoft.TeamFoundation.Framework.Common;

namespace BuildsUtility
{
    public class BuildDetailsRequest
    {
        public List<IBuildDetail> buildDetails;
        public Exception ExceptionObject;

    }
    public class TestRunSummary
    {

        public int PassedTests { get; set; }
        public int FailedTests { get; set; }
        public int TotalTests { get; set; }
        public DateTime DateStarted { get; set; }
        public DateTime DateCompleted { get; set; }
        public Guid EnvironmentId { get; set; }


    }

    public class BuildDefinitionStat
    {
        public string Name { get; set; }
        public int PassedTests { get; set; }
        public int FailedTests { get; set; }
        public int TotalTests { get; set; }
        public int PartiallySucceededTests { get; set; }
        //public DateTime DateStarted { get; set; }
        //public DateTime DateCompleted { get; set; }
        //public Guid EnvironmentId { get; set; }
    }

    public class BuildUtility
    {
        const string TFS_SERVER_URL = "http://adsgroupvstf:8080/tfs/adsgroup";
        const string TFS_PROJECT = "AdsApps";
        public Dictionary<string, List<IBuildDetail>> GetBuilds(List<string> definitions, int days, string user = null)
        {
            string tfsProject = "AdsApps";
            //string BuildDefinition = "[LabMan2.0][BVT][AdsApps][MT][Apps]";
            string tfsServerUrl = "http://adsgroupvstf:8080/tfs/adsgroup";

            Dictionary<string, List<IBuildDetail>> builds = null;
            if (definitions != null)
            {
                builds = new Dictionary<string, List<IBuildDetail>>();
                List<IBuildDetail> returnedbuilds = null;
                if (user != null)
                {
                    foreach (string def in definitions)
                    {
                        returnedbuilds = new List<IBuildDetail>();
                        returnedbuilds = GetBuildByDef(tfsProject, def, tfsServerUrl, days, user);
                        builds.Add(def, returnedbuilds);
                    }
                }
                else
                {
                    foreach (string def in definitions)
                    {
                        returnedbuilds = new List<IBuildDetail>();
                        returnedbuilds = GetBuildByDef(tfsProject, def, tfsServerUrl, days);
                        builds.Add(def, returnedbuilds);
                    }

                }

            }
            return builds;
        }

        public static string getRowClass(IBuildDetail b, IDictionary<string, object> buildStats)
        {

            //////////
            string rowClass = "targetFailure";
            if (b.Status == BuildStatus.Succeeded || Convert.ToDouble(buildStats["passPerc"]).ToString("0.00").Equals("100.00"))
                rowClass = "targetSuccess";
            else if (b.Status == BuildStatus.PartiallySucceeded && !(Convert.ToDouble(buildStats["passPerc"]).ToString("0.00").Equals("0.00")))
                rowClass = "targetPartiallySucceededTestFailed";
            else if (b.Status == BuildStatus.PartiallySucceeded && Convert.ToDouble(buildStats["passPerc"]).ToString("0.00").Equals("0.00"))
                rowClass = "targetFailure";
            else if (b.Status == BuildStatus.InProgress || b.Status == BuildStatus.NotStarted)
                rowClass = "targetNotFinished";
            else
                rowClass = "targetFailure";
            return rowClass;
        }

        public static List<string> getBuildDefinitions()
        {
            string[] buildNames = MainSettings.Default.buildDefinitions.Split(',');
            return buildNames.ToList();
        }

        public static List<IBuildDetail> GetBuildByDef(string tfsProject, string BuildDefinition, string tfsServerUrl, int days, string user = null)
        {
            using (TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(tfsServerUrl)))
            {
                var buildServer = (IBuildServer)tfs.GetService(typeof(IBuildServer));

                IBuildDetailSpec buildDetailSpec = buildServer.CreateBuildDetailSpec(
                    tfsProject,
                    BuildDefinition);
                //if (user != null)
                //    buildDetailSpec.MaxBuildsPerDefinition = 5;
                //else
                //    //buildDetailSpec.MaxBuildsPerDefinition = 10;
                buildDetailSpec.QueryOrder = BuildQueryOrder.FinishTimeDescending;
                int lastXHours = 24;
                if (!int.TryParse(ConfigurationManager.AppSettings["lastXHours"], out lastXHours))
                    lastXHours = 24;
                buildDetailSpec.MinFinishTime = DateTime.Now.AddHours(-lastXHours);
                buildDetailSpec.Status = BuildStatus.All;
                buildDetailSpec.InformationTypes = null;
                if (user != null)
                    buildDetailSpec.RequestedFor = user;

                IBuildQueryResult results = buildServer.QueryBuilds(buildDetailSpec);


                return results.Builds.ToList();
            }
        }
        //public ITestCaseResultCollection GetFailedTests(ITestRun tr)
        //{
        //    return tr.QueryResultsByOutcome(TestOutcome.Failed);
        //}
        //public ITestCaseResultCollection GetPassedTests(ITestRun tr)
        //{
        //    return tr.QueryResultsByOutcome(TestOutcome.Passed);
        //}
        //public TimeSpan GetRunDuration(ITestRun tr)
        //{
        //    IEnumerable<ITestCaseResult> results = null;
        //    results = tr.QueryResults().AsEnumerable<ITestCaseResult>();
        //    //DateTime sum = new DateTime();
        //    TimeSpan sum = TimeSpan.Zero;
        //    foreach (ITestCaseResult tcr in results)
        //    {
        //        sum += tcr.DateCompleted.Subtract(tcr.DateStarted); 
        //    }
        //    return sum;
        //}


        public TimeSpan GetRunDuration(ITestRun tr)
        {
            return tr.DateCompleted.Subtract(tr.DateStarted);
        }
        public static TimeSpan GetRunDuration(TestRunSummary tr)
        {
            return tr.DateCompleted.Subtract(tr.DateStarted);
        }
        public static IDictionary<string, object> GetBuildStatisticsForHomePage(IBuildDetail build, List<TestRunSummary> testRuns)
        {
            int totalPassed = 0;
            int totalFailed = 0;
            int totalTests = 0;
            //string envName;
            TimeSpan sumTime = TimeSpan.Zero;
            Dictionary<string, object> stats = new Dictionary<string, object>();
            //IEnumerable<ITestRun> testRuns = null;
            if (build.Status == BuildStatus.PartiallySucceeded || build.Status == BuildStatus.Succeeded)
            {
                // using (TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(TFS_SERVER_URL)))
                // {
                //testRuns = tms.GetTeamProject("AdsApps").TestRuns.ByBuild(build.Uri);
                if (testRuns != null)
                {
                    //get the environment name for the build
                    //envName = GetEnvName(testRuns.FirstOrDefault());
                    //stats.Add("EnvironmentName", envName);
                    foreach (var tr in testRuns)
                    {
                        totalPassed += tr.PassedTests;
                        totalFailed += tr.FailedTests;
                        totalTests += tr.TotalTests;
                        sumTime += GetRunDuration(tr);
                    }
                }
                //}
            }
            stats.Add("totalPassed", totalPassed);
            stats.Add("totalFailed", totalFailed);
            stats.Add("totalTests", totalTests);
            stats.Add("duration", sumTime);

            if (totalTests != 0)
            {
                stats.Add("passPerc", ((double)totalPassed / (double)totalTests) * 100);
                stats.Add("failPerc", (totalFailed / totalTests) * 100);
            }
            else
            {
                stats.Add("passPerc", 0);
                stats.Add("failPerc", 0);
            }
            //stats.Add("html", getBuildDetailHtml(testRuns, build, stats));
            return stats;
        }

        public static string GetBuildEnvInfosForHomePage(IBuildDetail build, List<TestRunSummary> testRuns, IDictionary<string, string> envLabNames)
        {
            //IEnumerable<ITestRun> testRuns = null;
            if (build.Status == BuildStatus.PartiallySucceeded || build.Status == BuildStatus.Succeeded)
            {
                // using (TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(TFS_SERVER_URL)))
                // {
                //testRuns = tms.GetTeamProject("AdsApps").TestRuns.ByBuild(build.Uri);
                if (testRuns != null)
                {
                    //get the environment name for the build
                    foreach (var tr in testRuns)
                    {
                        if (envLabNames.ContainsKey(tr.EnvironmentId.ToString()))
                            return envLabNames[tr.EnvironmentId.ToString()];
                    }
                    return "Environment Deleted";
                }
                else
                    return "";
                //}
            }
            return "";
        }

        public static string GetBuildDefinition(string bn)
        {
            if (bn.Contains("[AdsApps][FFTP][MT-Full]"))
                return "[AdsApps][FFTP][MT-Full]";
            else if (bn.Contains("[AdsApps][FFTP][MT-Refresh]"))
                return "[AdsApps][FFTP][MT-Refresh]";
            else if (bn.Contains("[AdsApps][BVT][MT-Full]"))
                return "[AdsApps][BVT][MT-Full]";
            else if (bn.Contains("[AdsApps][BVT][MT-Refresh]"))
                return "[AdsApps][BVT][MT-Refresh]";
            else if (bn.Contains("[AdsApps][BVT-Buddy][MT-RefreshOnly]"))
                return "[AdsApps][BVT-Buddy][MT-RefreshOnly]";
            else if (bn.Contains("[LabMan2.0][Buddy][AdsApps][RME]-5"))
                return "[LabMan2.0][Buddy][AdsApps][RME]-5";
            else if (bn.Contains("[LabMan2.0][BVT][AdsApps][RME]-5"))
                return "[LabMan2.0][BVT][AdsApps][RME]-5";
            else if (bn.Contains("[LabMan2.0][BVT][AdsApps][RME]-10"))
                return "[LabMan2.0][BVT][AdsApps][RME]-10";
            else
                return "UnKnown";
        }
        //public IDictionary<string, object> GetBuildStatisticsForHomePage(IBuildDetail build, List<ITestRun> testRuns)
        //{
        //    int totalPassed = 0;
        //    int totalFailed = 0;
        //    int totalTests = 0;
        //    TimeSpan sumTime = TimeSpan.Zero;
        //    Dictionary<string, object> stats = new Dictionary<string, object>();
        //    //IEnumerable<ITestRun> testRuns = null;
        //    if (build.Status == BuildStatus.PartiallySucceeded || build.Status == BuildStatus.Succeeded)
        //    {
        //        using (TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(TFS_SERVER_URL)))
        //        {
        //            //testRuns = tms.GetTeamProject("AdsApps").TestRuns.ByBuild(build.Uri);
        //            if (testRuns != null)
        //            {
        //                foreach (var tr in testRuns)
        //                {
        //                    totalPassed += tr.Statistics.PassedTests;
        //                    totalFailed += tr.Statistics.FailedTests;
        //                    totalTests += tr.Statistics.TotalTests;
        //                    sumTime += GetRunDuration(tr);
        //                }
        //            }
        //        }
        //    }
        //    stats.Add("totalPassed", totalPassed);
        //    stats.Add("totalFailed", totalFailed);
        //    stats.Add("totalTests", totalTests);
        //    stats.Add("duration", sumTime);

        //    if (totalTests != 0)
        //    {
        //        stats.Add("passPerc", ((double)totalPassed / (double)totalTests) * 100);
        //        stats.Add("failPerc", (totalFailed / totalTests) * 100);
        //    }
        //    else
        //    {
        //        stats.Add("passPerc", 0);
        //        stats.Add("failPerc", 0);
        //    }
        //    //stats.Add("html", getBuildDetailHtml(testRuns, build, stats));
        //    return stats;
        //}
        public static List<LabEnvironment> GetLabEnvs(TagDetail tag)
        {
            try
            {
                using (var connection = TeamProjectCollectionConnection.FromFullUri(new Uri(TFS_SERVER_URL)))
                {
                    //LabService labService = connection.TeamProjectCollection.GetService<LabService>();
                    LabService labService = connection.TeamProjectCollection.GetService<LabService>();
                    string templateTagName = "Template";

                    string environmentPoolTagName = "LkgBuild_AdsApps_Main";
                    string environmentPoolTagValue = "10.00:00:00";

                    bool utilizeMachineFromPool = true;
                    bool utilizeTaggedEnvironmentPool = true;

                    Dictionary<string, object> environmentProperties = new Dictionary<string, object>();
                    environmentProperties.Add(environmentPoolTagName, environmentPoolTagValue);
                    IEnvironmentMatchingCriteria matchingCriteria;
                    int totalEnvsCount, inUseEnvsCount, freeEnvsCount;
                    totalEnvsCount = inUseEnvsCount = freeEnvsCount = 0;
                    if (utilizeTaggedEnvironmentPool)
                    {
                        matchingCriteria = new PoolTagMatchingCriteria(templateTagName, tag.DisplayName, environmentProperties);
                    }
                    else
                    {
                        matchingCriteria = new NonPooledMatchingCriteria(templateTagName, tag.DisplayName);
                    }

                    var querySpec = new LabEnvironmentQuerySpec { Disposition = LabEnvironmentDisposition.Active, Project = "AdsApps" };
                    LabEnvironmentQuerySpec[] querySpecs = new LabEnvironmentQuerySpec[] { querySpec };
                    IEnumerable<LabEnvironment> matchingEnvironments = labService
                           .QueryLabEnvironments(querySpec)
                           .Where(env => matchingCriteria.MatchEnvironment(env))
                           .OrderBy(environment => environment, new TaggedEnvironmentComparer(environmentPoolTagName, ClaimOrder.LastOperationAscending));
                    //IEnumerable<LabEnvironment> matchingEnvironments = labService
                    //       .QueryLabEnvironments(querySpec);

                    totalEnvsCount = matchingEnvironments.Count();

                    var filteredEnv = matchingEnvironments.Where(env => env.InUseMarker == null);
                    var InUseEnvironments = matchingEnvironments.Where(env => env.InUseMarker != null);
                    inUseEnvsCount = InUseEnvironments.Count();

                    var validEnvs = filteredEnv.Where((envir) =>
                    {
                        string value = string.Empty;
                        envir.CustomProperties.TryGetValue(environmentPoolTagName, out value);

                        if (string.IsNullOrWhiteSpace(value))
                            return false;

                        DateTime expiry = DateTime.Parse(value);
                        if (expiry > DateTime.Now.Subtract(TimeSpan.FromDays(2)))
                            return true;
                        else
                        {
                            //value.Dump(envir.Name);
                            return false;
                        }
                    });
                    return matchingEnvironments.ToList();
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public static List<LabEnvironment> GetAllLabEnvs()
        {
            try
            {

                using (var connection = TeamProjectCollectionConnection.FromFullUri(new Uri(TFS_SERVER_URL)))
                {
                    //LabService labService = connection.TeamProjectCollection.GetService<LabService>();
                    LabService labService = connection.TeamProjectCollection.GetService<LabService>();

                    var querySpec = new LabEnvironmentQuerySpec { Disposition = LabEnvironmentDisposition.Active, Project = "AdsApps", LabProvider = "SCVMM2012" };
                    LabEnvironmentQuerySpec[] querySpecs = new LabEnvironmentQuerySpec[] { querySpec };
                    IEnumerable<LabEnvironment> matchingEnvironments = labService.QueryLabEnvironments(querySpec);
                    return matchingEnvironments.ToList();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public static List<LabEnvironment> GetFreeLabEnv(List<LabEnvironment> labs)
        {
            string environmentPoolTagName = "LkgBuild_AdsApps_Main";
            var filteredEnv = labs.Where(env => env.InUseMarker == null);
            var validEnvs = filteredEnv.Where((envir) =>
            {
                string value = string.Empty;
                envir.CustomProperties.TryGetValue(environmentPoolTagName, out value);

                if (string.IsNullOrWhiteSpace(value))
                    return false;

                DateTime expiry = DateTime.Parse(value);
                if (expiry > DateTime.Now.Subtract(TimeSpan.FromDays(2)))
                    return true;
                else
                {
                    //value.Dump(envir.Name);
                    return false;
                }
            });
            return validEnvs.ToList();
        }

        public static IDictionary<string, List<TestRunSummary>> GetTestRuns(int planId)
        {
            int lastXHours;
            if (!int.TryParse(ConfigurationManager.AppSettings["lastXHours"], out lastXHours))
                lastXHours = 24;
            var createdAfter = DateTime.Now.AddHours(lastXHours * -1);
            try
            {

                using (var tfs = new TfsTeamProjectCollection(new Uri(TFS_SERVER_URL)))
                {
                    var tms = (ITestManagementService)tfs.GetService(typeof(ITestManagementService));

                    var tfsQuery =
                        string.Format(
                            "select * from TestRun where TestPlanId = {0} and CreationDate >= '{1}'",
                           planId,
                           createdAfter.ToString("MM/dd/yyyy"));

                    // Get the matching TFS test run data
                    var result = tms.QueryTestRuns(tfsQuery);


                    Dictionary<string, List<TestRunSummary>> results = new Dictionary<string, List<TestRunSummary>>();
                    foreach (var item in result)
                    {
                        if (!results.ContainsKey(item.BuildNumber))
                        {
                            results.Add(item.BuildNumber, new List<TestRunSummary>());
                        }
                        results[item.BuildNumber].Add(new TestRunSummary
                        {
                            FailedTests = item.Statistics.FailedTests,
                            PassedTests = item.Statistics.PassedTests,
                            TotalTests = item.Statistics.TotalTests,
                            DateCompleted = item.DateCompleted,
                            DateStarted = item.DateStarted,
                            EnvironmentId = item.TestEnvironmentId
                        }
                                                    );
                    }

                    return results;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IDictionary<string, object> GetBuildStatistics(IBuildDetail build)
        {
            int totalPassed = 0;
            int totalFailed = 0;
            int totalTests = 0;

            TimeSpan sumTime = TimeSpan.Zero;
            Dictionary<string, object> stats = new Dictionary<string, object>();
            IEnumerable<ITestRun> testRuns = null;
            if (build.Status == BuildStatus.PartiallySucceeded || build.Status == BuildStatus.Succeeded)
            {
                using (TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(TFS_SERVER_URL)))
                {
                    ITestManagementService tms = tfs.GetService<ITestManagementService>();
                    if (tms.GetTeamProject("AdsApps").TestRuns != null)
                    {
                        testRuns = tms.GetTeamProject("AdsApps").TestRuns.ByBuild(build.Uri);

                        if (testRuns != null)
                        {
                            foreach (var tr in testRuns)
                            {
                                totalPassed += tr.Statistics.PassedTests;
                                totalFailed += tr.Statistics.FailedTests;
                                totalTests += tr.Statistics.TotalTests;
                                sumTime += GetRunDuration(tr);
                            }
                        }
                    }
                }
            }
            stats.Add("totalPassed", totalPassed);
            stats.Add("totalFailed", totalFailed);
            stats.Add("totalTests", totalTests);
            stats.Add("duration", sumTime);

            if (totalTests != 0)
            {
                stats.Add("passPerc", ((double)totalPassed / (double)totalTests) * 100);
                stats.Add("failPerc", (totalFailed / totalTests) * 100);
            }
            else
            {
                stats.Add("passPerc", 0);
                stats.Add("failPerc", 0);
            }
            stats.Add("html", getBuildDetailHtml(testRuns, build, stats));
            return stats;
        }

        static string GetEnvName(TestRunSummary tr)
        {
            using (TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(TFS_SERVER_URL)))
            {
                ITestManagementService tms = (ITestManagementService)tfs.GetService(typeof(ITestManagementService));
                ITestManagementTeamProject tfsProject = tms.GetTeamProject(TFS_PROJECT);
                LabService service = tfs.GetService<LabService>();
                if (service == null)
                    throw new System.ArgumentOutOfRangeException("Unable to get hold of LabService...");
                var EnvInLab = service.QueryLabEnvironments(new LabEnvironmentQuerySpec() { Project = TFS_PROJECT, LabProvider = "Unmanaged" }).ToList();
                LabEnvironment returnedEnv = null;
                if (EnvInLab != null)
                {
                    returnedEnv = EnvInLab.Where(le => le.LabGuid.ToString().Equals(tr.EnvironmentId.ToString())).FirstOrDefault();
                }
                if (returnedEnv != null)
                    return returnedEnv.Name;
                return null;
            }
        }
        private string getBuildDetailHtml(IEnumerable<ITestRun> testRuns, IBuildDetail b, Dictionary<string, object> stats)
        {
            StringBuilder sbDays = new StringBuilder();
            sbDays.Append("");
            if (BuildUtility.isRme(b.BuildNumber))
                sbDays.Append(GetBuildHtmlForRME(b, stats));
            else
                sbDays.Append(GetBuildHtml(b, stats));
            if (testRuns != null)
            {
                sbDays.Append(getTestRunHtml(testRuns, b));
            }
            return sbDays.ToString();
        }

        private string GetBuildHtml(IBuildDetail b, Dictionary<string, object> buildStats)
        {
            StringBuilder sbDays = new StringBuilder();
            sbDays.Clear();

            //head
            sbDays.Append(getBuildHtmlHeader());
            //BODY
            string rowClass = "targetFailure";
            if (b.Status == BuildStatus.Succeeded || Convert.ToDouble(buildStats["passPerc"]).ToString("0.00").Equals("100.00"))
                rowClass = "targetSuccess";
            else if (b.Status == BuildStatus.PartiallySucceeded)
                rowClass = "targetPartiallySucceededTestFailed";
            else if (b.Status == BuildStatus.InProgress || b.Status == BuildStatus.NotStarted)
                rowClass = "targetNotFinished";
            else
                rowClass = "targetFailure";

            sbDays.Append(string.Format("<tr style='{0}' class='{1}'>", "background-color:#ddd", rowClass));
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 50));
            //\\asgdrops\AdsApps\AdsApps_vnext\Rolling\17.02.1.20141006.3347047\retail\amd64\App\Test
            //sbDays.Append(string.Format("<a href='{0}'>{1}</a>", "mtm://adsgroupvstf:8080/tfs/adsgroup/p:AdsApps/testing/testrun/open?id=" + tr.Id, b.BuildNumber.Substring(b.BuildNumber.IndexOf("_") + 1)));
            sbDays.Append(string.Format("<a href='{0}'>{1}</a>", string.Format("http://adsgroupvstf:8080/tfs/AdsGroup/AdsApps/_build#buildUri={0}&_a=summary", b.Uri), b.BuildNumber.Substring(b.BuildNumber.IndexOf("_") + 1)));
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 100));
            sbDays.Append(b.Status);
            sbDays.Append("</td>");
            ///COMMENTED FOR PERFORMANCE ANALYSIS
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 100));
            if (b.Status == BuildStatus.Succeeded || b.Status == BuildStatus.PartiallySucceeded)
                sbDays.Append(string.Format("{0}%", Convert.ToDouble(buildStats["passPerc"]).ToString("0.00")));
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}'>", "center"));
            if (b.Status == BuildStatus.Succeeded || b.Status == BuildStatus.PartiallySucceeded)
                sbDays.Append(string.Format("{0}/{1}", ((int)(buildStats["totalFailed"])).ToString(), ((int)(buildStats["totalTests"])).ToString()));
            sbDays.Append("</td>");

            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 100));
            //display build duration only if finish time is > than start time
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
            //sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 200));
            //if (b.Status == BuildStatus.Succeeded || b.Status == BuildStatus.PartiallySucceeded)
            //{
            //    if (!string.IsNullOrEmpty(buildEnvName))
            //        sbDays.Append(buildEnvName);
            //}
            //sbDays.Append("</td>");
            //sbDays.Append(string.Format("<td align='{0}'>", "right"));
            //sbDays.Append(((int)(buildStats["totalTests"])).ToString());
            //sbDays.Append("</td>");

            ///COMMENTED FOR PERFORMANCE ANALYSIS
            //sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center",100));
            //TimeSpan duration =(TimeSpan)buildStats["duration"];
            //if (b.Status == BuildStatus.Succeeded || b.Status == BuildStatus.PartiallySucceeded)
            //sbDays.Append(string.Format("{0}:{1}:{2}", duration.Hours.ToString(), duration.Minutes.ToString(), duration.Seconds.ToString()));
            //sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 200));
            sbDays.Append(b.StartTime);
            sbDays.Append("</td>");

            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 200));
            if (b.Status != BuildStatus.InProgress && b.Status != BuildStatus.None && b.Status != BuildStatus.NotStarted)
                sbDays.Append(b.FinishTime);
            sbDays.Append("</td>");
            //sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 100));
            //for production
            //sbDays.Append(string.Format("<a href='{0}'>{1}</a>", "http://adsappstesttools/LabmanBuilds/Home.aspx?name="+b.LabelName, "View Details"));
            //for dev?
            //sbDays.Append(string.Format("<a href='{0}'>{1}</a>", "BuildDetail.aspx?name=" + b.BuildNumber, "View Details"));
            //sbDays.Append("</td>");

            //sbDays.Append(string.Format("<td align='{0}'>","center"));
            //sbDays.Append(string.Format("<a href='{0}'>'{1}'</a>","/queue/adsapps_ap","Queuee"));
            //sbDays.Append("</td>");
            //sbDays.Append(string.Format("<td align='{0}'>","right"));
            //sbDays.Append("1:00:00");
            //sbDays.Append("</td>");
            sbDays.Append("</tr></tbody></table></div>");

            //<a href="/queue/adsapps_ap">adsapps_ap</a>
            return sbDays.ToString();
        }
        public static bool isRme(string buildNumber)
        {
            return buildNumber.Contains("[RME]");
        }
        private string GetBuildHtmlForRME(IBuildDetail b, Dictionary<string, object> buildStats)
        {

            StringBuilder sbDays = new StringBuilder();
            sbDays.Clear();
            //////COMMENTED FOR PERFORMANCE ANALYSIS
            //IDictionary<string, object> buildStats = bu.GetBuildStatistics(b);

            //head
            sbDays.Append(getBuildHtmlHeader());
            //BODY
            string rowClass = "targetFailure";
            if (b.Status == BuildStatus.Succeeded || b.Status == BuildStatus.PartiallySucceeded)
                rowClass = "targetSuccess";
            else if (b.Status == BuildStatus.InProgress || b.Status == BuildStatus.NotStarted)
                rowClass = "targetNotFinished";
            else
                rowClass = "targetFailure";

            sbDays.Append(string.Format("<tr style='{0}' class='{1}'>", "background-color:#ddd", rowClass));
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 50));
            //\\asgdrops\AdsApps\AdsApps_vnext\Rolling\17.02.1.20141006.3347047\retail\amd64\App\Test
            //sbDays.Append(string.Format("<a href='{0}'>{1}</a>", "mtm://adsgroupvstf:8080/tfs/adsgroup/p:AdsApps/testing/testrun/open?id=" + tr.Id, b.BuildNumber.Substring(b.BuildNumber.IndexOf("_") + 1)));
            sbDays.Append(string.Format("<a href='{0}'>{1}</a>", string.Format("http://adsgroupvstf:8080/tfs/AdsGroup/AdsApps/_build#buildUri={0}&_a=summary", b.Uri), b.BuildNumber.Substring(b.BuildNumber.IndexOf("_") + 1)));
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 100));
            sbDays.Append(b.Status);
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 100));
            //if (b.Status == BuildStatus.Succeeded || b.Status == BuildStatus.PartiallySucceeded)
            //    sbDays.Append(string.Format("{0}%", Convert.ToDouble(buildStats["passPerc"]).ToString("0.00")));
            sbDays.Append("</td>");
            ///COMMENTED FOR PERFORMANCE ANALYSIS
            sbDays.Append(string.Format("<td align='{0}'>", "right"));
            //if (b.Status == BuildStatus.Succeeded || b.Status == BuildStatus.PartiallySucceeded)
            //    sbDays.Append(string.Format("{0}/{1}", ((int)(buildStats["totalFailed"])).ToString(), ((int)(buildStats["totalTests"])).ToString()));
            sbDays.Append("</td>");



            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 100));
            //display build duration only if finish time is > than start time
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
            sbDays.Append(b.RequestedBy);
            sbDays.Append("</td>");
            //sbDays.Append(string.Format("<td align='{0}'>", "right"));
            //sbDays.Append(((int)(buildStats["totalTests"])).ToString());
            //sbDays.Append("</td>");

            ///COMMENTED FOR PERFORMANCE ANALYSIS
            //sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 100));
            //TimeSpan duration = (TimeSpan)buildStats["duration"];
            //if (b.Status == BuildStatus.Succeeded || b.Status == BuildStatus.PartiallySucceeded)
            //    sbDays.Append(string.Format("{0}:{1}:{2}", duration.Hours.ToString(), duration.Minutes.ToString(), duration.Seconds.ToString()));
            //sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 200));
            sbDays.Append(b.StartTime);
            sbDays.Append("</td>");

            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 200));
            if (b.Status != BuildStatus.InProgress && b.Status != BuildStatus.None && b.Status != BuildStatus.NotStarted)
                sbDays.Append(b.FinishTime);
            sbDays.Append("</td>");
            //sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 100));
            //for production
            //sbDays.Append(string.Format("<a href='{0}'>{1}</a>", "http://adsappstesttools/LabmanBuilds/Home.aspx?name="+b.LabelName, "View Details"));
            //for dev?
            //sbDays.Append(string.Format("<a href='{0}'>{1}</a>", "BuildDetail.aspx?name=" + b.BuildNumber, "View Details"));
            //sbDays.Append("</td>");

            //sbDays.Append(string.Format("<td align='{0}'>","center"));
            //sbDays.Append(string.Format("<a href='{0}'>'{1}'</a>","/queue/adsapps_ap","Queuee"));
            //sbDays.Append("</td>");
            //sbDays.Append(string.Format("<td align='{0}'>","right"));
            //sbDays.Append("1:00:00");
            //sbDays.Append("</td>");
            sbDays.Append("</tr></tbody></table></div>");

            //<a href="/queue/adsapps_ap">adsapps_ap</a>
            return sbDays.ToString();
        }

        static string getBuildHtmlHeader()
        {
            StringBuilder sbDays = new StringBuilder();
            //sbDays.Clear();
            sbDays.Append(string.Format("<div id='{0}' class='{1}'>", "mybuilds", "table_font_size"));
            sbDays.Append(string.Format("<table cellpadding='{0}'  style='{1}'>", 3, "FONT-SIZE: 13px"));
            sbDays.Append("<tbody>");
            sbDays.Append("<tr>");
            sbDays.Append(string.Format("<td class='{0}'>", "tableHeader"));
            sbDays.Append("Number");
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 100));
            sbDays.Append("Status");
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 100));
            sbDays.Append("Pass %");
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 100));
            sbDays.Append("Failed/Total");
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 100));
            sbDays.Append("Build Duration");
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 200));
            sbDays.Append("Owner");
            sbDays.Append("</td>");
            //sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 200));
            //sbDays.Append("Environment Name");
            //sbDays.Append("</td>");

            //sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 200));
            //sbDays.Append("Reason");
            //sbDays.Append("</td>");


            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 200));
            sbDays.Append("Start Time");
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 200));
            sbDays.Append("End Time");
            sbDays.Append("</td>");

            //sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 100));
            //sbDays.Append("Test Duration");
            //sbDays.Append("</td>");
            //////////////////////
            sbDays.Append("</tr>");

            return sbDays.ToString();
        }
        private string getTestRunHtml(IEnumerable<ITestRun> testRuns, IBuildDetail build)
        {
            bool hasBackground = true;
            StringBuilder sbDays = new StringBuilder();
            //IEnumerable<ITestRun> testRuns = null;
            //if (build.Status == BuildStatus.PartiallySucceeded || build.Status == BuildStatus.Succeeded || build.Status == BuildStatus.Failed || build.Status == BuildStatus.InProgress)
            //{
            using (TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(TFS_SERVER_URL)))
            {
                ITestManagementService tms = tfs.GetService<ITestManagementService>();
                //testRuns = tms.GetTeamProject("AdsApps").TestRuns.ByBuild(build.Uri);
                if (testRuns != null)
                {
                    if (testRuns.ToList().Count > 0)
                    {
                        sbDays.Append(string.Format("<div id='{0}'>", "mybuilds"));
                        sbDays.Append(string.Format("<table cellpadding='{0}'  style='{1}'>", 3, "FONT-SIZE: 13px"));
                        sbDays.Append("<tbody>");
                        sbDays.Append("<tr>");
                        sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 500));
                        sbDays.Append("Run Title");
                        sbDays.Append("</td>");
                        sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 50));
                        sbDays.Append("Run Id");
                        sbDays.Append("</td>");
                        sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 50));
                        sbDays.Append("Failed");
                        sbDays.Append("</td>");
                        sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 50));
                        sbDays.Append("Passed");
                        sbDays.Append("</td>");
                        sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 50));
                        sbDays.Append("Total");
                        sbDays.Append("</td>");
                        sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 100));
                        sbDays.Append("Pass %");
                        sbDays.Append("</td>");
                        sbDays.Append(string.Format("<td class='{0}'>", "tableHeader"));
                        sbDays.Append("Duration");
                        sbDays.Append("</td>");
                        sbDays.Append("</tr>");
                    }
                    foreach (var tr in testRuns)
                    {
                        string rowClass;
                        if ((((double)tr.Statistics.PassedTests / (double)tr.Statistics.TotalTests) * 100).ToString("0.0").Equals("100.0"))
                            rowClass = "targetSuccess";
                        else
                            rowClass = "targetFailure";
                        // we alternate the rows background color
                        if (hasBackground)
                            sbDays.Append(string.Format("<tr style='{0}' class='{1}'>", "background-color:#ddd", rowClass));
                        else
                            sbDays.Append(string.Format("<tr class='{0}'>", rowClass));
                        hasBackground = !hasBackground;
                        sbDays.Append(string.Format("<td align='{0}' width='{1}' align='{0}'>", "left", 500, "left"));
                        sbDays.Append(tr.Title);
                        sbDays.Append("</td>");
                        sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 50));
                        sbDays.Append(string.Format("<a href='{0}'>{1}</a>", "mtm://adsgroupvstf:8080/tfs/adsgroup/p:AdsApps/testing/testrun/open?id=" + tr.Id, tr.Id));
                        sbDays.Append("</td>");
                        sbDays.Append(string.Format("<td align='{0}'>", "right"));
                        sbDays.Append(tr.Statistics.FailedTests.ToString());
                        sbDays.Append("</td>");
                        sbDays.Append(string.Format("<td align='{0}'>", "right"));
                        sbDays.Append(tr.Statistics.PassedTests.ToString());
                        sbDays.Append("</td>");
                        sbDays.Append(string.Format("<td align='{0}'>", "right"));
                        sbDays.Append(tr.Statistics.TotalTests.ToString());
                        sbDays.Append("</td>");
                        sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 100));
                        sbDays.Append(string.Format("{0} %", (((double)tr.Statistics.PassedTests / (double)tr.Statistics.TotalTests) * 100).ToString("0.00")));
                        sbDays.Append("</td>");
                        sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 50));
                        TimeSpan duration = GetRunDuration(tr);
                        sbDays.Append(string.Format("{0}:{1}:{2}", duration.Hours.ToString(), duration.Minutes.ToString(), duration.Seconds.ToString()));
                        sbDays.Append("</td>");
                        sbDays.Append("</tr>");
                    }
                    sbDays.Append("</tbody></table></div>");
                }
            }
            //}
            return sbDays.ToString();
        }

        public static string GetBuildName(string bn)
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


        public static bool isAdsAppsBuild(IBuildDetail bd)
        {
            return !(bd.RequestedFor.Equals(bd.RequestedBy) && bd.RequestedFor.Equals("adslabtfsmgmt"));
        }

        public static List<IBuildDetail> GetBuildByName(Dictionary<string, List<IBuildDetail>> builds, string buildName)
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

        public static IDictionary<string, string> getEnvironmentPoolFriendlyNames()
        {
            IDictionary<string, string> friendlyNamesDict = new Dictionary<string, string>();

            friendlyNamesDict.Add("AppsEnv2K8VMs", "BVT Refresh");
            friendlyNamesDict.Add("AdsAppsBuddyBVT-Pooled", "Buddy BVT Refresh");
            friendlyNamesDict.Add("AdsAppEnv10VMs", "FFTP Refresh");
            friendlyNamesDict.Add("AdsAppsSingleBox", "Single box Refresh");
            return friendlyNamesDict;
        }

        public static IList<TagDetail> getPoolNames()
        {
            string[] templates = MainSettings.Default.templates.Split(',');
            var list = templates.ToList();

            var templateList = new List<TagDetail>();
            foreach (var t in list)
            {
                templateList.Add(new TagDetail() { DisplayName = t });
            }

            return templateList;
        }

        public static string GetTagName(string bn)
        {
            if (bn.Equals("AppsEnv2K8VMs"))
                return "BVT Refresh &nbsp;&nbsp;&nbsp; AppsEnv2K8VMs";
            else if (bn.Equals("AdsAppsBuddyBVT-Pooled"))
                return "Regression test runs with MT refresh deployment &nbsp;&nbsp;&nbsp; Buddy BVT Refresh";
            else if (bn.Equals("AdsAppEnv10VMs"))
                return "FFTP Refresh &nbsp;&nbsp;&nbsp; AdsAppEnv10VMs";
            else if (bn.Equals("AdsAppsSingleBox"))
                return "Single box Refresh &nbsp;&nbsp;&nbsp; AdsAppsSingleBox";
            else
                return "UnKnown";
        }

        public static void sendEmail(string body, string from, List<string> to, string subject, string server)
        {
            using (MailMessage msg = BuildUtility. GenerateMessage(body, from, to, subject))
            {
                try
                {
                    if (SendMail(msg, server))
                    {
                        
                    }
                    else
                    {
                        
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public static MailMessage GenerateMessage(string body, string from, List<string> to, string subject)
        {

            MailMessage msg = new MailMessage();
            //string address = "v-komefi@microsoft.com";
            try
            {
                msg.From = new MailAddress(from);

                if ( to != null )
                {
                    foreach ( var t in to )
                    msg.To.Add(t);
                }

                msg.Subject = subject;

                //msg.CC.Add(run.NotifyCc);
                //msg.Subject = string.Format("Test Run Complete: {0}", run.tfsTitle);
            }
            catch (Exception ex)
            {
                throw;
            }

            try
            {

                string content = body;
                msg.Body = content;
                msg.IsBodyHtml = true;
            }
            catch (Exception ex)
            {
                throw;
            }

            return msg;
        }

        public static bool SendMail(MailMessage msg, string server)
        {
            bool mailSent = false;

            if (msg == null)
                return mailSent;

            SmtpClient smtp = null;
            try
            {
                smtp = new SmtpClient(server);
                string bccList = null;
                if (!string.IsNullOrEmpty(bccList))
                {
                    try
                    {
                        msg.Bcc.Add(bccList);
                    }
                    catch
                    { }
                }

                smtp.UseDefaultCredentials = true;
                smtp.Send(msg);
                mailSent = true;
            }
            catch (SmtpFailedRecipientsException ex)
            {
                mailSent = true;
                throw;
            }
            catch (SmtpFailedRecipientException ex)
            {
                mailSent = true;
                throw;
            }
            catch (Exception ex)
            {
                mailSent = false;
                throw;
            }
            finally
            {
                if (smtp != null)
                    smtp.Dispose();
            }

            return mailSent;
        }

        public static bool isFFTP(string def)
        {
            return def.Contains("FFTP");
        }
    }
}
