using System.ComponentModel.DataAnnotations;

namespace OnlyUsersService.API.Models.RequestModels.Auth
{
    public class LoginRequestModel
    {
        [Required(ErrorMessage = "Username or Email is required.")]
        [StringLength(100, ErrorMessage = "Username or Email cannot exceed 100 characters.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
        public string Password { get; set; }
    }
}

