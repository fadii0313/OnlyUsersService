using System.Threading.Tasks;
using OnlyUsersService.API.Models.RequestModels.Auth;
using OnlyUsersService.API.Models.ResponseModels.BaseModels;

namespace OnlyUsersService.API.Services.Interface
{
    public interface IJwtAuthService
    {
        Task<BaseResponseModel> LoginAsync(LoginRequestModel request);
        Task<BaseResponseModel> RegisterAsync(RegisterRequestModel request);
    }
}
