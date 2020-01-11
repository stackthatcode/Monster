using System.Web.Mvc;
using Autofac.Integration.Mvc;
using Hangfire;
using Microsoft.Owin;
using Monster.Middle.Misc.Hangfire;
using Monster.Web.Attributes;
using Monster.Web.Plumbing;
using Owin;
using Startup = Monster.Web.Startup;

[assembly: OwinStartup(typeof(Startup))]
namespace Monster.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {            
            var container = WebAutofac.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            AuthConfig.ConfigureAuth(app);

            // HangFire setup...
            //
            HangFireConfig.ConfigureStorage();

            app.UseHangfireDashboard(
                "/hangfire", 
                new DashboardOptions
                {
                    Authorization = new[] { new HangFireAuthorizationFilter(), },
                    AppPath = GlobalConfig.LoginPage,
                });
        }
    }
}
