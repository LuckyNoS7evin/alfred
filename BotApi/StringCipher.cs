using Microsoft.AspNetCore.DataProtection;

namespace BotApi
{
    public class StringCipher
    {
        private const string Purpose = "my protection purpose";
        private readonly IDataProtectionProvider _provider;

        public StringCipher(IDataProtectionProvider provider)
        {
            _provider = provider;
        }

        public string Encrypt(string plainText)
        {
            var protector = _provider.CreateProtector(Purpose);
            return protector.Protect(plainText);
        }

        public string Decrypt(string cipherText)
        {
            var protector = _provider.CreateProtector(Purpose);
            return protector.Unprotect(cipherText);
        }
    }
}
