using System;
using System.Security.Cryptography;
using System.Text;

namespace OnlyUsersService.API.Common
{
    /// <summary>
    /// Service for SHA-256 cryptographic hashing and password validation.
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Computes a standard SHA-256 hash string (hexadecimal lowercase).
        /// </summary>
        string ComputeSha256Hash(string rawData, string salt = null);

        /// <summary>
        /// Verifies whether raw data matches a given SHA-256 hash.
        /// </summary>
        bool VerifySha256Hash(string rawData, string hash, string salt = null);

        /// <summary>
        /// Hashes a password using SHA-256.
        /// </summary>
        string HashPassword(string password, string salt = null);

        /// <summary>
        /// Verifies a password against a stored hash (supports SHA-256 and BCrypt).
        /// </summary>
        bool VerifyPassword(string password, string storedHash, string salt = null);
    }

    /// <summary>
    /// Default implementation of IEncryptionService using System.Security.Cryptography.SHA256.
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        public string ComputeSha256Hash(string rawData, string salt = null)
        {
            if (string.IsNullOrEmpty(rawData))
            {
                return string.Empty;
            }

            var input = string.IsNullOrEmpty(salt) ? rawData : $"{rawData}:{salt}";
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                var builder = new StringBuilder(bytes.Length * 2);
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public bool VerifySha256Hash(string rawData, string hash, string salt = null)
        {
            if (string.IsNullOrEmpty(rawData) || string.IsNullOrEmpty(hash))
            {
                return false;
            }

            var computed = ComputeSha256Hash(rawData, salt);
            return string.Equals(computed, hash, StringComparison.OrdinalIgnoreCase);
        }

        public string HashPassword(string password, string salt = null)
        {
            return ComputeSha256Hash(password, salt);
        }

        public bool VerifyPassword(string password, string storedHash, string salt = null)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            {
                return false;
            }

            // 1. Direct SHA-256 hash comparison
            if (VerifySha256Hash(password, storedHash, salt))
            {
                return true;
            }

            // 2. In case client already sent pre-hashed SHA-256 password matching stored hash
            if (string.Equals(password, storedHash, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // 3. Fallback check for BCrypt hashes (e.g. legacy/pre-existing hashes starting with $2)
            if (storedHash.StartsWith("$2a$") || storedHash.StartsWith("$2b$") || storedHash.StartsWith("$2y$"))
            {
                try
                {
                    return BCrypt.Net.BCrypt.Verify(password, storedHash);
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }
    }
}
