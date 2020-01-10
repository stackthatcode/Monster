using Push.Foundation.Utilities.Security;

namespace Push.Foundation.Utilities.Config
{
    public static class ConfigExtensions
    {
        public static readonly string X = "ABCDEABCDE1234567890123456789012";
        public static readonly string Y = "1234567890ABCDEF";

        public static string EncryptConfig(this string input)
        {
            return input.AesEncryptString(X, Y);
        }

        public static string DecryptConfig(this string input)
        {
            return input.AesDecryptString(X, Y);
        }
    }
}
