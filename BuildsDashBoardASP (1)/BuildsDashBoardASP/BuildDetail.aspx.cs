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
using HTMLUtility;

namespace BuildsDashBoardASP
{
    public partial class BuildDetail : System.Web.UI.Page
    {
        const string BUDDYBUILDBVT = "[AdsApps][BVT-Buddy][MT-RefreshOnly]";
        const string LABMANCIBVT = "[LabMan2.0][BVT][AdsApps][MT][Apps]";
        const string LABMANFFTP = "[AdsApps][Full-FFTP][MT-Full]";
        const string TFS_SERVER_URL = "http://adsgroupvstf:8080/tfs/adsgroup";
        const string TFSPROJECT = "AdsApps";
        //bool hasBackground = true;
        //static int requestCounter = 0;
        //static List<IBuildDetail> TeamBuilds = null;
        //static List<IBuildDetail> MyBuilds = null;
        string buildNumber;
        BuildUtility bu;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty((string)Request.QueryString["name"]))
            {
                Initialize((string)Request.QueryString["name"]);

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
            else
            {
                Response.Redirect("Error.aspx");
            }
        }

        private void Initialize(string bn)
        {
            buildNumber = bn;
            bu = new BuildUtility();
        }

        private string getHtmlHeader()
        {
            StringBuilder sbDays = new StringBuilder();
            sbDays.Append(BuildHTMLUtility.GetHeader());
            //get the hours from config
            //int lastXHours;
            //if (!int.TryParse(ConfigurationManager.AppSettings["lastXHours"], out lastXHours))
            //    lastXHours = 24;
            sbDays.Append("<h2>Build Details</h2>");
            return sbDays.ToString();
        }

        private string GetHtml()
        {
            StringBuilder sbDays = new StringBuilder();
            //sbDays.Append(GetHeader());
            //sbDays.Append("<h2>Build Details</h2>");

            var build = BuildUtility.GetBuildByDef(TFSPROJECT, BuildUtility.GetBuildDefinition(buildNumber), TFS_SERVER_URL, 24).Where(b => b.BuildNumber.Equals(buildNumber)).FirstOrDefault();

            if (build != null)
            {
                IDictionary<string, object> buildStats = bu.GetBuildStatistics(build);
                //IDictionary<string, object> buildStats = null;
                sbDays.Append((string)buildStats["html"]);
            }

            return sbDays.ToString();
        }

        private string GetHeader()
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
            sbDays.Append("</div>");
            sbDays.Append("</td>");
            sbDays.Append("</tr>");
            //body row
            sbDays.Append(string.Format("<td id='{0}'>", "bodyRow"));
            sbDays.Append(string.Format("<div id='{0}' class='{1}'>", "outageAlert", "outageAlert"));
            sbDays.Append("</div>");
            return sbDays.ToString();
        }
    }
}