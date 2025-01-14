using System.Security.Cryptography;
using System.Security.Authentication;
using System.Text;

/// <summary>
/// This class hashes each user-token and verfies that it is indeed the expected client
/// </summary>
public class HashTokens
{
    public bool ValidateToken(string token, string rawData, string salt)
    {
        string expectedHash = GenerateSaltedHash(rawData, salt);
        return token == expectedHash;
    }

    private string GenerateNewSHA256Hash(string rawData)
    {
        if (string.IsNullOrEmpty(rawData))
        {
            throw new ArgumentException("The raw data can not be null or empty!");
        }
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            return Convert.ToHexString(hash);
        }
    }
    public string GenerateSaltedHash(string rawData, string salt)
    {
        if (string.IsNullOrEmpty(rawData) || string.IsNullOrEmpty(salt))
        {
            throw new ArgumentException("Input and salt cannot be null or empty!");
        }
        string concat = rawData + salt; // salted hash
        return GenerateNewSHA256Hash(concat);
    }

    public string GenerateSalt(int size = 23)
    {
        byte[] saltBytes = new byte[size];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        return Convert.ToHexString(saltBytes);
    }
}