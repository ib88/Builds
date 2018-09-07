using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuildsUtility;
using Microsoft.TeamFoundation.Lab.Client;

namespace HTMLUtility
{
    public class EnvironmentHTMLUtility
    {
        public static string GetSummaryTableHeader()
        {
            StringBuilder sbDays = new StringBuilder();
            //sbDays.Clear();
            sbDays.Append(string.Format("<div id='{0}' class='{1}'>", "mybuilds", "table_font_size"));
            sbDays.Append(string.Format("<table cellpadding='{0}'  style='{1}'>", 3, "FONT-SIZE: 13px"));
            sbDays.Append("<tbody>");
            sbDays.Append("<tr>");
            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 300));
            sbDays.Append("Pool Name");
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 50));
            sbDays.Append("Total");
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 50));
            sbDays.Append("In Use");
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 50));
            sbDays.Append("Free");
            sbDays.Append("</td>");
            sbDays.Append("</tr>");
            return sbDays.ToString();
        }

        public static string GetSummaryTableFooter()
        {
            return "</tbody></table></div>";
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

        public static string GetTagDetailSummaryHtml(TagDetail td, bool hasBackground, IDictionary<string, string> friendlyNamesDict)
        {
            StringBuilder sbDays = new StringBuilder();
            string rowClass = "targetSuccess";
            if (td.ReadyLabs.Count == 0)
                rowClass = "targetFailure";
            else if (td.ReadyLabs.Count >= 1 && td.ReadyLabs.Count <= 5)
                rowClass = "targetPartiallySucceededTestFailed";
            else if (td.ReadyLabs.Count > 5)
                rowClass = "targetSuccess";
            else
                rowClass = "targetFailure";

            if (hasBackground)
                sbDays.Append(string.Format("<tr style='{0}' class='{1}'>", "background-color:#ddd", rowClass));
            else
                sbDays.Append(string.Format("<tr class='{0}'>", rowClass));
            //hasBackground = !hasBackground;
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 300));
            //\\asgdrops\AdsApps\AdsApps_vnext\Rolling\17.02.1.20141006.3347047\retail\amd64\App\Test
            //sbDays.Append(string.Format("<a href='{0}'>{1}</a>", string.Format("http://adsgroupvstf:8080/tfs/AdsGroup/AdsApps/_build#buildUri={0}&_a=summary", b.Uri), b.BuildNumber.Substring(b.BuildNumber.IndexOf("_") + 1)));
            sbDays.Append(friendlyNamesDict[td.DisplayName]);
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 50));
            sbDays.Append(td.Total);
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 50));
            sbDays.Append(td.InUseLabs.Count);
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 50));
            sbDays.Append(td.ReadyLabs.Count);
            sbDays.Append("</td>");
            sbDays.Append("</tr>");
            return sbDays.ToString();
        }

        public static string GetLabHtml(LabEnvironment le, bool hasBackground)
        {
            StringBuilder sbDays = new StringBuilder();
            string rowClass = "targetSuccess";

            if (hasBackground)
                sbDays.Append(string.Format("<tr style='{0}' class='{1}'>", "background-color:#ddd", rowClass));
            else
                sbDays.Append(string.Format("<tr class='{0}'>", rowClass));
            //hasBackground = !hasBackground;
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 300));
            sbDays.Append(le.FullName);
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 200));
            sbDays.Append(le.CreationTime);
            sbDays.Append("</td>");
            //sbDays.Append(string.Format("<td align='{0}' width='{1}'>", "center", 200));
            //sbDays.Append(le.ProjectName);
            //sbDays.Append("</td>");
            sbDays.Append("</tr>");
            return sbDays.ToString();
        }

        public static string GetTagSummary(TagDetail td)
        {
            return "  Total " + td.Total + ", InUse " + td.InUseLabs.Count + ", Free " + td.ReadyLabs.Count;
        }
        public static string GetTableFooter()
        {
            return "</tbody></table></div>";
        }

        public static string GetTableHeader()
        {
            StringBuilder sbDays = new StringBuilder();
            //sbDays.Clear();
            sbDays.Append(string.Format("<div id='{0}' class='{1}'>", "mybuilds", "table_font_size"));
            sbDays.Append(string.Format("<table cellpadding='{0}'  style='{1}'>", 3, "FONT-SIZE: 13px"));
            sbDays.Append("<tbody>");
            sbDays.Append("<tr>");
            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 300));
            sbDays.Append("Environment Name");
            sbDays.Append("</td>");
            sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 200));
            sbDays.Append("Created Time");
            sbDays.Append("</td>");
            //sbDays.Append(string.Format("<td class='{0}' width='{1}'>", "tableHeader", 100));
            //sbDays.Append("Project Name");
            //sbDays.Append("</td>");
            sbDays.Append("</tr>");

            return sbDays.ToString();
        }

        public static string getPoolSummaryHtml(List<TagDetail> templateList)
        {
            StringBuilder sbDays = new StringBuilder();
            sbDays.Append(EnvironmentHTMLUtility.GetSummaryTableHeader());
            foreach (var td in templateList)
            {
                sbDays.Append(td.SummaryHtml);
            }
            sbDays.Append(EnvironmentHTMLUtility.GetSummaryTableFooter());
            return sbDays.ToString();
        }


    }
}
