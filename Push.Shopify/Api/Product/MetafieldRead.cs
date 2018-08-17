using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Push.Shopify.Api.Product
{
    public class MetafieldRead
    {
        public long id { get; set; }
        public string @namespace { get; set; }
        public string key { get; set; }
        public string value { get; set; }
        public string value_type { get; set; }
        public string description { get; set; }
        public long owner_id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string owner_resource { get; set; }
        public string admin_graphql_api_id { get; set; }
    }

    public class MetafieldReadList
    {
        public List<MetafieldRead> metafields { get; set; }
    }
}
