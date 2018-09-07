using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using TFSDataSync.Common;
using System.Diagnostics;
using System.IO;
using System.Configuration;
//using Utility;

namespace Alert
{
    public class AlerService
    {
        private AlertHandler handler = new AlertHandler();

        public AlerService()
        {
        }

        public void process()
        {
            try
            {
               
                //handler.ProcessPoolAlerts();
                handler.ProcessBuildAlerts();
           
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
