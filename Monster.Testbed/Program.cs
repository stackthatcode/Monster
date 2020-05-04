using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry;

namespace Monster.Testbed
{
    class Program
    {
        static void Main(string[] args)
        {
            var x = 1;

            using (SentrySdk.Init("https://39a2ab07189641a3a10eeffd1354873d@o378852.ingest.sentry.io/5202882"))
            {
                try
                {
                    var exception = new Exception("Oh noes!!!");
                    throw exception;
                }
                catch (Exception ex)
                {
                    //SentrySdk.CaptureException(ex);
                }

                // Console.WriteLine(x / 0);
            }
        }
    }
}
