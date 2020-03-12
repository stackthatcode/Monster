using System;
using System.Collections.Generic;
using System.Linq;
using Monster.Middle.Persist.Instance;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api;
using Push.Shopify.Api.Customer;
using Push.Shopify.Api.Order;
using Push.Shopify.Api.Product;

namespace Monster.Middle.Processes.Shopify.Persist
{
    public class ShopifyJsonService
    {
        private readonly ProcessPersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        private readonly OrderApi _orderApi;
        private readonly ProductApi _productApi;
        private readonly InventoryApi _inventoryApi;
        private readonly CustomerApi _customerApi;

        public ShopifyJsonService(
                OrderApi orderApi, 
                ProductApi productApi, 
                InventoryApi inventoryApi, 
                CustomerApi customerApi, 
                ProcessPersistContext dataContext)
        {
            _orderApi = orderApi;
            _productApi = productApi;
            _inventoryApi = inventoryApi;
            _customerApi = customerApi;
            _dataContext = dataContext;
        }



        public void Upsert(int shopifyJsonType, long shopifyId, string json)
        {
            var record = RetrieveRecordOnly(shopifyJsonType, shopifyId);
            record.Json = json;
            Entities.SaveChanges();
        }

        public Variant RetrieveVariant(long shopifyId)
        {
            return RetrieveJson(ShopifyJsonType.Variant, shopifyId).DeserializeFromJson<Variant>();
        }

        public Product RetrieveProduct(long shopifyId)
        {
            return RetrieveJson(ShopifyJsonType.Product, shopifyId).DeserializeFromJson<Product>();
        }

        public Order RetrieveOrder(long shopifyId)
        {
            return RetrieveJson(ShopifyJsonType.Order, shopifyId).DeserializeToOrder();
        }

        public Push.Shopify.Api.Transactions.Transaction RetrieveTransaction(long shopifyId)
        {
            return RetrieveJson(ShopifyJsonType.Transaction, shopifyId)
                    .DeserializeFromJson<Push.Shopify.Api.Transactions.Transaction>();
        }

        public Customer RetrieveCustomer(long shopifyId)
        {
            return RetrieveJson(ShopifyJsonType.Customer, shopifyId).DeserializeFromJson<Customer>();
        }

        public string RetrieveJson(int shopifyJsonType, long shopifyId, bool rehydrate = true)
        {
            var record = RetrieveRecordOnly(shopifyJsonType, shopifyId);

            if (record.Json != null)
            {
                return record.Json;
            }
            else
            {
                if (rehydrate == true)
                {
                    HydrateExternalJson(shopifyJsonType, shopifyId);
                }

                var rehydratedRecord = RetrieveRecordOnly(shopifyJsonType, shopifyId);
                return rehydratedRecord.Json;
            }
        }

        public ShopifyJsonStore RetrieveRecordOnly(int shopifyJsonType, long shopifyId)
        {
            var output = Entities
                .ShopifyJsonStores
                .FirstOrDefault(x => x.ShopifyJsonType == shopifyJsonType && x.ShopifyId == shopifyId);

            if (output == null)
            {
                var newRecord = new ShopifyJsonStore();
                newRecord.ShopifyJsonType = shopifyJsonType;
                newRecord.ShopifyId = shopifyId;
                newRecord.LastAccessed = DateTime.UtcNow;
                Entities.ShopifyJsonStores.Add(newRecord);
                Entities.SaveChanges();
                return newRecord;
            }
            else
            {
                output.LastAccessed = DateTime.UtcNow;
                Entities.SaveChanges();
                return output;
            }
        }

        public void UpdateRecord(int shopifyJsonType, long shopifyId, string json)
        {
            var record = RetrieveRecordOnly(shopifyJsonType, shopifyId);
            record.Json = json;
            record.LastAccessed = DateTime.UtcNow;
            Entities.SaveChanges();
        }

        private void HydrateExternalJson(int shopifyJsonType, long shopifyId)
        {
            if (shopifyJsonType == ShopifyJsonType.Location)
            {
                var json = _inventoryApi.RetrieveLocation(shopifyId);
                UpdateRecord(shopifyJsonType, shopifyId, json);
            }
            if (shopifyJsonType == ShopifyJsonType.Customer)
            {
                var json = _customerApi.Retrieve(shopifyId);
                UpdateRecord(shopifyJsonType, shopifyId, json);
            }
            if (shopifyJsonType == ShopifyJsonType.Product)
            {
                var json = _productApi.RetrieveProducts(shopifyId);
                UpdateRecord(shopifyJsonType, shopifyId, json);
            }
            if (shopifyJsonType == ShopifyJsonType.Variant)
            {
                var json = _productApi.RetrieveVariant(shopifyId);
                UpdateRecord(shopifyJsonType, shopifyId, json);
            }
            if (shopifyJsonType == ShopifyJsonType.Order)
            {
                var json = _orderApi.Retrieve(shopifyId);
                UpdateRecord(shopifyJsonType, shopifyId, json);
            }
            if (shopifyJsonType == ShopifyJsonType.Transaction)
            {
                var transactions =
                    _orderApi.RetrieveTransactions(shopifyId)
                        .DeserializeFromJson<List<Push.Shopify.Api.Transactions.Transaction>>();

                foreach (var transaction in transactions)
                {
                    UpdateRecord(shopifyJsonType, transaction.id, transaction.SerializeToJson());
                }
            }

            throw new NotImplementedException();
        }
    }
}
