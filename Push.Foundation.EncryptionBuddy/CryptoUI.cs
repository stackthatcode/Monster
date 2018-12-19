using System;
using System.Windows.Forms;
using Autofac;
using Monster.Middle;
using Monster.Middle.Config;
using Monster.Middle.Persist.Multitenant;
using Monster.Middle.Persist.Sys.Repositories;
using Monster.Middle.Services;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Utilities.Security;

namespace Push.Foundation
{
    public partial class CryptoUi : Form
    {
        public CryptoUi()
        {
            InitializeComponent();
            Load += OnLoad;
        }
        
        private void OnLoad(object sender, EventArgs eventArgs)
        {
            this.Width = 700;
            this.Height = 600;
        }        

        private void button1_Click(object sender, EventArgs e)
        {
            // *** COME ON, PUT THE TEXT IN SEPARATE FIELDS


            textShopifyXml.Text =
                $@"<shopifyCredentials 
    xdt:Transform=""Replace""
    ApiKey=""{textShopifyApiKey.Text.ToSecureString().DpApiEncryptString()}"" 
    ApiPassword=""{textShopifyApiPwd.Text.ToSecureString().DpApiEncryptString()}"" 
    ApiSecret=""{textShopifyApiSecret.Text.ToSecureString().DpApiEncryptString()}"" 
    Domain=""{textShopifyDomain.Text}""                        
/>";

            Clipboard.SetText(textShopifyXml.Text);
        }

        private void buttonEncrypt_Click(object sender, EventArgs e)
        {
            this.textEncryptedOutput.Text = 
                this.textNonEncrypted.Text.ToSecureString().DpApiEncryptString();
            Clipboard.SetText(textEncryptedOutput.Text);
        }

        private void buttonDecrypt_Click(object sender, EventArgs e)
        {
            this.textDecryptedOutput.Text =
                this.textEncryptedInput.Text.DpApiDecryptString().ToInsecureString();

            Clipboard.SetText(textDecryptedOutput.Text);
        }

        private void buttonAesEncrypt_Click(object sender, EventArgs e)
        {
            var crypto = new AesCrypto(this.textAesKey.Text, this.textAesIv.Text);

            this.textAesEncryptedOutput.Text = crypto.Encrypt(this.textAesPlaintext.Text);

            Clipboard.SetText(textAesEncryptedOutput.Text);
        }

        private void buttonAesDecrypt_Click(object sender, EventArgs e)
        {
            var crypto = new AesCrypto(this.textAesKey.Text, this.textAesIv.Text);

            this.textAesDecryptedOutput.Text = crypto.Decrypt(this.textAesEncryptedInput.Text);
            Clipboard.SetText(textAesDecryptedOutput.Text);
        }

        private void buttonHMAC256_Click(object sender, EventArgs e)
        {
            var hmacCrypto = new HmacCryptoService(this.textHMACSecret.Text);

            var hashedResult = 
                hmacCrypto.ToBase64EncodedSha256(this.textHMACPayload.Text);

            this.textHMACOutput.Text = hashedResult;
        }

        private void buttonAcumaticaXml_Click(object sender, EventArgs e)
        {

            textAcumaticaXml.Text =
                $@"<acumaticaSecurityConfiguration 
    xdt:Transform=""Replace""
    InstanceUrl=""{textAcumaticaUrl.Text}"" 
    CompanyName=""{textAcumaticaCompany.Text}"" 
    Branch=""{textAcumaticaBranch.Text}"" 
    Username=""{textAcumaticaUsername.Text.ToSecureString().DpApiEncryptString()}"" 
    Password=""{textAcumaticaPassword.Text.ToSecureString().DpApiEncryptString()}""                                               
/>";

            Clipboard.SetText(textAcumaticaXml.Text);
        }
        

        private void buttonMonsterSettings_Click(object sender, EventArgs e)
        {
            textMonsterConfig.Text =
                $@"<monsterConfig 
    xdt:Transform=""Replace""
    EncryptKey=""{this.textMonsterAesKey.Text.ToSecureString().DpApiEncryptString()}"" 
    EncryptIv=""{this.textMonsterAesIv.Text.ToSecureString().DpApiEncryptString()}"" 
    SystemDatabaseConnection=""{this.textMonsterSystemConnstr.Text}""
/>";
        }
        

        private void buttonShopifyLoadContext_Click(object sender, EventArgs e)
        {
            var builder = new ContainerBuilder();
            using (var container = MiddleAutofac.Build(builder).Build())
            using (var scope = container.BeginLifetimeScope())
            {                
                var logger = scope.Resolve<IPushLogger>();

                try
                {
                    var item = this.comboShopifyTenantId.SelectedItem as ComboboxItem;
                    var tenantId = Guid.Parse(item.Value.ToString());

                    var contextLoader = scope.Resolve<TenantContext>();
                    contextLoader.InitializePersistOnly(tenantId);

                    var repository = scope.Resolve<TenantRepository>();

                    repository.CreateIfMissingContext();
                    repository.UpdateContextShopify(
                            this.textShopifyDomain.Text,
                            this.textShopifyApiKey.Text,
                            this.textShopifyApiPwd.Text,
                            this.textShopifyApiSecret.Text);
                }
                catch (Exception ex)
                {
                   logger.Error(ex);
                    throw;
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            var builder = new ContainerBuilder();
            using (var container = MiddleAutofac.Build(builder).Build())
            using (var scope = container.BeginLifetimeScope())
            {
                var logger = scope.Resolve<IPushLogger>();

                try
                {
                    var item = this.comboAcumaticaTenantId.SelectedItem as ComboboxItem;
                    var tenantId = Guid.Parse(item.Value.ToString());

                    var contextLoader = scope.Resolve<TenantContext>();
                    contextLoader.InitializePersistOnly(tenantId);

                    var repository = scope.Resolve<TenantRepository>();

                    repository.CreateIfMissingContext();
                    repository.UpdateContextAcumatica(
                        this.textAcumaticaUrl.Text,
                        this.textAcumaticaCompany.Text,
                        this.textAcumaticaBranch.Text,
                        this.textAcumaticaUsername.Text,
                        this.textAcumaticaPassword.Text);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    throw;
                }
            }
        }


        private void GetTenants(ComboBox comboBox)
        {
            Helper.RunInLifetimeScope((scope) =>
            {
                var repository = scope.Resolve<SystemRepository>();
                var tenants = repository.RetrieveTenants();
                comboBox.Items.Clear();

                foreach (var t in tenants)
                {
                    var item = new ComboboxItem()
                    {
                        Text = t.Nickname,
                        Value = t.InstallationId,
                    };

                    comboBox.Items.Add(item);
                }
            });

        }

        private void buttonShopifyGetTenants_Click(object sender, EventArgs e)
        {
            GetTenants(this.comboShopifyTenantId);
        }

        private void buttonAcumaticaRefresh_Click(object sender, EventArgs e)
        {
            GetTenants(this.comboAcumaticaTenantId);
        }

        private void CryptoUi_Load(object sender, EventArgs e)
        {
            GetTenants(this.comboShopifyTenantId);
            GetTenants(this.comboAcumaticaTenantId);
            GetTenants(this.comboSummaryTenantId);
        }

        private void buttonSummaryRefreshTenant_Click(object sender, EventArgs e)
        {
            GetTenants(this.comboSummaryTenantId);
        }

        private void buttonSummaryGetCurrent_Click(object sender, EventArgs e)
        {
            Helper.RunInLifetimeScope((scope) =>
            {
                var item = this.comboSummaryTenantId.SelectedItem as ComboboxItem;
                var installationId = Guid.Parse(item.Value.ToString());

                var systemRepository = scope.Resolve<SystemRepository>();
                var tenant = systemRepository.RetrieveInstallation(installationId);

                var tenantContextLoader = scope.Resolve<TenantContext>();
                tenantContextLoader.InitializePersistOnly(installationId);

                var persistContext = scope.Resolve<PersistContext>();

                var repository = scope.Resolve<TenantRepository>();
                var tenantContext = repository.RetrieveRawTenant();

                var output =
                    $"System Connection = {MonsterConfig.Settings.SystemDatabaseConnection}" + Environment.NewLine +
                    $"Nickname = {tenant.Nickname}" + Environment.NewLine +
                    $"Instance Connection = {persistContext.ConnectionString}" + Environment.NewLine +
                    $"Acumatica URL = {tenantContext.AcumaticaInstanceUrl}" + Environment.NewLine +
                    $"Acumatica Company = {tenantContext.AcumaticaCompanyName}" + Environment.NewLine +
                    $"Acumatica Branch = {tenantContext.AcumaticaBranch}" + Environment.NewLine +
                    $"Shopify URL = {tenantContext.ShopifyDomain}" + Environment.NewLine;

                this.textSummary.Text = output;
            });

        }
    }        
}

