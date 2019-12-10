using Monster.Middle.Misc.Shopify;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Orders;
using Monster.Middle.Processes.Sync.Model.PendingActions;
using Monster.Middle.Processes.Sync.Persist;
using Push.Shopify.Api.Order;


namespace Monster.Middle.Processes.Sync.Services
{
    public class PendingActionService
    {
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly OrderValidationService _orderValidation;
        private readonly PaymentValidationService _paymentValidation;
        private readonly ShopifyUrlService _urlService;

        public PendingActionService(
                SyncOrderRepository orderRepository,
                OrderValidationService orderValidation,
                PaymentValidationService paymentValidation,
                ShopifyUrlService urlService)
        {
            _syncOrderRepository = orderRepository;
            _orderValidation = orderValidation;
            _paymentValidation = paymentValidation;
            _urlService = urlService;
        }


        public RootAction Create(long shopifyOrderId)
        {
            var orderRecord = _syncOrderRepository.RetrieveShopifyOrderWithNoTracking(shopifyOrderId);
            var output = new RootAction();

            output.ShopifyOrderId = orderRecord.ShopifyOrderId;
            output.ShopifyOrderHref = _urlService.ShopifyOrderUrl(orderRecord.ShopifyOrderId);
            output.ShopifyOrderName = orderRecord.ToShopifyObj().name;

            BuildOrderPendingAction(orderRecord, output);
            BuildPaymentActions(orderRecord, output);
            BuildRefundPaymentActions(orderRecord, output);
            BuildRefundAdjustmentActions(orderRecord, output);
            BuildShipmentInvoiceActions(orderRecord, output);

            return output;
        }

        private void BuildOrderPendingAction(ShopifyOrder orderRecord, RootAction output)
        {
            output.OrderAction = new OrderAction();
            output.OrderAction.ActionCode = ActionCode.None;

            if (!orderRecord.ExistsInAcumatica())
            {
                var order = orderRecord.ToShopifyObj();

                if (!order.IsEmptyOrCancelled)
                {
                    output.OrderAction.ActionCode = ActionCode.CreateInAcumatica;
                    output.OrderAction.CreateOrderValidation =
                        _orderValidation.ReadyToCreateOrder(orderRecord.ShopifyOrderId);
                    return;
                }

                if (order.IsEmptyOrCancelled)
                {
                    output.OrderAction.ActionCode = ActionCode.CreateBlankSyncRecord;
                    return;
                }
            }

            if (orderRecord.ExistsInAcumatica() && orderRecord.NeedsOrderPut)
            {
                output.OrderAction.ActionCode = ActionCode.UpdateInAcumatica;
                output.OrderAction.UpdateOrderValidation =
                    _orderValidation.ReadyToUpdateOrder(orderRecord.ShopifyOrderId);
                return;
            }
        }

        private void BuildPaymentActions(ShopifyOrder orderRecord, RootAction output)
        {
            var paymentAction = new PaymentAction();

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
                    paymentAction.Validation = _paymentValidation.ReadyToCreatePayment(payment);
                }

                if (payment.ExistsInAcumatica() && payment.NeedsPaymentPut)
                {
                    paymentAction.ActionCode = ActionCode.UpdateInAcumatica;
                    paymentAction.Validation = _paymentValidation.ReadyToUpdatePayment(payment);
                }
                else if (payment.ExistsInAcumatica() && !payment.AcumaticaPayment.IsReleased)
                {
                    paymentAction.ActionCode = ActionCode.ReleaseInAcumatica;
                    paymentAction.Validation = _paymentValidation.ReadyToRelease(payment);
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

            output.PaymentAction = paymentAction;

        }

        private void BuildRefundPaymentActions(ShopifyOrder orderRecord, RootAction output)
        {
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
                    refundAction.Validation = _paymentValidation.ReadyToCreateRefundPayment(refund);
                }

                if (refund.ExistsInAcumatica() && !refund.IsReleased())
                {
                    refundAction.ActionCode = ActionCode.ReleaseInAcumatica;
                    refundAction.Validation = _paymentValidation.ReadyToReleaseRefundPayment(refund);
                }

                output.RefundPaymentActions.Add(refundAction);
            }
        }

        private void BuildRefundAdjustmentActions(ShopifyOrder orderRecord, RootAction output)
        {
            foreach (var creditAdj in orderRecord.CreditAdustmentRefunds())
            {
                var action = new AdjustmentAction();
                action.ActionCode = ActionCode.CreateInAcumatica;
                action.MemoType = AdjustmentMemoType.CreditMemo;
                action.MemoAmount = creditAdj.CreditAdjustment;

                output.AdjustmentMemoActions.Add(action);
            }

            foreach (var debitAdj in orderRecord.DebitAdustmentRefunds())
            {
                var action = new AdjustmentAction();
                action.ActionCode = ActionCode.CreateInAcumatica;
                action.MemoType = AdjustmentMemoType.DebitMemo;
                action.MemoAmount = debitAdj.DebitAdjustment;

                output.AdjustmentMemoActions.Add(action);
            }
        }

        private void BuildShipmentInvoiceActions(ShopifyOrder orderRecord, RootAction output)
        {
            foreach (var soShipment in orderRecord.SoShipments())
            {
                var action = new ShipmentAction();

                action.ShipmentNbr = soShipment.AcumaticaShipmentNbr;
                action.InvoiceNbr = soShipment.AcumaticaInvoiceNbr;
                action.ActionCode 
                    = soShipment.ShopifyFulfillment == null
                        ? ActionCode.CreateInShopify : ActionCode.None;

                action.InvoiceAmount = soShipment.AcumaticaInvoiceAmount.Value;
                action.InvoiceTax = soShipment.AcumaticaInvoiceTax.Value;

                output.ShipmentInvoiceActions.Add(action);
            }
        }
    }
}
