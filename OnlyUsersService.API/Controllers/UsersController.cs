using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlyUsersService.API.Common;
using OnlyUsersService.API.Models.RequestModels.Auth;
using OnlyUsersService.API.Models.RequestModels.Users;
using OnlyUsersService.API.Models.ResponseModels.Auth;
using OnlyUsersService.API.Models.ResponseModels.BaseModels;
using OnlyUsersService.API.Models.ResponseModels.Users;
using OnlyUsersService.API.Services.Interface;
using Swashbuckle.AspNetCore.Annotations;

namespace OnlyUsersService.API.Controllers
{
    /// <summary>
    /// User authentication, profile management, role management, and user CRUD operations.
    /// </summary>
    [ApiController]
    [ApiVersion("1")]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class UsersController : CustomBaseController
    {
        private readonly IJwtAuthService _jwtAuthService;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly ICurrentUserContext _currentUserContext;
        private readonly IEncryptionService _encryptionService;

        public UsersController(
            IJwtAuthService jwtAuthService,
            IUserService userService,
            IRoleService roleService,
            ICurrentUserContext currentUserContext,
            IEncryptionService encryptionService = null)
        {
            _jwtAuthService = jwtAuthService;
            _userService = userService;
            _roleService = roleService;
            _currentUserContext = currentUserContext;
            _encryptionService = encryptionService ?? new EncryptionService();
        }

        /// <summary>
        /// Compute a SHA-256 cryptographic hash for data encryption and validation.
        /// No User ID or Office ID required.
        /// </summary>
        /// <param name="request">Text to hash and optional salt.</param>
        /// <returns>Computed SHA-256 hexadecimal hash.</returns>
        [HttpPost("hash")]
        [HttpPost("encrypt")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Compute SHA-256 Hash", Description = "Generates a standard SHA-256 hash. Can be used for password hashing, payload integrity, and encryption without passing any User ID or Office ID.")]
        [ProducesResponseType(typeof(BaseResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponseModel), StatusCodes.Status400BadRequest)]
        public IActionResult ComputeSha256([FromBody] HashRequestModel request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Text))
            {
                var validationResponse = new BaseResponseModel(false);
                validationResponse.ValidationModel("Text to hash cannot be null or empty.");
                return BadRequest(validationResponse);
            }

            var hash = _encryptionService.ComputeSha256Hash(request.Text, request.Salt);
            var response = new BaseResponseModel(true);
            response.SuccessModel(new
            {
                text = request.Text,
                salt = request.Salt,
                sha256Hash = hash,
                algorithm = "SHA-256"
            }, "SHA-256 hash computed successfully.");

            return Ok(response);
        }

        /// <summary>
        /// Register a new user account.
        /// </summary>
        /// <param name="request">User registration credentials and profile details.</param>
        /// <returns>Standard BaseResponseModel containing newly created user info and JWT session token.</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Register a new user", Description = "Public endpoint to register a new user account. Automatically assigns default 'User' role and returns JWT.")]
        [ProducesResponseType(typeof(BaseResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponseModel), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestModel request)
        {
            var result = await _jwtAuthService.RegisterAsync(request);
            return HandleResult(result);
        }

        /// <summary>
        /// Authenticate user credentials and retrieve a JWT Bearer token.
        /// </summary>
        /// <param name="request">Username/Email and Password.</param>
        /// <returns>Standard BaseResponseModel containing JWT token, expiration, user ID, and assigned roles.</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "User Login", Description = "Authenticates user credentials and generates a signed JWT token. Use this token in Swagger's Authorize dialog (top right).")]
        [ProducesResponseType(typeof(BaseResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponseModel), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel request)
        {
            var result = await _jwtAuthService.LoginAsync(request);
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieve the profile of the currently authenticated user.
        /// </summary>
        /// <returns>Current user information and profile.</returns>
        [HttpGet("profile")]
        [HttpGet("me")]
        [Authorize]
        [SwaggerOperation(Summary = "Get Current User Profile", Description = "Securely retrieves profile details for the authenticated user without exposing UserID in the route.")]
        [ProducesResponseType(typeof(BaseResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentProfile()
        {
            long currentUserId = _currentUserContext.UserId;
            if (currentUserId <= 0)
            {
                return Unauthorized(new BaseResponseModel(false)
                {
                    ResponseCode = ApiResponseCode.Unauthorized,
                    ResponseMessage = "User is not authenticated."
                });
            }

            var result = await _userService.GetProfileAsync(currentUserId);
            return HandleResult(result);
        }

        /// <summary>
        /// Update the profile of the currently authenticated user.
        /// </summary>
        /// <param name="request">Updated profile details.</param>
        /// <returns>Updated user profile.</returns>
        [HttpPut("profile")]
        [Authorize]
        [SwaggerOperation(Summary = "Update Current User Profile", Description = "Updates profile details for the authenticated user.")]
        [ProducesResponseType(typeof(BaseResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateCurrentProfile([FromBody] UpdateProfileRequestModel request)
        {
            long currentUserId = _currentUserContext.UserId;
            if (currentUserId <= 0)
            {
                return Unauthorized(new BaseResponseModel(false)
                {
                    ResponseCode = ApiResponseCode.Unauthorized,
                    ResponseMessage = "User is not authenticated."
                });
            }

            var result = await _userService.UpdateProfileAsync(currentUserId, request);
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieve all users with pagination and search filter. Restricted to Admin.
        /// </summary>
        /// <param name="pageNumber">Page index (default: 1).</param>
        /// <param name="pageSize">Number of items per page (default: 10).</param>
        /// <param name="searchTerm">Optional search query across username, email, name, department.</param>
        /// <returns>Paginated list of users.</returns>
        [HttpGet]
        [Authorize(Roles = AppRoles.AdminRoles)]
        [SwaggerOperation(Summary = "Get All Users (Admin Only)", Description = "Retrieves all registered users with pagination and optional search filter. Restricted to Admin.")]
        [ProducesResponseType(typeof(BaseResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery, Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1.")] int pageNumber = 1,
            [FromQuery, Range(1, 100, ErrorMessage = "Page size must be between 1 and 100.")] int pageSize = 10,
            [FromQuery, StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters.")] string searchTerm = null)
        {
            var result = await _userService.GetAllAsync(pageNumber, pageSize, searchTerm);
            return HandleResult(result);
        }

        /// <summary>
        /// Admin Create User: Create a new user with specific roles and profile.
        /// </summary>
        /// <param name="request">New user creation parameters.</param>
        /// <returns>Created user response.</returns>
        [HttpPost]
        [HttpPost("create")]
        [Authorize(Roles = AppRoles.AdminRoles)]
        [SwaggerOperation(Summary = "Create User (Admin Only)", Description = "Allows administrators to create a user with specified roles.")]
        [ProducesResponseType(typeof(BaseResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestModel request)
        {
            var result = await _userService.CreateAsync(request);
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieve user details by User ID.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <returns>User details.</returns>
        [HttpGet("{id:long}")]
        [Authorize]
        [SwaggerOperation(Summary = "Get User by ID", Description = "Retrieves user details by ID. Non-admin users may only view their own user record.")]
        [ProducesResponseType(typeof(BaseResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(
            [FromRoute, Range(1, long.MaxValue, ErrorMessage = "User ID must be greater than zero.")] long id)
        {
            long currentUserId = _currentUserContext.UserId;
            bool isAdmin = _currentUserContext.IsInRole(AppRoles.Admin);

            if (!isAdmin && currentUserId != id)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new BaseResponseModel(false)
                {
                    ResponseCode = ApiResponseCode.Forbidden,
                    ResponseMessage = "You are not authorized to view another user's details."
                });
            }

            var result = await _userService.GetByIdAsync(id);
            return HandleResult(result);
        }

        /// <summary>
        /// Update user details by User ID.
        /// </summary>
        /// <param name="id">User ID to update.</param>
        /// <param name="request">Updated fields.</param>
        /// <returns>Updated user model.</returns>
        [HttpPut("{id:long}")]
        [Authorize]
        [SwaggerOperation(Summary = "Update User by ID", Description = "Updates user fields. Normal users can only update their own record; Admins can update any user and modify roles/status.")]
        [ProducesResponseType(typeof(BaseResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(
            [FromRoute, Range(1, long.MaxValue, ErrorMessage = "User ID must be greater than zero.")] long id,
            [FromBody] UpdateUserRequestModel request)
        {
            long currentUserId = _currentUserContext.UserId;
            bool isAdmin = _currentUserContext.IsInRole(AppRoles.Admin);

            if (!isAdmin && currentUserId != id)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new BaseResponseModel(false)
                {
                    ResponseCode = ApiResponseCode.Forbidden,
                    ResponseMessage = "You are not authorized to update another user's record."
                });
            }

            // If not admin, non-admins cannot change their own roles or active status
            if (!isAdmin && request != null)
            {
                request.Roles = null;
                request.IsActive = null;
            }

            var result = await _userService.UpdateAsync(id, request);
            return HandleResult(result);
        }

        /// <summary>
        /// Delete user by User ID. Restricted to Admin.
        /// </summary>
        /// <param name="id">User ID to delete.</param>
        /// <returns>Deletion confirmation.</returns>
        [HttpDelete("{id:long}")]
        [Authorize(Roles = AppRoles.AdminRoles)]
        [SwaggerOperation(Summary = "Delete User by ID (Admin Only)", Description = "Permanently deletes a user by ID. Restricted to Admin.")]
        [ProducesResponseType(typeof(BaseResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(
            [FromRoute, Range(1, long.MaxValue, ErrorMessage = "User ID must be greater than zero.")] long id)
        {
            long currentUserId = _currentUserContext.UserId;
            if (currentUserId == id)
            {
                return BadRequest(new BaseResponseModel(false)
                {
                    ResponseCode = ApiResponseCode.Validation,
                    ResponseMessage = "Administrators cannot delete their own active account."
                });
            }

            var result = await _userService.DeleteAsync(id);
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieve assigned roles for the currently authenticated user.
        /// No User ID or Office ID parameter required.
        /// </summary>
        /// <returns>Assigned roles for authenticated caller.</returns>
        [HttpGet("my-roles")]
        [Authorize]
        [SwaggerOperation(Summary = "Get My Roles", Description = "Securely retrieves roles for the authenticated user context. No User ID or Office ID parameter is required.")]
        [ProducesResponseType(typeof(BaseResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyRoles()
        {
            long currentUserId = _currentUserContext.UserId;
            var result = await _roleService.GetUserRolesAsync(currentUserId);
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieve all available system roles or user-assigned roles.
        /// If userId is omitted, defaults to authenticated user context (no user ID or office ID parameter required).
        /// </summary>
        /// <param name="userId">Optional target User ID. If omitted, uses authenticated user context.</param>
        /// <returns>List of roles.</returns>
        [HttpGet("roles")]
        [Authorize]
        [SwaggerOperation(Summary = "Get Roles", Description = "Gets system roles, or user-assigned roles. If userId parameter is omitted, defaults to authenticated user context without passing any parameters.")]
        [ProducesResponseType(typeof(BaseResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRoles(
            [FromQuery, Range(1, long.MaxValue, ErrorMessage = "User ID must be greater than zero.")] long? userId = null)
        {
            bool isAdmin = _currentUserContext.IsInRole(AppRoles.Admin);
            long targetId = userId.HasValue && userId.Value > 0 ? userId.Value : (!isAdmin ? _currentUserContext.UserId : 0);

            if (targetId > 0)
            {
                var userRolesResult = await _roleService.GetUserRolesAsync(targetId);
                return HandleResult(userRolesResult);
            }

            var result = await _roleService.GetAllRolesAsync();
            return HandleResult(result);
        }

        /// <summary>
        /// Assign role(s) to a user. Restricted to Admin.
        /// If UserId is omitted in the body, defaults to authenticated user context.
        /// </summary>
        /// <param name="request">Role assignment payload.</param>
        /// <returns>Updated user with assigned roles.</returns>
        [HttpPost("roles")]
        [HttpPut("roles")]
        [Authorize(Roles = AppRoles.AdminRoles)]
        [SwaggerOperation(Summary = "Assign Roles to User (Admin Only)", Description = "Assigns or replaces roles. If UserId is omitted in body, defaults to authenticated user context.")]
        [ProducesResponseType(typeof(BaseResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AssignRoles([FromBody] AssignRoleRequestModel request)
        {
            if (request == null)
            {
                return BadRequest(new BaseResponseModel(false) { ResponseCode = ApiResponseCode.Validation, ResponseMessage = "Payload cannot be null." });
            }

            if (!request.UserId.HasValue || request.UserId.Value <= 0)
            {
                request.UserId = _currentUserContext.UserId;
            }

            var result = await _userService.AssignRolesAsync(request);
            return HandleResult(result);
        }

        /// <summary>
        /// Remove role(s) from a user. Restricted to Admin.
        /// If userId query parameter is omitted, defaults to authenticated user context.
        /// </summary>
        /// <param name="roles">List of role names to remove.</param>
        /// <param name="userId">Optional target User ID.</param>
        /// <returns>Updated user model.</returns>
        [HttpDelete("roles")]
        [Authorize(Roles = AppRoles.AdminRoles)]
        [SwaggerOperation(Summary = "Remove Roles from User (Admin Only)", Description = "Removes specific roles. If userId parameter is omitted, defaults to authenticated user context.")]
        [ProducesResponseType(typeof(BaseResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RemoveRoles(
            [FromQuery, Required(ErrorMessage = "At least one role must be specified.")] List<string> roles,
            [FromQuery, Range(1, long.MaxValue, ErrorMessage = "User ID must be greater than zero.")] long? userId = null)
        {
            long targetId = (userId.HasValue && userId.Value > 0) ? userId.Value : _currentUserContext.UserId;
            var result = await _userService.RemoveRolesAsync(targetId, roles);
            return HandleResult(result);
        }
    }
}
