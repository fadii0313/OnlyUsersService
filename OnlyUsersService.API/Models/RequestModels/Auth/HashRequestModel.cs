using System.ComponentModel.DataAnnotations;

namespace OnlyUsersService.API.Models.RequestModels.Auth
{
    /// <summary>
    /// Payload for SHA-256 cryptographic hashing operations.
    /// </summary>
    public class HashRequestModel
    {
        [Required(ErrorMessage = "Text to hash is required.")]
        public string Text { get; set; }

        public string Salt { get; set; }
    }
}
