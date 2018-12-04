using AutoMapper;
using Monster.Middle.Persist.Multitenant;

namespace Monster.Web.Models
{
    public class Preferences
    {
        public string DataPullStart { get; set; } // DataPullStart
        public string ShopifyOrderPushStart { get; set; } // ShopifyOrderPushStart
        public string DefaultCoGsMargin { get; set; } // DefaultCoGSMargin

        public string AcumaticaDefaultItemClass { get; set; } // AcumaticaDefaultItemClass (length: 50)
        public string AcumaticaDefaultPostingClass { get; set; } // AcumaticaDefaultPostingClass (length: 50)
        public string AcumaticaPostingDate { get; set; } // AcumaticaPostingDate
        public string AcumaticaTimeZone { get; set; } // AcumaticaTimeZone (length: 50)
        public string AcumaticaPaymentMethod { get; set; } // AcumaticaPaymentMethod (length: 50)
        public string AcumaticaPaymentCashAccount { get; set; } // AcumaticaPaymentCashAccount (length: 50)
        public string AcumaticaTaxZone { get; set; } // AcumaticaTaxZone (length: 50)
        public string AcumaticaTaxCategory { get; set; } // AcumaticaTaxCategory (length: 50)
        public string AcumaticaTaxId { get; set; } // AcumaticaTaxId (length: 50)
        public bool FulfillmentInAcumatica { get; set; }

        public string FulfillmentSystem => FulfillmentInAcumatica ? "ACUMATICA" : "SHOPIFY";
    }
}