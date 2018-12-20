namespace Monster.Web.Models.ShopifyAuth
{
    public class DomainModel
    {
        public bool IsShopifyConnectionBroken { get; set; }
        public bool IsRandomAccessMode { get; set; }
        public bool IsShopifyUrlFinalized { get; set; }
        public string ShopDomain { get; set; }
    }
}

