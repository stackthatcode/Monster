namespace Monster.Web.Plumbing
{
    public static class UtilityExtensions
    {
        public static string ShopNameOnly(this string shopNameOrUrl)
        {
            return shopNameOrUrl
                .Replace("https://", "")
                .Replace("http://", "")
                .Replace(".myshopify.com", "");
        }

        public static string CorrectedShopUrl(this string shopNameOrUrl)
        {
            var shopName = shopNameOrUrl.ShopNameOnly();
            return $"{shopName}.myshopify.com";
        }
    }
}
