using System;

namespace Push.Foundation.Utilities.Security
{
    public class AesCrypto : ICryptoService
    {
        private readonly string _key;
        private readonly string _iv;
        
        public AesCrypto(string key, string iv)
        {
            _key = key;
            _iv = iv;
        }

        public string Encrypt(string input)
        {
            if (_key == null)
                throw new InvalidOperationException("You must set the EncryptionService.Key to function properly");
            if (_iv == null)
                throw new InvalidOperationException("You must set the EncryptionService.IV to function properly");

            return input.AesEncryptString(_key, _iv);
        }

        public string Decrypt(string input)
        {
            if (_key == null)
                throw new InvalidOperationException("You must set the EncryptionService.Key to function properly");
            if (_iv == null)
                throw new InvalidOperationException("You must set the EncryptionService.IV to function properly");

            return input.AesDecryptString(_key, _iv);
        }
    }
}
