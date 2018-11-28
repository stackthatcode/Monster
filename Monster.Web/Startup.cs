using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Monster.Web.Startup))]
namespace Monster.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {            
            var autofacContainer = WebAutofac.Build();

            // TODO - rip all that shit out and replace with Push
            //AuthConfig.Configure(app, autofacContainer);

            HangFireConfig.Configure(app);
        }
    }
}
