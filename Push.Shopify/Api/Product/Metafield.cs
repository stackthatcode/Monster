namespace Push.Shopify.Api.Product
{
    public class Metafield
    {
        public string @namespace { get; set; }
        public string key { get; set; }
        public string value { get; set; }
        public string value_type { get; set; }

        public override string ToString()
        {
            return $"Metafield - namespace:{@namespace} - " +
                   $"key:{@namespace} - " +
                   $"value_type:{value_type} - " +
                   $"value:{value}";
        }
    }

    public class MetafieldParent
    {
        public Metafield metafield { get; set; }
    }
}
