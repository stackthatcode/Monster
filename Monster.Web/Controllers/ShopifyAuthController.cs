using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Monster.Middle.Attributes;
using Monster.Middle.Hangfire;
using Monster.Middle.Persist.Multitenant;
using Monster.Web.Models.ShopifyAuth;
using Monster.Web.Plumbing;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Utilities.Security;
using Push.Foundation.Web.Helpers;
using Push.Shopify.Api;
using Push.Shopify.Config;
using Push.Shopify.Http;

namespace Monster.Web.Controllers
{
    [IdentityProcessor]
    public class ShopifyAuthController : Controller
    {
        private readonly OAuthApi _oAuthApi;
        private readonly TenantRepository _tenantRepository;
        private readonly ShopifyHttpContext _shopifyHttpContext;
        private readonly StateRepository _stateRepository;
        private readonly IPushLogger _logger;


        private readonly
            List<string> _shopifyOAuthScopes = new List<string>()
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
                TenantRepository tenantRepository, 
                ShopifyHttpContext shopifyHttpContext, 
                StateRepository stateRepository)
        {
            _logger = logger;
            _oAuthApi = oAuthApi;
            _tenantRepository = tenantRepository;
            _shopifyHttpContext = shopifyHttpContext;
            _stateRepository = stateRepository;
        }
        

        [HttpGet]
        public ActionResult Domain()
        {
            var tenant = _tenantRepository.Retrieve();
            var state = _stateRepository.RetrieveSystemState();

            var model = new DomainModel
            {
                IsShopifyConnectionOk = state.ShopifyConnection == SystemState.Ok,
                IsShopifyConnectionBroken = state.ShopifyConnection.IsBroken(),
                IsRandomAccessMode = state.IsRandomAccessMode,
                IsShopifyUrlFinalized = state.IsShopifyUrlFinalized,
                ShopDomain = tenant.ShopifyDomain,
            };

            return View(model);
        }

        // Shopify OAuth Authentication (Authorization) flow        
        public ActionResult Login(string shop, string returnUrl)
        {
            if (ShopifyCredentialsConfig.Settings.ApiKey.IsNullOrEmpty())
            {
                throw new Exception("Null or empty Shopify -> ApiKey - please check configuration");
            }

            // Guard against attempts to change finalized Shopify Domain
            string fullShopDomain;
            var state = _stateRepository.RetrieveSystemState();
            
            if (state.IsShopifyUrlFinalized)
            {
                var tenant = _tenantRepository.Retrieve();
                fullShopDomain = tenant.ShopifyDomain;
            }
            else
            {
                fullShopDomain = shop.CorrectedShopUrl();
            }
            
            // Build the Shopify OAuth request 
            var scopes = _shopifyOAuthScopes.ToCommaDelimited();
            var redirectUrl = GlobalConfig.Url("ShopifyAuth/Return");

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
        

        public async Task<ActionResult> 
                    Return(string code, string shop, string returnUrl)
        {
            // Did the User hit the Back Button...?
            if (_tenantRepository.IsSameAuthCode(code))
            {
                return Redirect("Domain");
            }

            // Get Key and Secret credentials from config file
            var credentials = ShopifyCredentialsConfig.Settings.ToApiKeyAndSecret(shop);
            _shopifyHttpContext.Initialize(credentials);

            try
            {

                if (!VerifyShopifyHmac())
                {
                    throw new Exception("Failed HMAC verification from Shopify Return");
                }

                // Get Access Token from Shopify and store
                var accessToken = _oAuthApi.RetrieveAccessToken(code, credentials);

                // Save Access Token and update State
                _tenantRepository.UpdateShopifyCredentials(accessToken, code);

                _stateRepository
                    .UpdateSystemState(x => x.ShopifyConnection, SystemState.Ok);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _stateRepository
                    .UpdateSystemState(x => x.ShopifyConnection, SystemState.SystemFault);

                return Redirect("Domain");
            }

            var state = _stateRepository.RetrieveSystemState();
            var model = new ReturnModel()
            {
                IsWizardMode = !state.IsRandomAccessMode
            };

            return View(model);
        }
        
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