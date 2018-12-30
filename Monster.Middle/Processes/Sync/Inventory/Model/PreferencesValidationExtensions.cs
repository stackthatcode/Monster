using Monster.Middle.Persist.Multitenant;
using Push.Foundation.Utilities.Helpers;

namespace Monster.Middle.Processes.Sync.Inventory.Model
{
    public static class PreferencesValidationExtensions
    {
        // Seems obvious that we'll provide more detail for the future..
        public static bool AreValid(this UsrPreference preferences)
        {
            return preferences.ShopifyDataPullStart.HasValue
                   && preferences.AcumaticaTimeZone.HasValue()
                   && preferences.AcumaticaDefaultItemClass.HasValue()
                   && preferences.AcumaticaDefaultPostingClass.HasValue()
                   && preferences.AcumaticaPaymentMethod.HasValue()
                   && preferences.AcumaticaTaxCategory.HasValue()
                   && preferences.AcumaticaTaxId.HasValue()
                   && preferences.AcumaticaTaxZone.HasValue();
        }
    }
}
