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

        public OrderManager(
                BatchStateRepository batchStateRepository,
                ShopifyCustomerPull shopifyCustomerPull, 
                ShopifyOrderPull shopifyOrderPull, 
                AcumaticaCustomerPull acumaticaCustomerPull,
                AcumaticaOrderPull acumaticaOrderPull, 
                AcumaticaHttpContext acumaticaContext)
        {
            _batchStateRepository = batchStateRepository;
            _shopifyCustomerPull = shopifyCustomerPull;
            _shopifyOrderPull = shopifyOrderPull;
            _acumaticaCustomerPull = acumaticaCustomerPull;
            _acumaticaOrderPull = acumaticaOrderPull;
            _acumaticaContext = acumaticaContext;
        }


        public void RunBaseline()
        {
            _batchStateRepository.ResetOrderBatchState();

            _shopifyCustomerPull.RunAll();
            _shopifyOrderPull.RunAll();

            // Optional...
            _acumaticaContext.Begin();

            //_acumaticaCustomerPull.RunAll();
            _acumaticaOrderPull.RunAll();

            // TODO - why not an idempotent function....?
            _acumaticaOrderPull.RunUpdated();
        }

        public void RunUpdate()
        {
            _shopifyCustomerPull.RunUpdated();
            _shopifyOrderPull.RunUpdated();

            _acumaticaContext.Begin();
            _acumaticaOrderPull.RunUpdated();
        }
    }
}

