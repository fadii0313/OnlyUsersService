using System;
using System.Collections.Generic;

namespace OnlyUsersService.API.Models.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
