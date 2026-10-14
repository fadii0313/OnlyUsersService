using OnlyUsersService.API.Common;
using Xunit;

namespace OnlyUsersService.Tests
{
    public class EncryptionServiceTests
    {
        private readonly IEncryptionService _encryptionService = new EncryptionService();

        [Fact]
        public void ComputeSha256Hash_ReturnsConsistentLowercaseHex()
        {
            // Arrange
            var input = "Admin@123456";

            // Act
            var hash1 = _encryptionService.ComputeSha256Hash(input);
            var hash2 = _encryptionService.ComputeSha256Hash(input);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(hash1));
            Assert.Equal(64, hash1.Length); // 256 bits = 64 hex characters
            Assert.Equal(hash1, hash2);
            Assert.Equal(hash1.ToLowerInvariant(), hash1);
        }

        [Fact]
        public void ComputeSha256Hash_WithSalt_ChangesHashOutput()
        {
            // Arrange
            var input = "MySecretPassword";
            var salt = "UniqueSalt123";

            // Act
            var plainHash = _encryptionService.ComputeSha256Hash(input);
            var saltedHash = _encryptionService.ComputeSha256Hash(input, salt);

            // Assert
            Assert.NotEqual(plainHash, saltedHash);
            Assert.True(_encryptionService.VerifySha256Hash(input, saltedHash, salt));
        }

        [Fact]
        public void VerifyPassword_MatchesBothSha256AndBCrypt()
        {
            // Arrange
            var rawPassword = "User@123456";
            var sha256Hash = _encryptionService.HashPassword(rawPassword);
            var bcryptHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

            // Act & Assert
            Assert.True(_encryptionService.VerifyPassword(rawPassword, sha256Hash));
            Assert.True(_encryptionService.VerifyPassword(rawPassword, bcryptHash));
            Assert.False(_encryptionService.VerifyPassword("WrongPassword", sha256Hash));
            Assert.False(_encryptionService.VerifyPassword("WrongPassword", bcryptHash));
        }
    }
}
