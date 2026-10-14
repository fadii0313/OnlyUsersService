using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OnlyUsersService.API.Common;
using OnlyUsersService.API.Data;
using OnlyUsersService.API.Models.RequestModels.Auth;
using OnlyUsersService.API.Models.ResponseModels.Auth;
using OnlyUsersService.API.Models.ResponseModels.BaseModels;
using OnlyUsersService.API.Repositories.Implementation;
using OnlyUsersService.API.Services.Implementation;
using Xunit;

namespace OnlyUsersService.Tests
{
    public class JwtAuthServiceTests
    {
        private OnlyUsersDbContext CreateDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<OnlyUsersDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new OnlyUsersDbContext(options);
        }

        private IConfiguration CreateConfiguration()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                {"SecuritySettings:JwtSecretKey", "OnlyUsersService_Secret_Key_Super_Secure_2026_JWT_Token_Generator_Key!#$"},
                {"SecuritySettings:JwtIssuer", "OnlyUsersService"},
                {"SecuritySettings:JwtAudience", "OnlyUsersService.Client"},
                {"SecuritySettings:TokenExpiryMinutes", "1440"}
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

        [Fact]
        public async Task RegisterAsync_ValidData_ReturnsSuccessAndJwtToken()
        {
            // Arrange
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            await DbInitializer.SeedAsync(context);

            var userRepo = new UserRepository(context);
            var roleRepo = new RoleRepository(context);
            var config = CreateConfiguration();
            var authService = new JwtAuthService(userRepo, roleRepo, config);

            var registerModel = new RegisterRequestModel
            {
                Username = "test_user_1",
                Email = "test1@onlyusers.local",
                Password = "Password@123",
                FirstName = "Test",
                LastName = "User",
                Department = "QA"
            };

            // Act
            var response = await authService.RegisterAsync(registerModel);

            // Assert
            Assert.True(response.Success);
            Assert.Equal(ApiResponseCode.Success, response.ResponseCode);
            var authData = Assert.IsType<LoginResponseModel>(response.Data);
            Assert.False(string.IsNullOrWhiteSpace(authData.Token));
            Assert.Equal("test_user_1", authData.Username);
            Assert.Contains(AppRoles.User, authData.Roles);
        }

        [Fact]
        public async Task RegisterAsync_DuplicateUsername_ReturnsValidationFailure()
        {
            // Arrange
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            await DbInitializer.SeedAsync(context);

            var userRepo = new UserRepository(context);
            var roleRepo = new RoleRepository(context);
            var config = CreateConfiguration();
            var authService = new JwtAuthService(userRepo, roleRepo, config);

            var registerModel = new RegisterRequestModel
            {
                Username = "admin", // Already seeded in DbInitializer
                Email = "new_admin@onlyusers.local",
                Password = "Password@123",
                FirstName = "Admin",
                LastName = "Duplicate"
            };

            // Act
            var response = await authService.RegisterAsync(registerModel);

            // Assert
            Assert.False(response.Success);
            Assert.Equal(ApiResponseCode.Validation, response.ResponseCode);
            Assert.Contains("already taken", response.ResponseMessage);
        }

        [Fact]
        public async Task LoginAsync_ValidAdminCredentials_ReturnsSuccessAndAdminRole()
        {
            // Arrange
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            await DbInitializer.SeedAsync(context);

            var userRepo = new UserRepository(context);
            var roleRepo = new RoleRepository(context);
            var config = CreateConfiguration();
            var authService = new JwtAuthService(userRepo, roleRepo, config);

            var loginModel = new LoginRequestModel
            {
                Username = "admin",
                Password = "Admin@123456"
            };

            // Act
            var response = await authService.LoginAsync(loginModel);

            // Assert
            Assert.True(response.Success);
            Assert.Equal(ApiResponseCode.Success, response.ResponseCode);
            var loginData = Assert.IsType<LoginResponseModel>(response.Data);
            Assert.False(string.IsNullOrWhiteSpace(loginData.Token));
            Assert.Contains(AppRoles.Admin, loginData.Roles);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ReturnsUnauthorized()
        {
            // Arrange
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            await DbInitializer.SeedAsync(context);

            var userRepo = new UserRepository(context);
            var roleRepo = new RoleRepository(context);
            var config = CreateConfiguration();
            var authService = new JwtAuthService(userRepo, roleRepo, config);

            var loginModel = new LoginRequestModel
            {
                Username = "admin",
                Password = "WrongPassword!999"
            };

            // Act
            var response = await authService.LoginAsync(loginModel);

            // Assert
            Assert.False(response.Success);
            Assert.Equal(ApiResponseCode.Fail, response.ResponseCode);
            Assert.Contains("Invalid Username or Password", response.ResponseMessage);
        }
    }
}
