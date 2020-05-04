using System.Collections.Generic;

namespace Monster.Acumatica.Api.Reference
{
    public class ShopifyCarrier
    {
        public long id { get; set; }
        public string name { get; set; }
        public bool active { get; set; }
        public bool service_discovery { get; set; }
        public string carrier_service_type { get; set; }
        public string admin_graphql_api_id { get; set; }
        public string format { get; set; }
        public string callback_url { get; set; }
    }

    public class ShopifyCarrierParent
    {
        public List<ShopifyCarrier> carrier_services { get; set; }
    }
}

