using System.Configuration;
using System.Web;
using System.Web.Mvc;

namespace Monster.Web.Plumbing
{
    public class GlobalConfig
    {
        public static readonly string AppName = "Odysseus - Automated Shopify to Acumatica Synchronization";
        public static readonly string Organization = "Logic Automated LLC";
        public static readonly string BaseUrl = ConfigurationManager.AppSettings["application_root_url"];
        public static readonly string LogoUrl = BaseUrl + "/Content/images/EWC_logo_circle.jpg";
        public static readonly string SupportEmail = "aleksjones@gmail.com";        
        public static readonly string FileUploadMaxSize = "(5 MB max.)";

#if DEBUG
        public const bool DebugMode = true;
        public const bool ReleaseMode = false;
#else
        public const bool DebugMode = false;
        public const bool ReleaseMode = true;
#endif

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
        

        public static int GoogleMapsDefaultZoom = 9;
        public static int GoogleMapsMaxZoom = 10;
    }
}
