using System.Collections.Generic;
using System.Threading.Tasks;
using OnlyUsersService.API.Models.ResponseModels.BaseModels;

namespace OnlyUsersService.API.Services.Interface
{
    public interface IRoleService
    {
        Task<BaseResponseModel> GetAllRolesAsync();
        Task<BaseResponseModel> GetUserRolesAsync(long userId);
    }
}
