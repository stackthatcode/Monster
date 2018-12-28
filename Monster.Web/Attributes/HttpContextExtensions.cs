using System.Web;
using Monster.Middle.Security;

namespace Monster.Middle.Attributes
{
    public static class HttpContextExtensions
    {
        public const string Key = "AppRequestContext";        
        
        public static IdentityContext GetIdentity(this HttpContextBase context)
        {
            return context.Items[Key] as IdentityContext;
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

