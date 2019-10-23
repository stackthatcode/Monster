﻿using AutoMapper;
using Microsoft.Owin;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes.Sync.Model.Status;
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

            AuthConfig.ConfigureAuth(app);
            HangFireConfig.Configure(app);
        }

        public static void AutomapperConfigure()
        {
            Mapper.Initialize(x =>
            {
                x.CreateMap<Preference, PreferencesModel>();
                x.CreateMap<SystemState, ConfigStateSummaryModel>();
                x.CreateMap<Preference, SyncEnablesModel>();
                x.CreateMap<Preference, OrderSyncSettingsModel>();
            });
        }
    }
}
