using System;
using System.Linq;
using System.Transactions;
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
            return RetrieveJson(ShopifyJsonType.Order, shopifyId).DeserializeFromJson<Order>();
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
                var json = RetrieveExternalJson(shopifyJsonType, shopifyId);
                record.Json = json;
                record.LastAccessed = DateTime.UtcNow;
                Entities.SaveChanges();
                return json;
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

        private string RetrieveExternalJson(int shopifyJsonType, long shopifyId)
        {
            if (shopifyJsonType == ShopifyJsonType.Location)
            {
                return _inventoryApi.RetrieveLocation(shopifyId);
            }
            if (shopifyJsonType == ShopifyJsonType.Customer)
            {
                return _customerApi.Retrieve(shopifyId);
            }
            if (shopifyJsonType == ShopifyJsonType.Product)
            {
                return _productApi.RetrieveProduct(shopifyId);
            }
            if (shopifyJsonType == ShopifyJsonType.Variant)
            {
                return _productApi.RetrieveVariant(shopifyId);
            }
            if (shopifyJsonType == ShopifyJsonType.Order)
            {
                return _orderApi.Retrieve(shopifyId);
            }
            if (shopifyJsonType == ShopifyJsonType.Transaction)
            {
                return _orderApi.RetrieveTransactions(shopifyId);
            }

            throw new NotImplementedException();
        }
    }
}
