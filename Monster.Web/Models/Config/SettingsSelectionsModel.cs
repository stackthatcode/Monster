using System.Collections.Generic;
using Monster.Middle.Processes.Sync.Model.Settings;

namespace Monster.Web.Models.Config
{
    public class SettingsSelectionsModel
    {
        public string AcumaticaTimeZone { get; set; }
        public string AcumaticaDefaultItemClass { get; set; }
        public string AcumaticaDefaultPostingClass { get; set; }
        public string AcumaticaDefaultCustomerClass { get; set; }
        public List<PaymentGatewaySelectionModel> PaymentGateways { get; set; }
        public List<CarrierToShipViaSelection> RateToShipVias { get; set; }
        
        public SettingsSelectionsModel()
        {
            PaymentGateways = new List<PaymentGatewaySelectionModel>();
            RateToShipVias = new List<CarrierToShipViaSelection>();
        }
    }
}

