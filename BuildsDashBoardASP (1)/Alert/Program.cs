using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alert
{
    class Program
    {
        static void Main(string[] args)
        {
            //var syncService = new TFSDataSyncService();
            //syncService.TestRunSync();
            //////////////

            var tcms = new AlerService();
            tcms.process();
            //System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
            ///////////////
        }
    }
}
