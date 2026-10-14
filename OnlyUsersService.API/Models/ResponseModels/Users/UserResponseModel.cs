using System;
using System.Collections.Generic;

namespace OnlyUsersService.API.Models.ResponseModels.Users
{
    public class UserResponseModel
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public List<string> Roles { get; set; } = new List<string>();
        public UserProfileResponseModel Profile { get; set; }
    }
}
