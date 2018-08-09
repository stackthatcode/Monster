using System.Collections.Generic;
using System.Linq;

namespace Push.Shopify.Api.Order
{
    public class DiscountApplication
    {
        public string type { get; set; }
        public decimal value { get; set; }
        public string value_type { get; set; }
        public string allocation_method { get; set; }
        public string target_selection { get; set; }
        public string target_type { get; set; }
        public string description { get; set; }
        public string title { get; set; }

        public Order Parent { get; set; }

        public List<DiscountAllocation> Allocations => Parent.FindAllocations(this);

        public decimal TotalAllocations => Allocations.Sum(x => x.amount);
    }
}
