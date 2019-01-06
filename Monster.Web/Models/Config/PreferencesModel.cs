using System;

namespace Monster.Web.Models.Config
{
    public class PreferencesModel
    {
        public string AcumaticaTimeZone { get; set; }

        public DateTime? ShopifyOrderDateStart { get; set; }
        public string 
            ShopifyOrderDateStartFormatted 
                => (ShopifyOrderDateStart ?? DateTime.Today).Date.ToString("MM/dd/yyyy");

        public int? ShopifyOrderNumberStart { get; set; }
        
        [Obsolete("Need to use Shopify's CoGS margin data")]
        public string DefaultCoGsMargin { get; set; }
        

        public string AcumaticaDefaultItemClass { get; set; }
        public string AcumaticaDefaultPostingClass { get; set; }

        public string AcumaticaPaymentMethod { get; set; }
        public string AcumaticaPaymentCashAccount { get; set; }

        public string AcumaticaTaxZone { get; set; }
        public string AcumaticaTaxCategory { get; set; }
        public string AcumaticaTaxId { get; set; }

        //public bool FulfillmentInAcumatica { get; set; }

        //public string FulfillmentSystem => FulfillmentInAcumatica ? "ACUMATICA" : "SHOPIFY";
    }
}