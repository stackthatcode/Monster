using Push.Foundation.Utilities.Validation;

namespace Monster.Middle.Processes.Sync.Model.PendingActions
{
    public class OrderAction
    {
        public ActionCode ActionCode { get; set; }
        public ValidationResult CreateOrderValidation { get; set; }
        public ValidationResult UpdateOrderValidation { get; set; }

        public string ActionDesc => ActionCode.Description();
        public bool IsValid => CreateOrderValidation.Success;
        
        public OrderAction()
        {
            ActionCode = ActionCode.None;
            CreateOrderValidation = new ValidationResult();
        }
    }
}

