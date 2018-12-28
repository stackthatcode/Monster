using Hangfire;
using Hangfire.SqlServer;
using Monster.Middle.Attributes;
using Monster.Web.Plumbing;
using Owin;

namespace Monster.Middle
{
    public class HangFireConfig
    {
        public static void Configure(IAppBuilder app)
        {
            GlobalConfiguration
                .Configuration
                .UseSqlServerStorage("DefaultConnection",
                    new SqlServerStorageOptions
                    {
                        PrepareSchemaIfNecessary = false
                    });

            app.UseHangfireDashboard(
                "/hangfire", new DashboardOptions
                {
                    Authorization = new[] {new HangFireAuthorizationFilter(),},
                    AppPath = GlobalConfig.LoginPage,
                });
        }
    }
}
