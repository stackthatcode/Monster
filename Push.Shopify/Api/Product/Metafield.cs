namespace Push.Shopify.Api.Product
{
    public class Metafield
    {
        public string @namespace { get; set; }
        public string key { get; set; }
        public string value { get; set; }
        public string value_type { get; set; }
    }

    public class MetafieldParent
    {
        public Metafield metafield { get; set; }
    }
}
