using ExpenseTracker.BLL.Interfaces;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace ExpenseTracker.BLL.Services
{
    public class CryptoOptions
    {
        public string PrivateKey { get; set; } = string.Empty;
    }

    public class CryptoService : ICryptoService
    {
        private readonly string privateKey;

        public CryptoService(IOptions<CryptoOptions> options)
        {
            if (string.IsNullOrWhiteSpace(options.Value.PrivateKey))
            {
                throw new ArgumentException("Private key is not configured.");
            }

            privateKey = Encoding.UTF8.GetString(
                Convert.FromBase64String(options.Value.PrivateKey)
            );
        }

        public string Decrypt(string encryptedData)
        {
            try
            {
                var encryptedBytes = Convert.FromBase64String(encryptedData);

                using var rsa = RSA.Create();

                rsa.ImportFromPem(privateKey);

                var decryptedBytes = rsa.Decrypt(
                    encryptedBytes,
                    RSAEncryptionPadding.OaepSHA256);

                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (Exception)
            {
                throw new ApplicationException("Unable to decrypt the request data.");
            }
        }
    }
}
