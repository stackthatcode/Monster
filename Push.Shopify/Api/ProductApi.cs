﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.Http;
using Push.Shopify.Api.Product;
using Push.Shopify.Http;

namespace Push.Shopify.Api
{

    public class ProductApi
    {
        private readonly ShopifyHttpContext _httpClient;
        private readonly IPushLogger _logger;
        

        public ProductApi(
                ShopifyHttpContext client, IPushLogger logger)
        {
            _httpClient = client;
            _logger = logger;
        }

        public virtual int RetrieveCount(ProductFilter filter)
        {
            var path = "/admin/products/count.json?" + filter.ToQueryStringBuilder();
            var clientResponse = _httpClient.Get(path);

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);
            var count = parent.count;
            return count;
        }

        public virtual string Retrieve(ProductFilter filter)
        {
            var querystring = filter.ToQueryString();                
            var path = "/admin/products.json?" + querystring;

            var clientResponse = _httpClient.Get(path);
            return clientResponse.Body;
        }

        public virtual string Retrieve(long id)
        {
            var path = $"/admin/products/{id}.json";
            var clientResponse = _httpClient.Get(path);
            return clientResponse.Body;
        }

        public virtual string RetrieveByCollection(long id)
        {
            var path = $"/admin/products.json?collection_id={id}";
            var clientResponse = _httpClient.Get(path);
            return clientResponse.Body;
        }

        public virtual string RetrieveProductMetafields(long product_id)
        {
            var path = $"/admin/products/{product_id}/metafields.json";
            var clientResponse = _httpClient.Get(path);
            return clientResponse.Body;
        }
        
        
        public virtual void AddMetafield(
                long productId, MetafieldRead metafield)
        {
            var json = metafield.SerializeToJson();
            var path = $"/admin/products/{productId}/metafields.json";
            var clientResponse = _httpClient.Post(path, json);
        }
        
        public virtual void UpdateMetafield(
                    long product_id, MetafieldUpdateParent metafield)
        {
            var json = metafield.SerializeToJson();
            var path = $"/admin/products/{product_id}/metafields/{metafield.metafield.id}.json";
            var clientResponse = _httpClient.Put(path, json);
        }

        public virtual string RetrieveLocations()
        {
            var path = $"/admin/locations.json";
            var clientResponse = _httpClient.Get(path);
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
            var clientResponse = _httpClient.Get(path);
            _logger.Trace(clientResponse.Body);
            return clientResponse.Body;
        }
    }
}