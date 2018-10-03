using System.Linq;
using Monster.Acumatica.Http;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Security;
using Push.Shopify.Http;
using Push.Shopify.Http.Credentials;


namespace Monster.Middle.Persist.Multitenant
{
    public class TenantDataRepository
    {
        private readonly PersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        private readonly ICryptoService _cryptoService;

        public TenantDataRepository(
                PersistContext dataContext,
                ICryptoService cryptoService)
        {
            _dataContext = dataContext;
            _cryptoService = cryptoService;
        }


        // TODO => implement this https://stackoverflow.com/questions/202011/encrypt-and-decrypt-a-string-in-c/10366194#10366194

        public UsrTenantContext RetrieveRawContext()
        {
            return Entities.UsrTenantContexts.FirstOrDefault();
        }

        public AcumaticaCredentials RetrieveAcumaticaCredentials()
        {
            var context = RetrieveRawContext();

            var output = new AcumaticaCredentials();
            output.InstanceUrl = context.AcumaticaInstanceUrl;
            output.Branch = context.AcumaticaBranch;
            output.CompanyName = context.AcumaticaCompanyName;

            output.Username = 
                context.AcumaticaUsername.IsNullOrEmpty() 
                    ? "" : _cryptoService.Decrypt(context.AcumaticaUsername);

            output.Password = 
                context.AcumaticaPassword.IsNullOrEmpty()
                    ? "" : _cryptoService.Decrypt(context.AcumaticaPassword);

            return output;
        }

        public void CreateIfMissingContext()
        {
            if (!Entities.UsrTenantContexts.Any())
            {
                this.InsertContext(new UsrTenantContext()
                {
                    CompanyId = _dataContext.CompanyId
                });
            }
        }

        public PrivateAppCredentials RetrieveShopifyCredentials()
        {
            var context = RetrieveRawContext();

            var apiKey =
                context.ShopifyApiKey.IsNullOrEmpty() 
                    ? "" : _cryptoService.Decrypt(context.ShopifyApiKey);

            var apiPassword = 
                context.ShopifyApiPassword.IsNullOrEmpty()
                    ? "" : _cryptoService.Decrypt(context.ShopifyApiPassword);

            var domain = new ShopDomain(context.ShopifyDomain);

            var output = 
                new PrivateAppCredentials(apiKey, apiPassword, domain);
            
            return output;
        }

        public void InsertContext(UsrTenantContext context)
        {
            Entities.UsrTenantContexts.Add(context);
            Entities.SaveChanges();
        }

        public void UpdateContextShopify(
            string shopifyPrivateAppDomain,
            string shopifyApiKey,
            string shopifyApiPassword,
            string shopifyApiSecret)
        {
            var context = RetrieveRawContext();

            var encryptedKey = _cryptoService.Encrypt(shopifyApiKey);
            var encryptedPassword = _cryptoService.Encrypt(shopifyApiPassword);
            var encryptedSecret = _cryptoService.Encrypt(shopifyApiSecret);

            context.ShopifyDomain = shopifyPrivateAppDomain;
            context.ShopifyApiKey = encryptedKey;
            context.ShopifyApiPassword = encryptedPassword;
            context.ShopifyApiSecret = encryptedSecret;

            Entities.SaveChanges();
        }

        public void UpdateContextAcumatica(
            string acumaticaInstanceUrl,
            string acumaticaBranch,
            string acumaticaCompanyName,
            string acumaticaUsername,
            string acumaticaPassword)
        {
            var context = RetrieveRawContext();

            var encryptedUsername = _cryptoService.Encrypt(acumaticaUsername);
            var encryptedPassword = _cryptoService.Encrypt(acumaticaPassword);

            context.AcumaticaInstanceUrl = acumaticaInstanceUrl;
            context.AcumaticaBranch = acumaticaBranch;
            context.AcumaticaCompanyName = acumaticaCompanyName;
            context.AcumaticaUsername = encryptedUsername;
            context.AcumaticaPassword = encryptedPassword;

            this.Entities.SaveChanges();
        }
    }
}
