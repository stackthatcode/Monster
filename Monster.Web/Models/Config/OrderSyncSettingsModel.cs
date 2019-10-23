using System;
using System.Globalization;
using Push.Foundation.Utilities.Helpers;

namespace Monster.Web.Models.Config
{
    public class OrderSyncSettingsModel
    {
        public long? StartingShopifyOrderId { get; set; }
        public string StartingShopifyOrderIdFormatted 
                => StartingShopifyOrderId.HasValue ? StartingShopifyOrderId.ToString() : "(not set)";

        public string ShopifyOrderHref { get; set; }

        public bool IsSet => StartingShopifyOrderId.HasValue;


        public string StartingShopifyOrderName { get; set; }
        public string StartingShopifyOrderNameFormatted 
                => StartingShopifyOrderName.IsNullOrEmptyAlt("(not set)");


        public DateTime? StartingShopifyOrderCreatedAtUtc { get; set; }
        public string StartingShopifyOrderCreatedAtUtcFormatted
                => StartingShopifyOrderCreatedAtUtc != null
                    ? StartingShopifyOrderCreatedAtUtc.Value.ToString(CultureInfo.InvariantCulture) 
                    : "(not set)";

        public int MaxParallelAcumaticaSyncs { get; set; }
    }
}