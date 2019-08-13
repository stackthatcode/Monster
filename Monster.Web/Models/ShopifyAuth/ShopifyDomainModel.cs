namespace Monster.Web.Models.ShopifyAuth
{
    public class ShopifyDomainModel
    {
        public bool IsConnectionBroken { get; set; }
        public bool IsWizardMode { get; set; }
        public bool CanEditShopifyUrl { get; set; }
        public string ShopDomain { get; set; }
    }
}

