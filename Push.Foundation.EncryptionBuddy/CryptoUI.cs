using System;
using System.Windows.Forms;
using Monster.Acumatica.Config;
using Monster.Acumatica.Http;
using Newtonsoft.Json;
using Push.Foundation.Utilities.Security;
using Push.Shopify.Config;

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

//            var config = JsonConvert.DeserializeObject<Extensions>(textJson.Text);

//            textXML.Text =
//                $@"<shopifyCredentials 
//    xdt:Transform=""Replace""
//    ApiKey=""{config.ApiKey.ToSecureString().DpApiEncryptString()}"" 
//    ApiPassword=""{config.ApiPassword.ToSecureString().DpApiEncryptString()}"" 
//    ApiSecret=""{config.ApiSecret.ToSecureString().DpApiEncryptString()}"" 
//    Domain=""{config.Domain}""                        
///>";

            Clipboard.SetText(textXML.Text);
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
            var hmacCrypto = new HmacCryptoService();

            var hashedResult = 
                hmacCrypto.ToBase64EncodedSha256(
                        this.textHMACSecret.Text, this.textHMACPayload.Text);

            this.textHMACOutput.Text = hashedResult;
        }

        private void buttonAcumaticaXml_Click(object sender, EventArgs e)
        {
            
            textAcumaticaXml.Text =
                $@"<acumaticaSecurityConfiguration 
    xdt:Transform=""Replace""
    InstanceUrl=""{config.InstanceUrl}"" 
    CompanyName=""{config.CompanyName}"" 
    Branch=""{config.Branch}"" 
    Username=""{config.Username.ToSecureString().DpApiEncryptString()}"" 
    Password=""{config.Password.ToSecureString().DpApiEncryptString()}""                                               
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
    SystemConnectionString=""{this.textMonsterSystemConnstr.Text}""
/>";
        }
        
    }        
}

