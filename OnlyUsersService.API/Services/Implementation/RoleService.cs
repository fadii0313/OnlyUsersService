using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OnlyUsersService.API.Models.ResponseModels.BaseModels;
using OnlyUsersService.API.Models.ResponseModels.Users;
using OnlyUsersService.API.Repositories.Interface;
using OnlyUsersService.API.Services.Interface;

namespace OnlyUsersService.API.Services.Implementation
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRepository _userRepository;

        public RoleService(IRoleRepository roleRepository, IUserRepository userRepository)
        {
            _roleRepository = roleRepository;
            _userRepository = userRepository;
        }

        public async Task<BaseResponseModel> GetAllRolesAsync()
        {
            var response = new BaseResponseModel();
            var roles = await _roleRepository.GetAllWithUserCountsAsync();

            var data = roles.Select(r => new RoleResponseModel
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                UserCount = r.UserRoles?.Count ?? 0,
                CreatedAt = r.CreatedAt
            }).ToList();

            response.SuccessModel(data);
            return response;
        }

        public async Task<BaseResponseModel> GetUserRolesAsync(long userId)
        {
            var response = new BaseResponseModel();
            var user = await _userRepository.GetByIdWithDetailsAsync(userId);

            if (user == null)
            {
                response.FailedModel($"User with ID {userId} was not found.");
                return response;
            }

            var roles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
            response.SuccessModel(new { UserId = userId, Username = user.Username, Roles = roles });
            return response;
        }
    }
}
