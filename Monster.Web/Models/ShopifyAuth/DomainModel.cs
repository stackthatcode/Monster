namespace Monster.Web.Models.ShopifyAuth
{
    public class DomainModel
    {
        public bool IsShopifyUrlFinalized { get; set; }
        public bool IsShopifyConnectionOk { get; set; }
        public bool IsShopifyConnectionBroken { get; set; }
        public bool IsRandomAccessMode { get; set; }
        public string ShopDomain { get; set; }

        public bool IsNextButtonEnabled
            => IsShopifyConnectionOk && !IsRandomAccessMode;
    }
}

