using System.Data.Entity;
using System.Linq;
using Monster.Acumatica.Http;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Security;
using Push.Shopify.Http;
using Push.Shopify.Http.Credentials;

namespace Monster.Middle.Persist.Instance
{
    public class ExternalServiceRepository
    {
        private readonly ProcessPersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;

        private readonly ICryptoService _cryptoService;

        public ExternalServiceRepository(
                ProcessPersistContext dataContext,
                ICryptoService cryptoService)
        {
            _dataContext = dataContext;
            _cryptoService = cryptoService;
        }


        public DbContextTransaction BeginTransaction()
        {
            return Entities.Database.BeginTransaction();
        }

        // TODO => implement this https://stackoverflow.com/questions/202011/encrypt-and-decrypt-a-string-in-c/10366194#10366194


        
        public void CreateIfMissing()
        {
            if (!Entities.Connections.Any())
            {
                this.InsertConnection(new Connection());
            }
        }

        public void InsertConnection(Connection context)
        {
            Entities.Connections.Add(context);
            Entities.SaveChanges();
        }
        
        public Connection Retrieve()
        {
            CreateIfMissing();
            return Entities.Connections.FirstOrDefault();
        }

        

        // Shopify Credentials
        //
        public bool IsSameAuthCode(string code)
        {
            var tenant = Retrieve();
            return tenant.ShopifyAuthCodeHash == code;
        }

        public IShopifyCredentials RetrieveShopifyCredentials()
        {
            var connection = Retrieve();
            
            var accessToken =
                connection.ShopifyAccessToken.IsNullOrEmpty()
                    ? "" : _cryptoService.Decrypt(connection.ShopifyAccessToken);
            
            var apiKey =
                connection.ShopifyApiKey.IsNullOrEmpty() 
                    ? "" : _cryptoService.Decrypt(connection.ShopifyApiKey);

            var apiPassword = 
                connection.ShopifyApiPassword.IsNullOrEmpty()
                    ? "" : _cryptoService.Decrypt(connection.ShopifyApiPassword);

            var domain = new ShopDomain(connection.ShopifyDomain);

            if (accessToken.IsNullOrEmpty())
            {
                return null;
            }
            else
            {
                return new OAuthAccessToken(connection.ShopifyDomain, accessToken);
            }
        }
        
        public void UpdateShopifyCredentials(
                string shopifyDomain,
                string shopifyApiKey,
                string shopifyApiPassword,
                string shopifyApiSecret)
        {
            var context = Retrieve();

            var encryptedKey = _cryptoService.Encrypt(shopifyApiKey);
            var encryptedPassword = _cryptoService.Encrypt(shopifyApiPassword);
            var encryptedSecret = _cryptoService.Encrypt(shopifyApiSecret);

            context.ShopifyDomain = shopifyDomain;
            context.ShopifyApiKey = encryptedKey;
            context.ShopifyApiPassword = encryptedPassword;
            context.ShopifyApiSecret = encryptedSecret;

            Entities.SaveChanges();
        }

        public void UpdateShopifyCredentials(
                string shopifyDomain,
                string shopifyAccessToken,
                string shopifyAuthCodeHash)
        {
            var context = Retrieve();
            var encryptedAccessToken = _cryptoService.Encrypt(shopifyAccessToken);
            //var hashedAuthCode = _hmacService.Encrypt(shopifyAuthCode);

            context.ShopifyDomain = shopifyDomain;
            context.ShopifyAccessToken = encryptedAccessToken;
            context.ShopifyAuthCodeHash = shopifyAuthCodeHash;

            Entities.SaveChanges();
        }



        // Acumatica Credentials
        //
        public AcumaticaCredentials RetrieveAcumaticaCredentials()
        {
            var context = Retrieve();

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

            if (output.Username.IsNullOrEmpty() && output.Password.IsNullOrEmpty())
            {
                return null;
            }

            return output;
        }

        public void UpdateAcumaticaCredentials(
                string acumaticaInstanceUrl,
                string acumaticaBranch,
                string acumaticaCompanyName,
                string acumaticaUsername,
                string acumaticaPassword)
        {
            var context = Retrieve();

            var encryptedUsername = _cryptoService.Encrypt(acumaticaUsername);
            var encryptedPassword = _cryptoService.Encrypt(acumaticaPassword);

            context.AcumaticaInstanceUrl = acumaticaInstanceUrl;
            context.AcumaticaBranch = acumaticaBranch;
            context.AcumaticaCompanyName = acumaticaCompanyName;
            context.AcumaticaUsername = encryptedUsername;
            context.AcumaticaPassword = encryptedPassword;

            this.Entities.SaveChanges();
        }
        
        public void UpdateAcumaticaCredentials(
                string acumaticaUsername,
                string acumaticaPassword)
        {
            var context = Retrieve();

            var encryptedUsername = _cryptoService.Encrypt(acumaticaUsername);
            var encryptedPassword = _cryptoService.Encrypt(acumaticaPassword);

            context.AcumaticaUsername = encryptedUsername;
            context.AcumaticaPassword = encryptedPassword;

            this.Entities.SaveChanges();
        }
                
        public void SaveChanges()
        {
            this.Entities.SaveChanges();
        }
    }
}
