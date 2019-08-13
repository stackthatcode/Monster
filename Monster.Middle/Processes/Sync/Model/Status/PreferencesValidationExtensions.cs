﻿using Monster.Middle.Persist.Instance;
using Push.Foundation.Utilities.Helpers;

namespace Monster.Middle.Processes.Sync.Model.Status
{
    public static class PreferencesValidationExtensions
    {
        // Seems obvious that we'll provide more detail for the future..
        public static bool AreValid(this Preference preferences)
        {
            return preferences.AcumaticaTimeZone.HasValue()
                   && preferences.AcumaticaDefaultItemClass.HasValue()
                   && preferences.AcumaticaDefaultPostingClass.HasValue()
                   && preferences.AcumaticaPaymentMethod.HasValue()
                   && preferences.AcumaticaTaxCategory.HasValue()
                   && preferences.AcumaticaTaxId.HasValue()
                   && preferences.AcumaticaTaxZone.HasValue();
        }
    }
}
