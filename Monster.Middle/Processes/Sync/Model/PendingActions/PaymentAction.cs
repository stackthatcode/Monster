﻿using Push.Foundation.Utilities.Validation;


namespace Monster.Middle.Processes.Sync.Model.PendingActions
{
    public class PaymentAction
    {
        public long ShopifyTransactionId { get; set; }
        public string AcumaticaPaymentDocType { get; set; }
        public string AcumaticaPaymentRef { get; set; }
        public string AcumaticaHref { get; set; }

        public string TransDesc { get; set; }
        public decimal Amount { get; set; }

        public bool HasShipping { get; set; }
        public decimal Shipping { get; set; }
        public decimal ShippingTax { get; set; }

        public string PaymentGateway { get; set; }

        public ActionCode ActionCode { get; set; }
        public ValidationResult Validation { get; set; }

        // Computed helpers
        //
        public string ActionDesc => ActionCode.Description();
        public bool IsValid => Validation.Success;
        public bool IsManualApply => ActionCode == ActionCode.NeedManualApply;

        public PaymentAction()
        {
            Validation = new ValidationResult();
            ActionCode = ActionCode.None;
        }
    }
}
