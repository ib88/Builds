using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.Lab.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using BuildsUtility;

namespace HTMLUtility
{
    public class BuildHTMLUtility
    {
        public static string getHtmlHeader(int lastXHours)
        {
            StringBuilder sbDays = new StringBuilder();
            sbDays.Append(GetHeader());
            //get the hours from config
            sbDays.Append(string.Format("<h2>Team Test Runs ( Last {0} hours )</h2>", lastXHours.ToString()));
            return sbDays.ToString();
        }

        public static string getFooter()
        {
            StringBuilder sbDays = new StringBuilder();
            sbDays.Append("</td>");
            sbDays.Append("</tr>");
            sbDays.Append("</tbody>");
            sbDays.Append("</table>");
            return sbDays.ToString();
        }

        public static string GetHeader()
        {
            StringBuilder sbDays = new StringBuilder();
            sbDays.Append(string.Format("<table id='{0}'>", "batmonWebLayout"));
            sbDays.Append("<tbody>");
            sbDays.Append("<tr>");
            sbDays.Append(string.Format("<td id='{0}' style='{1}'>", "headerRow", "FONT-SIZE: 13px"));
            sbDays.Append(string.Format("<div class='{0}'>", "header"));
            sbDays.Append(string.Format("<a href='{0}'>My Test Runs</a>", "MyRuns.aspx"));
            sbDays.Append(string.Format("  | <a href='{0}'>Team Test Runs</a>", "TeamRuns.aspx"));
            sbDays.Append(string.Format("  | <a href='{0}'>Test Environments</a>", "TestEnvs.aspx"));
            sbDays.Append(string.Format("  | <a href='{0}'>Runs Summary</a>", "RunSummary.aspx"));
            sbDays.Append("</div>");
            sbDays.Append("</td>");
            sbDays.Append("</tr>");
            //body row
            sbDays.Append(string.Format("<td id='{0}'>", "bodyRow"));
            sbDays.Append(string.Format("<div id='{0}' class='{1}'>", "outageAlert", "outageAlert"));
            sbDays.Append("</div>");
            return sbDays.ToString();
        }

        public static string getBuilDefinitionStatdHtmlHeader()
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

            sbDays.Append("</tr>");

            return sbDays.ToString();
        }

        public static string getBuildsHtmlByDefinition(List<IBuildDetail> builds, string definition, IDictionary<string, List<TestRunSummary>> PlantestRuns, IDictionary<string, string> AllEnvDictionary)
        {
            StringBuilder sbDays = new StringBuilder();
            if (builds == null)
                return "";
            if (builds.Count > 0)
            {
                sbDays.Append(string.Format("<h2 style='{0}'>{1}</h2>", "FONT-SIZE: 13px", BuildUtility.GetBuildName(definition)));
                sbDays.Append(getBuildHtmlHeader());

                StringBuilder sbHtml = new StringBuilder();
                bool hasBackground = true;
                foreach (IBuildDetail b in builds)
                {
                    if (BuildUtility.isRme(b.BuildNumber))
                    {
                        sbHtml.Append(GetBuildHtmlForRME(b, hasBackground, PlantestRuns, AllEnvDictionary));
                        hasBackground = !hasBackground;
                    }
                    else
                        if (BuildUtility.isAdsAppsBuild(b))
                        {
                            sbHtml.Append(GetBuildHtml(b, hasBackground, PlantestRuns, AllEnvDictionary));
                            hasBackground = !hasBackground;
                        }
                }
                if (string.IsNullOrEmpty(sbHtml.ToString()))
                    return getBuildsFooter();
                sbDays.Append(sbHtml.ToString());
                sbDays.Append(getBuildsFooter());
            }
            return sbDays.ToString();
        }

        public static string getBuildDefStats(IDictionary<string, BuildDefinitionStat> builds, List<string> names)
        {

            StringBuilder sbDays = new StringBuilder();
            if (builds == null)
                return "";
            if (builds.Count > 0)
            {
                sbDays.Append(getBuilDefinitionStatdHtmlHeader());

                StringBuilder sbHtml = new StringBuilder();
                bool hasBackground = true;
                //foreach (KeyValuePair<string, BuildDefinitionStat> b in builds)
                foreach (var def in names)
                {
                    sbHtml.Append(GetBuildDefinitionStatHtml(builds[def], hasBackground));
                    hasBackground = !hasBackground;
                }
                if (string.IsNullOrEmpty(sbHtml.ToString()))
                    return getBuildsFooter();
                sbDays.Append(sbHtml.ToString());
                sbDays.Append(getBuildsFooter());
            }
            return sbDays.ToString();
        }

        static string GetBuildDefinitionStatHtml(BuildDefinitionStat b, bool hasBackground)
        {
            StringBuilder sbDays = new StringBuilder();
            //List<ITestRun> testRuns;
            //IDictionary<string, object> buildStats = BuildUtility.GetBuildStatisticsForHomePage(b, testRuns);
            //string buildEnvName = BuildUtility.GetBuildEnvInfosForHomePage(b, testRuns, AllEnvDictionary);
            //BODY
            string rowClass = "targetSuccess";
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

            sbDays.Append("</tr>");
            return sbDays.ToString();
        }

        public static string getBuildHtmlHeader()
        {
            StringBuilder sbDays = new StringBuilder();

            sbDays.Append(GetBuildsTableHeader());

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
            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 200));
            sbDays.Append("Environment Name");
            sbDays.Append("</td>");

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

        public static string GetBuildsTableHeader()
        {
            StringBuilder sbDays = new StringBuilder();
            sbDays.Append(string.Format("<div id='{0}' class='{1}'>", "mybuilds", "table_font_size"));
            sbDays.Append(string.Format("<table cellpadding='{0}'  style='{1}'>", 3, "FONT-SIZE: 13px"));
            sbDays.Append("<tbody>");
            return sbDays.ToString();
        }

        static string GetRunStatsHtml(dynamic anonymousType)
        {
            return "";
        }


        public static string GetBuildHtml(IBuildDetail b, bool hasBackground, IDictionary<string, List<TestRunSummary>> PlantestRuns, IDictionary<string, string> AllEnvDictionary)
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
            string rowClass = "targetFailure";
            if (b.Status == BuildStatus.Succeeded || Convert.ToDouble(buildStats["passPerc"]).ToString("0.00").Equals("100.00"))
                rowClass = "targetSuccess";
            else if (b.Status == BuildStatus.PartiallySucceeded)
                rowClass = "targetPartiallySucceededTestFailed";
            else if (b.Status == BuildStatus.InProgress || b.Status == BuildStatus.NotStarted)
                rowClass = "targetNotFinished";
            else
                rowClass = "targetFailure";

            if (hasBackground)
                sbDays.Append(string.Format("<tr style='{0}' class='{1}'>", "background-color:#ddd", rowClass));
            else
                sbDays.Append(string.Format("<tr class='{0}'>", rowClass));
            //hasBackground = !hasBackground;
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 50));
            //\\asgdrops\AdsApps\AdsApps_vnext\Rolling\17.02.1.20141006.3347047\retail\amd64\App\Test
            sbDays.Append(string.Format("<a href='{0}'>{1}</a>", string.Format("http://adsgroupvstf:8080/tfs/AdsGroup/AdsApps/_build#buildUri={0}&_a=summary", b.Uri), b.BuildNumber.Substring(b.BuildNumber.IndexOf("_") + 1)));
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 100));
            sbDays.Append(string.Format("<a href='{0}'>{1}</a>", "BuildDetail.aspx?name=" + b.BuildNumber, b.Status));
            sbDays.Append("</td>");
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

        public static string GetBuildHtmlForRME(IBuildDetail b, bool hasBackground, IDictionary<string, List<TestRunSummary>> PlantestRuns, IDictionary<string, string> AllEnvDictionary)
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
            string rowClass = "targetFailure";
            if (b.Status == BuildStatus.Succeeded || b.Status == BuildStatus.PartiallySucceeded)
                rowClass = "targetSuccess";
            //else if (b.Status == BuildStatus.PartiallySucceeded && Convert.ToDouble(buildStats["passPerc"]).ToString("0.00").Equals("0.00"))
            //    rowClass = "targetPartiallySucceededTestFailed";
            else if (b.Status == BuildStatus.PartiallySucceeded)
                rowClass = "targetPartiallySucceededTestFailed";
            else if (b.Status == BuildStatus.InProgress || b.Status == BuildStatus.NotStarted)
                rowClass = "targetNotFinished";
            else
                rowClass = "targetFailure";

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

        public static string getBuildsFooter()
        {
            return "</tbody></table></div>";
        }
    }
}
