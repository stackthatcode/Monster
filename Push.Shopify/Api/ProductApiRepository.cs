﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.HttpClient;
using Push.Shopify.Api.Product;
using Push.Shopify.Config;
using Push.Shopify.HttpClient;
using Push.Shopify.Model;

namespace Push.Shopify.Api
{

    public class ProductApiRepository
    {
        private readonly ShopifyRequestBuilder _requestFactory;
        private readonly HttpFacade _client;
        private readonly IPushLogger _logger;
        

        public ProductApiRepository(
                HttpFacade client,
                ShopifyClientSettings settings,
                ShopifyRequestBuilder requestFactory, 
                IPushLogger logger)
        {
            _client = client;
            _client.Settings = settings;
            _requestFactory = requestFactory;
            _logger = logger;
        }

        public virtual int RetrieveCount(ProductFilter filter)
        {
            var path = "/admin/products/count.json?" + filter.ToQueryStringBuilder();

            var request = _requestFactory.HttpGet(path);
            var clientResponse = _client.ExecuteRequest(request);

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

            var request = _requestFactory.HttpGet(path);
            var clientResponse = _client.ExecuteRequest(request);
            _logger.Trace(clientResponse.Body);            
            return clientResponse.Body;
        }

        public virtual string Retrieve(long id)
        {
            var path = $"/admin/products/{id}.json";
            var request = _requestFactory.HttpGet(path);
            var clientResponse = _client.ExecuteRequest(request);
            _logger.Trace(clientResponse.Body);
            return clientResponse.Body;
        }
        public virtual string RetrieveByCollection(long id)
        {
            var path = $"/admin/products.json?collection_id={id}";
            var request = _requestFactory.HttpGet(path);
            var clientResponse = _client.ExecuteRequest(request);
            _logger.Trace(clientResponse.Body);
            return clientResponse.Body;
        }
        public virtual string RetrieveProductMetafields(long product_id)
        {
            var path = $"/admin/products/{product_id}/metafields.json";
            var request = _requestFactory.HttpGet(path);
            var clientResponse = _client.ExecuteRequest(request);
            _logger.Trace(clientResponse.Body);
            return clientResponse.Body;
        }

        public virtual void DeleteMetafield(long metafieldId)
        {
            var path = $"/admin/metafields/{metafieldId}.json";
            var request = _requestFactory.HttpDelete(path);
            var clientResponse = _client.ExecuteRequest(request);
            _logger.Trace(clientResponse.Body);
        }
        
        public virtual void AddMetafield(long productId, MetafieldParent metafield)
        {
            var json = metafield.SerializeToJson();
            var path = $"/admin/products/{productId}/metafields.json";
            var request = _requestFactory.HttpPost(path, json);
            var clientResponse = _client.ExecuteRequest(request);
            _logger.Trace(clientResponse.Body);            
        }


        public virtual void UpdateMetafield(long productId, MetafieldParent metafield)
        {
            var json = metafield.SerializeToJson();
            var path = $"/admin/products/#{productId}/metafields.json";
            var request = _requestFactory.HttpPut(path, json);
            var clientResponse = _client.ExecuteRequest(request);
            _logger.Trace(clientResponse.Body);
        }

        public virtual string RetrieveLocations()
        {
            var path = $"/admin/locations.json";
            var request = _requestFactory.HttpGet(path);
            var clientResponse = _client.ExecuteRequest(request);
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
            var request = _requestFactory.HttpGet(path);
            var clientResponse = _client.ExecuteRequest(request);
            _logger.Trace(clientResponse.Body);
            return clientResponse.Body;
        }
    }
}
