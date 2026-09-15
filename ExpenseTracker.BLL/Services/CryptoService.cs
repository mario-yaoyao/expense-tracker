using ExpenseTracker.BLL.Interfaces;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

public class CryptoOptions
{
    public string PrivateKeyPath { get; set; } = string.Empty;
}

public class CryptoService : ICryptoService
{
    private readonly string privateKey;

    public CryptoService(IOptions<CryptoOptions> options)
    {
        if (string.IsNullOrWhiteSpace(options.Value.PrivateKeyPath))
        {
            throw new ArgumentException("Private key path is not configured.");
        }

        if (!File.Exists(options.Value.PrivateKeyPath))
        {
            throw new FileNotFoundException(
                $"Private key file not found: {options.Value.PrivateKeyPath}");
        }

        privateKey = File.ReadAllText(options.Value.PrivateKeyPath);
    }

    public string Decrypt(string encryptedData)
    {
        var encryptedBytes = Convert.FromBase64String(encryptedData);

        using var rsa = RSA.Create();

        rsa.ImportFromPem(privateKey);

        var decryptedBytes = rsa.Decrypt(
            encryptedBytes,
            RSAEncryptionPadding.OaepSHA256);

        return Encoding.UTF8.GetString(decryptedBytes);
    }
}