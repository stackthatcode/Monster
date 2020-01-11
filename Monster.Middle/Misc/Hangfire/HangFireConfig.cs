using Hangfire;
using Hangfire.SqlServer;
using Monster.Middle.Config;

namespace Monster.Middle.Misc.Hangfire
{
    public class HangFireConfig
    {
        public static void ConfigureStorage()
        {
            GlobalConfiguration
                .Configuration
                .UseSqlServerStorage(
                    MonsterConfig.Settings.SystemDatabaseConnection,
                    new SqlServerStorageOptions
                    {
                        PrepareSchemaIfNecessary = true
                    });
        }
    }
}

