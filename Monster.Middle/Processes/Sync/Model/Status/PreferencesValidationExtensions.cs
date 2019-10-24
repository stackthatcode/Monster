using System;
using Monster.Middle.Persist.Instance;
using Push.Foundation.Utilities.Helpers;

namespace Monster.Middle.Processes.Sync.Model.Status
{
    public static class PreferencesValidationExtensions
    {
        // Seems obvious that we'll provide more detail for the future..
        public static bool AreAcumaticaPreferencesValid(this Preference preferences)
        {
            return preferences.AcumaticaTimeZone.HasValue()
                   && preferences.AcumaticaDefaultItemClass.HasValue()
                   && preferences.AcumaticaDefaultPostingClass.HasValue()
                   && preferences.AcumaticaPaymentMethod.HasValue()
                   && preferences.AcumaticaTaxCategory.HasValue()
                   && preferences.AcumaticaTaxId.HasValue()
                   && preferences.AcumaticaTaxZone.HasValue();
        }


        public static void AssertStartingOrderIsValid(this Preference preferences)
        {
            if (preferences.ShopifyOrderId == null ||
                preferences.ShopifyOrderCreatedAtUtc == null ||
                preferences.ShopifyOrderName.IsNullOrEmpty())
            {
                throw new Exception("ShopifyOrderCreatedAtUtc has not been set");
            }
        }
    }
}

