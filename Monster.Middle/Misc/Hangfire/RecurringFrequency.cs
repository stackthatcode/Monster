using System.Collections.Generic;

namespace Monster.Middle.Misc.Hangfire
{
    public class RecurringFrequency
    {
        public int Id { get; set; }
        public string Desc { get; set; }
        public string Cron { get; set; }


        public static readonly 
            List<RecurringFrequency> Options = new List<RecurringFrequency>()
        {
            new RecurringFrequency
            {
                Id = 1,
                Desc = "every minute",
                Cron = "* * * * *"
            },
            new RecurringFrequency
            {
                Id = 2,
                Desc = "every 15 minutes",
                Cron = "*/15 * * * *"
            },
            new RecurringFrequency
            {
                Id = 3,
                Desc = "every 1 hour",
                Cron = "0 * * * *"
            },
            new RecurringFrequency
            {
                Id = 4,
                Desc = "every 4 hours",
                Cron = "0 */4 * * *"
            },
            new RecurringFrequency
            {
                Id = 5,
                Desc = "every 12 hours",
                Cron = "0 */12 * * *"
            },
            new RecurringFrequency
            {
                Id = 6,
                Desc = "once a day at midnight",
                Cron = "0 0 * * *"
            },
        };
    }
}
