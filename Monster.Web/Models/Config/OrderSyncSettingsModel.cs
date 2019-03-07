using System;

namespace Monster.Web.Models.Config
{
    public class OrderSyncSettingsModel
    {
        public DateTime? ShopifyOrderDateStart { get; set; }

        public string ShopifyOrderDateStartFormatted
            => (ShopifyOrderDateStart ?? DateTime.Today).Date.ToString("MM/dd/yyyy");

        public int? ShopifyOrderNumberStart { get; set; }

        public string ShopifyOrderNumberStartFormatted =>
            ShopifyOrderNumberStart.HasValue 
                ? ShopifyOrderNumberStart.Value.ToString() : "(not set)";

        public int MaxParallelAcumaticaSyncs { get; set; }
    }
}