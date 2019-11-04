using Monster.Middle.Misc.External;

namespace Monster.Middle.Misc.Acumatica
{
    public class AcumaticaUrlService
    {
        private readonly ExternalServiceRepository _connectionRepository;

        public AcumaticaUrlService(ExternalServiceRepository connectionRepository)
        {
            _connectionRepository = connectionRepository;
        }


        public string AcumaticaStockItemUrl(string id)
        {
            var acumaticaCredentials = _connectionRepository.RetrieveAcumaticaCredentials();
            return $"{acumaticaCredentials.InstanceUrl}/Main?ScreenId=IN202500&InventoryCD={id}";
        }

        public string AcumaticaSalesOrderUrl(string orderType, string refNbr)
        {
            var acumaticaCredentials = _connectionRepository.RetrieveAcumaticaCredentials();
            return $"{acumaticaCredentials.InstanceUrl}Main" 
                   + $"?ScreenId=SO301000&OrderType={orderType}&OrderNbr={refNbr}";
        }
    }
}
