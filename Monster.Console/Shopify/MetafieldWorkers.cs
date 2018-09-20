using System.Linq;
using Autofac;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Product;
using Push.Shopify.Http.Credentials;

namespace Monster.ConsoleApp.Shopify
{
    public class MetafieldWorkers
    {
        public static void UpdateMetadata(ILifetimeScope scope)
        {
            var factory = scope.Resolve<ShopifyApiFactory>();
            var credentials = ShopifyHarness.CredentialsFactory();
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

        public static 
                void CopyShoppingFeedMetadata(
                    ILifetimeScope scope,
                    IShopifyCredentials credentials,
                    long sourceProductId,
                    long targetProductId,
                    string @namespace)
        {
            var factory = scope.Resolve<ShopifyApiFactory>();
            var logger = scope.Resolve<IPushLogger>();

            var productApi = factory.MakeProductApi(credentials);
            
            var sourceMetafields =
                productApi
                    .RetrieveProductMetafields(sourceProductId)
                    .DeserializeFromJson<MetafieldReadList>()
                    .metafields;

            var filteredSourceMetafields =
                sourceMetafields
                    .Where(x => x.@namespace == @namespace)
                    .ToList();

            var targetMetafields =
                productApi
                    .RetrieveProductMetafields(targetProductId)
                    .DeserializeFromJson<MetafieldReadList>()
                    .metafields;

            foreach (var sourceMetaField in filteredSourceMetafields)
            {
                var newMeta = new Metafield()
                {
                    @namespace = sourceMetaField.@namespace,
                    key = sourceMetaField.key,
                    value_type = sourceMetaField.value_type,
                    value = sourceMetaField.value,
                };

                var newMetaParent = new MetafieldParent()
                {
                    metafield = newMeta
                };

                var exists =
                    targetMetafields
                        .Any(x => x.@namespace == sourceMetaField.@namespace
                                  && x.key == sourceMetaField.key);

                if (exists)
                {
                    logger.Info($"Updating Metafield => {newMeta.ToString()} ");
                    productApi.UpdateMetafield(targetProductId, newMetaParent);
                }
                else
                {
                    logger.Info($"Add Metafield => {newMeta.ToString()} ");
                    productApi.AddMetafield(targetProductId, newMetaParent);
                }
            }
        }

    }
}
