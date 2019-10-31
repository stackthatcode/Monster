using AutoMapper;
using Microsoft.Owin;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Model.Settings;
using Monster.Web.Models.Config;
using Monster.Web.Models.Sync;
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
