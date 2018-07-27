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
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.textXML = new System.Windows.Forms.TextBox();
            this.textJson = new System.Windows.Forms.TextBox();
            this.label28 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label27 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.buttonDecrypt = new System.Windows.Forms.Button();
            this.textDecryptedOutput = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonEncrypt = new System.Windows.Forms.Button();
            this.textEncryptedInput = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textEncryptedOutput = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textNonEncrypted = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.textAesIv = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.textAesKey = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.buttonAesDecrypt = new System.Windows.Forms.Button();
            this.textAesDecryptedOutput = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.buttonAesEncrypt = new System.Windows.Forms.Button();
            this.textAesEncryptedInput = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textAesEncryptedOutput = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textAesPlaintext = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.tabPage5.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.textXML);
            this.tabPage5.Controls.Add(this.textJson);
            this.tabPage5.Controls.Add(this.label28);
            this.tabPage5.Controls.Add(this.button1);
            this.tabPage5.Controls.Add(this.label27);
            this.tabPage5.Location = new System.Drawing.Point(8, 39);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(1222, 988);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "Shopify Security";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // textXML
            // 
            this.textXML.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textXML.Location = new System.Drawing.Point(303, 367);
            this.textXML.Margin = new System.Windows.Forms.Padding(6);
            this.textXML.Multiline = true;
            this.textXML.Name = "textXML";
            this.textXML.Size = new System.Drawing.Size(660, 191);
            this.textXML.TabIndex = 59;
            // 
            // textJson
            // 
            this.textJson.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textJson.Location = new System.Drawing.Point(303, 97);
            this.textJson.Margin = new System.Windows.Forms.Padding(6);
            this.textJson.Multiline = true;
            this.textJson.Name = "textJson";
            this.textJson.Size = new System.Drawing.Size(660, 191);
            this.textJson.TabIndex = 50;
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label28.Location = new System.Drawing.Point(79, 370);
            this.label28.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(173, 36);
            this.label28.TabIndex = 58;
            this.label28.Text = "Config XML";
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(85, 634);
            this.button1.Margin = new System.Windows.Forms.Padding(6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(890, 88);
            this.button1.TabIndex = 57;
            this.button1.Text = "Generate XML config settings and CTRL+C all of it!";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label27.Location = new System.Drawing.Point(79, 100);
            this.label27.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(189, 36);
            this.label27.TabIndex = 49;
            this.label27.Text = "Config JSON";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(53, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1238, 1035);
            this.tabControl1.TabIndex = 30;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.buttonDecrypt);
            this.tabPage1.Controls.Add(this.textDecryptedOutput);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.buttonEncrypt);
            this.tabPage1.Controls.Add(this.textEncryptedInput);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.textEncryptedOutput);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.textNonEncrypted);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Location = new System.Drawing.Point(8, 39);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1222, 988);
            this.tabPage1.TabIndex = 5;
            this.tabPage1.Text = "Machine Key Crypto";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // buttonDecrypt
            // 
            this.buttonDecrypt.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.125F);
            this.buttonDecrypt.Location = new System.Drawing.Point(359, 791);
            this.buttonDecrypt.Name = "buttonDecrypt";
            this.buttonDecrypt.Size = new System.Drawing.Size(660, 87);
            this.buttonDecrypt.TabIndex = 60;
            this.buttonDecrypt.Text = "Decrypt + Copy";
            this.buttonDecrypt.UseVisualStyleBackColor = true;
            this.buttonDecrypt.Click += new System.EventHandler(this.buttonDecrypt_Click);
            // 
            // textDecryptedOutput
            // 
            this.textDecryptedOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textDecryptedOutput.Location = new System.Drawing.Point(359, 682);
            this.textDecryptedOutput.Margin = new System.Windows.Forms.Padding(6);
            this.textDecryptedOutput.Name = "textDecryptedOutput";
            this.textDecryptedOutput.Size = new System.Drawing.Size(660, 47);
            this.textDecryptedOutput.TabIndex = 59;
            this.textDecryptedOutput.WordWrap = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(56, 699);
            this.label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(247, 36);
            this.label4.TabIndex = 58;
            this.label4.Text = "Decrypted Output";
            // 
            // buttonEncrypt
            // 
            this.buttonEncrypt.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.125F);
            this.buttonEncrypt.Location = new System.Drawing.Point(359, 337);
            this.buttonEncrypt.Name = "buttonEncrypt";
            this.buttonEncrypt.Size = new System.Drawing.Size(660, 87);
            this.buttonEncrypt.TabIndex = 57;
            this.buttonEncrypt.Text = "Encrypt + Copy";
            this.buttonEncrypt.UseVisualStyleBackColor = true;
            this.buttonEncrypt.Click += new System.EventHandler(this.buttonEncrypt_Click);
            // 
            // textEncryptedInput
            // 
            this.textEncryptedInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textEncryptedInput.Location = new System.Drawing.Point(359, 570);
            this.textEncryptedInput.Margin = new System.Windows.Forms.Padding(6);
            this.textEncryptedInput.Name = "textEncryptedInput";
            this.textEncryptedInput.Size = new System.Drawing.Size(660, 47);
            this.textEncryptedInput.TabIndex = 56;
            this.textEncryptedInput.WordWrap = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(56, 582);
            this.label3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(225, 36);
            this.label3.TabIndex = 55;
            this.label3.Text = "Encrypted Input";
            // 
            // textEncryptedOutput
            // 
            this.textEncryptedOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
            this.textEncryptedOutput.Location = new System.Drawing.Point(359, 223);
            this.textEncryptedOutput.Margin = new System.Windows.Forms.Padding(6);
            this.textEncryptedOutput.Name = "textEncryptedOutput";
            this.textEncryptedOutput.Size = new System.Drawing.Size(660, 47);
            this.textEncryptedOutput.TabIndex = 54;
            this.textEncryptedOutput.WordWrap = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(56, 223);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(247, 36);
            this.label2.TabIndex = 53;
            this.label2.Text = "Encrypted Output";
            // 
            // textNonEncrypted
            // 
            this.textNonEncrypted.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textNonEncrypted.Location = new System.Drawing.Point(359, 101);
            this.textNonEncrypted.Margin = new System.Windows.Forms.Padding(6);
            this.textNonEncrypted.Name = "textNonEncrypted";
            this.textNonEncrypted.Size = new System.Drawing.Size(660, 47);
            this.textNonEncrypted.TabIndex = 52;
            this.textNonEncrypted.WordWrap = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(56, 104);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(222, 36);
            this.label1.TabIndex = 51;
            this.label1.Text = "Plain Text Input";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.label11);
            this.tabPage2.Controls.Add(this.textAesIv);
            this.tabPage2.Controls.Add(this.label9);
            this.tabPage2.Controls.Add(this.textAesKey);
            this.tabPage2.Controls.Add(this.label10);
            this.tabPage2.Controls.Add(this.buttonAesDecrypt);
            this.tabPage2.Controls.Add(this.textAesDecryptedOutput);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.buttonAesEncrypt);
            this.tabPage2.Controls.Add(this.textAesEncryptedInput);
            this.tabPage2.Controls.Add(this.label6);
            this.tabPage2.Controls.Add(this.textAesEncryptedOutput);
            this.tabPage2.Controls.Add(this.label7);
            this.tabPage2.Controls.Add(this.textAesPlaintext);
            this.tabPage2.Controls.Add(this.label8);
            this.tabPage2.Location = new System.Drawing.Point(8, 39);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1222, 988);
            this.tabPage2.TabIndex = 6;
            this.tabPage2.Text = "AES Crypto";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // textAesIv
            // 
            this.textAesIv.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
            this.textAesIv.Location = new System.Drawing.Point(433, 187);
            this.textAesIv.Margin = new System.Windows.Forms.Padding(6);
            this.textAesIv.Name = "textAesIv";
            this.textAesIv.Size = new System.Drawing.Size(660, 47);
            this.textAesIv.TabIndex = 74;
            this.textAesIv.Text = "1234567890123456";
            this.textAesIv.WordWrap = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(130, 187);
            this.label9.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(256, 36);
            this.label9.TabIndex = 73;
            this.label9.Text = "AES IV (16-bytes)";
            // 
            // textAesKey
            // 
            this.textAesKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textAesKey.Location = new System.Drawing.Point(433, 103);
            this.textAesKey.Margin = new System.Windows.Forms.Padding(6);
            this.textAesKey.Name = "textAesKey";
            this.textAesKey.Size = new System.Drawing.Size(660, 47);
            this.textAesKey.TabIndex = 72;
            this.textAesKey.Text = "12345678901234567890123456789012";
            this.textAesKey.WordWrap = false;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(133, 103);
            this.label10.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(278, 36);
            this.label10.TabIndex = 71;
            this.label10.Text = "AES Key (32-bytes)";
            // 
            // buttonAesDecrypt
            // 
            this.buttonAesDecrypt.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.125F);
            this.buttonAesDecrypt.Location = new System.Drawing.Point(433, 856);
            this.buttonAesDecrypt.Name = "buttonAesDecrypt";
            this.buttonAesDecrypt.Size = new System.Drawing.Size(660, 87);
            this.buttonAesDecrypt.TabIndex = 70;
            this.buttonAesDecrypt.Text = "Decrypt + Copy";
            this.buttonAesDecrypt.UseVisualStyleBackColor = true;
            this.buttonAesDecrypt.Click += new System.EventHandler(this.buttonAesDecrypt_Click);
            // 
            // textAesDecryptedOutput
            // 
            this.textAesDecryptedOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textAesDecryptedOutput.Location = new System.Drawing.Point(433, 783);
            this.textAesDecryptedOutput.Margin = new System.Windows.Forms.Padding(6);
            this.textAesDecryptedOutput.Name = "textAesDecryptedOutput";
            this.textAesDecryptedOutput.Size = new System.Drawing.Size(660, 47);
            this.textAesDecryptedOutput.TabIndex = 69;
            this.textAesDecryptedOutput.WordWrap = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(130, 800);
            this.label5.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(247, 36);
            this.label5.TabIndex = 68;
            this.label5.Text = "Decrypted Output";
            // 
            // buttonAesEncrypt
            // 
            this.buttonAesEncrypt.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.125F);
            this.buttonAesEncrypt.Location = new System.Drawing.Point(433, 520);
            this.buttonAesEncrypt.Name = "buttonAesEncrypt";
            this.buttonAesEncrypt.Size = new System.Drawing.Size(660, 87);
            this.buttonAesEncrypt.TabIndex = 67;
            this.buttonAesEncrypt.Text = "Encrypt + Copy";
            this.buttonAesEncrypt.UseVisualStyleBackColor = true;
            this.buttonAesEncrypt.Click += new System.EventHandler(this.buttonAesEncrypt_Click);
            // 
            // textAesEncryptedInput
            // 
            this.textAesEncryptedInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textAesEncryptedInput.Location = new System.Drawing.Point(433, 707);
            this.textAesEncryptedInput.Margin = new System.Windows.Forms.Padding(6);
            this.textAesEncryptedInput.Name = "textAesEncryptedInput";
            this.textAesEncryptedInput.Size = new System.Drawing.Size(660, 47);
            this.textAesEncryptedInput.TabIndex = 66;
            this.textAesEncryptedInput.WordWrap = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(130, 719);
            this.label6.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(225, 36);
            this.label6.TabIndex = 65;
            this.label6.Text = "Encrypted Input";
            // 
            // textAesEncryptedOutput
            // 
            this.textAesEncryptedOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
            this.textAesEncryptedOutput.Location = new System.Drawing.Point(433, 440);
            this.textAesEncryptedOutput.Margin = new System.Windows.Forms.Padding(6);
            this.textAesEncryptedOutput.Name = "textAesEncryptedOutput";
            this.textAesEncryptedOutput.Size = new System.Drawing.Size(660, 47);
            this.textAesEncryptedOutput.TabIndex = 64;
            this.textAesEncryptedOutput.WordWrap = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(130, 440);
            this.label7.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(247, 36);
            this.label7.TabIndex = 63;
            this.label7.Text = "Encrypted Output";
            // 
            // textAesPlaintext
            // 
            this.textAesPlaintext.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textAesPlaintext.Location = new System.Drawing.Point(433, 356);
            this.textAesPlaintext.Margin = new System.Windows.Forms.Padding(6);
            this.textAesPlaintext.Name = "textAesPlaintext";
            this.textAesPlaintext.Size = new System.Drawing.Size(660, 47);
            this.textAesPlaintext.TabIndex = 62;
            this.textAesPlaintext.WordWrap = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(133, 356);
            this.label8.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(222, 36);
            this.label8.TabIndex = 61;
            this.label8.Text = "Plain Text Input";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(270, 103);
            this.label11.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(0, 36);
            this.label11.TabIndex = 75;
            // 
            // CryptoUi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1374, 1247);
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "CryptoUi";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Who\'s Your Best Encryption Buddy?  I am!";
            this.tabPage5.ResumeLayout(false);
            this.tabPage5.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }


        #endregion

        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.TextBox textXML;
        private System.Windows.Forms.TextBox textJson;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox textEncryptedInput;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textEncryptedOutput;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textNonEncrypted;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonEncrypt;
        private System.Windows.Forms.Button buttonDecrypt;
        private System.Windows.Forms.TextBox textDecryptedOutput;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button buttonAesDecrypt;
        private System.Windows.Forms.TextBox textAesDecryptedOutput;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button buttonAesEncrypt;
        private System.Windows.Forms.TextBox textAesEncryptedInput;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textAesEncryptedOutput;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textAesPlaintext;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textAesIv;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textAesKey;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
    }
}

