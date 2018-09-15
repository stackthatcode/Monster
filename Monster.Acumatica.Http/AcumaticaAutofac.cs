﻿using Autofac;
using Monster.Acumatica.Api;
using Monster.Acumatica.Config;
using Monster.Acumatica.Http;

namespace Monster.Acumatica
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
            builder.RegisterType<CustomerRepository>();
        }        
    }
}

