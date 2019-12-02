using System;

namespace Push.Foundation
{
    partial class CryptoUi
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.textHMACOutput = new System.Windows.Forms.TextBox();
            this.textHMACPayload = new System.Windows.Forms.TextBox();
            this.textHMACSecret = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.buttonHMAC256 = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label11 = new System.Windows.Forms.Label();
            this.textAesIv = new System.Windows.Forms.TextBox();
            this.textAesKey = new System.Windows.Forms.TextBox();
            this.textAesDecryptedOutput = new System.Windows.Forms.TextBox();
            this.textAesEncryptedInput = new System.Windows.Forms.TextBox();
            this.textAesEncryptedOutput = new System.Windows.Forms.TextBox();
            this.textAesPlaintext = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.buttonAesDecrypt = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.buttonAesEncrypt = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.tabPage7 = new System.Windows.Forms.TabPage();
            this.buttonSummaryRefreshTenant = new System.Windows.Forms.Button();
            this.comboSummaryTenantId = new System.Windows.Forms.ComboBox();
            this.label33 = new System.Windows.Forms.Label();
            this.textSummary = new System.Windows.Forms.TextBox();
            this.label32 = new System.Windows.Forms.Label();
            this.buttonSummaryGetCurrent = new System.Windows.Forms.Button();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.buttonAcumaticaRefresh = new System.Windows.Forms.Button();
            this.comboAcumaticaTenantId = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label31 = new System.Windows.Forms.Label();
            this.textAcumaticaPassword = new System.Windows.Forms.TextBox();
            this.textAcumaticaUsername = new System.Windows.Forms.TextBox();
            this.textAcumaticaBranch = new System.Windows.Forms.TextBox();
            this.textAcumaticaCompany = new System.Windows.Forms.TextBox();
            this.textAcumaticaUrl = new System.Windows.Forms.TextBox();
            this.textAcumaticaXml = new System.Windows.Forms.TextBox();
            this.label30 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.buttonAcumaticaXml = new System.Windows.Forms.Button();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.buttonShopifyGetTenants = new System.Windows.Forms.Button();
            this.comboShopifyTenantId = new System.Windows.Forms.ComboBox();
            this.buttonShopifyLoadContext = new System.Windows.Forms.Button();
            this.label25 = new System.Windows.Forms.Label();
            this.textShopifyDomain = new System.Windows.Forms.TextBox();
            this.textShopifyApiSecret = new System.Windows.Forms.TextBox();
            this.textShopifyApiPwd = new System.Windows.Forms.TextBox();
            this.textShopifyApiKey = new System.Windows.Forms.TextBox();
            this.textShopifyXml = new System.Windows.Forms.TextBox();
            this.label24 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.buttonShopifyXml = new System.Windows.Forms.Button();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.textMonsterSystemConnstr = new System.Windows.Forms.TextBox();
            this.textMonsterConfig = new System.Windows.Forms.TextBox();
            this.textMonsterAesIv = new System.Windows.Forms.TextBox();
            this.textMonsterAesKey = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.buttonMonsterSettings = new System.Windows.Forms.Button();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage7.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.tabPage6.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.textHMACOutput);
            this.tabPage3.Controls.Add(this.textHMACPayload);
            this.tabPage3.Controls.Add(this.textHMACSecret);
            this.tabPage3.Controls.Add(this.label14);
            this.tabPage3.Controls.Add(this.buttonHMAC256);
            this.tabPage3.Controls.Add(this.label13);
            this.tabPage3.Controls.Add(this.label12);
            this.tabPage3.Location = new System.Drawing.Point(10, 48);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(5);
            this.tabPage3.Size = new System.Drawing.Size(2255, 1292);
            this.tabPage3.TabIndex = 7;
            this.tabPage3.Text = "HMAC256";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // textHMACOutput
            // 
            this.textHMACOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textHMACOutput.Location = new System.Drawing.Point(352, 680);
            this.textHMACOutput.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textHMACOutput.Multiline = true;
            this.textHMACOutput.Name = "textHMACOutput";
            this.textHMACOutput.Size = new System.Drawing.Size(879, 235);
            this.textHMACOutput.TabIndex = 70;
            // 
            // textHMACPayload
            // 
            this.textHMACPayload.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textHMACPayload.Location = new System.Drawing.Point(352, 181);
            this.textHMACPayload.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textHMACPayload.Multiline = true;
            this.textHMACPayload.Name = "textHMACPayload";
            this.textHMACPayload.Size = new System.Drawing.Size(879, 235);
            this.textHMACPayload.TabIndex = 65;
            // 
            // textHMACSecret
            // 
            this.textHMACSecret.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textHMACSecret.Location = new System.Drawing.Point(352, 74);
            this.textHMACSecret.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textHMACSecret.Name = "textHMACSecret";
            this.textHMACSecret.Size = new System.Drawing.Size(879, 57);
            this.textHMACSecret.TabIndex = 64;
            this.textHMACSecret.WordWrap = false;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(112, 694);
            this.label14.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(130, 42);
            this.label14.TabIndex = 69;
            this.label14.Text = "Output";
            // 
            // buttonHMAC256
            // 
            this.buttonHMAC256.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.125F);
            this.buttonHMAC256.Location = new System.Drawing.Point(352, 496);
            this.buttonHMAC256.Margin = new System.Windows.Forms.Padding(5);
            this.buttonHMAC256.Name = "buttonHMAC256";
            this.buttonHMAC256.Size = new System.Drawing.Size(880, 107);
            this.buttonHMAC256.TabIndex = 68;
            this.buttonHMAC256.Text = "Encrypt + Copy";
            this.buttonHMAC256.UseVisualStyleBackColor = true;
            this.buttonHMAC256.Click += new System.EventHandler(this.buttonHMAC256_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(85, 86);
            this.label13.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(126, 42);
            this.label13.TabIndex = 63;
            this.label13.Text = "Secret";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(85, 196);
            this.label12.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(154, 42);
            this.label12.TabIndex = 62;
            this.label12.Text = "Payload";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.label11);
            this.tabPage2.Controls.Add(this.textAesIv);
            this.tabPage2.Controls.Add(this.textAesKey);
            this.tabPage2.Controls.Add(this.textAesDecryptedOutput);
            this.tabPage2.Controls.Add(this.textAesEncryptedInput);
            this.tabPage2.Controls.Add(this.textAesEncryptedOutput);
            this.tabPage2.Controls.Add(this.textAesPlaintext);
            this.tabPage2.Controls.Add(this.label9);
            this.tabPage2.Controls.Add(this.label10);
            this.tabPage2.Controls.Add(this.buttonAesDecrypt);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.buttonAesEncrypt);
            this.tabPage2.Controls.Add(this.label6);
            this.tabPage2.Controls.Add(this.label7);
            this.tabPage2.Controls.Add(this.label8);
            this.tabPage2.Location = new System.Drawing.Point(10, 48);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(5);
            this.tabPage2.Size = new System.Drawing.Size(2255, 1292);
            this.tabPage2.TabIndex = 6;
            this.tabPage2.Text = "AES Crypto";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(360, 129);
            this.label11.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(0, 42);
            this.label11.TabIndex = 75;
            // 
            // textAesIv
            // 
            this.textAesIv.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
            this.textAesIv.Location = new System.Drawing.Point(576, 231);
            this.textAesIv.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textAesIv.Name = "textAesIv";
            this.textAesIv.Size = new System.Drawing.Size(879, 57);
            this.textAesIv.TabIndex = 74;
            this.textAesIv.Text = "1234567890123456";
            this.textAesIv.WordWrap = false;
            // 
            // textAesKey
            // 
            this.textAesKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textAesKey.Location = new System.Drawing.Point(576, 129);
            this.textAesKey.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textAesKey.Name = "textAesKey";
            this.textAesKey.Size = new System.Drawing.Size(879, 57);
            this.textAesKey.TabIndex = 72;
            this.textAesKey.Text = "12345678901234567890123456789012";
            this.textAesKey.WordWrap = false;
            // 
            // textAesDecryptedOutput
            // 
            this.textAesDecryptedOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textAesDecryptedOutput.Location = new System.Drawing.Point(576, 971);
            this.textAesDecryptedOutput.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textAesDecryptedOutput.Name = "textAesDecryptedOutput";
            this.textAesDecryptedOutput.Size = new System.Drawing.Size(879, 57);
            this.textAesDecryptedOutput.TabIndex = 69;
            this.textAesDecryptedOutput.WordWrap = false;
            // 
            // textAesEncryptedInput
            // 
            this.textAesEncryptedInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textAesEncryptedInput.Location = new System.Drawing.Point(576, 878);
            this.textAesEncryptedInput.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textAesEncryptedInput.Name = "textAesEncryptedInput";
            this.textAesEncryptedInput.Size = new System.Drawing.Size(879, 57);
            this.textAesEncryptedInput.TabIndex = 66;
            this.textAesEncryptedInput.WordWrap = false;
            // 
            // textAesEncryptedOutput
            // 
            this.textAesEncryptedOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
            this.textAesEncryptedOutput.Location = new System.Drawing.Point(576, 546);
            this.textAesEncryptedOutput.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textAesEncryptedOutput.Name = "textAesEncryptedOutput";
            this.textAesEncryptedOutput.Size = new System.Drawing.Size(879, 57);
            this.textAesEncryptedOutput.TabIndex = 64;
            this.textAesEncryptedOutput.WordWrap = false;
            // 
            // textAesPlaintext
            // 
            this.textAesPlaintext.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textAesPlaintext.Location = new System.Drawing.Point(576, 441);
            this.textAesPlaintext.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textAesPlaintext.Name = "textAesPlaintext";
            this.textAesPlaintext.Size = new System.Drawing.Size(879, 57);
            this.textAesPlaintext.TabIndex = 62;
            this.textAesPlaintext.WordWrap = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(173, 231);
            this.label9.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(315, 42);
            this.label9.TabIndex = 73;
            this.label9.Text = "AES IV (16-bytes)";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(176, 129);
            this.label10.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(346, 42);
            this.label10.TabIndex = 71;
            this.label10.Text = "AES Key (32-bytes)";
            // 
            // buttonAesDecrypt
            // 
            this.buttonAesDecrypt.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.125F);
            this.buttonAesDecrypt.Location = new System.Drawing.Point(576, 1061);
            this.buttonAesDecrypt.Margin = new System.Windows.Forms.Padding(5);
            this.buttonAesDecrypt.Name = "buttonAesDecrypt";
            this.buttonAesDecrypt.Size = new System.Drawing.Size(880, 107);
            this.buttonAesDecrypt.TabIndex = 70;
            this.buttonAesDecrypt.Text = "Decrypt + Copy";
            this.buttonAesDecrypt.UseVisualStyleBackColor = true;
            this.buttonAesDecrypt.Click += new System.EventHandler(this.buttonAesDecrypt_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(173, 992);
            this.label5.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(311, 42);
            this.label5.TabIndex = 68;
            this.label5.Text = "Decrypted Output";
            // 
            // buttonAesEncrypt
            // 
            this.buttonAesEncrypt.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.125F);
            this.buttonAesEncrypt.Location = new System.Drawing.Point(576, 644);
            this.buttonAesEncrypt.Margin = new System.Windows.Forms.Padding(5);
            this.buttonAesEncrypt.Name = "buttonAesEncrypt";
            this.buttonAesEncrypt.Size = new System.Drawing.Size(880, 107);
            this.buttonAesEncrypt.TabIndex = 67;
            this.buttonAesEncrypt.Text = "Encrypt + Copy";
            this.buttonAesEncrypt.UseVisualStyleBackColor = true;
            this.buttonAesEncrypt.Click += new System.EventHandler(this.buttonAesEncrypt_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(173, 892);
            this.label6.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(279, 42);
            this.label6.TabIndex = 65;
            this.label6.Text = "Encrypted Input";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(173, 546);
            this.label7.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(309, 42);
            this.label7.TabIndex = 63;
            this.label7.Text = "Encrypted Output";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(176, 441);
            this.label8.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(276, 42);
            this.label8.TabIndex = 61;
            this.label8.Text = "Plain Text Input";
            // 
            // tabPage7
            // 
            this.tabPage7.Controls.Add(this.buttonSummaryRefreshTenant);
            this.tabPage7.Controls.Add(this.comboSummaryTenantId);
            this.tabPage7.Controls.Add(this.label33);
            this.tabPage7.Controls.Add(this.textSummary);
            this.tabPage7.Controls.Add(this.label32);
            this.tabPage7.Controls.Add(this.buttonSummaryGetCurrent);
            this.tabPage7.Location = new System.Drawing.Point(10, 48);
            this.tabPage7.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.tabPage7.Name = "tabPage7";
            this.tabPage7.Padding = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.tabPage7.Size = new System.Drawing.Size(2255, 1292);
            this.tabPage7.TabIndex = 10;
            this.tabPage7.Text = "Config Summary";
            this.tabPage7.UseVisualStyleBackColor = true;
            // 
            // buttonSummaryRefreshTenant
            // 
            this.buttonSummaryRefreshTenant.Location = new System.Drawing.Point(1928, 72);
            this.buttonSummaryRefreshTenant.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.buttonSummaryRefreshTenant.Name = "buttonSummaryRefreshTenant";
            this.buttonSummaryRefreshTenant.Size = new System.Drawing.Size(211, 86);
            this.buttonSummaryRefreshTenant.TabIndex = 102;
            this.buttonSummaryRefreshTenant.Text = "Refresh ";
            this.buttonSummaryRefreshTenant.UseVisualStyleBackColor = true;
            this.buttonSummaryRefreshTenant.Click += new System.EventHandler(this.buttonSummaryRefreshTenant_Click);
            // 
            // comboSummaryTenantId
            // 
            this.comboSummaryTenantId.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboSummaryTenantId.FormattingEnabled = true;
            this.comboSummaryTenantId.Location = new System.Drawing.Point(395, 91);
            this.comboSummaryTenantId.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.comboSummaryTenantId.Name = "comboSummaryTenantId";
            this.comboSummaryTenantId.Size = new System.Drawing.Size(1484, 59);
            this.comboSummaryTenantId.TabIndex = 101;
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label33.Location = new System.Drawing.Point(51, 91);
            this.label33.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(235, 42);
            this.label33.TabIndex = 100;
            this.label33.Text = "Installation Id";
            // 
            // textSummary
            // 
            this.textSummary.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textSummary.Location = new System.Drawing.Point(395, 181);
            this.textSummary.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textSummary.Multiline = true;
            this.textSummary.Name = "textSummary";
            this.textSummary.Size = new System.Drawing.Size(1737, 450);
            this.textSummary.TabIndex = 67;
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label32.Location = new System.Drawing.Point(51, 224);
            this.label32.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(260, 42);
            this.label32.TabIndex = 66;
            this.label32.Text = "Current Config";
            // 
            // buttonSummaryGetCurrent
            // 
            this.buttonSummaryGetCurrent.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSummaryGetCurrent.Location = new System.Drawing.Point(395, 696);
            this.buttonSummaryGetCurrent.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.buttonSummaryGetCurrent.Name = "buttonSummaryGetCurrent";
            this.buttonSummaryGetCurrent.Size = new System.Drawing.Size(1744, 110);
            this.buttonSummaryGetCurrent.TabIndex = 65;
            this.buttonSummaryGetCurrent.Text = "Retrieve Current Config";
            this.buttonSummaryGetCurrent.UseVisualStyleBackColor = true;
            this.buttonSummaryGetCurrent.Click += new System.EventHandler(this.buttonSummaryGetCurrent_Click);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.buttonAcumaticaRefresh);
            this.tabPage4.Controls.Add(this.comboAcumaticaTenantId);
            this.tabPage4.Controls.Add(this.button1);
            this.tabPage4.Controls.Add(this.label31);
            this.tabPage4.Controls.Add(this.textAcumaticaPassword);
            this.tabPage4.Controls.Add(this.textAcumaticaUsername);
            this.tabPage4.Controls.Add(this.textAcumaticaBranch);
            this.tabPage4.Controls.Add(this.textAcumaticaCompany);
            this.tabPage4.Controls.Add(this.textAcumaticaUrl);
            this.tabPage4.Controls.Add(this.textAcumaticaXml);
            this.tabPage4.Controls.Add(this.label30);
            this.tabPage4.Controls.Add(this.label16);
            this.tabPage4.Controls.Add(this.label26);
            this.tabPage4.Controls.Add(this.label27);
            this.tabPage4.Controls.Add(this.label29);
            this.tabPage4.Controls.Add(this.label15);
            this.tabPage4.Controls.Add(this.buttonAcumaticaXml);
            this.tabPage4.Location = new System.Drawing.Point(10, 48);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.tabPage4.Size = new System.Drawing.Size(2255, 1292);
            this.tabPage4.TabIndex = 8;
            this.tabPage4.Text = "Acumatica Config";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // buttonAcumaticaRefresh
            // 
            this.buttonAcumaticaRefresh.Location = new System.Drawing.Point(1563, 994);
            this.buttonAcumaticaRefresh.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.buttonAcumaticaRefresh.Name = "buttonAcumaticaRefresh";
            this.buttonAcumaticaRefresh.Size = new System.Drawing.Size(211, 86);
            this.buttonAcumaticaRefresh.TabIndex = 99;
            this.buttonAcumaticaRefresh.Text = "Refresh ";
            this.buttonAcumaticaRefresh.UseVisualStyleBackColor = true;
            this.buttonAcumaticaRefresh.Click += new System.EventHandler(this.buttonAcumaticaRefresh_Click);
            // 
            // comboAcumaticaTenantId
            // 
            this.comboAcumaticaTenantId.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboAcumaticaTenantId.FormattingEnabled = true;
            this.comboAcumaticaTenantId.Location = new System.Drawing.Point(416, 1004);
            this.comboAcumaticaTenantId.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.comboAcumaticaTenantId.Name = "comboAcumaticaTenantId";
            this.comboAcumaticaTenantId.Size = new System.Drawing.Size(1124, 59);
            this.comboAcumaticaTenantId.TabIndex = 98;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(416, 1114);
            this.button1.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(1139, 110);
            this.button1.TabIndex = 97;
            this.button1.Text = "Load into Installation Context Persistence";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.shopifyConfig_Click_1);
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label31.Location = new System.Drawing.Point(83, 1004);
            this.label31.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(235, 42);
            this.label31.TabIndex = 95;
            this.label31.Text = "Installation Id";
            // 
            // textAcumaticaPassword
            // 
            this.textAcumaticaPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textAcumaticaPassword.Location = new System.Drawing.Point(416, 496);
            this.textAcumaticaPassword.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textAcumaticaPassword.Name = "textAcumaticaPassword";
            this.textAcumaticaPassword.Size = new System.Drawing.Size(1132, 57);
            this.textAcumaticaPassword.TabIndex = 94;
            this.textAcumaticaPassword.Text = "l0c4lInstance";
            this.textAcumaticaPassword.WordWrap = false;
            // 
            // textAcumaticaUsername
            // 
            this.textAcumaticaUsername.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textAcumaticaUsername.Location = new System.Drawing.Point(416, 403);
            this.textAcumaticaUsername.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textAcumaticaUsername.Name = "textAcumaticaUsername";
            this.textAcumaticaUsername.Size = new System.Drawing.Size(1132, 57);
            this.textAcumaticaUsername.TabIndex = 92;
            this.textAcumaticaUsername.Text = "admin";
            this.textAcumaticaUsername.WordWrap = false;
            // 
            // textAcumaticaBranch
            // 
            this.textAcumaticaBranch.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textAcumaticaBranch.Location = new System.Drawing.Point(416, 289);
            this.textAcumaticaBranch.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textAcumaticaBranch.Name = "textAcumaticaBranch";
            this.textAcumaticaBranch.Size = new System.Drawing.Size(1132, 57);
            this.textAcumaticaBranch.TabIndex = 90;
            this.textAcumaticaBranch.Text = "MYCOMPANY";
            this.textAcumaticaBranch.WordWrap = false;
            // 
            // textAcumaticaCompany
            // 
            this.textAcumaticaCompany.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textAcumaticaCompany.Location = new System.Drawing.Point(416, 176);
            this.textAcumaticaCompany.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textAcumaticaCompany.Name = "textAcumaticaCompany";
            this.textAcumaticaCompany.Size = new System.Drawing.Size(1132, 57);
            this.textAcumaticaCompany.TabIndex = 88;
            this.textAcumaticaCompany.Text = "MyCompany";
            this.textAcumaticaCompany.WordWrap = false;
            // 
            // textAcumaticaUrl
            // 
            this.textAcumaticaUrl.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textAcumaticaUrl.Location = new System.Drawing.Point(416, 69);
            this.textAcumaticaUrl.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textAcumaticaUrl.Name = "textAcumaticaUrl";
            this.textAcumaticaUrl.Size = new System.Drawing.Size(1132, 57);
            this.textAcumaticaUrl.TabIndex = 86;
            this.textAcumaticaUrl.Text = "http://localhost/AcuInst2";
            this.textAcumaticaUrl.WordWrap = false;
            // 
            // textAcumaticaXml
            // 
            this.textAcumaticaXml.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textAcumaticaXml.Location = new System.Drawing.Point(416, 608);
            this.textAcumaticaXml.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textAcumaticaXml.Multiline = true;
            this.textAcumaticaXml.Name = "textAcumaticaXml";
            this.textAcumaticaXml.Size = new System.Drawing.Size(1132, 138);
            this.textAcumaticaXml.TabIndex = 64;
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label30.Location = new System.Drawing.Point(83, 496);
            this.label30.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(183, 42);
            this.label30.TabIndex = 93;
            this.label30.Text = "Password";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(83, 403);
            this.label16.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(190, 42);
            this.label16.TabIndex = 91;
            this.label16.Text = "Username";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label26.Location = new System.Drawing.Point(83, 298);
            this.label26.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(137, 42);
            this.label26.TabIndex = 89;
            this.label26.Text = "Branch";
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label27.Location = new System.Drawing.Point(83, 186);
            this.label27.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(178, 42);
            this.label27.TabIndex = 87;
            this.label27.Text = "Company";
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label29.Location = new System.Drawing.Point(83, 79);
            this.label29.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(244, 42);
            this.label29.TabIndex = 85;
            this.label29.Text = "Instance URL";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(83, 615);
            this.label15.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(213, 42);
            this.label15.TabIndex = 63;
            this.label15.Text = "Config XML";
            // 
            // buttonAcumaticaXml
            // 
            this.buttonAcumaticaXml.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAcumaticaXml.Location = new System.Drawing.Point(416, 792);
            this.buttonAcumaticaXml.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.buttonAcumaticaXml.Name = "buttonAcumaticaXml";
            this.buttonAcumaticaXml.Size = new System.Drawing.Size(1139, 110);
            this.buttonAcumaticaXml.TabIndex = 62;
            this.buttonAcumaticaXml.Text = "Generate XML config settings and CTRL+C all of it!";
            this.buttonAcumaticaXml.UseVisualStyleBackColor = true;
            this.buttonAcumaticaXml.Click += new System.EventHandler(this.buttonAcumaticaXml_Click);
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.buttonShopifyGetTenants);
            this.tabPage5.Controls.Add(this.comboShopifyTenantId);
            this.tabPage5.Controls.Add(this.buttonShopifyLoadContext);
            this.tabPage5.Controls.Add(this.label25);
            this.tabPage5.Controls.Add(this.textShopifyDomain);
            this.tabPage5.Controls.Add(this.textShopifyApiSecret);
            this.tabPage5.Controls.Add(this.textShopifyApiPwd);
            this.tabPage5.Controls.Add(this.textShopifyApiKey);
            this.tabPage5.Controls.Add(this.textShopifyXml);
            this.tabPage5.Controls.Add(this.label24);
            this.tabPage5.Controls.Add(this.label23);
            this.tabPage5.Controls.Add(this.label22);
            this.tabPage5.Controls.Add(this.label21);
            this.tabPage5.Controls.Add(this.label28);
            this.tabPage5.Controls.Add(this.buttonShopifyXml);
            this.tabPage5.Location = new System.Drawing.Point(10, 48);
            this.tabPage5.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(5);
            this.tabPage5.Size = new System.Drawing.Size(2255, 1292);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "Shopify Config";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // buttonShopifyGetTenants
            // 
            this.buttonShopifyGetTenants.Location = new System.Drawing.Point(1552, 987);
            this.buttonShopifyGetTenants.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.buttonShopifyGetTenants.Name = "buttonShopifyGetTenants";
            this.buttonShopifyGetTenants.Size = new System.Drawing.Size(211, 86);
            this.buttonShopifyGetTenants.TabIndex = 89;
            this.buttonShopifyGetTenants.Text = "Refresh ";
            this.buttonShopifyGetTenants.UseVisualStyleBackColor = true;
            this.buttonShopifyGetTenants.Click += new System.EventHandler(this.buttonShopifyGetTenants_Click);
            // 
            // comboShopifyTenantId
            // 
            this.comboShopifyTenantId.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboShopifyTenantId.FormattingEnabled = true;
            this.comboShopifyTenantId.Location = new System.Drawing.Point(405, 1006);
            this.comboShopifyTenantId.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.comboShopifyTenantId.Name = "comboShopifyTenantId";
            this.comboShopifyTenantId.Size = new System.Drawing.Size(1124, 59);
            this.comboShopifyTenantId.TabIndex = 88;
            // 
            // buttonShopifyLoadContext
            // 
            this.buttonShopifyLoadContext.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonShopifyLoadContext.Location = new System.Drawing.Point(405, 1123);
            this.buttonShopifyLoadContext.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.buttonShopifyLoadContext.Name = "buttonShopifyLoadContext";
            this.buttonShopifyLoadContext.Size = new System.Drawing.Size(1139, 110);
            this.buttonShopifyLoadContext.TabIndex = 87;
            this.buttonShopifyLoadContext.Text = "Load into Installation Context Persistence";
            this.buttonShopifyLoadContext.UseVisualStyleBackColor = true;
            this.buttonShopifyLoadContext.Click += new System.EventHandler(this.buttonShopifyLoadContext_Click);
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label25.Location = new System.Drawing.Point(72, 1013);
            this.label25.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(235, 42);
            this.label25.TabIndex = 85;
            this.label25.Text = "Installation Id";
            // 
            // textShopifyDomain
            // 
            this.textShopifyDomain.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textShopifyDomain.Location = new System.Drawing.Point(405, 432);
            this.textShopifyDomain.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textShopifyDomain.Name = "textShopifyDomain";
            this.textShopifyDomain.Size = new System.Drawing.Size(1132, 57);
            this.textShopifyDomain.TabIndex = 84;
            this.textShopifyDomain.Text = "bridge-over-monsters.myshopify.com";
            this.textShopifyDomain.WordWrap = false;
            // 
            // textShopifyApiSecret
            // 
            this.textShopifyApiSecret.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textShopifyApiSecret.Location = new System.Drawing.Point(405, 317);
            this.textShopifyApiSecret.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textShopifyApiSecret.Name = "textShopifyApiSecret";
            this.textShopifyApiSecret.Size = new System.Drawing.Size(1132, 57);
            this.textShopifyApiSecret.TabIndex = 82;
            this.textShopifyApiSecret.Text = "773ede6e12f63cc260fe74b1d3817650";
            this.textShopifyApiSecret.WordWrap = false;
            // 
            // textShopifyApiPwd
            // 
            this.textShopifyApiPwd.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textShopifyApiPwd.Location = new System.Drawing.Point(405, 205);
            this.textShopifyApiPwd.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textShopifyApiPwd.Name = "textShopifyApiPwd";
            this.textShopifyApiPwd.Size = new System.Drawing.Size(1132, 57);
            this.textShopifyApiPwd.TabIndex = 80;
            this.textShopifyApiPwd.Text = "2b9672727ac6f2f868a624081bcb7b93";
            this.textShopifyApiPwd.WordWrap = false;
            // 
            // textShopifyApiKey
            // 
            this.textShopifyApiKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textShopifyApiKey.Location = new System.Drawing.Point(405, 98);
            this.textShopifyApiKey.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textShopifyApiKey.Name = "textShopifyApiKey";
            this.textShopifyApiKey.Size = new System.Drawing.Size(1132, 57);
            this.textShopifyApiKey.TabIndex = 78;
            this.textShopifyApiKey.Text = "cf690d0cba40a18f21557ab6b6601ffe";
            this.textShopifyApiKey.WordWrap = false;
            // 
            // textShopifyXml
            // 
            this.textShopifyXml.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textShopifyXml.Location = new System.Drawing.Point(405, 587);
            this.textShopifyXml.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textShopifyXml.Multiline = true;
            this.textShopifyXml.Name = "textShopifyXml";
            this.textShopifyXml.Size = new System.Drawing.Size(1124, 159);
            this.textShopifyXml.TabIndex = 59;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label24.Location = new System.Drawing.Point(72, 432);
            this.label24.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(146, 42);
            this.label24.TabIndex = 83;
            this.label24.Text = "Domain";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label23.Location = new System.Drawing.Point(72, 327);
            this.label23.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(195, 42);
            this.label23.TabIndex = 81;
            this.label23.Text = "API Secret";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label22.Location = new System.Drawing.Point(72, 215);
            this.label22.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(252, 42);
            this.label22.TabIndex = 79;
            this.label22.Text = "API Password";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label21.Location = new System.Drawing.Point(72, 107);
            this.label21.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(152, 42);
            this.label21.TabIndex = 77;
            this.label21.Text = "API Key";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label28.Location = new System.Drawing.Point(72, 594);
            this.label28.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(213, 42);
            this.label28.TabIndex = 58;
            this.label28.Text = "Config XML";
            // 
            // buttonShopifyXml
            // 
            this.buttonShopifyXml.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonShopifyXml.Location = new System.Drawing.Point(405, 811);
            this.buttonShopifyXml.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.buttonShopifyXml.Name = "buttonShopifyXml";
            this.buttonShopifyXml.Size = new System.Drawing.Size(1139, 110);
            this.buttonShopifyXml.TabIndex = 57;
            this.buttonShopifyXml.Text = "Generate XML config settings and CTRL+C all of it!";
            this.buttonShopifyXml.UseVisualStyleBackColor = true;
            this.buttonShopifyXml.Click += new System.EventHandler(this.shopifyConfig_Click);
            // 
            // tabPage6
            // 
            this.tabPage6.Controls.Add(this.textMonsterSystemConnstr);
            this.tabPage6.Controls.Add(this.textMonsterConfig);
            this.tabPage6.Controls.Add(this.textMonsterAesIv);
            this.tabPage6.Controls.Add(this.textMonsterAesKey);
            this.tabPage6.Controls.Add(this.label20);
            this.tabPage6.Controls.Add(this.label19);
            this.tabPage6.Controls.Add(this.buttonMonsterSettings);
            this.tabPage6.Controls.Add(this.label17);
            this.tabPage6.Controls.Add(this.label18);
            this.tabPage6.Location = new System.Drawing.Point(10, 48);
            this.tabPage6.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Padding = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.tabPage6.Size = new System.Drawing.Size(2255, 1292);
            this.tabPage6.TabIndex = 9;
            this.tabPage6.Text = "Monster Config";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // textMonsterSystemConnstr
            // 
            this.textMonsterSystemConnstr.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
            this.textMonsterSystemConnstr.Location = new System.Drawing.Point(635, 343);
            this.textMonsterSystemConnstr.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textMonsterSystemConnstr.Name = "textMonsterSystemConnstr";
            this.textMonsterSystemConnstr.Size = new System.Drawing.Size(1132, 57);
            this.textMonsterSystemConnstr.TabIndex = 83;
            this.textMonsterSystemConnstr.Text = "Server=localhost; Database=MonsterSys; Trusted_Connection=True;";
            this.textMonsterSystemConnstr.WordWrap = false;
            // 
            // textMonsterConfig
            // 
            this.textMonsterConfig.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textMonsterConfig.Location = new System.Drawing.Point(635, 491);
            this.textMonsterConfig.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textMonsterConfig.Multiline = true;
            this.textMonsterConfig.Name = "textMonsterConfig";
            this.textMonsterConfig.Size = new System.Drawing.Size(1132, 235);
            this.textMonsterConfig.TabIndex = 81;
            // 
            // textMonsterAesIv
            // 
            this.textMonsterAesIv.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
            this.textMonsterAesIv.Location = new System.Drawing.Point(635, 234);
            this.textMonsterAesIv.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textMonsterAesIv.Name = "textMonsterAesIv";
            this.textMonsterAesIv.Size = new System.Drawing.Size(1132, 57);
            this.textMonsterAesIv.TabIndex = 78;
            this.textMonsterAesIv.Text = "1234567890123456";
            this.textMonsterAesIv.WordWrap = false;
            // 
            // textMonsterAesKey
            // 
            this.textMonsterAesKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textMonsterAesKey.Location = new System.Drawing.Point(635, 122);
            this.textMonsterAesKey.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textMonsterAesKey.Name = "textMonsterAesKey";
            this.textMonsterAesKey.Size = new System.Drawing.Size(1132, 57);
            this.textMonsterAesKey.TabIndex = 76;
            this.textMonsterAesKey.Text = "12345678901234567890123456789012";
            this.textMonsterAesKey.WordWrap = false;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label20.Location = new System.Drawing.Point(64, 355);
            this.label20.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(449, 42);
            this.label20.TabIndex = 82;
            this.label20.Text = "System Connection String";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label19.Location = new System.Drawing.Point(315, 510);
            this.label19.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(213, 42);
            this.label19.TabIndex = 80;
            this.label19.Text = "Config XML";
            // 
            // buttonMonsterSettings
            // 
            this.buttonMonsterSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonMonsterSettings.Location = new System.Drawing.Point(635, 806);
            this.buttonMonsterSettings.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.buttonMonsterSettings.Name = "buttonMonsterSettings";
            this.buttonMonsterSettings.Size = new System.Drawing.Size(1139, 110);
            this.buttonMonsterSettings.TabIndex = 79;
            this.buttonMonsterSettings.Text = "Generate XML config settings and CTRL+C all of it!";
            this.buttonMonsterSettings.UseVisualStyleBackColor = true;
            this.buttonMonsterSettings.Click += new System.EventHandler(this.buttonMonsterSettings_Click);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.Location = new System.Drawing.Point(216, 255);
            this.label17.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(315, 42);
            this.label17.TabIndex = 77;
            this.label17.Text = "AES IV (16-bytes)";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.Location = new System.Drawing.Point(181, 134);
            this.label18.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(346, 42);
            this.label18.TabIndex = 75;
            this.label18.Text = "AES Key (32-bytes)";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage6);
            this.tabControl1.Controls.Add(this.tabPage7);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(69, 26);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(5);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(2275, 1350);
            this.tabControl1.TabIndex = 30;
            // 
            // CryptoUi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(3157, 1576);
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.Name = "CryptoUi";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Who\'s Your Best Encryption Buddy?  I am!";
            this.Load += new System.EventHandler(this.CryptoUi_Load);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage7.ResumeLayout(false);
            this.tabPage7.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.tabPage5.PerformLayout();
            this.tabPage6.ResumeLayout(false);
            this.tabPage6.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }


        #endregion

        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TextBox textHMACOutput;
        private System.Windows.Forms.TextBox textHMACPayload;
        private System.Windows.Forms.TextBox textHMACSecret;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button buttonHMAC256;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox textAesIv;
        private System.Windows.Forms.TextBox textAesKey;
        private System.Windows.Forms.TextBox textAesDecryptedOutput;
        private System.Windows.Forms.TextBox textAesEncryptedInput;
        private System.Windows.Forms.TextBox textAesEncryptedOutput;
        private System.Windows.Forms.TextBox textAesPlaintext;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button buttonAesDecrypt;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button buttonAesEncrypt;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TabPage tabPage7;
        private System.Windows.Forms.Button buttonSummaryRefreshTenant;
        private System.Windows.Forms.ComboBox comboSummaryTenantId;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.TextBox textSummary;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.Button buttonSummaryGetCurrent;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Button buttonAcumaticaRefresh;
        private System.Windows.Forms.ComboBox comboAcumaticaTenantId;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.TextBox textAcumaticaPassword;
        private System.Windows.Forms.TextBox textAcumaticaUsername;
        private System.Windows.Forms.TextBox textAcumaticaBranch;
        private System.Windows.Forms.TextBox textAcumaticaCompany;
        private System.Windows.Forms.TextBox textAcumaticaUrl;
        private System.Windows.Forms.TextBox textAcumaticaXml;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button buttonAcumaticaXml;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.Button buttonShopifyGetTenants;
        private System.Windows.Forms.ComboBox comboShopifyTenantId;
        private System.Windows.Forms.Button buttonShopifyLoadContext;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.TextBox textShopifyDomain;
        private System.Windows.Forms.TextBox textShopifyApiSecret;
        private System.Windows.Forms.TextBox textShopifyApiPwd;
        private System.Windows.Forms.TextBox textShopifyApiKey;
        private System.Windows.Forms.TextBox textShopifyXml;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Button buttonShopifyXml;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.TextBox textMonsterSystemConnstr;
        private System.Windows.Forms.TextBox textMonsterConfig;
        private System.Windows.Forms.TextBox textMonsterAesIv;
        private System.Windows.Forms.TextBox textMonsterAesKey;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Button buttonMonsterSettings;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TabControl tabControl1;
    }
}

