﻿using System;
using System.Collections.Generic;
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
            _acumaticaHttpContext.Login();
            _acumaticaHttpContext.Logout();
        }

        public void PullReferenceData()
        {
            SessionRun(
                () => _acumaticaReferencePull.RunItemClass(),
                () => _acumaticaReferencePull.RunPaymentMethod(),
                () => _acumaticaReferencePull.RunTaxCategories(),
                () => _acumaticaReferencePull.RunTaxIds(),
                () => _acumaticaReferencePull.RunTaxZones());
        }
        
        public void PullWarehouses()
        {
            SessionRun(() => _acumaticaWarehousePull.Run());
        }

        public void PullInventory()
        {
            SessionRun(() => _acumaticaInventoryPull.RunAutomatic());
        }

        public void PullOrderAndCustomersAndShipments()
        {
            SessionRun(
                () => _acumaticaCustomerPull.RunAutomatic(),
                () => _acumaticaOrderPull.RunAutomatic(),
                () => _acumaticaShipmentPull.RunAutomatic());
        }


        public bool SessionRun(Action action)
        {
            try
            {
                if (!_acumaticaHttpContext.IsLoggedIn)
                {
                    _acumaticaHttpContext.Login();
                }

                action();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }
            finally
            {
                if (_acumaticaHttpContext.IsLoggedIn)
                {
                    _acumaticaHttpContext.Logout();
                }
            }
        }

        public bool SessionRun(params Action[] actions)
        {
            var faultDetected = false;
            foreach (var action in actions)
            {
                try
                {
                    if (!_acumaticaHttpContext.IsLoggedIn)
                    {
                        _acumaticaHttpContext.Login();
                    }

                    action();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    faultDetected = true;
                }
            }

            if (_acumaticaHttpContext.IsLoggedIn)
            {
                _acumaticaHttpContext.Logout();
            }

            return faultDetected;
        }
    }
}
