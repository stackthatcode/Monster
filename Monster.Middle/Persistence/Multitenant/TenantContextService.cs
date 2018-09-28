using System.Linq;
using Monster.Acumatica.Config;
using Monster.Middle.Persistence;
using Push.Foundation.Utilities.Security;
using Push.Shopify.Config;

namespace Monster.Middle.Persist.Multitenant
{
    public class TenantContextService
    {
        private readonly MonsterDataContext _dataContext;
        private readonly ICryptoService _cryptoService;

        public TenantContextService(
                MonsterDataContext dataContext,
                ICryptoService cryptoService)
        {
            _dataContext = dataContext;
            _cryptoService = cryptoService;
        }


        // TODO => implement this https://stackoverflow.com/questions/202011/encrypt-and-decrypt-a-string-in-c/10366194#10366194

        public UsrAccountContext RetrieveRawContext()
        {
            return _dataContext.UsrAccountContexts.FirstOrDefault();
        }

        public AcumaticaCredentials RetrieveAcumaticaCredentials()
        {
            var context = RetrieveRawContext();

            var output = new AcumaticaCredentials();
            output.InstanceUrl = context.AcumaticaInstanceUrl;
            output.Branch = context.AcumaticaBranch;
            output.CompanyName = context.AcumaticaCompanyName;
            output.Username = _cryptoService.Decrypt(context.AcumaticaUsername);
            output.Password = _cryptoService.Decrypt(context.AcumaticaPassword);

            return output;
        }


        public ShopifyCredentials RetrieveShopifyCredentials()
        {
            var context = RetrieveRawContext();

            var output = new ShopifyCredentials();
            output.Domain = context.ShopifyDomain;
            output.ApiKey = _cryptoService.Decrypt(context.ShopifyApiKey);
            output.ApiPassword = _cryptoService.Decrypt(context.ShopifyApiPassword);
            output.ApiSecret = _cryptoService.Decrypt(context.ShopifyApiSecret);

            return output;
        }

        public bool ContextExists()
        {
            return _dataContext.UsrAccountContexts.Any();
        }

        public void InsertContext(UsrAccountContext context)
        {
            _dataContext.UsrAccountContexts.Add(context);
            _dataContext.SaveChanges();
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

            _dataContext.SaveChanges();
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

            _dataContext.SaveChanges();
        }
    }
}
