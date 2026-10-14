using System;

namespace OnlyUsersService.API.Models.ResponseModels.Users
{
    public class RoleResponseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int UserCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
