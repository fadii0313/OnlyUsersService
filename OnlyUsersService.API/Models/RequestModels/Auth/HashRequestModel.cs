using System.ComponentModel.DataAnnotations;

namespace OnlyUsersService.API.Models.RequestModels.Auth
{
    /// <summary>
    /// Payload for SHA-256 cryptographic hashing operations.
    /// </summary>
    public class HashRequestModel
    {
        [Required(ErrorMessage = "Text to hash is required.")]
        [StringLength(10000, ErrorMessage = "Text cannot exceed 10,000 characters.")]
        public string Text { get; set; }

        [StringLength(256, ErrorMessage = "Salt cannot exceed 256 characters.")]
        public string Salt { get; set; }
    }
}

