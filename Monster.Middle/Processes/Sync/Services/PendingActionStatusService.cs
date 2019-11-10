using System.Collections.Generic;
using Monster.Middle.Misc.Shopify;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Shopify.Persist;
using Monster.Middle.Processes.Sync.Model.Orders;
using Monster.Middle.Processes.Sync.Model.Status;
using Monster.Middle.Processes.Sync.Persist;
using Push.Foundation.Utilities.Validation;
using Push.Shopify.Api.Order;

namespace Monster.Middle.Processes.Sync.Services
{
    public class PendingActionStatusService
    {
        private readonly SyncOrderRepository _syncOrderRepository;
        private readonly OrderSyncValidationService _syncValidationService;
        private readonly ShopifyUrlService _urlService;

        public PendingActionStatusService(
                SyncOrderRepository orderRepository, 
                OrderSyncValidationService syncValidationService,
                ShopifyUrlService urlService)
        {
            _syncOrderRepository = orderRepository;
            _syncValidationService = syncValidationService;
            _urlService = urlService;
        }


        public OrderPendingActionStatus GetPendingActionStatus(long shopifyOrderId)
        {
            var orderRecord = _syncOrderRepository.RetrieveShopifyOrder(shopifyOrderId);
            var output = new OrderPendingActionStatus();
            BuildOrderPendingAction(orderRecord, output);
            BuildTransactionPendingActions(orderRecord, output);
            BuildRefundAdjPendingActions(orderRecord, output);

            return output;
        }

        private void BuildOrderPendingAction(ShopifyOrder orderRecord, OrderPendingActionStatus output)
        {
            var order = orderRecord.ToShopifyObj();
            
            output.ShopifyOrderId = orderRecord.ShopifyOrderId;
            output.ShopifyOrderHref = _urlService.ShopifyOrderUrl(orderRecord.ShopifyOrderId);
            output.ShopifyOrderName = order.name;

            if (!orderRecord.ExistsInAcumatica())
            {
                output.ShopifyOrderAction = PendingAction.CreateInAcumatica;
                output.OrderSyncValidation =
                    _syncValidationService.GetOrderSyncValidator(orderRecord.ShopifyOrderId).Result();
            }

            if (orderRecord.ExistsInAcumatica() && orderRecord.NeedsOrderPut)
            {
                output.ShopifyOrderAction = PendingAction.UpdateInAcumatica;
            }
        }

        private static void BuildTransactionPendingActions(ShopifyOrder orderRecord, OrderPendingActionStatus output)
        {
            output.MissingShopifyPayment = !orderRecord.HasPayment();

            if (orderRecord.HasPayment())
            {
                var payment = orderRecord.PaymentTransaction();
                var paymentAction = new TransactionPendingAction();

                paymentAction.TransDesc = $"Shopify Payment ({payment.ShopifyTransactionId})";
                paymentAction.PaymentGateway = payment.ShopifyGateway;
                paymentAction.Amount = payment.ShopifyAmount;
                paymentAction.Action = PendingAction.None;

                if (!payment.ExistsInAcumatica())
                {
                    paymentAction.Action = PendingAction.CreateInAcumatica;
                }
                if (payment.ExistsInAcumatica() && payment.NeedsPaymentPut)
                {
                    paymentAction.Action = PendingAction.UpdateInAcumatica;
                }
                else if (payment.ExistsInAcumatica() && !payment.AcumaticaPayment.IsReleased)
                {
                    paymentAction.Action = PendingAction.ReleaseInAcumatica;
                }

                output.PaymentPendingAction = paymentAction;
            }


            foreach (var refund in orderRecord.RefundTransactions())
            {
                var refundAction = new TransactionPendingAction();

                refundAction.TransDesc = $"Shopify Refund ({refund.ShopifyTransactionId})";
                refundAction.PaymentGateway = refund.ShopifyGateway;
                refundAction.Amount = refund.ShopifyAmount;
                refundAction.Action = PendingAction.None;


                if (!refund.ExistsInAcumatica())
                {
                    refundAction.Action = PendingAction.CreateInAcumatica;
                }

                if (refund.ExistsInAcumatica() && !refund.IsReleased())
                {
                    refundAction.Action = PendingAction.ReleaseInAcumatica;
                }

                output.RefundPendingActions.Add(refundAction);
            }
        }

        private static void BuildRefundAdjPendingActions(ShopifyOrder orderRecord, OrderPendingActionStatus output)
        {
            foreach (var creditAdj in orderRecord.CreditAdustmentRefunds())
            {
                var action = new AdjustmentMemoPendingAction();
                action.Action = PendingAction.CreateInAcumatica;
                action.MemoType = AdjustmentMemoType.CreditMemo;
                action.MemoAmount = creditAdj.CreditAdjustment;

                output.AdjustmentMemoPendingActions.Add(action);
            }

            foreach (var debitAdj in orderRecord.DebitAdustmentRefunds())
            {
                var action = new AdjustmentMemoPendingAction();
                action.Action = PendingAction.CreateInAcumatica;
                action.MemoType = AdjustmentMemoType.DebitMemo;
                action.MemoAmount = debitAdj.DebitAdjustment;

                output.AdjustmentMemoPendingActions.Add(action);
            }
        }
    }
}
