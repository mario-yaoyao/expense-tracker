using ExpenseTracker.BLL.Services;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Crypto;
using System.Security.Cryptography;
using System.Text;

namespace ExpenseTracker.Tests.Unit.Services
{
    public class CryptoServiceTests
    {
        [Fact]
        public void Decrypt_ReturnsPlainText_WhenEncryptedDataIsValid()
        {
            // Arrange
            var plainText = "Hello World";

            var rsa = RSA.Create(2048);
            var privateKeyPem = rsa.ExportRSAPrivateKeyPem();

            var options = Options.Create(new CryptoOptions
            {
                PrivateKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(privateKeyPem))
            });

            var service = new CryptoService(options);

            var encryptedBytes = rsa.Encrypt(
                Encoding.UTF8.GetBytes(plainText),
                RSAEncryptionPadding.OaepSHA256);

            var encryptedData = Convert.ToBase64String(encryptedBytes);

            // Act
            var result = service.Decrypt(encryptedData);

            // Assert
            Assert.Equal(plainText, result);
        }

        [Fact]
        public void Decrypt_ThrowsFormatException_WhenEncryptedDataIsNotBase64()
        {
            // Arrange
            var invalidEncryptedData = "not-a-base64-string";

            var rsa = RSA.Create(2048);
            var privateKeyPem = rsa.ExportRSAPrivateKeyPem();

            var options = Options.Create(new CryptoOptions
            {
                PrivateKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(privateKeyPem))
            });

            var service = new CryptoService(options);

            // Act & Assert
            Assert.Throws<ApplicationException>(
                () => service.Decrypt(invalidEncryptedData));
        }

        [Fact]
        public void Decrypt_ThrowsCryptographicException_WhenCipherTextIsInvalid()
        {
            // Arrange
            var rsa = RSA.Create(2048);
            var privateKeyPem = rsa.ExportRSAPrivateKeyPem();

            var options = Options.Create(new CryptoOptions
            {
                PrivateKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(privateKeyPem))
            });

            var service = new CryptoService(options);

            // Valid Base64, but not valid RSA encrypted data
            var invalidCipherText = Convert.ToBase64String(Encoding.UTF8.GetBytes("Hello"));

            // Act & Assert
            Assert.Throws<ApplicationException>(
                () => service.Decrypt(invalidCipherText));
        }

        [Fact]
        public void Constructor_ThrowsException_WhenPrivateKeyIsEmpty()
        {
            // Arrange
            var options = Options.Create(new CryptoOptions
            {
                PrivateKey = null!
            });

            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => new CryptoService(options));
        }
    }
}
