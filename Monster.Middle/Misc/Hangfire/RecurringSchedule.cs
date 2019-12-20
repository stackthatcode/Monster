using System.Collections.Generic;
using System.Linq;

namespace Monster.Middle.Misc.Hangfire
{
    public class RecurringSchedule
    {
        public int Id { get; set; }
        public string Desc { get; set; }
        public string Cron { get; set; }

        public static RecurringSchedule Default => Options.FirstOrDefault(x => x.Id == 6);

        public static readonly 
            List<RecurringSchedule> Options = new List<RecurringSchedule>()
        {
            new RecurringSchedule
            {
                Id = 1,
                Desc = "every minute aka real-time",
                Cron = "* * * * *"
            },
            new RecurringSchedule
            {
                Id = 2,
                Desc = "every 15 minutes",
                Cron = "*/15 * * * *"
            },
            new RecurringSchedule
            {
                Id = 3,
                Desc = "every 1 hour",
                Cron = "0 * * * *"
            },
            new RecurringSchedule
            {
                Id = 4,
                Desc = "every 4 hours",
                Cron = "0 */4 * * *"
            },
            new RecurringSchedule
            {
                Id = 5,
                Desc = "every 12 hours",
                Cron = "0 */12 * * *"
            },
            new RecurringSchedule
            {
                Id = 6,
                Desc = "once a day at midnight",
                Cron = "0 0 * * *"
            },
        };
    }
}
