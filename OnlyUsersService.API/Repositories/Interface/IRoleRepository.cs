using System.Collections.Generic;
using System.Threading.Tasks;
using OnlyUsersService.API.Models.Entities;

namespace OnlyUsersService.API.Repositories.Interface
{
    public interface IRoleRepository : IGenericRepository<Role>
    {
        Task<Role> GetByNameAsync(string roleName);
        Task<IEnumerable<Role>> GetAllWithUserCountsAsync();
        Task<IEnumerable<string>> GetRolesByUserIdAsync(long userId);
    }
}
