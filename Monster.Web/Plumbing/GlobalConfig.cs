using System.Configuration;
using System.Web;
using System.Web.Mvc;

namespace Monster.Web.Plumbing
{
    public class GlobalConfig
    {
        public static readonly bool DemoMode;

        static GlobalConfig()
        {
#if DEBUG
            DemoMode = (ConfigurationManager.AppSettings["demodata"] ?? "") == "demodata";
#endif
        }

        public static readonly string BaseUrl = ConfigurationManager.AppSettings["application_root_url"];
        public static readonly string AppName = ConfigurationManager.AppSettings["application_name"];

        public static readonly string Organization = "Logic Automated Co.";
        public static readonly string SupportEmail = "info@logicautomated.com";

        public static readonly string DiagnosticsHome = Url("/Config/Diagnostics");

        public static string FullAppName => $"{AppName} for Shopify-Acumatica";



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

        public static string LoginPage => Url("/ShopifyAuth/Domain");
    }
}

