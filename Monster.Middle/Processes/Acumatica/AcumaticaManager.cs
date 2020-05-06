using Monster.Acumatica.Http;
using Monster.Middle.Misc.Logging;
using Monster.Middle.Misc.State;
using Monster.Middle.Processes.Acumatica.Workers;
using Push.Foundation.Utilities.Logging;


namespace Monster.Middle.Processes.Acumatica
{
    public class AcumaticaManager
    {
        private readonly AcumaticaHttpContext _acumaticaHttpContext;
        private readonly AcumaticaReferenceGet _acumaticaReferencePull;
        private readonly AcumaticaWarehouseGet _acumaticaWarehousePull;
        private readonly AcumaticaInventoryGet _acumaticaInventoryPull;
        private readonly ExecutionLogService _executionLogService;
        private readonly AcumaticaCustomerGet _acumaticaCustomerPull;
        private readonly AcumaticaOrderGet _acumaticaOrderPull;


        public AcumaticaManager(
                AcumaticaHttpContext acumaticaHttpContext,
                AcumaticaReferenceGet acumaticaReferencePull,
                AcumaticaCustomerGet acumaticaCustomerPull, 
                AcumaticaOrderGet acumaticaOrderPull, 
                AcumaticaWarehouseGet acumaticaWarehousePull, 
                AcumaticaInventoryGet acumaticaInventoryPull,  
                StateRepository stateRepository,
                ExecutionLogService executionLogService,
                IPushLogger logger)
        {
            _acumaticaHttpContext = acumaticaHttpContext;
            _acumaticaCustomerPull = acumaticaCustomerPull;
            _acumaticaOrderPull = acumaticaOrderPull;
            _acumaticaWarehousePull = acumaticaWarehousePull;
            _acumaticaInventoryPull = acumaticaInventoryPull;
            _executionLogService = executionLogService;
            _acumaticaReferencePull = acumaticaReferencePull;
        }


        public void TestConnection()
        {
            _executionLogService.Log("Testing Acumatica Connection");
            _acumaticaHttpContext.SessionRun(() => { }, throwException:true);
        }

        public void PullReferenceData()
        {
            _acumaticaHttpContext.SessionRun(() =>
            {
                _acumaticaReferencePull.RunItemClass();
                _acumaticaReferencePull.RunPaymentMethod();
                _acumaticaReferencePull.RunTaxCategories();
                _acumaticaReferencePull.RunTaxIds();
                _acumaticaReferencePull.RunTaxZones();
                _acumaticaReferencePull.RunCustomerClasses();
                _acumaticaReferencePull.RunShipVia();
            }, throwException:true);
        }
        
        public void PullWarehouses()
        {
            _acumaticaHttpContext.SessionRun(() => _acumaticaWarehousePull.Run());
        }

        public void PullInventory()
        {
            _acumaticaHttpContext.SessionRun(() => _acumaticaInventoryPull.RunAutomatic());
        }

        public void PullOrdersAndCustomer()
        {
            _acumaticaHttpContext.
                SessionRun(() =>
                {
                    _acumaticaCustomerPull.RunAutomatic();
                    _acumaticaOrderPull.RunAutomatic();
                });
        }

    }
}
