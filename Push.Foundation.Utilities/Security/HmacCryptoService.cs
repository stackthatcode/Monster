using System;
using System.Security.Cryptography;

namespace Push.Foundation.Utilities.Security
{
    public class HmacCryptoService
    {
        public string ToBase64EncodedSha256(string secret, string input)
        {
            var encoding = new System.Text.UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(input);

            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }
    }
}
