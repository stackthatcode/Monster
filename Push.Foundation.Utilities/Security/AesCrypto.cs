using System;

namespace Push.Foundation.Utilities.Security
{
    public class AesCrypto : ICryptoService
    {
        public string Key { get; set; }
        public string IV { get; set; }

        public AesCrypto(string key, string iv)
        {
            Key = key;
            IV = iv;
        }

        public string Encrypt(string input)
        {
            if (Key == null)
                throw new InvalidOperationException("You must set the EncryptionService.Key to function properly");
            if (IV == null)
                throw new InvalidOperationException("You must set the EncryptionService.IV to function properly");

            return input.AesEncryptString(Key, IV);
        }

        public string Decrypt(string input)
        {
            if (Key == null)
                throw new InvalidOperationException("You must set the EncryptionService.Key to function properly");
            if (IV == null)
                throw new InvalidOperationException("You must set the EncryptionService.IV to function properly");

            return input.AesDecryptString(Key, IV);
        }
    }
}
