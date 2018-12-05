using System.Configuration;
using System.Web;
using System.Web.Mvc;

namespace Monster.Web.Plumbing
{
    public class GlobalConfig
    {

#if DEBUG
        public const bool DebugMode = true;
        public const bool ReleaseMode = false;
#else
        public const bool DebugMode = false;
        public const bool ReleaseMode = true;
#endif

        public static readonly string BaseUrl = ConfigurationManager.AppSettings["application_root_url"];
        public static readonly string AppName = ConfigurationManager.AppSettings["application_name"];

        public static readonly string Organization = "Logic Automated LLC";
        public static readonly string SupportEmail = "aleksjones@gmail.com";

        public static string FullAppName 
                => $"{AppName} - Automated Shopify to Acumatica Synchronization";



        public static RedirectResult Redirect(string destinationUrl, string returnUrl = null)
        {
            var url = $"{BaseUrl}" + $"{destinationUrl}";

            if (returnUrl != null)
            {
                url += url.Contains("?") ? "&" : "?";
                url += $"returnUrl={HttpUtility.UrlEncode(returnUrl)}";
            }
            return new RedirectResult(url);
        }

        public static string Url(string relativepath)
        {
            return $"{BaseUrl}{relativepath}";
        }        

        public static string LoginPage => Url("Config/Home");
    }
}
