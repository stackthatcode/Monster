﻿using System;
using System.Windows.Forms;
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
            var config = JsonConvert.DeserializeObject<ShopifySecuritySettings>(textJson.Text);

            textXML.Text =
                $@"<shopifySecurityConfiguration 
    xdt:Transform=""Replace""
    ApiKey=""{config.ApiKey.ToSecureString().DpApiEncryptString()}"" 
    ApiPassword=""{config.ApiPassword.ToSecureString().DpApiEncryptString()}"" 
    ApiSecret=""{config.ApiSecret.ToSecureString().DpApiEncryptString()}"" 
    PrivateAppDomain=""{config.PrivateAppDomain}""                        
    EncryptKey=""{config.EncryptKey.ToSecureString().DpApiEncryptString()}""                        
    EncryptIv=""{config.EncryptIv.ToSecureString().DpApiEncryptString()}""                                                
/>";

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
    }        
}

