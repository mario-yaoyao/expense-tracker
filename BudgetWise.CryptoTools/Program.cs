using BudgetWise.Models.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

var projectRoot = Path.GetFullPath(
    Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..")
);
var keysFolder = Path.Combine(projectRoot, "Keys");

// To create private & public keys
using var rsa = RSA.Create(2048);

var privateKey = rsa.ExportRSAPrivateKeyPem();
var publicKey = rsa.ExportSubjectPublicKeyInfoPem();

Directory.CreateDirectory(keysFolder);

File.WriteAllText(Path.Combine(keysFolder, "private.pem"), privateKey);
File.WriteAllText(Path.Combine(keysFolder, "public.pem"), publicKey);

Console.WriteLine("Keys generated.");
Console.WriteLine(Directory.GetCurrentDirectory());

// Plain text to Base64 converter
string plainPem = File.ReadAllText(
    //Path.Combine(keysFolder, "private.pem")
    Path.Combine(keysFolder, "public.pem")
);

string base64 = Convert.ToBase64String(
    Encoding.UTF8.GetBytes(plainPem)
);

Console.WriteLine("Cipher text generated.");
Console.WriteLine(base64);

// To create hashedPassword
var plainPassword = "password";
var user = new User();

var hashedPassword =
    new PasswordHasher<User>()
        .HashPassword(user, plainPassword);

Console.WriteLine("Hashed text generated.");
Console.WriteLine(hashedPassword);