namespace Application.Common;

using System.Security.Cryptography;

// Source: https://stackoverflow.com/questions/2138429/hash-and-salt-passwords-in-c-sharp/73126492#73126492

public static class SecretHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int KeySize = 32; // 256 bits
    private const int Iterations = 50000;
    private const char SegmentDelimiter = ':';
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public static string Hash(string input)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            input,
            salt,
            Iterations,
            Algorithm,
            KeySize
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
