using System.Web;
using Autofac;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using Push.Foundation.Utilities.Http;
using Push.Foundation.Web.Identity;


namespace Push.Foundation.Web
{
    public class FoundationWebAutofac
    {
        public static void Build(ContainerBuilder builder)
        {
            RegisterOwinIdentity(builder);

            // Misc
            builder.RegisterType<HttpFacade>();
        }


        public static void RegisterOwinIdentity(ContainerBuilder builder)
        {
            // OWIN framework objects
            builder
                .RegisterType<Identity.PushIdentityDbContext>()
                .As<Identity.PushIdentityDbContext>()
                .InstancePerLifetimeScope();

            builder.RegisterType<IdentityRoleManager>();
            builder.RegisterType<IdentityUserManager>();
            builder.RegisterType<IdentitySignInManager>();
            builder.RegisterType<ClaimsRepository>();

            builder
                .RegisterType<UserStore<ApplicationUser>>()
                .As<IUserStore<ApplicationUser>>()
                .As<UserStore<ApplicationUser>>();

            builder
                .Register(ctx => new HttpContextWrapper(HttpContext.Current).GetOwinContext().Authentication)
                .As<IAuthenticationManager>();

            builder.RegisterType<RoleStore<IdentityRole>>();


            // Standard ASP.NET OWIN data protection dependencies
            //
            builder
                .RegisterType<MachineKeyProtectionProvider>()
                .As<IDataProtectionProvider>();

            builder
                .RegisterType<MachineKeyDataProtector>()
                .As<IDataProtector>();

            builder.RegisterType<DataProtectorTokenProvider<ApplicationUser>>();
        }
    }
}

