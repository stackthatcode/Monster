using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Push.Shopify.Api.Order
{
    public class DiscountApplication
    {
        public string type { get; set; }
        public string value { get; set; }
        public string value_type { get; set; }
        public string allocation_method { get; set; }
        public string target_selection { get; set; }
        public string target_type { get; set; }
        public object description { get; set; }
        public object title { get; set; }
    }
}
