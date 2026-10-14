using System.ComponentModel.DataAnnotations;

namespace OnlyUsersService.API.Models.RequestModels.Auth
{
    public class LoginRequestModel
    {
        [Required(ErrorMessage = "Username or Email is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}
