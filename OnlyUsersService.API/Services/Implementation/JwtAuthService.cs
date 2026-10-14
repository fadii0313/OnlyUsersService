using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OnlyUsersService.API.Common;
using OnlyUsersService.API.Models.Entities;
using OnlyUsersService.API.Models.RequestModels.Auth;
using OnlyUsersService.API.Models.ResponseModels.Auth;
using OnlyUsersService.API.Models.ResponseModels.BaseModels;
using OnlyUsersService.API.Repositories.Interface;
using OnlyUsersService.API.Services.Interface;

namespace OnlyUsersService.API.Services.Implementation
{
    public class JwtAuthService : IJwtAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IConfiguration _configuration;
        private readonly IEncryptionService _encryptionService;

        public JwtAuthService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IConfiguration configuration,
            IEncryptionService encryptionService = null)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _configuration = configuration;
            _encryptionService = encryptionService ?? new EncryptionService();
        }

        public async Task<BaseResponseModel> LoginAsync(LoginRequestModel request)
        {
            var response = new BaseResponseModel();

            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                response.ValidationModel("Username and Password are required.");
                return response;
            }

            var user = await _userRepository.GetByUsernameOrEmailWithDetailsAsync(request.Username);
            if (user == null)
            {
                response.FailedModel("Invalid Username or Password.");
                return response;
            }

            if (!user.IsActive)
            {
                response.FailedModel("Your account has been deactivated. Please contact an administrator.");
                return response;
            }

            bool isPasswordValid = _encryptionService.VerifyPassword(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                response.FailedModel("Invalid Username or Password.");
                return response;
            }

            // Transparently migrate BCrypt hash to SHA-256 hash if needed
            if (user.PasswordHash != null && user.PasswordHash.StartsWith("$2"))
            {
                user.PasswordHash = _encryptionService.HashPassword(request.Password);
            }

            // Update LastLoginAt
            user.LastLoginAt = DateTime.UtcNow;
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            var roles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
            var tokenString = GenerateToken(user, roles, out DateTime expiresAt);

            var loginData = new LoginResponseModel
            {
                Token = tokenString,
                TokenType = "Bearer",
                ExpiresAt = expiresAt,
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Roles = roles
            };

            response.SuccessModel(loginData, "Login successful.");
            return response;
        }

        public async Task<BaseResponseModel> RegisterAsync(RegisterRequestModel request)
        {
            var response = new BaseResponseModel();

            if (request == null)
            {
                response.ValidationModel("Registration request cannot be null.");
                return response;
            }

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                response.ValidationModel("Username, Email, and Password are required fields.");
                return response;
            }

            // Check if username already exists
            if (await _userRepository.AnyAsync(u => u.Username.ToLower() == request.Username.Trim().ToLower()))
            {
                response.ValidationModel($"Username '{request.Username}' is already taken. Please choose another.");
                return response;
            }

            // Check if email already exists
            if (await _userRepository.AnyAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower()))
            {
                response.ValidationModel($"An account with email '{request.Email}' already exists.");
                return response;
            }

            // Hash password securely with SHA-256
            var passwordHash = _encryptionService.HashPassword(request.Password);

            var newUser = new User
            {
                Username = request.Username.Trim(),
                Email = request.Email.Trim(),
                PasswordHash = passwordHash,
                FirstName = request.FirstName?.Trim(),
                LastName = request.LastName?.Trim(),
                PhoneNumber = request.PhoneNumber?.Trim(),
                IsActive = true,
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

            await _userRepository.AddAsync(newUser);
            await _userRepository.SaveChangesAsync();

            // Assign default User role
            var defaultRole = await _roleRepository.GetByNameAsync(AppRoles.User);
            if (defaultRole != null)
            {
                await _userRepository.AssignRolesAsync(newUser.Id, new[] { defaultRole.Name }, replaceExisting: false);
            }

            // Fetch newly created user with details
            var userWithDetails = await _userRepository.GetByIdWithDetailsAsync(newUser.Id);
            var roles = userWithDetails.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string> { AppRoles.User };
            var tokenString = GenerateToken(userWithDetails, roles, out DateTime expiresAt);

            var authResponse = new LoginResponseModel
            {
                Token = tokenString,
                TokenType = "Bearer",
                ExpiresAt = expiresAt,
                UserId = userWithDetails.Id,
                Username = userWithDetails.Username,
                Email = userWithDetails.Email,
                FullName = $"{userWithDetails.FirstName} {userWithDetails.LastName}".Trim(),
                Roles = roles
            };

            response.SuccessModel(authResponse, "User registered successfully.");
            return response;
        }

        private string GenerateToken(User user, IEnumerable<string> roles, out DateTime expiresAt)
        {
            var jwtSecret = _configuration["SecuritySettings:JwtSecretKey"] ?? "P4a59nfuPoD7r5j9TO84z0iGVSjZK0VrmkHi4UeoYE8=";
            var issuer = _configuration["SecuritySettings:JwtIssuer"] ?? "OnlyUsersService";
            var audience = _configuration["SecuritySettings:JwtAudience"] ?? "OnlyUsersService.Client";
            var expiryMinutes = int.TryParse(_configuration["SecuritySettings:TokenExpiryMinutes"], out var minutes) ? minutes : 1440;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("uid", user.Id.ToString()),
                new Claim("username", user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
