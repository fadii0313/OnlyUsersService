using System.Collections.Generic;
using System.Threading.Tasks;
using OnlyUsersService.API.Models.Entities;

namespace OnlyUsersService.API.Repositories.Interface
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> GetByIdWithDetailsAsync(long id);
        Task<User> GetByUsernameWithDetailsAsync(string username);
        Task<User> GetByEmailWithDetailsAsync(string email);
        Task<User> GetByUsernameOrEmailWithDetailsAsync(string usernameOrEmail);
        Task<(IEnumerable<User> Users, int TotalCount)> GetUsersPagedAsync(int pageNumber, int pageSize, string searchTerm = null);
        Task<bool> AssignRolesAsync(long userId, IEnumerable<string> roleNames, bool replaceExisting);
        Task<bool> RemoveRolesAsync(long userId, IEnumerable<string> roleNames);
    }
}
