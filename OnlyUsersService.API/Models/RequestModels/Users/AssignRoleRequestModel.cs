using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlyUsersService.API.Models.RequestModels.Users
{
    public class AssignRoleRequestModel
    {
        /// <summary>
        /// Optional target UserId. If omitted, defaults to currently authenticated user context.
        /// </summary>
        [Range(1, long.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public long? UserId { get; set; }

        [Required(ErrorMessage = "At least one role must be specified.")]
        [MinLength(1, ErrorMessage = "At least one role must be specified.")]
        public List<string> Roles { get; set; } = new List<string>();

        /// <summary>
        /// If true, replaces existing roles with the provided list.
        /// If false, appends the provided roles.
        /// </summary>
        public bool ReplaceExisting { get; set; } = true;
    }
}

