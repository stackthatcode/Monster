namespace Push.Shopify.HttpClient
{
    public class ShopDomain
    {
        private readonly string _domain;

        public string BaseUrl => $"https://{_domain}";
        
        public ShopDomain(string domain)
        {
            _domain = domain;
        }

        public static ShopDomain MakeFromShopName(string shopName)
        {
            return new ShopDomain($"{shopName}.myshopify.com");
        }
    }
}

