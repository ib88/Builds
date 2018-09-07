using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alert;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            var tcms = new AlerService();
            tcms.process();
        }
    }
}
