using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OnlyUsersService.API.Common;
using OnlyUsersService.API.Models.Entities;
using OnlyUsersService.API.Models.RequestModels.Users;
using OnlyUsersService.API.Models.ResponseModels.BaseModels;
using OnlyUsersService.API.Models.ResponseModels.Users;
using OnlyUsersService.API.Repositories.Interface;
using OnlyUsersService.API.Services.Interface;

namespace OnlyUsersService.API.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IEncryptionService _encryptionService;

        public UserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IEncryptionService encryptionService = null)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _encryptionService = encryptionService ?? new EncryptionService();
        }

        public async Task<BaseResponseModel> GetByIdAsync(long id)
        {
            var response = new BaseResponseModel();
            var user = await _userRepository.GetByIdWithDetailsAsync(id);

            if (user == null)
            {
                response.FailedModel($"User with ID {id} was not found.");
                return response;
            }

            var model = MapToUserResponseModel(user);
            response.SuccessModel(model);
            return response;
        }

        public async Task<BaseResponseModel> GetAllAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            var response = new BaseResponseModel();
            var (users, totalCount) = await _userRepository.GetUsersPagedAsync(pageNumber, pageSize, searchTerm);

            var userModels = users.Select(MapToUserResponseModel).ToList();

            var paginatedData = new
            {
                Users = userModels,
                Pagination = new PaginationResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalCount
                }
            };

            response.SuccessModel(paginatedData);
            return response;
        }

        public async Task<BaseResponseModel> CreateAsync(CreateUserRequestModel request)
        {
            var response = new BaseResponseModel();

            if (request == null)
            {
                response.ValidationModel("User creation payload cannot be null.");
                return response;
            }

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                response.ValidationModel("Username, Email, and Password are required fields.");
                return response;
            }

            if (await _userRepository.AnyAsync(u => u.Username.ToLower() == request.Username.Trim().ToLower()))
            {
                response.ValidationModel($"Username '{request.Username}' is already taken.");
                return response;
            }

            if (await _userRepository.AnyAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower()))
            {
                response.ValidationModel($"Email '{request.Email}' is already registered.");
                return response;
            }

            var passwordHash = _encryptionService.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username.Trim(),
                Email = request.Email.Trim(),
                PasswordHash = passwordHash,
                FirstName = request.FirstName?.Trim(),
                LastName = request.LastName?.Trim(),
                PhoneNumber = request.PhoneNumber?.Trim(),
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                Profile = new UserProfile
                {
                    Department = request.Department?.Trim(),
                    Designation = request.Designation?.Trim(),
                    Address = request.Address?.Trim(),
                    City = request.City?.Trim(),
                    Country = request.Country?.Trim(),
                    Bio = request.Bio?.Trim(),
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            var requestedRoles = (request.Roles != null && request.Roles.Any()) ? request.Roles : new List<string> { AppRoles.User };
            await _userRepository.AssignRolesAsync(user.Id, requestedRoles, replaceExisting: true);

            var createdUser = await _userRepository.GetByIdWithDetailsAsync(user.Id);
            response.SuccessModel(MapToUserResponseModel(createdUser), "User created successfully.");
            return response;
        }

        public async Task<BaseResponseModel> UpdateAsync(long id, UpdateUserRequestModel request)
        {
            var response = new BaseResponseModel();

            if (request == null)
            {
                response.ValidationModel("Update payload cannot be null.");
                return response;
            }

            var user = await _userRepository.GetByIdWithDetailsAsync(id);
            if (user == null)
            {
                response.FailedModel($"User with ID {id} was not found.");
                return response;
            }

            // Check if email conflicts with another user
            if (!string.IsNullOrWhiteSpace(request.Email) && !user.Email.Equals(request.Email.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                if (await _userRepository.AnyAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower() && u.Id != id))
                {
                    response.ValidationModel($"Email '{request.Email}' is already used by another account.");
                    return response;
                }
                user.Email = request.Email.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.FirstName)) user.FirstName = request.FirstName.Trim();
            if (!string.IsNullOrWhiteSpace(request.LastName)) user.LastName = request.LastName.Trim();
            if (request.PhoneNumber != null) user.PhoneNumber = request.PhoneNumber.Trim();
            if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;

            if (!string.IsNullOrWhiteSpace(request.NewPassword))
            {
                user.PasswordHash = _encryptionService.HashPassword(request.NewPassword);
            }

            user.UpdatedAt = DateTime.UtcNow;

            // Update Profile fields
            if (user.Profile == null)
            {
                user.Profile = new UserProfile { UserId = user.Id };
            }

            if (request.Department != null) user.Profile.Department = request.Department.Trim();
            if (request.Designation != null) user.Profile.Designation = request.Designation.Trim();
            if (request.Address != null) user.Profile.Address = request.Address.Trim();
            if (request.City != null) user.Profile.City = request.City.Trim();
            if (request.Country != null) user.Profile.Country = request.Country.Trim();
            if (request.Bio != null) user.Profile.Bio = request.Bio.Trim();
            user.Profile.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            // Update Roles if provided
            if (request.Roles != null && request.Roles.Any())
            {
                await _userRepository.AssignRolesAsync(user.Id, request.Roles, replaceExisting: true);
            }

            var updatedUser = await _userRepository.GetByIdWithDetailsAsync(user.Id);
            response.SuccessModel(MapToUserResponseModel(updatedUser), "User updated successfully.");
            return response;
        }

        public async Task<BaseResponseModel> DeleteAsync(long id)
        {
            var response = new BaseResponseModel();
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                response.FailedModel($"User with ID {id} was not found.");
                return response;
            }

            _userRepository.Remove(user);
            await _userRepository.SaveChangesAsync();

            response.SuccessModel(null, $"User with ID {id} was deleted successfully.");
            return response;
        }

        public async Task<BaseResponseModel> GetProfileAsync(long userId)
        {
            var response = new BaseResponseModel();
            var user = await _userRepository.GetByIdWithDetailsAsync(userId);

            if (user == null)
            {
                response.FailedModel("User profile not found.");
                return response;
            }

            response.SuccessModel(MapToUserResponseModel(user));
            return response;
        }

        public async Task<BaseResponseModel> UpdateProfileAsync(long userId, UpdateProfileRequestModel request)
        {
            var response = new BaseResponseModel();

            if (request == null)
            {
                response.ValidationModel("Profile data cannot be null.");
                return response;
            }

            var user = await _userRepository.GetByIdWithDetailsAsync(userId);
            if (user == null)
            {
                response.FailedModel("User not found.");
                return response;
            }

            if (!string.IsNullOrWhiteSpace(request.FirstName)) user.FirstName = request.FirstName.Trim();
            if (!string.IsNullOrWhiteSpace(request.LastName)) user.LastName = request.LastName.Trim();
            if (request.PhoneNumber != null) user.PhoneNumber = request.PhoneNumber.Trim();
            user.UpdatedAt = DateTime.UtcNow;

            if (user.Profile == null)
            {
                user.Profile = new UserProfile { UserId = user.Id };
            }

            user.Profile.Department = request.Department?.Trim();
            user.Profile.Designation = request.Designation?.Trim();
            user.Profile.Address = request.Address?.Trim();
            user.Profile.City = request.City?.Trim();
            user.Profile.Country = request.Country?.Trim();
            user.Profile.Bio = request.Bio?.Trim();
            user.Profile.AvatarUrl = request.AvatarUrl?.Trim();
            user.Profile.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            var updatedUser = await _userRepository.GetByIdWithDetailsAsync(userId);
            response.SuccessModel(MapToUserResponseModel(updatedUser), "Profile updated successfully.");
            return response;
        }

        public async Task<BaseResponseModel> AssignRolesAsync(AssignRoleRequestModel request)
        {
            var response = new BaseResponseModel();

            if (request == null || !request.UserId.HasValue || request.UserId.Value <= 0)
            {
                response.ValidationModel("Valid UserId is required.");
                return response;
            }

            long targetUserId = request.UserId.Value;

            if (request.Roles == null || !request.Roles.Any())
            {
                response.ValidationModel("At least one role name must be provided.");
                return response;
            }

            var user = await _userRepository.GetByIdWithDetailsAsync(targetUserId);
            if (user == null)
            {
                response.FailedModel($"User with ID {targetUserId} was not found.");
                return response;
            }

            var success = await _userRepository.AssignRolesAsync(targetUserId, request.Roles, request.ReplaceExisting);
            if (!success)
            {
                response.FailedModel("Failed to assign roles. Verify that the requested roles exist.");
                return response;
            }

            var updatedUser = await _userRepository.GetByIdWithDetailsAsync(targetUserId);
            response.SuccessModel(MapToUserResponseModel(updatedUser), "Roles assigned successfully.");
            return response;
        }

        public async Task<BaseResponseModel> RemoveRolesAsync(long userId, IEnumerable<string> roles)
        {
            var response = new BaseResponseModel();

            if (userId <= 0 || roles == null || !roles.Any())
            {
                response.ValidationModel("Valid UserId and roles list are required.");
                return response;
            }

            var user = await _userRepository.GetByIdWithDetailsAsync(userId);
            if (user == null)
            {
                response.FailedModel($"User with ID {userId} was not found.");
                return response;
            }

            await _userRepository.RemoveRolesAsync(userId, roles);

            var updatedUser = await _userRepository.GetByIdWithDetailsAsync(userId);
            response.SuccessModel(MapToUserResponseModel(updatedUser), "Roles removed successfully.");
            return response;
        }

        private static UserResponseModel MapToUserResponseModel(User user)
        {
            if (user == null) return null;

            return new UserResponseModel
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>(),
                Profile = user.Profile != null ? new UserProfileResponseModel
                {
                    Id = user.Profile.Id,
                    UserId = user.Profile.UserId,
                    Department = user.Profile.Department,
                    Designation = user.Profile.Designation,
                    Address = user.Profile.Address,
                    City = user.Profile.City,
                    Country = user.Profile.Country,
                    Bio = user.Profile.Bio,
                    AvatarUrl = user.Profile.AvatarUrl,
                    UpdatedAt = user.Profile.UpdatedAt
                } : null
            };
        }
    }
}
