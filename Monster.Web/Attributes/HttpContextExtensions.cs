using System.Web;
using Monster.Middle.Identity;

namespace Monster.Web.Attributes
{
    public static class HttpContextExtensions
    {
        public const string Key = "AppRequestContext";        
        
        public static IdentityContext GetIdentity(this HttpContextBase context)
        {
            var identity = context.Items[Key] as IdentityContext;
            return identity ?? IdentityContext.AnonymousIdentity();
        }

        public static IdentityContext GetIdentity(this HttpContext context)
        {
            return context.Items[Key] as IdentityContext;
        }
        
        public static void SetIdentity(this HttpContextBase context, IdentityContext identity)
        {
            context.Items[Key] = identity;
        }

        public static void SetIdentity(this HttpContext context, IdentityContext identity)
        {
            context.Items[Key] = identity;
        }
    }
}

