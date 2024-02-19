namespace Application.Common;

// Source: https://stackoverflow.com/questions/2138429/hash-and-salt-passwords-in-c-sharp/73126492#73126492
// Source: https://stackoverflow.com/questions/10168240/encrypting-decrypting-a-string-in-c-sharp

using Microsoft.AspNetCore.Identity;
using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Linq;

public static class Secret
{
    private const int SaltSize = 16; // 128 bits
    private const int SecretKeySize = 16; // 128 bits
    private const int HashKeySize = 32; // 256 bits
    private const int Iterations = 50000;
    private const char SegmentDelimiter = ':';
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;
    private static readonly PasswordOptions PasswordOptions = new()
    {
        RequiredLength = 24,
        RequiredUniqueChars = 8,
        RequireDigit = true,
        RequireLowercase = true,
        RequireNonAlphanumeric = true,
        RequireUppercase = true
    };

    public static string Generate(PasswordOptions? opts = null)
    {
        opts ??= PasswordOptions;

        var randomChars = new[] {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
            "abcdefghijkmnopqrstuvwxyz",    // lowercase
            "0123456789",                   // digits
            "!@$?_-"                        // non-alphanumeric
        };
        var rand = new Random();
        var chars = new List<char>();

        if (opts.RequireUppercase)
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[0][rand.Next(0, randomChars[0].Length)]);

        if (opts.RequireLowercase)
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[1][rand.Next(0, randomChars[1].Length)]);

        if (opts.RequireDigit)
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[2][rand.Next(0, randomChars[2].Length)]);

        if (opts.RequireNonAlphanumeric)
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[3][rand.Next(0, randomChars[3].Length)]);

        for (var i = chars.Count; i < opts.RequiredLength || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
        {
            var rcs = randomChars[rand.Next(0, randomChars.Length)];
            chars.Insert(rand.Next(0, chars.Count),
                rcs[rand.Next(0, rcs.Length)]);
        }

        return new string(chars.ToArray());
    }

    public static string Encrypt(string plainText, string passPhrase)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var iv = RandomNumberGenerator.GetBytes(SecretKeySize);
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        var keyBytes = Rfc2898DeriveBytes.Pbkdf2(passPhrase, salt, Iterations, Algorithm, SecretKeySize);
        using var symmetricKey = Aes.Create();
        symmetricKey.BlockSize = SecretKeySize * 8;
        symmetricKey.Mode = CipherMode.CBC;
        symmetricKey.Padding = PaddingMode.PKCS7;
        using var encryptor = symmetricKey.CreateEncryptor(keyBytes, iv);
        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
        cryptoStream.FlushFinalBlock();
        var cipherTextBytes = salt;
        cipherTextBytes = cipherTextBytes.Concat(iv).ToArray();
        cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
        memoryStream.Close();
        cryptoStream.Close();
        return Convert.ToBase64String(cipherTextBytes);
    }

    public static string Decrypt(string cipherText, string passPhrase)
    {
        var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
        var salt = cipherTextBytesWithSaltAndIv.Take(SaltSize).ToArray();
        var iv = cipherTextBytesWithSaltAndIv.Skip(SecretKeySize).Take(SecretKeySize).ToArray();
        var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip(SecretKeySize * 2).Take(cipherTextBytesWithSaltAndIv.Length - SecretKeySize * 2).ToArray();
        var keyBytes = Rfc2898DeriveBytes.Pbkdf2(passPhrase, salt, Iterations, Algorithm, SecretKeySize);
        using var symmetricKey = Aes.Create();
        symmetricKey.BlockSize = SecretKeySize * 8;
        symmetricKey.Mode = CipherMode.CBC;
        symmetricKey.Padding = PaddingMode.PKCS7;
        using var decryptor = symmetricKey.CreateDecryptor(keyBytes, iv);
        using var memoryStream = new MemoryStream(cipherTextBytes);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var streamReader = new StreamReader(cryptoStream, Encoding.UTF8);
        return streamReader.ReadToEnd();
    }

    public static string Hash(string input)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            input,
            salt,
            Iterations,
            Algorithm,
            HashKeySize
        );
        return string.Join(
            SegmentDelimiter,
            Convert.ToHexString(hash),
            Convert.ToHexString(salt),
            Iterations,
            Algorithm
        );
    }

    public static bool Verify(string input, string hashString)
    {
        var segments = hashString.Split(SegmentDelimiter);
        var hash = Convert.FromHexString(segments[0]);
        var salt = Convert.FromHexString(segments[1]);
        var iterations = int.Parse(segments[2]);
        var algorithm = new HashAlgorithmName(segments[3]);
        var inputHash = Rfc2898DeriveBytes.Pbkdf2(
            input,
            salt,
            iterations,
            algorithm,
            hash.Length
        );
        return CryptographicOperations.FixedTimeEquals(inputHash, hash);
    }
}
