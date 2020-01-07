using System;
using System.Windows.Forms;
using Autofac;
using Monster.Middle;
using Monster.Middle.Config;
using Monster.Middle.Misc.External;
using Monster.Middle.Persist.Instance;
using Monster.Middle.Persist.Master;
using Push.Foundation.Utilities.Helpers;
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

        private void shopifyConfig_Click(object sender, EventArgs e)
        {
            // *** COME ON, PUT THE TEXT IN SEPARATE FIELDS
            // ... UPDATE 1/7/2020 - Or rly??
            //
            textShopifyXml.Text =
                $@"<shopifyCredentials 
    xdt:Transform=""Replace""
    ApiKey=""{textShopifyApiKey.Text.EncryptConfig()}"" 
    ApiPassword=""{textShopifyApiPwd.Text.EncryptConfig()}"" 
    ApiSecret=""{textShopifyApiSecret.Text.EncryptConfig()}"" 
    Domain=""{textShopifyDomain.Text}""                        
/>";

            Clipboard.SetText(textShopifyXml.Text);
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
    EncryptKey=""{this.textMonsterAesKey.Text.EncryptConfig()}"" 
    EncryptIv=""{this.textMonsterAesIv.Text.EncryptConfig()}"" 
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

                    var contextLoader = scope.Resolve<InstanceContext>();
                    contextLoader.InitializePersistOnly(tenantId);

                    var repository = scope.Resolve<CredentialsRepository>();

                    repository.CreateIfMissing();
                    repository.UpdateShopifyCredentials(
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

        private void shopifyConfig_Click_1(object sender, EventArgs e)
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

                    var contextLoader = scope.Resolve<InstanceContext>();
                    contextLoader.InitializePersistOnly(tenantId);

                    var repository = scope.Resolve<CredentialsRepository>();

                    repository.CreateIfMissing();
                    repository.UpdateAcumaticaCredentials(
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


        private void GetInstances(ComboBox comboBox)
        {
            Helper.RunInLifetimeScope((scope) =>
            {
                var repository = scope.Resolve<MasterRepository>();
                var instances = repository.RetrieveInstances();
                comboBox.Items.Clear();

                foreach (var t in instances)
                {
                    var item = new ComboboxItem()
                    {
                        Text = t.OwnerDomain ?? "(not assigned)",
                        Value = t.Id,
                    };

                    comboBox.Items.Add(item);
                }
            });

        }

        private void buttonShopifyGetTenants_Click(object sender, EventArgs e)
        {
            GetInstances(this.comboShopifyTenantId);
        }

        private void buttonAcumaticaRefresh_Click(object sender, EventArgs e)
        {
            GetInstances(this.comboAcumaticaTenantId);
        }

        private void CryptoUi_Load(object sender, EventArgs e)
        {
            GetInstances(this.comboShopifyTenantId);
            GetInstances(this.comboAcumaticaTenantId);
            GetInstances(this.comboSummaryTenantId);
        }

        private void buttonSummaryRefreshTenant_Click(object sender, EventArgs e)
        {
            GetInstances(this.comboSummaryTenantId);
        }

        private void buttonSummaryGetCurrent_Click(object sender, EventArgs e)
        {
            Helper.RunInLifetimeScope((scope) =>
            {
                var item = this.comboSummaryTenantId.SelectedItem as ComboboxItem;
                var installationId = Guid.Parse(item.Value.ToString());

                var systemRepository = scope.Resolve<MasterRepository>();
                var tenant = systemRepository.RetrieveInstance(installationId);

                var tenantContextLoader = scope.Resolve<InstanceContext>();
                tenantContextLoader.InitializePersistOnly(installationId);

                var persistContext = scope.Resolve<ProcessPersistContext>();

                var repository = scope.Resolve<CredentialsRepository>();
                var connection = repository.Retrieve();

                var notset = "(not set)";

                var output =
                    $"System Connection = {MonsterConfig.Settings.SystemDatabaseConnection}" + Environment.NewLine +
                    $"Nickname = {tenant.OwnerNickname ?? "(no instance assigned)"}" + Environment.NewLine +
                    $"Instance Connection = {persistContext.ConnectionString ?? "(no instance assigned)"}" + Environment.NewLine;

                if (connection != null)
                {
                    output +=
                        $"Acumatica URL = {connection.AcumaticaInstanceUrl.IsNullOrEmptyAlt(notset)}" +
                        Environment.NewLine +
                        $"Acumatica Company = {connection.AcumaticaCompanyName.IsNullOrEmptyAlt(notset)}" +
                        Environment.NewLine +
                        $"Acumatica Branch = {connection.AcumaticaBranch.IsNullOrEmptyAlt(notset)}" +
                        Environment.NewLine +
                        $"Shopify URL = {connection.ShopifyDomain.IsNullOrEmptyAlt(notset)}" + Environment.NewLine;
                }

                this.textSummary.Text = output;
            });
        }
    }
}

