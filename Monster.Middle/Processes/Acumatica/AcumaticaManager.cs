using Monster.Acumatica.Http;
using Monster.Middle.Processes.Acumatica.Workers;

namespace Monster.Middle.Processes.Acumatica
{
    public class AcumaticaManager
    {
        private readonly AcumaticaHttpContext _acumaticaHttpContext;

        private readonly AcumaticaWarehousePull _acumaticaWarehousePull;
        private readonly AcumaticaInventoryPull _acumaticaInventoryPull;

        private readonly AcumaticaCustomerPull _acumaticaCustomerPull;
        private readonly AcumaticaOrderPull _acumaticaOrderPull;
        private readonly AcumaticaShipmentPull _acumaticaShipmentPull;

        public AcumaticaManager(
                AcumaticaHttpContext acumaticaHttpContext,
                AcumaticaCustomerPull acumaticaCustomerPull, 
                AcumaticaOrderPull acumaticaOrderPull, 
                AcumaticaShipmentPull acumaticaShipmentPull, 
                AcumaticaWarehousePull acumaticaWarehousePull, 
                AcumaticaInventoryPull acumaticaInventoryPull)
        {
            _acumaticaHttpContext = acumaticaHttpContext;
            _acumaticaCustomerPull = acumaticaCustomerPull;
            _acumaticaOrderPull = acumaticaOrderPull;
            _acumaticaShipmentPull = acumaticaShipmentPull;
            _acumaticaWarehousePull = acumaticaWarehousePull;
            _acumaticaInventoryPull = acumaticaInventoryPull;
        }

        public void PullWarehouses()
        {
            _acumaticaHttpContext.Login();

            _acumaticaWarehousePull.Run();

            _acumaticaHttpContext.Logout();
        }

        public void PullInventory()
        {
            _acumaticaHttpContext.Login();

            _acumaticaInventoryPull.RunAutomatic();

            _acumaticaHttpContext.Logout();
        }

        public void PullCustomerAndOrdersAndShipments()
        {
            _acumaticaHttpContext.Login();

            _acumaticaCustomerPull.RunAutomatic();
            _acumaticaOrderPull.RunAutomatic();
            _acumaticaShipmentPull.RunAutomatic();

            _acumaticaHttpContext.Logout();
        }
    }
}
