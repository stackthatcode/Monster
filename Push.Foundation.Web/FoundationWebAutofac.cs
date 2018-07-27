using System.Data.Entity;
using System.Web;
using Autofac;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using Push.Foundation.Utilities.Security;
using Push.Foundation.Web.HttpClient;
using Push.Foundation.Web.Identity;
using Push.Foundation.Web.Misc;


namespace Push.Foundation.Web
{
    public class FoundationWebAutofac
    {
        //
        // Registration of HttpClient and Entity-framework backed OWIN Identity
        //
        // Note: needs a DbConnection registration for OWIN piece to work properly
        // ALSO: excludes EncryptionService, as each consumer will inject different keys
        public static void Build(ContainerBuilder builder)
        {
            RegisterOwinIdentity(builder);
            
            // The Hmac Service will be used to validate Webhooks from Shopify
            builder.RegisterType<HmacCryptoService>();            
            
            // Http Client namespace
            builder.RegisterType<ClientSettings>();
            builder.RegisterType<HttpWebRequestProcessor>();
            builder.RegisterType<ClientFacade>();
            builder.RegisterType<RequestFactory>();

            // Misc (but very important)
            builder.RegisterType<InsistentExecutor>();
            builder.RegisterType<Throttler>();
        }


        public static void RegisterOwinIdentity(ContainerBuilder builder)
        {
            // OWIN framework objects
            builder
                .RegisterType<ApplicationDbContext>()
                .As<DbContext>()
                .As<ApplicationDbContext>()
                .InstancePerLifetimeScope();

            builder.RegisterType<ApplicationRoleManager>();
            builder.RegisterType<ApplicationUserManager>();
            builder.RegisterType<ApplicationSignInManager>();
            builder.RegisterType<ClaimsRepository>();

            builder
                .RegisterType<UserStore<ApplicationUser>>()
                .As<IUserStore<ApplicationUser>>()
                .As<UserStore<ApplicationUser>>();

            builder
                .Register(ctx => new HttpContextWrapper(HttpContext.Current).GetOwinContext().Authentication)
                .As<IAuthenticationManager>();

            builder.RegisterType<RoleStore<IdentityRole>>();

            // We need to pull the DataProtectionProvider from the App Builder
            builder.RegisterType<MachineKeyProtectionProvider>()
                .As<IDataProtectionProvider>();
            builder.RegisterType<MachineKeyDataProtector>()
                .As<IDataProtector>();

            builder.RegisterType<DataProtectorTokenProvider<ApplicationUser>>();

            // These are stubbed out with "do nothing" services. Consumers of this library
            // ... can easily register/inject their own appropriate implementation.
            builder.RegisterType<EmailService>().As<IIdentityMessageService>();
            builder.RegisterType<SmsService>().As<SmsService>();
        }
    }
}

