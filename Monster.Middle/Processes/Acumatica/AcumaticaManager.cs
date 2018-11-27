using Monster.Middle.Processes.Acumatica.Workers;

namespace Monster.Middle.Processes.Acumatica
{
    public class AcumaticaManager
    {
        private readonly AcumaticaWarehousePull _acumaticaWarehousePull;
        private readonly AcumaticaInventoryPull _acumaticaInventoryPull;

        private readonly AcumaticaCustomerPull _acumaticaCustomerPull;
        private readonly AcumaticaOrderPull _acumaticaOrderPull;
        private readonly AcumaticaShipmentPull _acumaticaShipmentPull;

        public AcumaticaManager(
                AcumaticaCustomerPull acumaticaCustomerPull, 
                AcumaticaOrderPull acumaticaOrderPull, 
                AcumaticaShipmentPull acumaticaShipmentPull, 
                AcumaticaWarehousePull acumaticaWarehousePull, 
                AcumaticaInventoryPull acumaticaInventoryPull)
        {
            _acumaticaCustomerPull = acumaticaCustomerPull;
            _acumaticaOrderPull = acumaticaOrderPull;
            _acumaticaShipmentPull = acumaticaShipmentPull;
            _acumaticaWarehousePull = acumaticaWarehousePull;
            _acumaticaInventoryPull = acumaticaInventoryPull;
        }

        public void PullWarehouses()
        {
            _acumaticaWarehousePull.Run();
        }

        public void PullInventory()
        {
            _acumaticaInventoryPull.RunAutomatic();
        }

        public void PullCustomerAndOrdersAndShipments()
        {
            _acumaticaCustomerPull.RunAutomatic();
            _acumaticaOrderPull.RunAutomatic();
            _acumaticaShipmentPull.RunAutomatic();
        }
    }
}
