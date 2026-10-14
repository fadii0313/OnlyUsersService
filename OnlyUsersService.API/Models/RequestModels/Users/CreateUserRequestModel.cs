using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlyUsersService.API.Models.RequestModels.Users
{
    public class CreateUserRequestModel
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "FirstName is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "LastName is required.")]
        public string LastName { get; set; }

        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; } = true;

        public List<string> Roles { get; set; } = new List<string> { "User" };

        // Profile fields
        public string Department { get; set; }
        public string Designation { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Bio { get; set; }
    }
}
