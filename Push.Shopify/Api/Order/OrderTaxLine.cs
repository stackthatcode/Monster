using Newtonsoft.Json;

namespace Push.Shopify.Api.Order
{
    public class OrderTaxLine
    {
        [JsonIgnore]
        public Order Parent { get; set; }

        public decimal price { get; set; }
        public decimal rate { get; set; }
        public string title { get; set; }

        public decimal PercentOfTotalTaxes => price / Parent.TaxLinesTotal;
    }
}
