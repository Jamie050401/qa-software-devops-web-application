namespace Application.Common;

using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Linq;

// Source: https://stackoverflow.com/questions/10168240/encrypting-decrypting-a-string-in-c-sharp

public static class StringCipher
{
    private const int KeySize = 128; // Bits
    private const int DerivationIterations = 50000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public static string Encrypt(string plainText, string passPhrase)
    {
        var salt = Generate128BitsOfRandomEntropy();
        var iv = Generate128BitsOfRandomEntropy();
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        var keyBytes = Rfc2898DeriveBytes.Pbkdf2(passPhrase, salt, DerivationIterations, Algorithm, KeySize / 8);
        using var symmetricKey = Aes.Create();
        symmetricKey.BlockSize = KeySize;
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
        var salt = cipherTextBytesWithSaltAndIv.Take(KeySize / 8).ToArray();
        var iv = cipherTextBytesWithSaltAndIv.Skip(KeySize / 8).Take(KeySize / 8).ToArray();
        var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((KeySize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((KeySize / 8) * 2)).ToArray();
        var keyBytes = Rfc2898DeriveBytes.Pbkdf2(passPhrase, salt, DerivationIterations, Algorithm, KeySize / 8);
        using var symmetricKey = Aes.Create();
        symmetricKey.BlockSize = KeySize;
        symmetricKey.Mode = CipherMode.CBC;
        symmetricKey.Padding = PaddingMode.PKCS7;
        using var decryptor = symmetricKey.CreateDecryptor(keyBytes, iv);
        using var memoryStream = new MemoryStream(cipherTextBytes);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var streamReader = new StreamReader(cryptoStream, Encoding.UTF8);
        return streamReader.ReadToEnd();
    }

    private static byte[] Generate128BitsOfRandomEntropy()
    {
        var randomBytes = new byte[16];
        using var rngCsp = RandomNumberGenerator.Create();
        rngCsp.GetBytes(randomBytes);
        return randomBytes;
    }
}