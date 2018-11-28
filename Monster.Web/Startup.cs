using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Monster.Web.Startup))]
namespace Monster.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // TODO - rip all that shit out and replace with Push
            //ConfigureAuth(app);
        }
    }
}
