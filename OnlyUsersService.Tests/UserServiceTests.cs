using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OnlyUsersService.API.Common;
using OnlyUsersService.API.Data;
using OnlyUsersService.API.Models.RequestModels.Users;
using OnlyUsersService.API.Models.ResponseModels.BaseModels;
using OnlyUsersService.API.Models.ResponseModels.Users;
using OnlyUsersService.API.Repositories.Implementation;
using OnlyUsersService.API.Services.Implementation;
using Xunit;

namespace OnlyUsersService.Tests
{
    public class UserServiceTests
    {
        private OnlyUsersDbContext CreateDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<OnlyUsersDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new OnlyUsersDbContext(options);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsSeededUsers()
        {
            // Arrange
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            await DbInitializer.SeedAsync(context);

            var userRepo = new UserRepository(context);
            var roleRepo = new RoleRepository(context);
            var userService = new UserService(userRepo, roleRepo);

            // Act
            var response = await userService.GetAllAsync(1, 10);

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
        }

        [Fact]
        public async Task CreateAsync_ValidUser_CreatesUserWithAssignedRole()
        {
            // Arrange
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            await DbInitializer.SeedAsync(context);

            var userRepo = new UserRepository(context);
            var roleRepo = new RoleRepository(context);
            var userService = new UserService(userRepo, roleRepo);

            var request = new CreateUserRequestModel
            {
                Username = "bob_marley",
                Email = "bob@example.com",
                Password = "Password@123",
                FirstName = "Bob",
                LastName = "Marley",
                Roles = new List<string> { AppRoles.User }
            };

            // Act
            var response = await userService.CreateAsync(request);

            // Assert
            Assert.True(response.Success);
            var userResponse = Assert.IsType<UserResponseModel>(response.Data);
            Assert.Equal("bob_marley", userResponse.Username);
            Assert.Contains(AppRoles.User, userResponse.Roles);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesProfileAndFields()
        {
            // Arrange
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            await DbInitializer.SeedAsync(context);

            var userRepo = new UserRepository(context);
            var roleRepo = new RoleRepository(context);
            var userService = new UserService(userRepo, roleRepo);

            var user = await userRepo.FirstOrDefaultAsync(u => u.Username == "john_doe");
            Assert.NotNull(user);

            var updateRequest = new UpdateUserRequestModel
            {
                Email = "john.updated@onlyusers.local",
                FirstName = "Jonathan",
                LastName = "Doe",
                Department = "DevOps",
                Designation = "Principal Engineer"
            };

            // Act
            var response = await userService.UpdateAsync(user.Id, updateRequest);

            // Assert
            Assert.True(response.Success);
            var updated = Assert.IsType<UserResponseModel>(response.Data);
            Assert.Equal("Jonathan", updated.FirstName);
            Assert.Equal("john.updated@onlyusers.local", updated.Email);
            Assert.Equal("DevOps", updated.Profile?.Department);
        }

        [Fact]
        public async Task AssignRolesAsync_ReplacesOrAppendsRoles()
        {
            // Arrange
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            await DbInitializer.SeedAsync(context);

            var userRepo = new UserRepository(context);
            var roleRepo = new RoleRepository(context);
            var userService = new UserService(userRepo, roleRepo);

            var user = await userRepo.FirstOrDefaultAsync(u => u.Username == "john_doe");
            Assert.NotNull(user);

            var roleRequest = new AssignRoleRequestModel
            {
                UserId = user.Id,
                Roles = new List<string> { AppRoles.Admin, AppRoles.User },
                ReplaceExisting = true
            };

            // Act
            var response = await userService.AssignRolesAsync(roleRequest);

            // Assert
            Assert.True(response.Success);
            var updated = Assert.IsType<UserResponseModel>(response.Data);
            Assert.Contains(AppRoles.Admin, updated.Roles);
            Assert.Contains(AppRoles.User, updated.Roles);
        }

        [Fact]
        public async Task DeleteAsync_DeletesUser()
        {
            // Arrange
            using var context = CreateDbContext(Guid.NewGuid().ToString());
            await DbInitializer.SeedAsync(context);

            var userRepo = new UserRepository(context);
            var roleRepo = new RoleRepository(context);
            var userService = new UserService(userRepo, roleRepo);

            var user = await userRepo.FirstOrDefaultAsync(u => u.Username == "john_doe");
            Assert.NotNull(user);

            // Act
            var response = await userService.DeleteAsync(user.Id);

            // Assert
            Assert.True(response.Success);
            var deletedUser = await userRepo.GetByIdAsync(user.Id);
            Assert.Null(deletedUser);
        }
    }
}
