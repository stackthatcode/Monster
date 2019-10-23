﻿using Monster.Middle.Misc.External;
using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Utility
{
    public class UrlService
    {
        private readonly ExternalServiceRepository _connectionRepository;

        public UrlService(ExternalServiceRepository connectionRepository)
        {
            _connectionRepository = connectionRepository;
        }

        public string ShopifyOrderUrl(long id)
        {
            var shopifyCredentials = _connectionRepository.RetrieveShopifyCredentials();
            return $"{shopifyCredentials.Domain.BaseUrl}/admin/orders/{id}";
        }

        public string ShopifyProductUrl(long id)
        {
            var shopifyCredentials = _connectionRepository.RetrieveShopifyCredentials();
            return $"{shopifyCredentials.Domain.BaseUrl}/admin/products/{id}";
        }

        public string ShopifyVariantUrl(long product_id, long variant_id)
        {
            var shopifyCredentials = _connectionRepository.RetrieveShopifyCredentials();
            return $"{shopifyCredentials.Domain.BaseUrl}" +
                        $"/admin/products/{product_id}/variants/{variant_id}";
        }


        public string AcumaticaStockItemUrl(string id)
        {
            var acumaticaCredentials = _connectionRepository.RetrieveAcumaticaCredentials();
            return $"{acumaticaCredentials.InstanceUrl}/Main?ScreenId=IN202500&InventoryCD={id}";
        }
    }
}