using System;
using System.Collections.Generic;


namespace Push.Shopify.Api.Product
{

    public class Image
    {
        public long id { get; set; }
        public long product_id { get; set; }
        public int position { get; set; }
        public DateTimeOffset created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string alt { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string src { get; set; }
        public List<long> variant_ids { get; set; }
    }

}
