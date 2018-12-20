using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Monster.Middle.Persist.Multitenant;
using Monster.Web.Plumbing;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Utilities.Security;
using Push.Foundation.Web.Helpers;
using Push.Shopify.Api;
using Push.Shopify.Config;

namespace Monster.Web.Controllers
{
    public class ShopifyAuthController : Controller
    {
        private readonly OAuthApi _oAuthApi;
        private readonly TenantRepository _tenantRepository;
        private readonly IPushLogger _logger;

        private readonly
            List<string> _shopifyScopes = new List<string>()
                {
                    "read_orders",
                    "read_products",
                    "write_products",
                    "read_customers",
                    "read_locations",
                    "read_fulfillments",
                    "write_fulfillments",
                    "read_inventory",
                    "write_inventory",
                    "read_shipping",
                    "read_shopify_payments_payouts",
                };

        public ShopifyAuthController(
                IPushLogger logger, 
                OAuthApi oAuthApi, 
                TenantRepository tenantRepository)
        {
            _logger = logger;
            _oAuthApi = oAuthApi;
            _tenantRepository = tenantRepository;
        }


        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }


        // Shopify OAuth Authentication (Authorization) flow
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(string shop, string returnUrl)
        {
            // First strip everything off so we can standardize
            var fullShopDomain = shop.CorrectedShopUrl();

            var redirectUrl = GlobalConfig.Url("ShopifyAuth/Return");

            if (ShopifyCredentialsConfig.Settings.ApiKey.IsNullOrEmpty())
            {
                throw new Exception("Null or empty Shopify -> ApiKey - please check configuration");
            }

            var scopes = _shopifyScopes.ToCommaDelimited();

            var urlBase = $"https://{fullShopDomain}/admin/oauth/authorize";
            var queryString =
                new QueryStringBuilder()
                    .Add("client_id", ShopifyCredentialsConfig.Settings.ApiKey)
                    .Add("scope", scopes)
                    .Add("redirect_uri", redirectUrl)
                    .ToString();

            var finalUrl = $"{urlBase}?{queryString}";
            return Redirect(finalUrl);
        }
        

        [AllowAnonymous]
        public async Task<ActionResult> 
                    Return(string code, string shop, string returnUrl)
        {
            if (!VerifyShopifyHmac())
            {
                throw new Exception("Failed HMAC verification from Shopify Return");
            }

            var accessToken = _oAuthApi.RetrieveAccessToken(code);

            _tenantRepository.UpdateShopifyCredentials(accessToken);

            // Attempt to complete Shopify Authentication
            //var profitWiseSignIn = CompleteShopifyAuth(code, shop);

            return GlobalConfig.Redirect("");
        }


        // TODO - wire in using current framework
        //
        //private async Task<ProfitWiseSignIn> CompleteShopifyAuth(string code, string shopDomain)
        //{
        //    var apikey = ProfitWiseConfiguration.Settings.ShopifyApiKey;
        //    var apisecret = ProfitWiseConfiguration.Settings.ShopifyApiSecret;

        //    try
        //    {
        //        var nonAccessTokenCredentials =
        //            ShopifyCredentials.Build(shopDomain, apikey, apisecret);

        //        var oauthRepository = _factory.MakeOAuthRepository(nonAccessTokenCredentials);

        //        var accessToken = oauthRepository.RetrieveAccessToken(code);

        //        var credentials = ShopifyCredentials.Build(shopDomain, accessToken);
        //        var shopApiRepository = _factory.MakeShopApiRepository(credentials);
        //        var shopFromShopify = shopApiRepository.Retrieve();

        //        return new ProfitWiseSignIn
        //        {
        //            AccessToken = accessToken,
        //            Shop = shopFromShopify,
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(ex);
        //        return null;
        //    }
        //}


        private bool VerifyShopifyHmac()
        {
            // Extract and remove Shopify's HMAC parameter
            var queryStringDictionary = HttpContext.QueryStringToDictionary();
            var shopifyHmacHash = queryStringDictionary["hmac"].ToString();
            queryStringDictionary.Remove("hmac");

            // Lexographically order parameters and regenerate Query String
            var builder = new QueryStringBuilder();
            queryStringDictionary
                .OrderBy(x => x.Key)
                .ForEach(x => builder.Add(x.Key, x.Value));
            var queryString = builder.ToString();

            // Build HMAC digestion of query string...
            var hmacCrypto = new HmacCryptoService(ShopifyCredentialsConfig.Settings.ApiSecret);
            var hashedResult = hmacCrypto. ToHexStringSha256(queryString);

            // ... and compare
            return hashedResult == shopifyHmacHash;
        }

    }
}