using System;
using System.Collections.Generic;

namespace OnlyUsersService.API.Models.ResponseModels.Auth
{
    public class LoginResponseModel
    {
        public string Token { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public DateTime ExpiresAt { get; set; }
        public long UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
