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
        private readonly MemoValidationService _memoValidationService;
        private readonly FulfillmentStatusService _fulfillmentStatusService;
        private readonly ShopifyUrlService _shopifyUrlService;
        private readonly AcumaticaUrlService _acumaticaUrlService;
        private readonly ShopifyJsonService _shopifyJsonService;


        public PendingActionService(
                SyncOrderRepository orderRepository,
                OrderValidationService orderValidation,
                PaymentValidationService paymentValidation,
                MemoValidationService memoValidationService,
                FulfillmentStatusService fulfillmentStatusService,
                ShopifyUrlService shopifyUrlService, 
                AcumaticaUrlService acumaticaUrlService, 
                ShopifyJsonService shopifyJsonService)
        {
            _syncOrderRepository = orderRepository;
            _orderValidation = orderValidation;
            _paymentValidation = paymentValidation;
            _memoValidationService = memoValidationService;
            _fulfillmentStatusService = fulfillmentStatusService;
            _shopifyUrlService = shopifyUrlService;
            _acumaticaUrlService = acumaticaUrlService;
            _shopifyJsonService = shopifyJsonService;
        }


        public RootAction Create(long shopifyOrderId, bool validate = true)
        {
            var orderRecord = _syncOrderRepository.RetrieveShopifyOrderWithNoTracking(shopifyOrderId);
            return Create(orderRecord, validate);
        }

        public RootAction Create(ShopifyOrder orderRecord, bool validate = true)
        {
            var output = new RootAction();
            output.OrderAction = BuildOrderPendingAction(orderRecord);
            output.PaymentAction = BuildPaymentActions(orderRecord);
            output.RefundPaymentActions = BuildRefundPaymentActions(orderRecord);
            output.AdjustmentMemoActions = BuildRefundAdjustmentActions(orderRecord);
            output.ShipmentInvoiceActions = BuildShipmentInvoiceActions(orderRecord);

            output.ErrorCount = orderRecord.ErrorCount;
            output.ExceedsErrorLimit = orderRecord.ExceedsErrorLimit();
            output.Ignore = orderRecord.Ignore;


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

                foreach (var action in output.AdjustmentMemoActions)
                {
                    _memoValidationService.Validate(output, action);
                }
            }

            return output;
        }


        private OrderAction BuildOrderPendingAction(ShopifyOrder record)
        {
            var output = new OrderAction();
            var order = _shopifyJsonService.RetrieveOrder(record.ShopifyOrderId);

            output.ShopifyOrderId = record.ShopifyOrderId;
            output.ShopifyOrderHref = _shopifyUrlService.ShopifyOrderUrl(record.ShopifyOrderId);
            output.ShopifyOrderName = order.name;
            
            output.Validation = new ValidationResult();
            output.ActionCode = ActionCode.None;

            if (!record.ExistsInAcumatica())
            {
                output.ActionCode =
                    order.IsCancelled || order.AreAllLineItemsRefunded
                        ? ActionCode.CreateBlankSyncRecord
                        : ActionCode.CreateInAcumatica;
            }
            else // Exists in Acumatica
            {
                output.AcumaticaSalesOrderNbr = record.AcumaticaSalesOrder.AcumaticaOrderNbr;
                output.AcumaticaSalesOrderHref
                        = _acumaticaUrlService.AcumaticaSalesOrderUrl(
                                SalesOrderType.SO, record.AcumaticaSalesOrder.AcumaticaOrderNbr);

                if (record.NeedsOrderPut || 
                    record.ShopifyTotalQuantity != record.SyncedSalesOrder().AcumaticaQtyTotal)
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

                if (payment.ExistsInAcumatica())
                {
                    paymentAction.AcumaticaPaymentRef = payment.AcumaticaPayment.AcumaticaRefNbr;
                    paymentAction.AcumaticaHref
                        = _acumaticaUrlService.AcumaticaPaymentUrl(
                                PaymentType.Payment, payment.AcumaticaPayment.AcumaticaRefNbr);

                    if (orderRecord.OriginalPaymentNeedsUpdateForRefund())
                    {
                        paymentAction.ActionCode = ActionCode.UpdateInAcumatica;
                    }
                    else if (payment.ExistsInAcumatica() && payment.AcumaticaPayment.NeedRelease)
                    {
                        paymentAction.ActionCode = ActionCode.ReleaseInAcumatica;
                    }
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

            foreach (var refundTrans in orderRecord.RefundTransactions())
            {
                var refundAction = new PaymentAction();

                refundAction.ShopifyTransactionId = refundTrans.ShopifyTransactionId;
                refundAction.TransDesc = $"Shopify Refund ({refundTrans.ShopifyTransactionId})";
                refundAction.PaymentGateway = refundTrans.ShopifyGateway;
                refundAction.Amount = refundTrans.ShopifyAmount;

                var refund = orderRecord.Refund(refundTrans.ShopifyRefundId.Value);

                refundAction.HasShipping = refund.HasShipping();
                refundAction.Shipping = refund.Shipping;
                refundAction.ShippingTax = refund.ShippingTax;

                refundAction.ActionCode = ActionCode.None;

                if (!refundTrans.ExistsInAcumatica())
                {
                    refundAction.ActionCode = ActionCode.CreateInAcumatica;
                }

                if (refundTrans.ExistsInAcumatica())
                {
                    refundAction.AcumaticaPaymentRef = refundTrans.AcumaticaPayment.AcumaticaRefNbr;
                    refundAction.AcumaticaHref
                        = _acumaticaUrlService.AcumaticaPaymentUrl(
                            PaymentType.CustomerRefund, refundTrans.AcumaticaPayment.AcumaticaRefNbr);
                }

                if (refundTrans.ExistsInAcumatica() 
                    && !refundTrans.AcumaticaPayment.NeedManualApply 
                    && !refundTrans.IsReleased())
                {
                    refundAction.ActionCode = ActionCode.ReleaseInAcumatica;
                }

                if (refundTrans.ExistsInAcumatica()
                    && refundTrans.AcumaticaPayment.NeedManualApply)
                {
                    refundAction.ActionCode = ActionCode.NeedManualApply;
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
                action.ActionCode = ActionCode.None;
                action.ShopifyOrderId = orderRecord.ShopifyOrderId;
                action.ShopifyRefundId = creditAdj.ShopifyRefundId;

                action.MemoType = AdjustmentMemoType.CreditMemo;
                action.MemoAmount = creditAdj.CreditAdjustment;


                if (creditAdj.AcumaticaMemo == null)
                {
                    action.ActionCode = ActionCode.CreateInAcumatica;
                }

                if (creditAdj.AcumaticaMemo != null)
                {
                    action.AcumaticaRefNbr = creditAdj.AcumaticaMemo.AcumaticaRefNbr;
                    action.AcumaticaDocType = creditAdj.AcumaticaMemo.AcumaticaDocType;
                    action.AcumaticaHref
                        = _acumaticaUrlService.AcumaticaInvoiceUrl(
                            SalesInvoiceType.Credit_Memo, creditAdj.AcumaticaMemo.AcumaticaRefNbr);
                }

                if (creditAdj.AcumaticaMemo != null 
                    && !creditAdj.AcumaticaMemo.NeedManualApply
                    && creditAdj.AcumaticaMemo.NeedRelease)
                {
                    action.ActionCode = ActionCode.ReleaseInAcumatica;
                }

                if (creditAdj.AcumaticaMemo != null
                    && !creditAdj.AcumaticaMemo.NeedManualApply)
                {
                    action.ActionCode = ActionCode.NeedManualApply;
                }

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
                action.ShipmentHref
                    = _acumaticaUrlService.AcumaticaShipmentUrl(soShipment.AcumaticaShipmentNbr);

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
                    action.ShopifyFulfillmentId = soShipment.ShopifyFulfillment.ShopifyFulfillmentId;
                }

                output.Add(action);
            }

            return output;
        }
    }
}
