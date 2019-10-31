using System;
using Monster.Middle.Persist.Instance;
using Push.Foundation.Utilities.Helpers;

namespace Monster.Middle.Processes.Sync.Model.Status
{
    public static class SettingssValidationExtensions
    {
        // Seems obvious that we'll provide more detail for the future..
        public static bool AreSettingsValid(this MonsterSetting settings)
        {
            return settings.AcumaticaTimeZone.HasValue()
                   && settings.AcumaticaDefaultItemClass.HasValue()
                   && settings.AcumaticaDefaultPostingClass.HasValue()
                   && settings.AcumaticaTaxCategory.HasValue()
                   && settings.AcumaticaTaxId.HasValue()
                   && settings.AcumaticaTaxZone.HasValue();
        }

        public static void AssertStartingOrderIsValid(this MonsterSetting settings)
        {
            if (settings.ShopifyOrderId == null ||
                settings.ShopifyOrderCreatedAtUtc == null ||
                settings.ShopifyOrderName.IsNullOrEmpty())
            {
                throw new Exception("ShopifyOrderCreatedAtUtc has not been set");
            }
        }
    }
}

