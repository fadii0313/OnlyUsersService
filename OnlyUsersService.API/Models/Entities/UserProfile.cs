using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlyUsersService.API.Models.Entities
{
    [Table("UserProfiles")]
    public class UserProfile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public long UserId { get; set; }

        public virtual User User { get; set; }

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

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

