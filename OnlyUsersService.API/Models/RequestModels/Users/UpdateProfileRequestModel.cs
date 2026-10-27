using System.ComponentModel.DataAnnotations;

namespace OnlyUsersService.API.Models.RequestModels.Users
{
    public class UpdateProfileRequestModel
    {
        [Required(ErrorMessage = "FirstName is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "LastName is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; set; }

        [StringLength(25, ErrorMessage = "Phone number cannot exceed 25 characters.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; }

        [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters.")]
        public string Department { get; set; }

        [StringLength(100, ErrorMessage = "Designation cannot exceed 100 characters.")]
        public string Designation { get; set; }

        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters.")]
        public string Address { get; set; }

        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string City { get; set; }

        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters.")]
        public string Country { get; set; }

        [StringLength(1000, ErrorMessage = "Bio cannot exceed 1000 characters.")]
        public string Bio { get; set; }

        [StringLength(500, ErrorMessage = "Avatar URL cannot exceed 500 characters.")]
        [Url(ErrorMessage = "Invalid avatar URL format.")]
        public string AvatarUrl { get; set; }
    }
}

