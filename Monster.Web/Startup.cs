using Microsoft.Owin;
using Owin;
using Startup = Monster.Web.Startup;

[assembly: OwinStartup(typeof(Startup))]
namespace Monster.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {            
            var autofacContainer = WebAutofac.Build();
            
            AuthConfig.ConfigureAuth(app);
            HangFireConfig.Configure(app);
        }
    }
}
