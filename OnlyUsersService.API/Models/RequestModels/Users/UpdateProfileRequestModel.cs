using System.ComponentModel.DataAnnotations;

namespace OnlyUsersService.API.Models.RequestModels.Users
{
    public class UpdateProfileRequestModel
    {
        [Required(ErrorMessage = "FirstName is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "LastName is required.")]
        public string LastName { get; set; }

        public string PhoneNumber { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Bio { get; set; }
        public string AvatarUrl { get; set; }
    }
}
