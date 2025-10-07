using System.Security.Cryptography;

namespace Domain.Extensions;

public static class PasswordExtension
{
    private const int SaltSize = 16; // 16 bytes = 128 bits (recommended)
    private const int HashSize = 32; // 32 bytes = 256 bits
    private const int Iterations = 100_000; // Recommended for security

    /// <summary>
    ///     Hashes a password using PBKDF2 with a random salt.
    /// </summary>
    public static string HashPassword(this string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}"; // Store salt + hash
    }

    /// <summary>
    ///     Verifies a password against the stored hash.
    /// </summary>
    public static bool VerifyPassword(this string password, string hashedPassword)
    {
        var parts = hashedPassword.Split('.');
        if (parts.Length != 2) return false; // Invalid format

        var salt = Convert.FromBase64String(parts[0]);
        var hash = Convert.FromBase64String(parts[1]);

        var computedHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, hash.Length);

        return CryptographicOperations.FixedTimeEquals(computedHash, hash); // Prevents timing attacks
    }
}