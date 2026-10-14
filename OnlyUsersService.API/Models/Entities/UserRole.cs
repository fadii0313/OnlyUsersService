using System;

namespace OnlyUsersService.API.Models.Entities
{
    public class UserRole
    {
        public long UserId { get; set; }
        public virtual User User { get; set; }

        public int RoleId { get; set; }
        public virtual Role Role { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
