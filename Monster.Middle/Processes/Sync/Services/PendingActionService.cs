using System;
using System.Collections.Generic;
using Monster.Middle.Misc.Acumatica;
using Monster.Middle.Misc.Shopify;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Acumatica.Persist;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Orders;
using Monster.Middle.Processes.Sync.Model.PendingActions;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Validation;
using Push.Shopify.Api.Order;


namespace Monster.Middle.Processes.Sync.Services
{
    public class PendingActionService
    {
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly OrderValidationService _orderValidation;
        private readonly PaymentValidationService _paymentValidation;
        private readonly FulfillmentStatusService _fulfillmentStatusService;
        private readonly ShopifyUrlService _shopifyUrlService;
        private readonly AcumaticaUrlService _acumaticaUrlService;

        public PendingActionService(
                SyncOrderRepository orderRepository,
                OrderValidationService orderValidation,
                PaymentValidationService paymentValidation,
                FulfillmentStatusService fulfillmentStatusService,
                ShopifyUrlService shopifyUrlService, 
                AcumaticaUrlService acumaticaUrlService)
        {
            _syncOrderRepository = orderRepository;
            _orderValidation = orderValidation;
            _paymentValidation = paymentValidation;
            _fulfillmentStatusService = fulfillmentStatusService;
            _shopifyUrlService = shopifyUrlService;
            _acumaticaUrlService = acumaticaUrlService;
        }


        public RootAction Create(long shopifyOrderId, bool validate = true)
        {
            var orderRecord = _syncOrderRepository.RetrieveShopifyOrderWithNoTracking(shopifyOrderId);

            var output = new RootAction();
            output.OrderAction = BuildOrderPendingAction(orderRecord);
            output.PaymentAction = BuildPaymentActions(orderRecord);
            output.RefundPaymentActions = BuildRefundPaymentActions(orderRecord);
            output.AdjustmentMemoActions = BuildRefundAdjustmentActions(orderRecord);
            output.ShipmentInvoiceActions = BuildShipmentInvoiceActions(orderRecord);

            if (validate)
            {
                _orderValidation.Validate(output.OrderAction);
                _paymentValidation.ValidatePayment(orderRecord, output.PaymentAction);

                foreach (var action in output.RefundPaymentActions)
                {
                    _paymentValidation.ValidateRefundPayment(orderRecord, action);
                }

                foreach (var action in output.ShipmentInvoiceActions)
                {
                    _fulfillmentStatusService.Validate(orderRecord, action);
                }
            }

            return output;
        }

        private OrderAction BuildOrderPendingAction(ShopifyOrder record)
        {
            var output = new OrderAction();

            output.ShopifyOrderId = record.ShopifyOrderId;
            output.ShopifyOrderHref = _shopifyUrlService.ShopifyOrderUrl(record.ShopifyOrderId);
            output.ShopifyOrderName = record.ToShopifyObj().name;

            output.Validation = new ValidationResult();
            output.ActionCode = ActionCode.None;

            if (!record.ExistsInAcumatica())
            {
                var order = record.ToShopifyObj();

                output.ActionCode =
                    !order.IsEmptyOrCancelled 
                        ? ActionCode.CreateInAcumatica : ActionCode.CreateBlankSyncRecord;
            }
            else // Exists in Acumatica
            {
                output.AcumaticaSalesOrderNbr = record.AcumaticaSalesOrder.AcumaticaOrderNbr;
                output.AcumaticaSalesOrderHref
                        = _acumaticaUrlService.AcumaticaSalesOrderUrl(
                                SalesOrderType.SO, record.AcumaticaSalesOrder.AcumaticaOrderNbr);

                if (record.NeedsOrderPut)
                {
                    output.ActionCode = ActionCode.UpdateInAcumatica;
                }
            }

            return output;
        }

        private PaymentAction BuildPaymentActions(ShopifyOrder orderRecord)
        {
            var paymentAction = new PaymentAction();
            paymentAction.ActionCode = ActionCode.None;
            paymentAction.Validation = new ValidationResult();

            if (orderRecord.HasPayment())
            {
                var payment = orderRecord.PaymentTransaction();
                
                paymentAction.ShopifyTransactionId = payment.ShopifyTransactionId;
                paymentAction.TransDesc = $"Shopify Payment ({payment.ShopifyTransactionId})";
                paymentAction.PaymentGateway = payment.ShopifyGateway;
                paymentAction.Amount = payment.ShopifyAmount;
                paymentAction.ActionCode = ActionCode.None;

                if (!payment.ExistsInAcumatica())
                {
                    paymentAction.ActionCode = ActionCode.CreateInAcumatica;
                }

                if (payment.ExistsInAcumatica() && payment.NeedsPaymentPut)
                {
                    paymentAction.ActionCode = ActionCode.UpdateInAcumatica;
                }
                else if (payment.ExistsInAcumatica() && !payment.AcumaticaPayment.IsReleased)
                {
                    paymentAction.ActionCode = ActionCode.ReleaseInAcumatica;
                }
            }
            else
            {
                paymentAction.ShopifyTransactionId = -1;
                paymentAction.TransDesc = $"Shopify Payment not found yet";
                paymentAction.PaymentGateway = "No Gateway";
                paymentAction.Amount = 0.00m;
                paymentAction.ActionCode = ActionCode.None;
            }

            return paymentAction;
        }

        private List<PaymentAction> BuildRefundPaymentActions(ShopifyOrder orderRecord)
        {
            var output = new List<PaymentAction>();

            foreach (var refund in orderRecord.RefundTransactions())
            {
                var refundAction = new PaymentAction();

                refundAction.ShopifyTransactionId = refund.ShopifyTransactionId;
                refundAction.TransDesc = $"Shopify Refund ({refund.ShopifyTransactionId})";
                refundAction.PaymentGateway = refund.ShopifyGateway;
                refundAction.Amount = refund.ShopifyAmount;
                refundAction.ActionCode = ActionCode.None;

                if (!refund.ExistsInAcumatica())
                {
                    refundAction.ActionCode = ActionCode.CreateInAcumatica;
                }

                if (refund.ExistsInAcumatica() && !refund.IsReleased())
                {
                    refundAction.ActionCode = ActionCode.ReleaseInAcumatica;
                }

                output.Add(refundAction);
            }

            return output;
        }

        private List<AdjustmentAction> BuildRefundAdjustmentActions(ShopifyOrder orderRecord)
        {
            var output = new List<AdjustmentAction>();

            foreach (var creditAdj in orderRecord.CreditAdustmentRefunds())
            {
                var action = new AdjustmentAction();
                action.ActionCode = ActionCode.CreateInAcumatica;
                action.MemoType = AdjustmentMemoType.CreditMemo;
                action.MemoAmount = creditAdj.CreditAdjustment;

                output.Add(action);
            }

            foreach (var debitAdj in orderRecord.DebitAdustmentRefunds())
            {
                var action = new AdjustmentAction();
                action.ActionCode = ActionCode.CreateInAcumatica;
                action.MemoType = AdjustmentMemoType.DebitMemo;
                action.MemoAmount = debitAdj.DebitAdjustment;

                output.Add(action);
            }

            return output;
        }

        private List<ShipmentAction> BuildShipmentInvoiceActions(ShopifyOrder orderRecord)
        {
            var output = new List<ShipmentAction>();

            foreach (var soShipment in orderRecord.SoShipments())
            {
                var action = new ShipmentAction();
                action.ShipmentNbr = soShipment.AcumaticaShipmentNbr;
                action.InvoiceNbr = soShipment.AcumaticaInvoiceNbr;
                action.InvoiceAmount = soShipment.AcumaticaInvoiceAmount.Value;
                action.InvoiceTax = soShipment.AcumaticaInvoiceTax.Value;

                if (soShipment.ShopifyFulfillment == null)
                {
                    action.ActionCode = ActionCode.CreateInShopify;
                }
                else
                {
                    action.ActionCode = ActionCode.None;
                }

                output.Add(action);
            }

            return output;
        }
    }
}
