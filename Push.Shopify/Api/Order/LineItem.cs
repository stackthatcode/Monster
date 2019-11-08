using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Push.Shopify.Api.Order
{
    public class LineItem
    {
        public long id { get; set; }
        public long? variant_id { get; set; }
        public string title { get; set; }
        public int quantity { get; set; }
        public decimal price { get; set; }
        public string sku { get; set; }
        public string variant_title { get; set; }
        public string vendor { get; set; }
        public string fulfillment_service { get; set; }
        public long? product_id { get; set; }
        public bool requires_shipping { get; set; }
        public bool taxable { get; set; }
        public bool gift_card { get; set; }
        public string name { get; set; }
        public string variant_inventory_management { get; set; }
        public List<object> properties { get; set; }
        public bool product_exists { get; set; }
        public int fulfillable_quantity { get; set; }
        public int grams { get; set; }
        public string total_discount { get; set; }
        public string fulfillment_status { get; set; }
        public List<DiscountAllocation> discount_allocations { get; set; }
        public List<TaxLine> tax_lines { get; set; }
        public OriginLocation origin_location { get; set; }
        public DestinationLocation destination_location { get; set; }


        // Computed-ish properties
        [JsonIgnore]
        public Order Parent { get; set; }

        [JsonIgnore]
        public int CancelAdjustedQuantity => quantity - CancelledQuantity;

        [JsonIgnore]
        public int CancelledQuantity => Parent.CancelledLineItems(this.id).Sum(x => x.quantity);


        [JsonIgnore]
        public decimal LineAmount => quantity * price;

        [JsonIgnore]
        public decimal Discount => discount_allocations.Sum(x => x.amount);

        [JsonIgnore]
        public decimal LineAmountAfterDiscount => LineAmount - Discount;

        [JsonIgnore]
        public decimal CancelledAmount => Parent.CancelledLineItems(this.id).Sum(x => x.subtotal);

        [JsonIgnore]
        public decimal NetLineAmount => LineAmountAfterDiscount - CancelledAmount;

        [JsonIgnore]
        public decimal UnitPriceAfterDiscount => LineAmountAfterDiscount / quantity;
            


        [JsonIgnore]
        public decimal Tax => tax_lines.Sum(x => x.price);

        [JsonIgnore]
        public decimal TaxableAmount => taxable ? LineAmountAfterDiscount : 0m;
    }
}
