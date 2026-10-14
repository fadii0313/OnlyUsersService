using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlyUsersService.API.Models.RequestModels.Users
{
    public class UpdateUserRequestModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "FirstName is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "LastName is required.")]
        public string LastName { get; set; }

        public string PhoneNumber { get; set; }
        public bool? IsActive { get; set; }

        // Optional new password if changing password
        public string NewPassword { get; set; }

        // Optional roles update
        public List<string> Roles { get; set; }

        // Profile fields
        public string Department { get; set; }
        public string Designation { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Bio { get; set; }
    }
}
