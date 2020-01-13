using Push.Foundation.Utilities.Validation;

namespace Monster.Middle.Processes.Sync.Model.PendingActions
{
    public class OrderAction
    {
        public string ShopifyOrderName { get; set; }
        public long ShopifyOrderId { get; set; }
        public string ShopifyOrderHref { get; set; }
        public string AcumaticaSalesOrderNbr { get; set; }
        public string AcumaticaSalesOrderHref { get; set; }

        public ActionCode ActionCode { get; set; }
        public string ActionDesc => ActionCode.Description();

        public ValidationResult Validation { get; set; }
        public bool IsValid => Validation.Success;
        
        public OrderAction()
        {
            ActionCode = ActionCode.None;
            Validation = new ValidationResult();
        }
    }
}

