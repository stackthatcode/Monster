using System;
using System.Collections.Generic;
using Monster.Middle.Misc.Shopify;
using Monster.Middle.Processes.Sync.Persist;

namespace Monster.Middle.Misc.Acumatica
{
    public class AcumaticaTimeZoneService
    {
        private readonly SettingsRepository _settingsRepository;

        public AcumaticaTimeZoneService(SettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public DateTime ToAcumaticaTimeZone(DateTime dateTimeUTC)
        {
            var Settingss = _settingsRepository.RetrieveSettingss();

            return dateTimeUTC.ToTimeZone(Settingss.AcumaticaTimeZone);
        }

        public List<AcumaticaTimeZone> RetrieveTimeZones()
        {
            var output = new List<AcumaticaTimeZone>();
            
            output.Add(new AcumaticaTimeZone
            {
                TimeZoneId = "Eastern Standard Time",
                Name = "(GMT-05:00) Eastern Time (US & Canada)"
            });
            output.Add(new AcumaticaTimeZone
            {
                TimeZoneId = "Central Standard Time",
                Name = "(GMT-06:00) Central Time (US & Canada)"
            });
            output.Add(new AcumaticaTimeZone
            {
                TimeZoneId = "Mountain Standard Time",
                Name = "(GMT-07:00) Mountain Time (US & Canada)"
            });
            output.Add(new AcumaticaTimeZone
            {
                TimeZoneId = "Pacific Standard Time",
                Name = "(GMT-08:00) Pacific Time (US & Canada)"
            });
            output.Add(new AcumaticaTimeZone
            {
                TimeZoneId = "Alaskan Standard Time",
                Name = "(GMT-09:00) Alaska"
            });
            output.Add(new AcumaticaTimeZone
            {
                TimeZoneId = "Hawaiian Standard Time",
                Name = "(GMT-10:00) Hawaii"
            });

            return output;
        }
    }
}
