using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monster.Middle.Processes.Orders.Workers.Model
{
    // TODO - do we need this validation...?
    public class RefundCancelStatus
    {
        public List<RefundCancelStatusDetail> Detail { get; set; }

        public RefundCancelStatus() { }
    }

    public class RefundCancelStatusDetail
    {
    }
}
