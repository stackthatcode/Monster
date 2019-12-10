namespace Monster.Middle.Processes.Sync.Model.PendingActions
{
    public class ShipmentAction
    {
        public string ShipmentNbr { get; set; }
        public string InvoiceNbr { get; set; }

        public decimal InvoiceAmount { get; set; }
        public decimal InvoiceTax { get; set; }

        public ActionCode ActionCode { get; set; }
        public string ActionDesc => ActionCode.Description();

        public ShipmentAction()
        {
            ActionCode = ActionCode.None;
        }
    }
}
