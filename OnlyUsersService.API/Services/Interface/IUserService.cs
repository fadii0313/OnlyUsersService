using System.Collections.Generic;
using System.Threading.Tasks;
using OnlyUsersService.API.Models.RequestModels.Users;
using OnlyUsersService.API.Models.ResponseModels.BaseModels;

namespace OnlyUsersService.API.Services.Interface
{
    public interface IUserService
    {
        Task<BaseResponseModel> GetByIdAsync(long id);
        Task<BaseResponseModel> GetAllAsync(int pageNumber, int pageSize, string searchTerm = null);
        Task<BaseResponseModel> CreateAsync(CreateUserRequestModel request);
        Task<BaseResponseModel> UpdateAsync(long id, UpdateUserRequestModel request);
        Task<BaseResponseModel> DeleteAsync(long id);
        Task<BaseResponseModel> GetProfileAsync(long userId);
        Task<BaseResponseModel> UpdateProfileAsync(long userId, UpdateProfileRequestModel request);
        Task<BaseResponseModel> AssignRolesAsync(AssignRoleRequestModel request);
        Task<BaseResponseModel> RemoveRolesAsync(long userId, IEnumerable<string> roles);
    }
}
