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
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(OnlyUsersDbContext context) : base(context)
        {
        }

        public async Task<User> GetByIdWithDetailsAsync(long id)
        {
            return await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> GetByUsernameWithDetailsAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }

        public async Task<User> GetByEmailWithDetailsAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User> GetByUsernameOrEmailWithDetailsAsync(string usernameOrEmail)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail))
                return null;

            var target = usernameOrEmail.Trim().ToLower();
            return await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username.ToLower() == target || u.Email.ToLower() == target);
        }

        public async Task<(IEnumerable<User> Users, int TotalCount)> GetUsersPagedAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            var query = _context.Users
                .Include(u => u.Profile)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(u =>
                    u.Username.ToLower().Contains(term) ||
                    u.Email.ToLower().Contains(term) ||
                    u.FirstName.ToLower().Contains(term) ||
                    u.LastName.ToLower().Contains(term) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(term)) ||
                    (u.Profile != null && u.Profile.Department != null && u.Profile.Department.ToLower().Contains(term)));
            }

            var totalCount = await query.CountAsync();

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }

        public async Task<bool> AssignRolesAsync(long userId, IEnumerable<string> roleNames, bool replaceExisting)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return false;

            var targetRoles = await _context.Roles
                .Where(r => roleNames.Contains(r.Name))
                .ToListAsync();

            if (replaceExisting)
            {
                var existingRoles = user.UserRoles.ToList();
                _context.UserRoles.RemoveRange(existingRoles);
                await _context.SaveChangesAsync();
                user.UserRoles.Clear();
            }

            foreach (var role in targetRoles)
            {
                if (!user.UserRoles.Any(ur => ur.RoleId == role.Id))
                {
                    user.UserRoles.Add(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = role.Id,
                        AssignedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveRolesAsync(long userId, IEnumerable<string> roleNames)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return false;

            var toRemove = user.UserRoles
                .Where(ur => roleNames.Contains(ur.Role.Name, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (toRemove.Any())
            {
                _context.UserRoles.RemoveRange(toRemove);
                await _context.SaveChangesAsync();
            }

            return true;
        }
    }
}
