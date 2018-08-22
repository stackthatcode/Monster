using System;
using System.IO;
using System.Linq;
using Autofac;
using Monster.ConsoleApp.TestJson;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Order;
using Push.Shopify.Api.Payout;
using Push.Shopify.Api.Product;
using Push.Shopify.Api.Transaction;
using Push.Shopify.Config;
using Push.Shopify.HttpClient.Credentials;

namespace Monster.ConsoleApp._3duStuff
{
    public class Metaplay
    {
        public static void UpdateMetadata(ILifetimeScope scope)
        {
            var factory = scope.Resolve<ApiFactory>();
            var credentials = Program.CredentialsFactory();
            var productApi = factory.MakeProductApi(credentials);

            var products =
                productApi
                    .RetrieveByCollection(56819023972)
                    .DeserializeFromJson<ProductList>();

            foreach (var product in products.products)
            {
                var metafields =
                    productApi
                        .RetrieveProductMetafields(product.id)
                        .DeserializeFromJson<MetafieldReadList>()
                        .metafields;

                var existingMeta =
                    metafields.FirstOrDefault(
                        x => x.@namespace == "global" && x.key == "lead_time");

                var newMeta = new Metafield()
                {
                    @namespace = "global",
                    key = "lead_time",
                    value_type = "string",
                    value = "1 to 2 weeks from time of placing order",
                };
                var newMetaParent = new MetafieldParent()
                {
                    metafield = newMeta
                };

                if (existingMeta != null)
                {
                    productApi.UpdateMetafield(product.id, newMetaParent);
                }
                else
                {
                    productApi.AddMetafield(product.id, newMetaParent);
                }
            }

        }

    }
}
