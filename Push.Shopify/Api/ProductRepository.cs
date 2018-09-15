using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Http;
using Push.Shopify.Api.Product;
using Push.Shopify.Model;

namespace Push.Shopify.Api
{

    public class ProductRepository
    {
        private readonly HttpFacade _client;
        private readonly IPushLogger _logger;
        

        public ProductRepository(
                HttpFacade client, IPushLogger logger)
        {
            _client = client;
            _logger = logger;
        }

        public virtual int RetrieveCount(ProductFilter filter)
        {
            var path = "/admin/products/count.json?" + filter.ToQueryStringBuilder();
            var clientResponse = _client.Get(path);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);
            var count = parent.count;
            return count;
        }

        public virtual string Retrieve(
                    ProductFilter filter, int page = 1, int limit = 250)
        {
            var querystring
                = new QueryStringBuilder()
                    .Add("page", page)
                    .Add("limit", limit)
                    .Add(filter.ToQueryStringBuilder())
                    .ToString();

            var path = "/admin/products.json?" + querystring;

            var clientResponse = _client.Get(path);
            _logger.Trace(clientResponse.Body);            
            return clientResponse.Body;
        }

        public virtual string Retrieve(long id)
        {
            var path = $"/admin/products/{id}.json";
            var clientResponse = _client.Get(path);
            _logger.Trace(clientResponse.Body);
            return clientResponse.Body;
        }

        public virtual string RetrieveByCollection(long id)
        {
            var path = $"/admin/products.json?collection_id={id}";
            var clientResponse = _client.Get(path);
            _logger.Trace(clientResponse.Body);
            return clientResponse.Body;
        }

        public virtual string RetrieveProductMetafields(long product_id)
        {
            var path = $"/admin/products/{product_id}/metafields.json";
            var clientResponse = _client.Get(path);
            _logger.Trace(clientResponse.Body);
            return clientResponse.Body;
        }
        
        
        public virtual void AddMetafield(long productId, MetafieldParent metafield)
        {
            var json = metafield.SerializeToJson();
            var path = $"/admin/products/{productId}/metafields.json";
            var clientResponse = _client.Post(path, json);
            _logger.Trace(clientResponse.Body);            
        }
        
        public virtual void UpdateMetafield(long productId, MetafieldParent metafield)
        {
            var json = metafield.SerializeToJson();
            var path = $"/admin/products/#{productId}/metafields.json";
            var clientResponse = _client.Put(path, json);
            _logger.Trace(clientResponse.Body);
        }

        public virtual string RetrieveLocations()
        {
            var path = $"/admin/locations.json";
            var clientResponse = _client.Get(path);
            _logger.Trace(clientResponse.Body);
            return clientResponse.Body;
        }

        public virtual string RetrieveInventoryLevels(List<long> inventoryItemIds)
        {
            if (inventoryItemIds.Count == 0)
                throw new ArgumentException("Empty list of InventoryItemIds");
            if (inventoryItemIds.Count > 50)
                throw new ArgumentException("Maximum size for InventoryItemIds is 50");

            var queryString =
                new QueryStringBuilder()
                    .Add("inventory_item_ids", inventoryItemIds.ToCommaSeparatedList())
                    .ToString();

            var path = $"/admin/inventoryItemIds?{queryString}";
            var clientResponse = _client.Get(path);
            _logger.Trace(clientResponse.Body);
            return clientResponse.Body;
        }
    }
}
