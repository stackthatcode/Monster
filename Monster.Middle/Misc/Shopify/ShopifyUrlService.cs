﻿using Monster.Middle.Misc.External;

namespace Monster.Middle.Misc.Shopify
{
    public class ShopifyUrlService
    {
        private readonly CredentialsRepository _connectionRepository;

        public ShopifyUrlService(CredentialsRepository connectionRepository)
        {
            _connectionRepository = connectionRepository;
        }

        public string ShopifyOrderUrl(long id)
        {
            var shopifyCredentials = _connectionRepository.RetrieveShopifyCredentials();
            return $"{shopifyCredentials.Domain.BaseUrl}/admin/orders/{id}";
        }
        public string ShopifyCustomerUrl(long id)
        {
            var shopifyCredentials = _connectionRepository.RetrieveShopifyCredentials();
            return $"{shopifyCredentials.Domain.BaseUrl}/admin/customers/{id}";
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

    }
}
