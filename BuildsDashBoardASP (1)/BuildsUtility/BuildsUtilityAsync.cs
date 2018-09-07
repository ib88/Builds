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
//using Microsoft.TeamFoundation.Framework.Common;

namespace BuildsUtility
{
    public class BuildUtilityAsync
    {
        const string TFS_SERVER_URL = "http://adsgroupvstf:8080/tfs/adsgroup";

        public async Task< List<IBuildDetail> > GetBuilds(string definition, int days, string user = null)
        {
            string tfsProject = "AdsApps";
            //string BuildDefinition = "[LabMan2.0][BVT][AdsApps][MT][Apps]";
            string tfsServerUrl = "http://adsgroupvstf:8080/tfs/adsgroup";

            Task<List<IBuildDetail>> returnedbuilds = null;
            if (definition != null)
            {
                    returnedbuilds = GetBuildByDef(tfsProject, definition, tfsServerUrl, days, user);
            }
            return returnedbuilds;
        }
        private static async Task< List<IBuildDetail> > GetBuildByDef(string tfsProject, string BuildDefinition, string tfsServerUrl, int days, string user = null)
        {
            using (TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(tfsServerUrl)))
            {
                var buildServer = (IBuildServer)tfs.GetService(typeof(IBuildServer));

                IBuildDetailSpec buildDetailSpec = buildServer.CreateBuildDetailSpec(
                    tfsProject,
                    BuildDefinition);

                buildDetailSpec.MaxBuildsPerDefinition = 1;
                buildDetailSpec.QueryOrder = BuildQueryOrder.FinishTimeDescending;
                buildDetailSpec.Status = BuildStatus.Succeeded;
                if (user != null)
                    buildDetailSpec.RequestedFor = user;

                IAsyncResult< IBuildQueryResult > results = buildServer.BeginQueryBuilds(buildDetailSpec);
                

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
        public TimeSpan GetRunDuration(ITestRun tr)
        {
            IEnumerable<ITestCaseResult> results = null;
            results = tr.QueryResults().AsEnumerable<ITestCaseResult>();
            //DateTime sum = new DateTime();
            TimeSpan sum = TimeSpan.Zero;
            foreach (ITestCaseResult tcr in results)
            {
                sum += tcr.DateCompleted.Subtract(tcr.DateStarted);
            }
            return sum;
        }

        public IDictionary<string, List<ITestRun>> GetRuns(List<IBuildDetail> builds)
        {
            IEnumerable<ITestRun> testRuns = null;
            IDictionary<string, List<ITestRun>> runs = new Dictionary<string, List<ITestRun>>();
            foreach (IBuildDetail build in builds)
            {
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
                                runs.Add(build.BuildNumber, testRuns.ToList());
                            }
                        }
                    }
                }
            }

            return runs;
        }
        public IDictionary<string, double> GetBuildStatistics(IBuildDetail build)
        {
            int totalPassed = 0;
            int totalFailed = 0;
            int totalTests = 0;
            Dictionary<string, double> stats = new Dictionary<string, double>();
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
                            }
                        }
                    }
                }
            }
            stats.Add("totalPassed", totalPassed);
            stats.Add("totalFailed", totalFailed);
            stats.Add("totalTests", totalTests);
            if (totalTests != 0)
            {
                stats.Add("passPerc", (totalPassed / totalTests) * 100);
                stats.Add("failPerc", (totalFailed / totalTests) * 100);
            }
            else
            {
                stats.Add("passPerc", 0);
                stats.Add("failPerc", 0);
            }

            return stats;
        }
    }
}
