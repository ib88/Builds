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
using System.Threading.Tasks;
using System.Configuration;
using System.Runtime.Remoting.Messaging;

namespace BuildsUtility
{
    public class TagDetail
    {

        public string DisplayName { get; set; }
        public int Total { get; set; }
        public int InUse { get; set; }
        public int Free { get; set; }
        public string Html { get; set; }
        public string SummaryHtml { get; set; }
        public List<LabEnvironment> InUseLabs { get; set; }
        public List<LabEnvironment> ReadyLabs { get; set; }
    }
}
