using Monster.Acumatica.Http;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Processes.Orders.Workers;

namespace Monster.Middle.Processes.Orders
{
    public class OrderManager
    {
        private readonly BatchStateRepository _batchStateRepository;
        private readonly ShopifyCustomerPull _shopifyCustomerPull;
        private readonly ShopifyOrderPull _shopifyOrderPull;
        private readonly AcumaticaHttpContext _acumaticaContext;
        private readonly AcumaticaCustomerPull _acumaticaCustomerPull;
        private readonly AcumaticaOrderPull _acumaticaOrderPull;
        private readonly AcumaticaCustomerSync _acumaticaCustomerSync;
        private readonly AcumaticaOrderSync _acumaticaOrderSync;

        public OrderManager(
                BatchStateRepository batchStateRepository,
                ShopifyCustomerPull shopifyCustomerPull, 
                ShopifyOrderPull shopifyOrderPull, 
                AcumaticaCustomerPull acumaticaCustomerPull,
                AcumaticaOrderPull acumaticaOrderPull, 
                AcumaticaHttpContext acumaticaContext, 
                AcumaticaCustomerSync acumaticaCustomerSync, 
                AcumaticaOrderSync acumaticaOrderSync)
        {
            _batchStateRepository = batchStateRepository;
            _shopifyCustomerPull = shopifyCustomerPull;
            _shopifyOrderPull = shopifyOrderPull;
            _acumaticaCustomerPull = acumaticaCustomerPull;
            _acumaticaOrderPull = acumaticaOrderPull;
            _acumaticaContext = acumaticaContext;
            _acumaticaCustomerSync = acumaticaCustomerSync;
            _acumaticaOrderSync = acumaticaOrderSync;
        }


        public void Baseline()
        {
            _batchStateRepository.ResetOrderBatchState();

            _shopifyCustomerPull.RunAll();
            _shopifyOrderPull.RunAll();

            // Optional...
            _acumaticaContext.Begin();

            _acumaticaCustomerPull.RunAll();
            _acumaticaOrderPull.RunAll();
            _acumaticaCustomerSync.RunMatch();
        }

        public void Incremental()
        {
            _shopifyCustomerPull.RunUpdated();
            _shopifyOrderPull.RunUpdated();

            _acumaticaContext.Begin();
            _acumaticaOrderPull.RunUpdated();

            _acumaticaOrderSync.Run();
        }
    }
}

