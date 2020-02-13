using Monster.Middle.Persist.Instance;

namespace Monster.Middle.Processes.Sync.Model.Orders
{
    public static class CustomerExtensions
    {
        public static AcumaticaCustomer Match(this ShopifyCustomer input)
        {
            return input.AcumaticaCustomer;
        }

        public static bool HasMatch(this ShopifyCustomer input)
        {
            return input.Match() != null;
        }


        public static string AcumaticaCustId(this ShopifyCustomer input)
        {
            return input.HasMatch() ? input.Match().AcumaticaCustomerId : null;
        }
    }
}
