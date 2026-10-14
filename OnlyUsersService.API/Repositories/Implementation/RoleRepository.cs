using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OnlyUsersService.API.Data;
using OnlyUsersService.API.Models.Entities;
using OnlyUsersService.API.Repositories.Interface;

namespace OnlyUsersService.API.Repositories.Implementation
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(OnlyUsersDbContext context) : base(context)
        {
        }

        public async Task<Role> GetByNameAsync(string roleName)
        {
            return await _context.Roles
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower());
        }

        public async Task<IEnumerable<Role>> GetAllWithUserCountsAsync()
        {
            return await _context.Roles
                .Include(r => r.UserRoles)
                .OrderBy(r => r.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetRolesByUserIdAsync(long userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.Name)
                .ToListAsync();
        }
    }
}
