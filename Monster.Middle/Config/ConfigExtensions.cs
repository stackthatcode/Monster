using Push.Foundation.Utilities.Security;

namespace Monster.Middle.Config
{
    public static class ConfigExtensions
    {
        public static string EncryptConfig(this string input)
        {
            return input.AesEncryptString("ABCDEABCDE1234567890123456789012", "1234567890ABCDEF");
        }

        public static string DecryptConfig(this string input)
        {
            return input.AesDecryptString("ABCDEABCDE1234567890123456789012", "1234567890ABCDEF");
        }
    }
}
