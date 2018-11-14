using System.Collections.Generic;


namespace Monster.Middle.Processes.Orders.Workers.Model
{
    public class ShipmentSyncReadiness
    {
        public bool OrderIsOnHold { get; set; }
        public List<string> SkuWithInsuffientInventory { get; set; }

        public bool IsReady 
            => !OrderIsOnHold && SkuWithInsuffientInventory.Count == 0;

        public ShipmentSyncReadiness()
        {
            SkuWithInsuffientInventory = new List<string>();
            OrderIsOnHold = false;
        }
    }
}

