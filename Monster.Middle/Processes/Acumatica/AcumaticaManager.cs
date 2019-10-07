using System;
using Monster.Acumatica.Http;
using Monster.Middle.Processes.Acumatica.Workers;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Processes.Acumatica
{
    public class AcumaticaManager
    {
        private readonly AcumaticaHttpContext _acumaticaHttpContext;
        private readonly AcumaticaReferencePull _acumaticaReferencePull;
        private readonly AcumaticaWarehousePull _acumaticaWarehousePull;
        private readonly AcumaticaInventoryPull _acumaticaInventoryPull;
        private readonly AcumaticaCustomerPull _acumaticaCustomerPull;
        private readonly AcumaticaOrderPull _acumaticaOrderPull;
        private readonly AcumaticaShipmentPull _acumaticaShipmentPull;
        private readonly IPushLogger _logger;


        public AcumaticaManager(
                AcumaticaHttpContext acumaticaHttpContext,
                AcumaticaReferencePull acumaticaReferencePull,
                AcumaticaCustomerPull acumaticaCustomerPull, 
                AcumaticaOrderPull acumaticaOrderPull,                 
                AcumaticaShipmentPull acumaticaShipmentPull, 
                AcumaticaWarehousePull acumaticaWarehousePull, 
                AcumaticaInventoryPull acumaticaInventoryPull,                 
                IPushLogger logger)
        {
            _acumaticaHttpContext = acumaticaHttpContext;
            _acumaticaCustomerPull = acumaticaCustomerPull;
            _acumaticaOrderPull = acumaticaOrderPull;
            _acumaticaShipmentPull = acumaticaShipmentPull;
            _acumaticaWarehousePull = acumaticaWarehousePull;
            _acumaticaInventoryPull = acumaticaInventoryPull;
            _acumaticaReferencePull = acumaticaReferencePull;
            _logger = logger;
        }


        public void TestConnection()
        {
            _acumaticaHttpContext.SessionRun(() => {});
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
            });
        }
        
        public void PullWarehouses()
        {
            _acumaticaHttpContext.SessionRun(() => _acumaticaWarehousePull.Run());
        }

        public void PullInventory()
        {
            _acumaticaHttpContext.SessionRun(() => _acumaticaInventoryPull.RunAutomatic());
        }

        public void PullOrdersCustomerShipments()
        {
            _acumaticaHttpContext.
                SessionRun(() =>
                {
                    _acumaticaCustomerPull.RunAutomatic();
                    _acumaticaOrderPull.RunAutomatic();
                    _acumaticaShipmentPull.RunAutomatic();
                });
        }

    }
}
