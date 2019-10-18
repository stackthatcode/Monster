using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Monster.Middle.Identity;
using Monster.Middle.Misc.External;
using Monster.Middle.Misc.State;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Processes;
using Monster.Middle.Processes.Sync.Model.Misc;
using Monster.Web.Attributes;
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
    public class ShopifyAuthController : Controller
    {
        private readonly OAuthApi _oAuthApi;
        private readonly ExternalServiceRepository _connectionRepository;
        private readonly InstanceContext _connectionContext;
        private readonly ShopifyHttpContext _shopifyHttpContext;
        private readonly IdentityService _identityService;
        private readonly StateRepository _stateRepository;
        private readonly IPushLogger _logger;
        private readonly HmacCryptoService _hmacCrypto;
        

        private readonly
            List<string> _shopifyOAuthScopes = new List<string>()
                {
                    "read_customers",
                    "write_customers",
                    "read_orders",
                    "write_orders",
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
                OAuthApi oAuthApi, 
                ExternalServiceRepository connectionRepository,
                InstanceContext connectionContext,
                ShopifyHttpContext shopifyHttpContext, 
                IdentityService identityService,
                StateRepository stateRepository, 
                HmacCryptoService hmacCrypto,
                IPushLogger logger)
        {
            _logger = logger;
            _oAuthApi = oAuthApi;
            _connectionRepository = connectionRepository;
            _connectionContext = connectionContext;
            _shopifyHttpContext = shopifyHttpContext;
            _identityService = identityService;
            _stateRepository = stateRepository;
            _hmacCrypto = hmacCrypto;
        }


        [HttpGet]
        [IdentityProcessor]
        public ActionResult Domain()
        {
            var identity = HttpContext.GetIdentity();

            if (identity.IsAuthenticated)
            {
                var state = identity.SystemState;
                var shopifyConnection = _connectionRepository.Retrieve();

                var model = new ShopifyDomainModel();
                model.IsWizardMode = !state.IsRandomAccessMode;
                model.IsConnectionBroken = state.ShopifyConnState.IsBroken();
                model.CanEditShopifyUrl = !state.IsShopifyUrlFinalized;
                model.ShopDomain = shopifyConnection.ShopifyDomain;

                return View(model);
            }
            else
            {
                var model = new ShopifyDomainModel();
                model.IsWizardMode = true;
                model.IsConnectionBroken = false;
                model.CanEditShopifyUrl = true;
                model.ShopDomain = "";

                return View(model);
            }
        }

        // Shopify OAuth Authentication (Authorization) flow    
        //
        [IdentityProcessor]
        public ActionResult Login(string shop, string returnUrl)
        {
            // Guard against attempts to change finalized Shopify Domain
            string fullShopDomain;
           
            var identity = HttpContext.GetIdentity();
            if  (identity.IsAuthenticated && identity.SystemState.IsShopifyUrlFinalized)
            {
                var tenant = _connectionRepository.Retrieve();
                fullShopDomain = tenant.ShopifyDomain;
            }
            else
            {
                fullShopDomain = shop.CorrectedShopUrl();
            }
            
            var finalUrl = BuildShopifyRedirectUrl(fullShopDomain);
            return Redirect(finalUrl);
        }

        // No [IdentityProcessor] attribute - the Identity will be dictated by "shop" i.e. domain
        public ActionResult Return(string code, string shop, string returnUrl)
        {
            // Attempt to locate the identity, as there are both a valid Domain and Access Token
            //
            var identity = _identityService.HydrateIdentityContextByDomain(shop);

            if (!identity.IsAuthenticated)
            {
                throw new HttpException(
                    (int)HttpStatusCode.Unauthorized,
                    $"Attempt to login using non-provisioned Shopify store {shop}");
            }

            // Did the User hit the Back Button...? If so, the codeHash will be the same
            //
            var codeHash = _hmacCrypto.ToBase64EncodedSha256(code);
            if (_connectionRepository.IsSameAuthCode(codeHash))
            {
                return Redirect("Domain");
            }

            try
            {
                if (!VerifyShopifyHmac())
                {
                    throw new Exception("Failed HMAC verification from Shopify Return");
                }

                // Get Key and Secret credentials from config file - and there is not Instance-specific.
                // 
                var credentials = ShopifyCredentialsConfig.Settings.ToApiKeyAndSecret(shop);
                _shopifyHttpContext.Initialize(credentials);

                var accessToken = _oAuthApi.RetrieveAccessToken(code, credentials);
                _connectionContext.UpdateShopifyCredentials(shop, accessToken, codeHash);

                // Update IdentityContext 
                //
                var updatedState = _stateRepository.RetrieveSystemStateNoTracking();
                identity.UpdateState(updatedState);
                HttpContext.SetIdentity(identity);

                // Sign the User in
                //
                _identityService.SignInAspNetUser(identity.AspNetUserId);

                var model = new ReturnModel
                {
                    IsWizardMode = !updatedState.IsRandomAccessMode,
                    IsConnectionOk = true,
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _stateRepository.UpdateSystemState(x => x.ShopifyConnState, StateCode.SystemFault);

                var model = new ReturnModel
                {
                    IsWizardMode = !identity.SystemState.IsRandomAccessMode,
                    IsConnectionOk = false,
                };
                return View(model);
            }
        }


        private string BuildShopifyRedirectUrl(string fullShopDomain)
        {
            if (ShopifyCredentialsConfig.Settings.ApiKey.IsNullOrEmpty())
            {
                throw new Exception(
                    "Null or empty Shopify -> ApiKey - please check configuration");
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
            return finalUrl;
        }

        private bool VerifyShopifyHmac()
        {
            // Extract and remove Shopify's HMAC parameter
            //
            var queryStringDictionary = HttpContext.QueryStringToDictionary();
            var shopifyHmacHash = queryStringDictionary["hmac"].ToString();
            queryStringDictionary.Remove("hmac");

            // Lexographically order parameters and regenerate Query String
            //
            var builder = new QueryStringBuilder();
            queryStringDictionary
                .OrderBy(x => x.Key)
                .ForEach(x => builder.Add(x.Key, x.Value));
            var queryString = builder.ToString();

            // Build HMAC digestion of query string...
            //
            var hashedResult = _hmacCrypto. ToHexStringSha256(queryString);

            // ... and compare
            return hashedResult == shopifyHmacHash;
        }
    }
}

