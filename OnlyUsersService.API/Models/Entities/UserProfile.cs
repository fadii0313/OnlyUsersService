using System;

namespace OnlyUsersService.API.Models.Entities
{
    public class UserProfile
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public virtual User User { get; set; }

        public string Department { get; set; }
        public string Designation { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Bio { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
