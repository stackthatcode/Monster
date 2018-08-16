using System.Collections.Generic;

namespace Push.Shopify.Api.Product
{
    public class Option
    {
        public long id { get; set; }
        public long product_id { get; set; }
        public string name { get; set; }
        public int position { get; set; }
        public List<string> values { get; set; }
    }

}
