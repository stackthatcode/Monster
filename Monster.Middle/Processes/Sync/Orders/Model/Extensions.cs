using System;
using System.Linq;
using Microsoft.Owin.Security;
using Monster.Acumatica.Api.Customer;
using Monster.Middle.Persist.Multitenant;
using Push.Foundation.Utilities.Json;
using Push.Shopify.Api.Transactions;

namespace Monster.Middle.Processes.Sync.Orders.Model
{
    public static class Extensions
    {
        public static UsrAcumaticaCustomer ToMonsterRecord(this Customer customer)
        {
            var output = new UsrAcumaticaCustomer();
            output.AcumaticaCustomerId = customer.CustomerID.value;
            output.AcumaticaJson = customer.SerializeToJson();
            output.AcumaticaMainContactEmail = customer.MainContact.Email.value;
            output.DateCreated = DateTime.UtcNow;
            output.LastUpdated = DateTime.UtcNow;
            return output;
        }

        // Only create Payments for Capture/Sale or Refund
        public static bool ShouldCreatePayment(this UsrShopifyTransaction input)
        {
            return input.ShopifyGateway != Gateway.Manual
                   && input.UsrShopifyAcuPayment == null
                   && input.ShopifyStatus == TransactionStatus.Success
                   && (input.ShopifyKind == TransactionKind.Capture
                       || input.ShopifyKind == TransactionKind.Sale);
        }


        // 
        public static bool ShouldCreateRefundPayment(this UsrShopifyTransaction input)
        {
            return input.ShopifyGateway != Gateway.Manual
                   && input.UsrShopifyAcuPayment == null
                   && input.ShopifyStatus == TransactionStatus.Success
                   && (input.ShopifyKind == TransactionKind.Refund);
        }


        public static bool HasCreditMemoOrder(this UsrShopifyRefund refund)
        {
            return refund.UsrShopAcuRefundCms.Any();
        }

        public static bool DoesNotHaveCreditMemoOrder(this UsrShopifyRefund refund)
        {
            return !refund.HasCreditMemoOrder();
        }

        public static bool CreditMemoTaxesAreSynced(this UsrShopifyRefund refund)
        {
            return refund.UsrShopAcuRefundCms.Any() &&
                   refund.UsrShopAcuRefundCms.First().IsCmOrderTaxLoaded;
        }

        public static bool CreditMemoTaxesAreNotSynced(this UsrShopifyRefund refund)
        {
            return !refund.CreditMemoTaxesAreSynced();
        }

        public static bool HasCreditMemoInvoice(this UsrShopifyRefund refund)
        {
            return refund.UsrShopAcuRefundCms.Any() &&
                refund.UsrShopAcuRefundCms.First().AcumaticaCreditMemoInvoiceNbr != null;
        }

        public static bool DoesNotHaveCreditMemoInvoice(this UsrShopifyRefund refund)
        {
            return !refund.HasCreditMemoInvoice();
        }


        public static bool IsSyncComplete(this UsrShopifyRefund refund)
        {
            return refund.HasCreditMemoOrder()
                   && refund.CreditMemoTaxesAreSynced()
                   && refund.HasCreditMemoInvoice();
        }
    }
}

