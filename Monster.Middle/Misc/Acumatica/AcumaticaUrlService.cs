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
    }
}
