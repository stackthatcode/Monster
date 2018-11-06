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

