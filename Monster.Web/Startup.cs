using AutoMapper;
using Microsoft.Owin;
using Monster.Middle.Persist.Tenant;
using Monster.Middle.Processes.Sync.Model.Misc;
using Monster.Middle.Processes.Sync.Services;
using Monster.Web.Models.Config;
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

            AutomapperConfigure();

            // TODO - rip all that shit out and replace with Push
            //AuthConfig.Configure(app, autofacContainer);
            
            HangFireConfig.Configure(app);
        }

        public static void AutomapperConfigure()
        {
            Mapper.Initialize(x =>
            {
                x.CreateMap<UsrPreference, PreferencesModel>();
                x.CreateMap<UsrSystemState, SystemStateSummaryModel>();
                x.CreateMap<UsrPreference, SyncEnablesModel>();
                x.CreateMap<UsrPreference, OrderSyncSettingsModel>();
            });
        }
    }
}
