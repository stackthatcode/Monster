namespace Push.Foundation.Utilities.Security
{
    public interface ICryptoService
    {
        string Encrypt(string input);
        string Decrypt(string input);
    }
}
