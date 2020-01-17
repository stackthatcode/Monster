using Monster.Middle.Misc.External;

namespace Monster.Middle.Misc.Acumatica
{
    public class AcumaticaUrlService
    {
        private readonly CredentialsRepository _connectionRepository;

        public AcumaticaUrlService(CredentialsRepository connectionRepository)
        {
            _connectionRepository = connectionRepository;
        }


        public string AcumaticaStockItemUrl(string id)
        {
            var acumaticaCredentials = _connectionRepository.RetrieveAcumaticaCredentials();
            return $"{acumaticaCredentials.InstanceUrl}/Main?ScreenId=IN202500&InventoryCD={id}";
        }

        public string AcumaticaCustomerUrl(string id)
        {
            var acumaticaCredentials = _connectionRepository.RetrieveAcumaticaCredentials();
            return $"{acumaticaCredentials.InstanceUrl}//Main?ScreenId=AR303000&AcctCD={id}";
        }

        public string AcumaticaSalesOrderUrl(string orderType, string refNbr)
        {
            var acumaticaCredentials = _connectionRepository.RetrieveAcumaticaCredentials();
            return $"{acumaticaCredentials.InstanceUrl}/Main" 
                   + $"?ScreenId=SO301000&OrderType={orderType}&OrderNbr={refNbr}";
        }

        public string AcumaticaPaymentUrl(string paymentType, string refNbr)
        {
            var acumaticaCredentials = _connectionRepository.RetrieveAcumaticaCredentials();
            return $"{acumaticaCredentials.InstanceUrl}/Main"
                   + $"?ScreenId=AR302000&DocType={paymentType}&RefNbr={refNbr}";
        }
        public string AcumaticaInvoiceUrl(string docType, string refNbr)
        {
            var acumaticaCredentials = _connectionRepository.RetrieveAcumaticaCredentials();
            return $"{acumaticaCredentials.InstanceUrl}/Main"
                   + $"?ScreenId=AR301000&DocType={docType}&RefNbr={refNbr}";
        }

        public string AcumaticaShipmentUrl(string shipmentNbr)
        {
            var acumaticaCredentials = _connectionRepository.RetrieveAcumaticaCredentials();
            return $"{acumaticaCredentials.InstanceUrl}/Main" 
                + $"?ScreenId=SO302000&ShipmentNbr={shipmentNbr}";
        }
    }
}
