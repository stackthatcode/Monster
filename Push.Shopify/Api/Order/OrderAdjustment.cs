using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Push.Shopify.Api.Order
{
    public class OrderAdjustment
    {
        public long id { get; set; }
        public long order_id { get; set; }
        public long refund_id { get; set; }
        public decimal amount { get; set; }
        public decimal tax_amount { get; set; }
        public string kind { get; set; }
        public string reason { get; set; }
    }
}
