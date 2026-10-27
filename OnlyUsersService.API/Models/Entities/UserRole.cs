using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlyUsersService.API.Models.Entities
{
    [Table("UserRoles")]
    public class UserRole
    {
        [Required]
        [ForeignKey(nameof(User))]
        public long UserId { get; set; }

        public virtual User User { get; set; }

        [Required]
        [ForeignKey(nameof(Role))]
        public int RoleId { get; set; }

        public virtual Role Role { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}

