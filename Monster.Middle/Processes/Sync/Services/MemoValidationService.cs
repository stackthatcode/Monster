using Monster.Middle.Processes.Sync.Model.PendingActions;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Validation;


namespace Monster.Middle.Processes.Sync.Services
{
    public class MemoValidationService
    {
        public void Validate(RootAction rootAction, AdjustmentAction adjustmentAction)
        {
            adjustmentAction.Validation = new ValidationResult();

            if (adjustmentAction.ActionCode == ActionCode.CreateInAcumatica)
            {
                adjustmentAction.Validation = ReadyToCreateMemo(rootAction);
                return;
            }

            if (adjustmentAction.ActionCode == ActionCode.ReleaseInAcumatica)
            {
                adjustmentAction.Validation = ReadyToReleaseMemo(adjustmentAction);
                return;
            }
        }


        private ValidationResult ReadyToCreateMemo(RootAction rootActions)
        {
            var validation = new Validation<RootAction>()
                .Add(x => !x.HasPendingOrderActions, "Cannot create Memo until Order is synced")
                .Add(x => !x.HasPendingPaymentActions, "Cannot create Memo until Payments are synced and/or Released")
                .Add(x => !x.HasPendingRefundActions, "Cannot create Memo until Refund(s) are synced and/or Released");

            return validation.Run(rootActions);
        }

        private ValidationResult ReadyToReleaseMemo(AdjustmentAction adjustmentAction)
        {

            var validation = new Validation<AdjustmentAction>()
                .Add(x => !x.AcumaticaRefNbr.IsNullOrEmpty(), "Cannot Release Memo until it is synced with Acumatica");

            return validation.Run(adjustmentAction);
        }
    }
}
