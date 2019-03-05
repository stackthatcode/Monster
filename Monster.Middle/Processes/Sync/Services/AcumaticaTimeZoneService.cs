using System;
using System.Collections.Generic;
using Monster.Middle.Services;
using TimeZone = Monster.Middle.Processes.Sync.Model.Misc.TimeZone;

namespace Monster.Middle.Processes.Sync.Services
{
    public class AcumaticaTimeZoneService
    {
        private readonly PreferencesRepository _preferencesRepository;

        public AcumaticaTimeZoneService(
                PreferencesRepository preferencesRepository)
        {
            _preferencesRepository = preferencesRepository;
        }

        public DateTime ToAcumaticaTimeZone(DateTime input)
        {
            var preferences = _preferencesRepository.RetrievePreferences();

            return input.ToTimeZone(preferences.AcumaticaTimeZone);
        }

        public List<TimeZone> RetrieveTimeZones()
        {
            var output = new List<TimeZone>();
            
            output.Add(new TimeZone
            {
                TimeZoneId = "Eastern Standard Time",
                Name = "(GMT-05:00) Eastern Time (US & Canada)"
            });
            output.Add(new TimeZone
            {
                TimeZoneId = "Central Standard Time",
                Name = "(GMT-06:00) Central Time (US & Canada)"
            });
            output.Add(new TimeZone
            {
                TimeZoneId = "Mountain Standard Time",
                Name = "(GMT-07:00) Mountain Time (US & Canada)"
            });
            output.Add(new TimeZone
            {
                TimeZoneId = "Pacific Standard Time",
                Name = "(GMT-08:00) Pacific Time (US & Canada)"
            });
            output.Add(new TimeZone
            {
                TimeZoneId = "Alaskan Standard Time",
                Name = "(GMT-09:00) Alaska"
            });
            output.Add(new TimeZone
            {
                TimeZoneId = "Hawaiian Standard Time",
                Name = "(GMT-10:00) Hawaii"
            });

            return output;
        }
    }
}
