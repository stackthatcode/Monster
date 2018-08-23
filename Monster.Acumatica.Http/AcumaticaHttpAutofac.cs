﻿using Autofac;
using Push.Foundation.Web.HttpClient;

namespace Monster.Acumatica.Http
{
    public class AcumaticaHttpAutofac
    {
        public static void Build(ContainerBuilder builder)
        {
            //// TODO: wire these into the DI architecture
            //var settings = new AcumaticaHttpSettings();
            //_credentials = new AcumaticaCredentials();
            //_requestBuilder = new AcumaticaRequestBuilder(settings);

            builder.RegisterType<AcumaticaRequestBuilder>()
                    .As<AcumaticaRequestBuilder>()
                    .As<IRequestBuilder>();

            builder.RegisterType<AcumaticaApiFactory>();
            builder.RegisterType<AcumaticaHttpSettings>();
            builder.RegisterType<SpikeRepository>();
        }        
    }
}
