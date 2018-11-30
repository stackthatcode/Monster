using AutoMapper;
using Microsoft.Owin;
using Monster.Middle.Persist.Multitenant;
using Monster.Web.Models;
using Owin;

[assembly: OwinStartupAttribute(typeof(Monster.Web.Startup))]
namespace Monster.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {            
            var autofacContainer = WebAutofac.Build();

            AutomapperConfigure();

            // TODO - rip all that shit out and replace with Push
            //AuthConfig.Configure(app, autofacContainer);
            
            HangFireConfig.Configure(app);
        }

        public static void AutomapperConfigure()
        {
            Mapper.Initialize(x => x.CreateMap<UsrPreference, Preferences>());
        }
    }
}
