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
using Monster.Web.Attributes;
using Monster.Web.Models.ShopifyAuth;
using Monster.Web.Plumbing;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Http;
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
        private readonly CredentialsRepository _connectionRepository;
        private readonly InstanceContext _connectionContext;
        private readonly ShopifyHttpContext _shopifyHttpContext;
        private readonly IdentityService _identityService;
        private readonly ProvisioningService _provisioningService;
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
                CredentialsRepository connectionRepository,
                InstanceContext connectionContext,
                ShopifyHttpContext shopifyHttpContext, 
                IdentityService identityService,
                StateRepository stateRepository, 
                HmacCryptoService hmacCrypto,
                ProvisioningService provisioningService,
                IPushLogger logger)
        {
            _provisioningService = provisioningService;
            _identityService = identityService;
            _oAuthApi = oAuthApi;
            _connectionRepository = connectionRepository;
            _connectionContext = connectionContext;
            _shopifyHttpContext = shopifyHttpContext;
            _stateRepository = stateRepository;
            _hmacCrypto = hmacCrypto;
            _logger = logger;
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
                model.CanEditShopifyUrl = !state.IsShopifyUrlFinalized();
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
            //
            string fullShopDomain;
           
            var identity = HttpContext.GetIdentity();
            if  (identity.IsAuthenticated && identity.SystemState.IsShopifyUrlFinalized())
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
        //
        public ActionResult Return(string code, string shop, string returnUrl)
        {

            // Attempt to locate the identity, as there maybe a valid Domain + Access Token
            // This needs to happen before try-catch, else failure will make it attempt to
            // update using System State Repository
            //
            var userId = _provisioningService.RetrieveUserIdByShopifyDomain(shop);

            var identity = _identityService.HydrateIdentity(userId);
            if (!identity.IsAuthenticated)
            {
                throw new HttpException(
                    (int)HttpStatusCode.Unauthorized, $"Attempt to login using invalid Shopify store {shop}");
            }

            try
            {
                if (!VerifyShopifyHmac())
                {
                    throw new Exception("Failed HMAC verification from Shopify Return");
                }


                if (BackButtonDetected(code))
                {
                    return Redirect("Domain");
                }

                // Get Key and Secret credentials from config file - and there is not Instance-specific.
                // 
                var credentials = ShopifyCredentialsConfig.Settings.ToApiKeyAndSecret(shop);
                _shopifyHttpContext.Initialize(credentials);

                // Refresh the Shopify Access Token
                //
                var codeHash = _hmacCrypto.ToBase64EncodedSha256(code);
                var accessToken = _oAuthApi.RetrieveAccessToken(code, credentials);
                _connectionContext.UpdateShopifyCredentials(shop, accessToken, codeHash);

                // Issue ASP.NET sign-in cookie
                //
                _identityService.SignInAspNetUser(identity.AspNetUserId);
                HttpContext.SetIdentity(identity);

                if (identity.SystemState.IsRandomAccessMode)
                {
                    return Redirect(GlobalConfig.Url("/Sync/EndToEnd"));
                }
                else
                {
                    return View(new ReturnModel
                    {
                        IsWizardMode = true,
                        IsConnectionOk = true,
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _stateRepository.UpdateSystemState(x => x.ShopifyConnState, StateCode.SystemFault);
                throw;
            }
        }


        private string BuildShopifyRedirectUrl(string fullShopDomain)
        {
            if (ShopifyCredentialsConfig.Settings.ApiKey.IsNullOrEmpty())
            {
                throw new Exception("Null or empty Shopify -> ApiKey - please check configuration");
            }

            // Build the Shopify OAuth request
            //
            var scopes = _shopifyOAuthScopes.ToCommaDelimited();
            var redirectUrl = GlobalConfig.Url($"/ShopifyAuth/Return");

            var urlBase = $"https://{fullShopDomain}/admin/oauth/authorize";
            var queryString =
                new QueryStringBuilder()
                    .Add("client_id", ShopifyCredentialsConfig.Settings.ApiKey)
                    .Add("scope", scopes)
                    .Add("redirect_uri", redirectUrl)
                    .ToString();

            return $"{urlBase}?{queryString}";
        }

        private bool BackButtonDetected(string code)
        {
            var codeHash = _hmacCrypto.ToBase64EncodedSha256(code);
            return _connectionRepository.IsSameAuthCode(codeHash);
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

