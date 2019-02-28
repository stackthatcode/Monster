using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monster.Middle.Processes.Sync.Orders.Workers
{
    public class ParallelWorker
    {
        private readonly AcumaticaOrderSync _acumaticaOrderSync;

        public ParallelWorker(AcumaticaOrderSync acumaticaOrderSync)
        {
            _acumaticaOrderSync = acumaticaOrderSync;
        }


    }
}
