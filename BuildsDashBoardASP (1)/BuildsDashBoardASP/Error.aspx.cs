using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BuildsDashBoardASP
{
    public partial class Error : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Write(GetHtml());
        }

        private string GetHtml()
        {
            StringBuilder sbDays = new StringBuilder();
            sbDays.Append("<h1>Something went wrong!</h1>");
            sbDays.Append("<h2>For help please contact:</h2>");
            sbDays.Append(string.Format("<p   style='{0}'> Niranjan.Dharmarajan@microsoft.com ", "FONT-SIZE: 13px"));
            sbDays.Append("</p>");
            sbDays.Append(string.Format("<p   style='{0}'> Mahesh.Arali@microsoft.com ", "FONT-SIZE: 13px"));
            sbDays.Append("</p>");
            sbDays.Append(string.Format("<p   style='{0}'> v-komefi@microsoft.com ", "FONT-SIZE: 13px"));
            sbDays.Append("</p>");
            return sbDays.ToString();
        }
    }
}