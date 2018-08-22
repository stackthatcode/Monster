using Autofac;

namespace Monster.Acumatica.Http
{
    public class AcumaticaHttpAutofac
    {
        public static void Build(ContainerBuilder builder)
        {            
            builder.RegisterType<SpikeRepository>();           
        }        
    }
}

