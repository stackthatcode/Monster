using System;
using Monster.Middle.Persist.Multitenant.Etc;
using Monster.Middle.Misc;

namespace Monster.Middle.Services
{
    public class TimeZoneService
    {
        private readonly TenantRepository _tenantRepository;

        public TimeZoneService(TenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        public DateTime ToAcumaticaTimeZone(DateTime input)
        {
            var preferences = _tenantRepository.RetrievePreferences();

            return input.ToTimeZone(preferences.AcumaticaTimeZone);
        }
    }
}
