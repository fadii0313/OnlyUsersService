using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using OnlyUsersService.API.Models.Entities;
using OnlyUsersService.API.Models.RequestModels.Auth;
using OnlyUsersService.API.Models.RequestModels.BaseModels;
using OnlyUsersService.API.Models.RequestModels.Users;
using Xunit;

namespace OnlyUsersService.Tests
{
    public class DataAnnotationValidationTests
    {
        private static IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        #region RegisterRequestModel Tests

        [Fact]
        public void RegisterRequestModel_ValidData_PassesValidation()
        {
            var model = new RegisterRequestModel
            {
                Username = "testuser",
                Email = "testuser@example.com",
                Password = "Password@123",
                FirstName = "Test",
                LastName = "User",
                PhoneNumber = "+1234567890",
                Department = "Engineering",
                Designation = "Software Developer",
                City = "New York",
                Country = "USA"
            };

            var errors = ValidateModel(model);
            Assert.Empty(errors);
        }

        [Fact]
        public void RegisterRequestModel_MissingRequiredFields_FailsValidation()
        {
            var model = new RegisterRequestModel(); // All required fields empty

            var errors = ValidateModel(model);
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(RegisterRequestModel.Username)));
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(RegisterRequestModel.Email)));
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(RegisterRequestModel.Password)));
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(RegisterRequestModel.FirstName)));
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(RegisterRequestModel.LastName)));
        }

        [Fact]
        public void RegisterRequestModel_InvalidEmail_FailsValidation()
        {
            var model = new RegisterRequestModel
            {
                Username = "testuser",
                Email = "not-a-valid-email",
                Password = "Password@123",
                FirstName = "Test",
                LastName = "User"
            };

            var errors = ValidateModel(model);
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(RegisterRequestModel.Email)));
        }

        [Fact]
        public void RegisterRequestModel_UsernameTooShortOrTooLong_FailsValidation()
        {
            var shortModel = new RegisterRequestModel
            {
                Username = "ab", // Min length 3
                Email = "test@example.com",
                Password = "Password@123",
                FirstName = "Test",
                LastName = "User"
            };

            var shortErrors = ValidateModel(shortModel);
            Assert.Contains(shortErrors, e => e.MemberNames.Contains(nameof(RegisterRequestModel.Username)));

            var longModel = new RegisterRequestModel
            {
                Username = new string('a', 51), // Max length 50
                Email = "test@example.com",
                Password = "Password@123",
                FirstName = "Test",
                LastName = "User"
            };

            var longErrors = ValidateModel(longModel);
            Assert.Contains(longErrors, e => e.MemberNames.Contains(nameof(RegisterRequestModel.Username)));
        }

        [Fact]
        public void RegisterRequestModel_PasswordTooShort_FailsValidation()
        {
            var model = new RegisterRequestModel
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "123", // Min length 6
                FirstName = "Test",
                LastName = "User"
            };

            var errors = ValidateModel(model);
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(RegisterRequestModel.Password)));
        }

        #endregion

        #region LoginRequestModel Tests

        [Fact]
        public void LoginRequestModel_ValidData_PassesValidation()
        {
            var model = new LoginRequestModel
            {
                Username = "admin",
                Password = "Password@123"
            };

            var errors = ValidateModel(model);
            Assert.Empty(errors);
        }

        [Fact]
        public void LoginRequestModel_EmptyFields_FailsValidation()
        {
            var model = new LoginRequestModel();

            var errors = ValidateModel(model);
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(LoginRequestModel.Username)));
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(LoginRequestModel.Password)));
        }

        #endregion

        #region CreateUserRequestModel Tests

        [Fact]
        public void CreateUserRequestModel_ValidData_PassesValidation()
        {
            var model = new CreateUserRequestModel
            {
                Username = "createduser",
                Email = "created@example.com",
                Password = "Password@123",
                FirstName = "Jane",
                LastName = "Smith",
                PhoneNumber = "+1555234567"
            };

            var errors = ValidateModel(model);
            Assert.Empty(errors);
        }

        [Fact]
        public void CreateUserRequestModel_InvalidData_FailsValidation()
        {
            var model = new CreateUserRequestModel
            {
                Username = "a", // Too short
                Email = "invalid-email",
                Password = "12", // Too short
                FirstName = new string('x', 51), // Too long
                LastName = "" // Required
            };

            var errors = ValidateModel(model);
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateUserRequestModel.Username)));
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateUserRequestModel.Email)));
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateUserRequestModel.Password)));
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateUserRequestModel.FirstName)));
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateUserRequestModel.LastName)));
        }

        #endregion

        #region UpdateProfileRequestModel Tests

        [Fact]
        public void UpdateProfileRequestModel_ValidData_PassesValidation()
        {
            var model = new UpdateProfileRequestModel
            {
                FirstName = "Updated",
                LastName = "Name",
                PhoneNumber = "1234567890",
                AvatarUrl = "https://example.com/images/avatar.png"
            };

            var errors = ValidateModel(model);
            Assert.Empty(errors);
        }

        [Fact]
        public void UpdateProfileRequestModel_InvalidUrl_FailsValidation()
        {
            var model = new UpdateProfileRequestModel
            {
                FirstName = "Updated",
                LastName = "Name",
                AvatarUrl = "not-a-valid-url"
            };

            var errors = ValidateModel(model);
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(UpdateProfileRequestModel.AvatarUrl)));
        }

        #endregion

        #region UpdateUserRequestModel Tests

        [Fact]
        public void UpdateUserRequestModel_ValidData_PassesValidation()
        {
            var model = new UpdateUserRequestModel
            {
                Email = "valid@example.com",
                FirstName = "Alice",
                LastName = "Wonderland",
                NewPassword = "NewSecurePassword@123"
            };

            var errors = ValidateModel(model);
            Assert.Empty(errors);
        }

        [Fact]
        public void UpdateUserRequestModel_ShortNewPassword_FailsValidation()
        {
            var model = new UpdateUserRequestModel
            {
                Email = "valid@example.com",
                FirstName = "Alice",
                LastName = "Wonderland",
                NewPassword = "123" // Minimum 6
            };

            var errors = ValidateModel(model);
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(UpdateUserRequestModel.NewPassword)));
        }

        #endregion

        #region PaginationRequest Tests

        [Fact]
        public void PaginationRequest_DefaultValues_PassesValidation()
        {
            var model = new PaginationRequest();
            var errors = ValidateModel(model);
            Assert.Empty(errors);
        }

        [Fact]
        public void PaginationRequest_OutOfRangeValues_FailsValidation()
        {
            var model = new PaginationRequest
            {
                PageNumber = 0, // Must be >= 1
                PageSize = 200  // Must be <= 100
            };

            var errors = ValidateModel(model);
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(PaginationRequest.PageNumber)));
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(PaginationRequest.PageSize)));
        }

        #endregion

        #region AssignRoleRequestModel Tests

        [Fact]
        public void AssignRoleRequestModel_ValidData_PassesValidation()
        {
            var model = new AssignRoleRequestModel
            {
                UserId = 5,
                Roles = new List<string> { "Admin" }
            };

            var errors = ValidateModel(model);
            Assert.Empty(errors);
        }

        [Fact]
        public void AssignRoleRequestModel_EmptyRoles_FailsValidation()
        {
            var model = new AssignRoleRequestModel
            {
                UserId = 1,
                Roles = new List<string>() // MinLength(1)
            };

            var errors = ValidateModel(model);
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(AssignRoleRequestModel.Roles)));
        }

        [Fact]
        public void AssignRoleRequestModel_NegativeUserId_FailsValidation()
        {
            var model = new AssignRoleRequestModel
            {
                UserId = -1, // Range(1, long.MaxValue)
                Roles = new List<string> { "User" }
            };

            var errors = ValidateModel(model);
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(AssignRoleRequestModel.UserId)));
        }

        #endregion

        #region HashRequestModel Tests

        [Fact]
        public void HashRequestModel_ValidData_PassesValidation()
        {
            var model = new HashRequestModel
            {
                Text = "StringToHash",
                Salt = "OptionalSalt"
            };

            var errors = ValidateModel(model);
            Assert.Empty(errors);
        }

        [Fact]
        public void HashRequestModel_MissingText_FailsValidation()
        {
            var model = new HashRequestModel();

            var errors = ValidateModel(model);
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(HashRequestModel.Text)));
        }

        #endregion

        #region Entity Validation Tests

        [Fact]
        public void UserEntity_ValidData_PassesValidation()
        {
            var user = new User
            {
                Username = "valid_user",
                Email = "valid@example.com",
                PasswordHash = "hash123456",
                FirstName = "First",
                LastName = "Last"
            };

            var errors = ValidateModel(user);
            Assert.Empty(errors);
        }

        [Fact]
        public void UserEntity_MissingRequiredProperties_FailsValidation()
        {
            var user = new User();

            var errors = ValidateModel(user);
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(User.Username)));
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(User.Email)));
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(User.PasswordHash)));
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(User.FirstName)));
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(User.LastName)));
        }

        [Fact]
        public void RoleEntity_ValidData_PassesValidation()
        {
            var role = new Role
            {
                Name = "Manager",
                Description = "Managerial role"
            };

            var errors = ValidateModel(role);
            Assert.Empty(errors);
        }

        [Fact]
        public void RoleEntity_MissingName_FailsValidation()
        {
            var role = new Role();

            var errors = ValidateModel(role);
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(Role.Name)));
        }

        #endregion
    }
}
