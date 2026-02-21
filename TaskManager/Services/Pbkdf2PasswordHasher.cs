using System.Security.Cryptography;

namespace TaskManager.Services;

public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    public void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
    {
        salt = RandomNumberGenerator.GetBytes(SaltSize);
        hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
    }

    public bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
    {
        var computedHash = Rfc2898DeriveBytes.Pbkdf2(password, storedSalt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
    }
}
