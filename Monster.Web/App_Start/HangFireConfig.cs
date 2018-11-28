using Hangfire;
using Monster.Web.Attributes;
using Monster.Web.Plumbing;
using Owin;

namespace Monster.Web
{
    public class HangFireConfig
    {
        public static void Configure(IAppBuilder app)
        {
            GlobalConfiguration
                .Configuration
                .UseSqlServerStorage("DefaultConnection");

            app.UseHangfireDashboard(
                "/hangfire", new DashboardOptions
                {
                    Authorization = new[] {new HangFireAuthorizationFilter(),},
                    AppPath = GlobalConfig.LoginPage,
                });
        }
    }
}
