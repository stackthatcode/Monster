namespace Monster.Web.Models.Config
{
    public class PreferencesModel
    {
        public string AcumaticaTimeZone { get; set; }
        public string AcumaticaDefaultItemClass { get; set; }
        public string AcumaticaDefaultPostingClass { get; set; }

        public string AcumaticaPaymentMethod { get; set; }
        public string AcumaticaPaymentCashAccount { get; set; }

        public string AcumaticaTaxZone { get; set; }
        public string AcumaticaTaxCategory { get; set; }
        public string AcumaticaTaxId { get; set; }
    }
}