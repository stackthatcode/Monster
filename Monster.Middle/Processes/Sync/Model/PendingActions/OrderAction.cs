using Push.Foundation.Utilities.Validation;

namespace Monster.Middle.Processes.Sync.Model.PendingActions
{
    public class OrderAction
    {
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

