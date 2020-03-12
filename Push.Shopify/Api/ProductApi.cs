using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Http;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Product;
using Push.Shopify.Http;

namespace Push.Shopify.Api
{

    public class ProductApi
    {
        private readonly ShopifyHttpContext _httpClient;
        private readonly IPushLogger _logger;
        

        public ProductApi(ShopifyHttpContext client, IPushLogger logger)
        {
            _httpClient = client;
            _logger = logger;
        }

        public ResponseEnvelope RetrieveProducts(SearchFilter filter)
        {
            var querystring = filter.ToQueryString();                
            var path = "/admin/api/2019-10/products.json?" + querystring;
            var clientResponse = _httpClient.Get(path);
            return clientResponse;
        }

        public ResponseEnvelope RetrieveProducts(string path)
        {
            var clientResponse = _httpClient.Get(path);
            return clientResponse;
        }

        public string RetrieveProducts(long id)
        {
            var path = $"/admin/api/2019-10/products/{id}.json";
            var clientResponse = _httpClient.Get(path);
            return clientResponse.Body;
        }

        public string CreateProduct(string json)
        {
            var path = $"/admin/api/2019-10/products.json";
            var clientResponse = _httpClient.Post(path, json);
            return clientResponse.Body;
        }

        public string RetrieveVariant(long id)
        {
            var path = $"/admin/api/2019-10/variants/{id}.json";
            var clientResponse = _httpClient.Get(path);
            return clientResponse.Body;
        }

        public string AddVariant(long productId, string json)
        {
            var path = $"/admin/api/2019-10/products/{productId}/variants.json";
            var clientResponse = _httpClient.Post(path, json);
            return clientResponse.Body;
        }


        public string RetrieveByCollection(long id)
        {
            var path = $"/admin/api/2019-10/products.json?collection_id={id}";
            var clientResponse = _httpClient.Get(path);
            return clientResponse.Body;
        }

        public string RetrieveProductMetafields(long product_id)
        {
            var path = $"/admin/api/2019-10/products/{product_id}/metafields.json";
            var clientResponse = _httpClient.Get(path);
            return clientResponse.Body;
        }
        
        
        public void AddMetafield(long productId, MetafieldRead metafield)
        {
            var json = metafield.SerializeToJson();
            var path = $"/admin/api/2019-10/products/{productId}/metafields.json";
            var clientResponse = _httpClient.Post(path, json);
        }
        
        public void UpdateMetafield(long product_id, MetafieldUpdateParent metafield)
        {
            var json = metafield.SerializeToJson();
            var path = $"/admin/api/2019-10/products/{product_id}/metafields/{metafield.metafield.id}.json";
            var clientResponse = _httpClient.Put(path, json);
        }

        public string RetrieveLocations()
        {
            var path = $"/admin/api/2019-10/locations.json";
            var clientResponse = _httpClient.Get(path);
            return clientResponse.Body;
        }

        public string RetrieveInventoryLevels(List<long> inventoryItemIds)
        {
            if (inventoryItemIds.Count == 0)
                throw new ArgumentException("Empty list of InventoryItemIds");
            if (inventoryItemIds.Count > 50)
                throw new ArgumentException("Maximum size for InventoryItemIds is 50");

            var queryString =
                new QueryStringBuilder()
                    .Add("inventory_item_ids", inventoryItemIds.ToCommaSeparatedList())
                    .ToString();

            var path = $"/admin/api/2019-10/inventoryItemIds?{queryString}";
            var clientResponse = _httpClient.Get(path);
            _logger.Trace(clientResponse.Body);
            return clientResponse.Body;
        }

        public string UpdateVariantPrice(long variantId, string json)
        {
            var path = $"/admin/api/2019-10/variants/{variantId}.json";
            var clientResponse = _httpClient.Put(path,json);
            return clientResponse.Body;
        }
    }
}

