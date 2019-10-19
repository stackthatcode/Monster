﻿using System;
using Monster.Acumatica.Http;
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
        private readonly AcumaticaCustomerGet _acumaticaCustomerPull;
        private readonly AcumaticaOrderGet _acumaticaOrderPull;
        private readonly AcumaticaShipmentGet _acumaticaShipmentPull;
        private readonly IPushLogger _logger;


        public AcumaticaManager(
                AcumaticaHttpContext acumaticaHttpContext,
                AcumaticaReferenceGet acumaticaReferencePull,
                AcumaticaCustomerGet acumaticaCustomerPull, 
                AcumaticaOrderGet acumaticaOrderPull,                 
                AcumaticaShipmentGet acumaticaShipmentPull, 
                AcumaticaWarehouseGet acumaticaWarehousePull, 
                AcumaticaInventoryGet acumaticaInventoryPull,                 
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
