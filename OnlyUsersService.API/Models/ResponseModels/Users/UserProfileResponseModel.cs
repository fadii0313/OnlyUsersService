using System;

namespace OnlyUsersService.API.Models.ResponseModels.Users
{
    public class UserProfileResponseModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Bio { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
