using Monster.Middle.Persist.Multitenant;

namespace Monster.Middle.Processes.Sync.Status
{
    public class UrlService
    {
        private readonly ConnectionRepository _connectionRepository;

        public UrlService(ConnectionRepository connectionRepository)
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
    }
}
