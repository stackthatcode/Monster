using System;
using Push.Foundation.Utilities.Helpers;
using TimeZoneConverter;

namespace Monster.Middle.Services
{

    public class TimeZoneTranslator
    {
        public TimeZoneTranslator()
        {
        }

        // Returns Date + Midnight of that Date in another Time Zone based on *now* in UTC
        public DateTime Today(string shopifyTimeZone)
        {
            return FromUtcToTimeZone(DateTime.UtcNow, shopifyTimeZone).DateOnly();
        }

        public DateTime FromUtcToTimeZone(DateTime dateTimeUtc, string timeZone)
        {
            //var timeZoneId = TZConvert.IanaToWindows(timeZone);
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, timeZoneInfo);
        }

        public DateTime ToUtcFromTimeZone(DateTime dateTimeLocal, string timeZone)
        {
            var dateTimeLocalTz = DateTime.SpecifyKind(dateTimeLocal, DateTimeKind.Unspecified);
           // var timeZoneId = TZConvert.IanaToWindows(timeZone);
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return TimeZoneInfo.ConvertTimeToUtc(dateTimeLocalTz, timeZoneInfo);
        }
    }
    
    // After all that dependency injecting hemming and hawwing, eh?
    public static class TimeZoneTranslatorExtensions
    {
        private static readonly TimeZoneTranslator _translator = new TimeZoneTranslator();

        public static DateTime Today(string shopifyTimeZone)
        {
            return _translator.Today(shopifyTimeZone);
        }

        public static DateTime ToTimeZone(this DateTime dateTimeUtc, string shopifyTimeZone)
        {
            return _translator.FromUtcToTimeZone(dateTimeUtc, shopifyTimeZone);
        }

        public static DateTime ToUtcFromTimeZone(this DateTime dateTimeUtc, string shopifyTimeZone)
        {
            return _translator.ToUtcFromTimeZone(dateTimeUtc, shopifyTimeZone);
        }


        public static DateTime? AsUtc(this DateTime? unspecifiedDateTime)
        {
            return unspecifiedDateTime?.AsUtc();
        }

        public static DateTime AsUtc(this DateTime unspecifiedDateTime)
        {
            return DateTime.SpecifyKind(unspecifiedDateTime, DateTimeKind.Utc);
        }
    }
}

