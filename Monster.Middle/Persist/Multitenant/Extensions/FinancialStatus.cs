using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monster.Middle.Persist.Multitenant.Extensions
{
    // Mirror's Shopify's Order -> Financial Status field
    public class FinancialStatus
    {
        public const string Pending ="pending";
        public const string Authorized = "authorized";
        public const string PartiallyPaid = "partially_paid";
        public const string Paid = "paid";
        public const string PartiallyRefunded = "partially_refunded";
        public const string Refunded = "refunded";
        public const string Voided = "voided";
    }

}
