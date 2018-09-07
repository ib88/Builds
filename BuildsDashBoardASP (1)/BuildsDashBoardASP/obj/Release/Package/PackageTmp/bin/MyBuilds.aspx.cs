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

namespace BuildsDashBoardASP
{
    public partial class MyBuilds : System.Web.UI.Page
    {
        const string BUDDYBUILDBVT = "[LabMan2.0][BVT][AdsApps][MT-RefreshOnly][Apps]";
        const string LABMANCIBVT = "[LabMan2.0][BVT][AdsApps][MT][Apps]";
        const string LABMANFFTP = "X64[LabMan][FFTP][AdsApps][MT][Apps]Advertiser";
        const string TFS_SERVER_URL = "http://adsgroupvstf:8080/tfs/adsgroup";

        string user = System.Web.HttpContext.Current.User.Identity.Name.ToLower();
        BuildUtility bu = new BuildUtility();
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Write(GetHtml());
        }

        private string GetHtml()
        {
            List<string> buildNames = new List<string>();
            //const string BUDDYBUILDBVT = "[LabMan2.0][BVT][AdsApps][MT-RefreshOnly][Apps]";
            //const string LABMANCIBVT = "[LabMan2.0][BVT][AdsApps][MT][Apps]";
            //const string LABMANFFTP = "X64[LabMan][FFTP][AdsApps][MT][Apps]Advertiser";
            //buildNames.Add(BUDDYBUILDBVT);
            buildNames.Add(LABMANCIBVT);
            // buildNames.Add(LABMANFFTP);
            //BuildUtility bu = new BuildUtility();
            Dictionary<string, List<IBuildDetail>> builds = bu.GetBuilds(buildNames, 24, user);
            StringBuilder sbDays = new StringBuilder();
            sbDays.Append(string.Format("<table id='{0}'>", "batmonWebLayout"));
            sbDays.Append("<tbody>");
            sbDays.Append("<tr>");
            sbDays.Append(string.Format("<td id='{0}'>", "headerRow"));
            sbDays.Append(string.Format("<div class='{0}'>", "header"));
            sbDays.Append(string.Format("<a href='{0}'>My Builds</a>", "/MyBuilds.aspx"));
            sbDays.Append(string.Format("  |<a href='{0}'>Team Builds</a>", "/Home.aspx"));
            sbDays.Append("</div>");
            sbDays.Append("</td>");
            sbDays.Append("</tr>");
            //body row
            sbDays.Append(string.Format("<td id='{0}'>", "bodyRow"));
            sbDays.Append(string.Format("<div id='{0}' class='{1}'>", "outageAlert", "outageAlert"));
            sbDays.Append("</div>");
            sbDays.Append(string.Format("<h2>My Recent Builds - {1}</h2>", "/MyBuilds.aspx", user));


            foreach (string bn in buildNames)
            {
                List<IBuildDetail> returnedBuilds = GetBuildByName(builds, bn);
                if (returnedBuilds != null)
                {
                    //sbDays.Append("<h2>" + GetBuildName(bn) + "</h2>");
                    foreach (IBuildDetail b in returnedBuilds)
                    {
                        //build table

                        //build headers
                        sbDays.Append(GetBuildHtml(b));
                        //build runs
                        sbDays.Append(getTestRunHtml(b));
                    }
                }
                else
                {
                    sbDays.Append("No data Found");
                }
            }
            sbDays.Append("</td>");
            sbDays.Append("</tr>");
            sbDays.Append("</tbody>");
            sbDays.Append("</table>");
            return sbDays.ToString();
        }

        private string GetBuildHtml(IBuildDetail b)
        {

            StringBuilder sbDays = new StringBuilder();
            IDictionary<string, object> buildStats = bu.GetBuildStatistics(b);

            //head
            sbDays.Append(string.Format("<div id='{0}'>", "mybuilds"));
            sbDays.Append(string.Format("<table cellpadding='{0}'>", 3));
            sbDays.Append("<tbody>");
            sbDays.Append("<tr>");
            sbDays.Append(string.Format("<td class='{0}'>", "tableHeader"));
            sbDays.Append("Build Number");
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 50));
            sbDays.Append("Status");
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 50));
            sbDays.Append("(Failed/Total)");
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 50));
            sbDays.Append("(Pass Perc)");
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 50));
            sbDays.Append("Build Duration");
            sbDays.Append("</td>");
            //sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 50));
            //sbDays.Append("Build <br>Total Tests");
            //sbDays.Append("</td>");
            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 50));
            sbDays.Append("Test Duration");
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td class='{0}'>", "tableHeader"));
            sbDays.Append("Build<br> Start Time");
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td class='{0}'>", "tableHeader"));
            sbDays.Append("Build<br> End Time");
            sbDays.Append("</td>");
            sbDays.Append("</tr>");
            //BODY
            sbDays.Append(string.Format("<tr style='{0}' class='{1}'>", "background-color:#ddd", "targetSuccess"));
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 50));
            sbDays.Append(b.BuildNumber.Substring(b.BuildNumber.IndexOf("_") + 1));
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 50));
            sbDays.Append(b.Status);
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}'>", "right"));
            sbDays.Append(string.Format("{0}/{1}", ((int)(buildStats["totalFailed"])).ToString(), ((int)(buildStats["totalPassed"])).ToString()));
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}'>", "right"));
            sbDays.Append(Convert.ToDouble(buildStats["passPerc"]).ToString("0.0"));
            sbDays.Append("</td>");


            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 50));
            if (b.Status != BuildStatus.InProgress || b.Status != BuildStatus.None || b.Status != BuildStatus.NotStarted)
                sbDays.Append(b.FinishTime.Subtract(b.StartTime).ToString());
            sbDays.Append("</td>");
            //sbDays.Append(string.Format("<td align='{0}'>", "right"));
            //sbDays.Append(((int)(buildStats["totalTests"])).ToString());
            //sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}'>", "right"));
            TimeSpan duration = (TimeSpan)buildStats["duration"];
            sbDays.Append(string.Format("{0}:{1}:{2}", duration.Hours.ToString(), duration.Minutes.ToString(), duration.Seconds.ToString()));
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}'>", "right"));
            sbDays.Append(b.StartTime);
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}'>", "right"));
            sbDays.Append(b.FinishTime);
            sbDays.Append("</td>");
            sbDays.Append("</tr></tbody></table></div>");

            //<a href="/queue/adsapps_ap">adsapps_ap</a>
            return sbDays.ToString();
        }

        private string getTestRunHtml(IBuildDetail build)
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
                            sbDays.Append(string.Format("<div id='{0}'>", "mybuilds"));
                            sbDays.Append(string.Format("<table cellpadding='{0}'>", 3));
                            sbDays.Append("<tbody>");
                            sbDays.Append("<tr>");
                            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 500));
                            sbDays.Append("Run Title");
                            sbDays.Append("</td>");
                            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 50));
                            sbDays.Append("Run Id");
                            sbDays.Append("</td>");
                            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 50));
                            sbDays.Append("(Failed)");
                            sbDays.Append("</td>");
                            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 50));
                            sbDays.Append("Total");
                            sbDays.Append("</td>");
                            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 50));
                            sbDays.Append("Pass Perc");
                            sbDays.Append("</td>");
                            sbDays.Append(string.Format("<td class='{0}'>", "tableHeader"));
                            sbDays.Append("Duration");
                            sbDays.Append("</td>");
                            sbDays.Append("</tr>");

                            foreach (var tr in testRuns)
                            {
                                sbDays.Append(string.Format("<tr style='{0}' class='{1}'>", "background-color:#ddd", "targetSuccess"));
                                sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 500));
                                sbDays.Append(tr.Title);
                                sbDays.Append("</td>");
                                sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 50));
                                sbDays.Append(string.Format("<a href='{0}'>{1}</a>", "http://saapptsthyp027/TestRun/RunDetails?runId=" + tr.Id, tr.Id));
                                sbDays.Append("</td>");
                                sbDays.Append(string.Format("<td align='{0}'>", "right"));
                                sbDays.Append(tr.Statistics.FailedTests.ToString());
                                sbDays.Append("</td>");
                                sbDays.Append(string.Format("<td align='{0}'>", "right"));
                                sbDays.Append(tr.TotalTests.ToString());
                                sbDays.Append("</td>");
                                sbDays.Append(string.Format("<td align='{0}'>", "right"));
                                sbDays.Append(((tr.Statistics.PassedTests / tr.TotalTests) * 100).ToString("0.0"));
                                sbDays.Append("</td>");
                                sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 50));
                                TimeSpan duration = bu.GetRunDuration(tr);
                                sbDays.Append(string.Format("{0}:{1}:{2}", duration.Hours.ToString(), duration.Minutes.ToString(), duration.Seconds.ToString()));
                                sbDays.Append("</td>");
                                sbDays.Append("</tr>");
                            }
                            sbDays.Append("</tbody></table></div>");
                        }
                    }
                }
            }
            return sbDays.ToString();
        }

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