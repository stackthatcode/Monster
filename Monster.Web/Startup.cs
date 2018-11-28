using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Monster.Web.Startup))]
namespace Monster.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
