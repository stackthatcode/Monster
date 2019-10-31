using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Sync.Model.Settings
{
    public class PaymentGatewaySelectionModel
    {
        public string ShopifyGatewayId { get; set; }
        public string AcumaticaPaymentMethod { get; set; }
        public string AcumaticaCashAcount { get; set; }

        public PaymentGatewaySelectionModel(PaymentGateway input)
        {
            ShopifyGatewayId = input.ShopifyGatewayId;
            AcumaticaPaymentMethod = input.AcumaticaPaymentMethod;
            AcumaticaPaymentMethod = input.AcumaticaCashAccount;
        }
    }
}
