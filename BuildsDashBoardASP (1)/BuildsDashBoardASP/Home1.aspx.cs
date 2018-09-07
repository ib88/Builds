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
using System.Threading.Tasks;
using System.Net.Http;

namespace BuildsDashBoardASP
{
    public partial class Home1 : System.Web.UI.Page
    {
        const string BUDDYBUILDBVT = "[LabMan2.0][BVT][AdsApps][MT-RefreshOnly][Apps]";
        const string LABMANCIBVT = "[LabMan2.0][BVT][AdsApps][MT][Apps]";
        const string LABMANFFTP = "X64[LabMan][FFTP][AdsApps][MT][Apps]Advertiser";
        const string TFS_SERVER_URL = "http://adsgroupvstf:8080/tfs/adsgroup";
        BuildUtilityAsync bu = new BuildUtilityAsync();
        protected async void Page_Load(object sender, EventArgs e)
        {
            Task<string> GetHtmlTask = GetHtml();
            string html = await GetHtmlTask;
            Response.Write(GetHtml());
        }

        private async Task<string> GetHtml()
        {
            StringBuilder bodyhtml = new StringBuilder();
            bodyhtml.Append("");
            Task<List<IBuildDetail>> buddybuildsTask = bu.GetBuilds(BUDDYBUILDBVT,24);
            Task<List<IBuildDetail>> labmancibuildsTask = bu.GetBuilds(LABMANCIBVT,24);
            List<IBuildDetail>  allbuilds = new List<IBuildDetail>();

            List<IBuildDetail> buddybuilds = await buddybuildsTask;
            List<IBuildDetail> labmancibuilds = await labmancibuildsTask;

            allbuilds.AddRange( buddybuilds );
            allbuilds.AddRange( labmancibuilds );

            //get test runs
            Task<IDictionary<string , List<ITestRun>>> buddybuildsRunsTask = bu.GetRuns(buddybuilds );
            Task<IDictionary<string , List<ITestRun>>> labmanciRunsTask = bu.GetRuns(labmancibuilds );

            IDictionary<string , List<ITestRun>> buddybuildsRuns = await buddybuildsRunsTask;
            IDictionary<string , List<ITestRun>> labmanciRuns = await labmanciRunsTask;

            foreach ( IBuildDetail build in allbuilds)
            {
                bodyhtml.Append( GetBuildHtml( build ) );
                bodyhtml.Append( GetRunHtml( build ) );
            }
            return bodyhtml.ToString();
            //bodyhtml.Append( GetBuildHtml( buddybuilds );
            //bodyhtml.Append( GetRunsHtml( buddybuildsRuns );

            //bodyhtml.Append( GetbuildHtml( labmancibuilds );
            //bodyhtml.Append( GetRunsHtml( labmanciRuns ) );
        }

        private string GetBuildHtml(IBuildDetail build)
        {
            StringBuilder sbDays = new StringBuilder();
            sbDays.Append("");
            if (build != null)
            {
                sbDays.Append(string.Format("<table cellpadding='{0}' cellspacing='{1}' width='{2}'>", 2, 0, "100%"));
                //build headers
                sbDays.Append("<thead>");
                sbDays.Append(string.Format("<tr style='{0}'>", "font-weight: bolder"));
                sbDays.Append(string.Format("<th align='{0}' class='{1}' width='{2}'>", "left", "cell headers", "200px"));
                sbDays.Append("Build");
                sbDays.Append("</th>");
                sbDays.Append(string.Format("<th align='{0}' class='{1}' width='{2}'>", "left", "cell headers", "200px"));
                sbDays.Append("State");
                sbDays.Append(string.Format("<th align='{0}' class='{1}' width='{2}'>", "left", "cell headers", "200px"));
                sbDays.Append("TestsFailed/Total Test passed  ");
                sbDays.Append("</th>");
                sbDays.Append(string.Format("<th align='{0}' class='{1}' width='{2}'>", "left", "cell headers", "200px"));
                sbDays.Append("Pass%age ");
                sbDays.Append("</th>");
                sbDays.Append(string.Format("<th align='{0}' class='{1}' width='{2}'>", "left", "cell headers", "200px"));
                sbDays.Append("BuildDuration ");
                sbDays.Append("</th>");
                sbDays.Append("</tr>");
                sbDays.Append("</head>");

                //build body
                sbDays.Append("<tbody>");
                IDictionary<string, double> buildStats = bu.GetBuildStatistics(b);
                sbDays.Append("<tr>");
                sbDays.Append(string.Format("<td align='{0}' class='{1}' width='{2}'>", "left", "cell", "200px"));
                sbDays.Append(b.BuildNumber);
                sbDays.Append("</td>");
                sbDays.Append(string.Format("<td width='{0}'>", "16px"));
                sbDays.Append(b.Status);
                sbDays.Append("</td>");
                sbDays.Append(string.Format("<td width='{0}'>", "16px"));
                sbDays.Append(string.Format("{0}/{1}", buildStats["totalFailed"].ToString(), buildStats["totalPassed"].ToString()));
                sbDays.Append("</td>");
                sbDays.Append(string.Format("<td width='{0}'>", "16px"));
                sbDays.Append(string.Format("{0}", buildStats["passPerc"].ToString()));
                sbDays.Append("</td>");
                sbDays.Append(string.Format("<td width='{0}'>", "16px"));
                if (build.Status != BuildStatus.InProgress || build.Status != BuildStatus.None || build.Status != BuildStatus.NotStarted)
                    sbDays.Append(build.FinishTime.Subtract(build.StartTime).ToString());
                sbDays.Append("</td>");
                sbDays.Append("</tr>");
                sbDays.Append("</table>");
            }
            return sbDays.ToString();
        }
        private string GetRunHtml(IBuildDetail build)
        {
            StringBuilder sbDays = new StringBuilder();
            sbDays.Append("");
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
                            sbDays.Append(string.Format("<table cellpadding='{0}' cellspacing='{1}' width='{2}'>", 2, 0, "50%"));
                            sbDays.Append("<thead>");
                            sbDays.Append(string.Format("<tr style='{0}'>", "font-weight: bolder"));
                            sbDays.Append(string.Format("<th align='{0}' class='{1}' width='{2}'>", "left", "cell headers", "100px"));
                            sbDays.Append("Run Title ");
                            sbDays.Append("</th>");
                            sbDays.Append(string.Format("<th align='{0}' class='{1}' width='{2}'>", "left", "cell headers", "100px"));
                            sbDays.Append("Rund ID");
                            sbDays.Append(string.Format("<th align='{0}' class='{1}' width='{2}'>", "left", "cell headers", "100px"));
                            sbDays.Append("Failed");
                            sbDays.Append("</th>");
                            sbDays.Append(string.Format("<th align='{0}' class='{1}' width='{2}'>", "left", "cell headers", "100px"));
                            sbDays.Append("Total");
                            sbDays.Append(string.Format("<th align='{0}' class='{1}' width='{2}'>", "left", "cell headers", "100px"));
                            sbDays.Append("Pass%age ");
                            sbDays.Append("</th>");
                            sbDays.Append(string.Format("<th align='{0}' class='{1}' width='{2}'>", "left", "cell headers", "100px"));
                            sbDays.Append("Duration ");
                            sbDays.Append("</th>");
                            sbDays.Append("</tr>");
                            sbDays.Append("</head>");

                            sbDays.Append("<tbody>");
                            foreach (var tr in testRuns)
                            {
                                sbDays.Append("<tr>");
                                sbDays.Append(string.Format("<td align='{0}' class='{1}' width='{2}'>", "left", "cell", "600px"));
                                sbDays.Append(tr.Title);
                                sbDays.Append("</td>");
                                sbDays.Append(string.Format("<td width='{0}'>", "16px"));
                                sbDays.Append(tr.Id);
                                sbDays.Append("</td>");
                                sbDays.Append(string.Format("<td width='{0}'>", "16px"));
                                sbDays.Append(tr.Statistics.FailedTests.ToString());
                                sbDays.Append("</td>");
                                sbDays.Append(string.Format("<td width='{0}'>", "16px"));
                                sbDays.Append(tr.TotalTests.ToString());
                                sbDays.Append("</td>");
                                sbDays.Append(string.Format("<td width='{0}'>", "16px"));
                                sbDays.Append(((tr.Statistics.PassedTests / tr.TotalTests) * 100).ToString("0.0"));
                                sbDays.Append("</td>");
                                sbDays.Append(string.Format("<td width='{0}'>", "16px"));
                                TimeSpan duration = bu.GetRunDuration(tr);
                                sbDays.Append(string.Format("{0}:{1}:{2}", duration.Hours.ToString(), duration.Minutes.ToString(), duration.Seconds.ToString()));
                                sbDays.Append("</td>");
                                sbDays.Append(string.Format("<td width='{0}'>", "16px"));
                                sbDays.Append("</tr>");
                            }
                            sbDays.Append("</table>");
                        }
                    }
                }
            }
            return sbDays.ToString();
        }
        private string GetTestRunHtml(IBuildDetail build)
        {
            StringBuilder sbDays = new StringBuilder();
            sbDays.Append("");
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
                            sbDays.Append(string.Format("<table cellpadding='{0}' cellspacing='{1}' width='{2}'>", 2, 0, "50%"));
                            sbDays.Append("<thead>");
                            sbDays.Append(string.Format("<tr style='{0}'>", "font-weight: bolder"));
                            sbDays.Append(string.Format("<th align='{0}' class='{1}' width='{2}'>", "left", "cell headers", "100px"));
                            sbDays.Append("Run Title ");
                            sbDays.Append("</th>");
                            sbDays.Append(string.Format("<th align='{0}' class='{1}' width='{2}'>", "left", "cell headers", "100px"));
                            sbDays.Append("Rund ID");
                            sbDays.Append(string.Format("<th align='{0}' class='{1}' width='{2}'>", "left", "cell headers", "100px"));
                            sbDays.Append("Failed");
                            sbDays.Append("</th>");
                            sbDays.Append(string.Format("<th align='{0}' class='{1}' width='{2}'>", "left", "cell headers", "100px"));
                            sbDays.Append("Total");
                            sbDays.Append(string.Format("<th align='{0}' class='{1}' width='{2}'>", "left", "cell headers", "100px"));
                            sbDays.Append("Pass%age ");
                            sbDays.Append("</th>");
                            sbDays.Append(string.Format("<th align='{0}' class='{1}' width='{2}'>", "left", "cell headers", "100px"));
                            sbDays.Append("Duration ");
                            sbDays.Append("</th>");
                            sbDays.Append("</tr>");
                            sbDays.Append("</head>");

                            sbDays.Append("<tbody>");
                            foreach (var tr in testRuns)
                            {
                                sbDays.Append("<tr>");
                                sbDays.Append(string.Format("<td align='{0}' class='{1}' width='{2}'>", "left", "cell", "600px"));
                                sbDays.Append(tr.Title);
                                sbDays.Append("</td>");
                                sbDays.Append(string.Format("<td width='{0}'>", "16px"));
                                sbDays.Append(tr.Id);
                                sbDays.Append("</td>");
                                sbDays.Append(string.Format("<td width='{0}'>", "16px"));
                                sbDays.Append(tr.Statistics.FailedTests.ToString());
                                sbDays.Append("</td>");
                                sbDays.Append(string.Format("<td width='{0}'>", "16px"));
                                sbDays.Append(tr.TotalTests.ToString());
                                sbDays.Append("</td>");
                                sbDays.Append(string.Format("<td width='{0}'>", "16px"));
                                sbDays.Append(((tr.Statistics.PassedTests / tr.TotalTests) * 100).ToString("0.0"));
                                sbDays.Append("</td>");
                                sbDays.Append(string.Format("<td width='{0}'>", "16px"));
                                TimeSpan duration = bu.GetRunDuration(tr);
                                sbDays.Append(string.Format("{0}:{1}:{2}", duration.Hours.ToString(), duration.Minutes.ToString(), duration.Seconds.ToString()));
                                sbDays.Append("</td>");
                                sbDays.Append(string.Format("<td width='{0}'>", "16px"));
                                sbDays.Append("</tr>");
                            }
                            sbDays.Append("</table>");
                        }
                    }
                }
            }
            return sbDays.ToString();
        }
        //private string getTestRunHtml(IBuildDetail build)
        //{
        //    StringBuilder sbDays = new StringBuilder();
        //    sbDays.Append("");
        //    IEnumerable<ITestRun> testRuns = null;
        //    if (build.Status == BuildStatus.PartiallySucceeded || build.Status == BuildStatus.Succeeded)
        //    {
        //        using (TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(TFS_SERVER_URL)))
        //        {
        //            ITestManagementService tms = tfs.GetService<ITestManagementService>();
        //            if (tms.GetTeamProject("AdsApps").TestRuns != null)
        //            {
        //                testRuns = tms.GetTeamProject("AdsApps").TestRuns.ByBuild(build.Uri);

        //                if (testRuns != null)
        //                {
        //                    //sbDays.Append(string.Format("<table cellpadding='{0}' cellspacing='{1}' width='{2}'>", 2, 0, "50%"));
        //                    //sbDays.Append("<thead>");
        //                    //sbDays.Append(string.Format("<tr style='{0}'>", "font-weight: bolder"));
        //                    //sbDays.Append(string.Format("<th align='{0}' class='{1}' width='{2}'>", "left", "cell headers", "100px"));
        //                    //sbDays.Append("Run Title ");
        //                    //sbDays.Append("</th>");
        //                    //sbDays.Append(string.Format("<th align='{0}' class='{1}' width='{2}'>", "left", "cell headers", "100px"));
        //                    //sbDays.Append("Rund ID");
        //                    //sbDays.Append(string.Format("<th align='{0}' class='{1}' width='{2}'>", "left", "cell headers", "100px"));
        //                    //sbDays.Append("Failed");
        //                    //sbDays.Append("</th>");
        //                    //sbDays.Append(string.Format("<th align='{0}' class='{1}' width='{2}'>", "left", "cell headers", "100px"));
        //                    //sbDays.Append("Total");
        //                    //sbDays.Append(string.Format("<th align='{0}' class='{1}' width='{2}'>", "left", "cell headers", "100px"));
        //                    //sbDays.Append("Pass%age ");
        //                    //sbDays.Append("</th>");
        //                    //sbDays.Append(string.Format("<th align='{0}' class='{1}' width='{2}'>", "left", "cell headers", "100px"));
        //                    //sbDays.Append("Duration ");
        //                    //sbDays.Append("</th>");
        //                    //sbDays.Append("</tr>");
        //                    //sbDays.Append("</head>");

        //                    //sbDays.Append("<tbody>");
        //                    sbDays.Append(string.Format("<tr class='{0}'>","child-right"));
        //                    sbDays.Append(string.Format("<td width='{0}'>", "16px"));
        //                    sbDays.Append("Run Title ");
        //                    sbDays.Append("</td>");
        //                    sbDays.Append(string.Format("<td width='{0}'>", "16px"));
        //                    sbDays.Append("Rund ID");
        //                    sbDays.Append("</td>");
        //                    sbDays.Append(string.Format("<td width='{0}'>", "16px"));
        //                    sbDays.Append("Failed");
        //                    sbDays.Append("</td>");
        //                    sbDays.Append(string.Format("<td width='{0}'>", "16px"));
        //                    sbDays.Append("Total");
        //                    sbDays.Append("</td>");
        //                    sbDays.Append(string.Format("<td width='{0}'>", "16px"));
        //                    sbDays.Append("Pass%age ");
        //                    sbDays.Append("</td>");
        //                    sbDays.Append(string.Format("<td width='{0}'>", "16px"));
        //                    sbDays.Append("Duration ");
        //                    sbDays.Append("</td>");
        //                    sbDays.Append("</tr>");
        //                    foreach (var tr in testRuns)
        //                    {
        //                        sbDays.Append(string.Format("<tr class='{0}'>", "child-right"));
        //                        sbDays.Append(string.Format("<td align='{0}' class='{1}' width='{2}'>", "left", "cell", "100px"));
        //                        sbDays.Append(tr.Title);
        //                        sbDays.Append("</td>");
        //                        sbDays.Append(string.Format("<td width='{0}'>", "16px"));
        //                        sbDays.Append(tr.Id);
        //                        sbDays.Append("</td>");
        //                        sbDays.Append(string.Format("<td width='{0}'>", "16px"));
        //                        sbDays.Append(tr.Statistics.FailedTests.ToString());
        //                        sbDays.Append("</td>");
        //                        sbDays.Append(string.Format("<td width='{0}'>", "16px"));
        //                        sbDays.Append(tr.TotalTests.ToString());
        //                        sbDays.Append("</td>");
        //                        sbDays.Append(string.Format("<td width='{0}'>", "16px"));
        //                        sbDays.Append(((tr.Statistics.PassedTests / tr.TotalTests) * 100).ToString("0.0"));
        //                        sbDays.Append("</td>");
        //                        sbDays.Append(string.Format("<td width='{0}'>", "16px"));
        //                        sbDays.Append(bu.GetRunDuration(tr).ToString(@"hh\:mm\:ss"));
        //                        sbDays.Append("</td>");
        //                        sbDays.Append(string.Format("<td width='{0}'>", "16px"));
        //                        sbDays.Append("</tr>");
        //                    }

        //                }
        //            }
        //        }
        //    }
        //    return sbDays.ToString();
        //}
        private string GetBuildName(string bn)
        {
            if (bn.Equals(BUDDYBUILDBVT))
                return "Buddy Build BVT";
            else if (bn.Equals(LABMANFFTP))
                return "Labman FFTP";
            else if (bn.Equals(LABMANCIBVT))
                return "LabMan CI BVT";
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