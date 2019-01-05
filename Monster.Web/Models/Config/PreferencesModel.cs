using System;

namespace Monster.Web.Models.Config
{
    public class PreferencesModel
    {
        public DateTime ShopifyOrderDateStart { get; set; }
        public string 
            ShopifyOrderDateStartFormatted 
                => ShopifyOrderDateStart.Date.ToString("MM/dd/yyyy");
        public string AcumaticaTimeZone { get; set; }

        [Obsolete("Maybe/maybe not")]
        public string ShopifyOrderNumberStart { get; set; }
        [Obsolete]
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