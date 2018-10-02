using System.Linq;
using Autofac;
using Push.Foundation.Utilities.Json;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api;
using Push.Shopify.Api.Product;

namespace Monster.ConsoleApp.Shopify
{
    public class MetafieldProcesses
    {
        public static void UpdateMetadata(
                    ILifetimeScope scope, long collectionId)
        {
            var productApi = scope.Resolve<ProductApi>();
                
            var products =
                productApi
                    .RetrieveByCollection(collectionId)
                    .DeserializeFromJson<ProductList>();

            foreach (var product in products.products)
            {
                var metafields =
                    productApi
                        .RetrieveProductMetafields(product.id)
                        .DeserializeFromJson<MetafieldList>()
                        .metafields;

                var existingMeta = metafields.Find("global", "lead_time");
                
                if (existingMeta != null)
                {
                    var updateMeteParent =
                        MetafieldUpdateParent.Make(
                            existingMeta.id, 
                            "string", 
                            "1 to 2 weeks from time of placing order");

                    productApi.UpdateMetafield(product.id, updateMeteParent);
                }
                else
                {
                    var newMetaParent = new MetafieldRead()
                    {
                        metafield = new Metafield()
                        {
                            @namespace = "global",
                            key = "lead_time",
                            value_type = "string",
                            value = "1 to 2 weeks from time of placing order",
                        }
                    };

                    productApi.AddMetafield(product.id, newMetaParent);
                }
            }

        }

        public static void CopyShoppingFeedMetadata(
                    ILifetimeScope scope,
                    long sourceProductId,
                    long targetProductId,
                    string @namespace)
        {
            var logger = scope.Resolve<IPushLogger>();
            var productApi = scope.Resolve<ProductApi>();
            
            var sourceMetafields =
                productApi
                    .RetrieveProductMetafields(sourceProductId)
                    .DeserializeFromJson<MetafieldList>()
                    .metafields;

            var filteredSourceMetafields =
                sourceMetafields
                    .Where(x => x.@namespace == @namespace)
                    .ToList();

            var targetMetafields =
                productApi
                    .RetrieveProductMetafields(targetProductId)
                    .DeserializeFromJson<MetafieldList>()
                    .metafields;

            foreach (var source in filteredSourceMetafields)
            {                
                var existing = targetMetafields.Find(source.@namespace, source.key);

                if (existing != null)
                {
                    logger.Info($"Updating Metafield => {existing.ToString()} ");

                    var update =
                        MetafieldUpdateParent.Make(
                            existing.id, existing.value_type, existing.value);

                    productApi.UpdateMetafield(targetProductId, update);
                }
                else
                {
                    var newMeta = 
                        MetafieldRead.MakeForInsert(
                            source.@namespace,
                            source.key,
                            source.value_type,
                            source.value);

                    logger.Info($"Add Metafield => {newMeta.metafield} ");
                    productApi.AddMetafield(targetProductId, newMeta);
                }
            }
        }
    }
}

