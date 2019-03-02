using System;
using Monster.Acumatica.Http;
using Push.Foundation.Utilities.Logging;

namespace Monster.Middle.Processes.Acumatica.Workers
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
            var sequence = new Action[]
            {
                () => _acumaticaReferencePull.RunItemClass(),
                () => _acumaticaReferencePull.RunPaymentMethod(),
                () => _acumaticaReferencePull.RunTaxCategories(),
                () => _acumaticaReferencePull.RunTaxIds(),
                () => _acumaticaReferencePull.RunTaxZones()
            };
            
            SessionRun(sequence);
        }
        
        public void PullWarehouses()
        {
            SessionRun(() => _acumaticaWarehousePull.Run());
        }

        public void PullInventory()
        {
            SessionRun(() => _acumaticaInventoryPull.RunAutomatic());
        }

        public void PullOrdersAndCustomersAndShipments()
        {
            var sequence = new Action[]
            {
                () => _acumaticaCustomerPull.RunAutomatic(),
                () => _acumaticaOrderPull.RunAutomatic(),
                () => _acumaticaShipmentPull.RunAutomatic()
            };

            SessionRun(sequence, throwException:false);
        }


        public void SessionRun(Action action, bool throwException = true)
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
                if (throwException)
                {
                    throw;
                }
                _logger.Error(ex);
            }
            finally
            {
                if (_acumaticaHttpContext.IsLoggedIn)
                {
                    _acumaticaHttpContext.Logout();
                }
            }
        }

        public void SessionRun(Action[] actions, bool throwException = true)
        {
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
                    if (throwException)
                    {
                        throw;
                    }

                    _logger.Error(ex);
                }
            }

            if (_acumaticaHttpContext.IsLoggedIn)
            {
                _acumaticaHttpContext.Logout();
            }
        }
    }
}

