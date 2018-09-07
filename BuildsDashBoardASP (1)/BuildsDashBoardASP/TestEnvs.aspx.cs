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
using Microsoft.LabMan;
using Microsoft.LabMan.Lab;
using System.Configuration;
using System.Runtime.Remoting.Messaging;
using TFSUtility;
using HTMLUtility;


namespace BuildsDashBoardASP
{
    public partial class TestEnvs : System.Web.UI.Page
    {
        /* •	FFTP: 
 o	[AdsApps][FFTP][MT-Full] 
 o	[AdsApps][Full-FFTP][MT-Full] 
 o	[AdsApps][FFTP][MT-Refresh]Advertiser 
 •	BVT: 
 o	[AdsApps][BVT][MT-Full] 
 o	[AdsApps][BVT][MT-Refresh]Advertiser 
 •	Buddy BVT: 
 o	[AdsApps][BVT-Buddy][MT-RefreshOnly] 
         */

        const string TFS_SERVER_URL = "http://adsgroupvstf:8080/tfs/adsgroup";
        const string TFSPROJECT = "AdsApps";
        const string TEST_REASON_FAILURE = "Test(s) failed or aborted";
        const string SETUP_REASON_FAILURE = "Setup or deployment failed";
        //bool hasBackground;
        int planId;
        static int requestCounter;
        static List<string> buildNames;
        //static Object thisLock = new Object();
        //static bool _hasBackground;
        //static List<IBuildDetail> allBuilds;
        //static List<IBuildDetail> TeamBuilds;

        //static List<IBuildDetail> AdsApps_FFTP_MT_Full;
        //static List<IBuildDetail> AdsApps_FFTP_MT_Refresh;
        //static List<IBuildDetail> AdsApps_BVT_MT_Full;
        //static List<IBuildDetail> AdsApps_BVT_MT_Refresh;
        //static List<IBuildDetail> AdsApps_BVT_Buddy_MT_RefreshOnly;
        //static List<IBuildDetail> LabMan_2_0_Buddy_AdsApps_RME_5;
        //static List<IBuildDetail> LabMan_2_0_BVT_AdsApps_RME_5; 
        //static List<IBuildDetail> LabMan_2_0_BVT_AdsApps_RME_10; 
        //static System.Object lockThis;

        //static IDictionary<string, List<ITestRun>> PlantestRuns;
        //static IDictionary<string, List<TestRunSummary>> PlantestRuns;
        static IDictionary<string, string> friendlyNamesDict;

        delegate IDictionary<string, List<TestRunSummary>> GetTestRunsDelegate(int planId);
        delegate List<LabEnvironment> GetLabsDelegate(TagDetail td);

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
            Response.Write(EnvironmentHTMLUtility.getFooter());
        }

        private string getHtmlHeader()
        {
            StringBuilder sbDays = new StringBuilder();
            sbDays.Append(BuildHTMLUtility.GetHeader());
            return sbDays.ToString();
        }

        private void Initialize()
        {
            friendlyNamesDict = BuildUtility.getEnvironmentPoolFriendlyNames();

            buildNames = new List<string>();
            requestCounter = 0;
            planId = 3047;
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

            //List<IAsyncResult> results = new List<IAsyncResult>();

            //foreach (var tag in templateList)
            //{
            //    GetLabsDelegate dlgt = new GetLabsDelegate(BuildUtility.GetLabEnvs);
            //    AsyncCallback cb = new AsyncCallback(ProcessLabEnvsCallBack);
            //    Interlocked.Increment(ref requestCounter);
            //    object state = tag;
            //    IAsyncResult ar = dlgt.BeginInvoke(tag, cb, tag);
            //    results.Add(ar);
            //}

            //foreach (IAsyncResult result in results)
            //{
            //    result.AsyncWaitHandle.WaitOne();
            //}
            /////////////////////////////
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