using System.Collections.Generic;
using System.Linq;

namespace Monster.Middle.Persist.Multitenant.Extensions
{
    public static class OrderExtensions
    {
        public static bool LineItemsAreReadyToSync(this UsrShopifyOrder order)
        {
            foreach (var item in order.UsrShopifyOrderLineItems)
            {
                if (item.UsrShopifyVariant == null)
                {
                    return false;
                }

                if (item.UsrShopifyVariant.IsMissing)
                {
                    return false;
                }

                if (item.UsrShopifyVariant.IsNotMatched())
                {
                    return false;
                }
            }

            return true;
        }
        
        public static bool IsPaid(this UsrShopifyOrder order)
        {
            return
                order.ShopifyFinancialStatus == FinancialStatus.Paid ||
                order.ShopifyFinancialStatus == FinancialStatus.PartiallyRefunded ||
                order.ShopifyFinancialStatus == FinancialStatus.Refunded;
        }

        public static bool IsReadyForAcumaticaShipment(
                    this UsrShopifyFulfillment fulfillment)
        {
            var acumaticaSalesOrder =
                    fulfillment
                        .UsrShopifyOrder
                        .UsrAcumaticaSalesOrders
                        .FirstOrDefault();

            if (acumaticaSalesOrder == null)
            {
                return false;
            }

            if (acumaticaSalesOrder.AcumaticaStatus != "Open")
            {
                return false;
            }

            return true;
        }


        public static List<UsrShopifyFulfillment> 
                    ReadyForAcumaticaShipment(
                        this List<UsrShopifyFulfillment> fulfillments)
        {
            return fulfillments
                .Where(x => x.IsReadyForAcumaticaShipment())
                .ToList();
        }


        //public static UsrAcumaticaInvoice AcumaticaInvoice(this UsrShopifyOrder order)
        //{
        //    if (!order.UsrAcumaticaSalesOrders.Any())
        //        return null;

        //    return order.UsrAcumaticaSalesOrders
        //        .First()
        //        .UsrAcumaticaInvoices
        //        .FirstOrDefault();
        //}
    }
}

