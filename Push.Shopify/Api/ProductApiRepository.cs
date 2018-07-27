﻿using System.Collections.Generic;
using Newtonsoft.Json;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Helpers;
using Push.Foundation.Web.HttpClient;
using Push.Shopify.Api.Product;
using Push.Shopify.Config;
using Push.Shopify.HttpClient;

namespace Push.Shopify.Api
{

    public class ProductApiRepository
    {
        private readonly ShopifyRequestBuilder _requestFactory;
        private readonly ClientFacade _client;
        private readonly IPushLogger _logger;
        

        public ProductApiRepository(
                ClientFacade client,
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

        public virtual IList<Product.Product> Retrieve(ProductFilter filter, int page = 1, int limit = 250)
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

            dynamic parent = JsonConvert.DeserializeObject(clientResponse.Body);
            var results = new List<Product.Product>();

            foreach (var product in parent.products)
            {
                var resultProduct =
                    new Product.Product
                    {
                        Id = product.id,
                        Title = product.title,
                        Tags = product.tags,
                        Vendor = product.vendor,
                        ProductType = product.product_type,
                        Variants = new List<Variant>(),
                    };


                foreach (var variant in product.variants)
                {
                    resultProduct.Variants.Add(
                        new Variant()
                        {
                            Id = variant.id,
                            Title = variant.title,
                            Price = variant.price,
                            Sku = variant.sku,
                            ParentProduct = resultProduct,
                            UpdatedAt = variant.updated_at,
                            Inventory = variant.inventory_quantity,
                            InventoryManagement = variant.inventory_management,
                        });
                }

                results.Add(resultProduct);
            }

            return results;
        }
    }
}
