using System.ComponentModel.DataAnnotations;

namespace OnlyUsersService.API.Models.RequestModels.BaseModels
{
    public class PaginationRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be greater than or equal to 1.")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
        public int PageSize { get; set; } = 10;

        [StringLength(100, ErrorMessage = "SearchTerm cannot exceed 100 characters.")]
        public string SearchTerm { get; set; }

        [StringLength(50, ErrorMessage = "SortBy cannot exceed 50 characters.")]
        public string SortBy { get; set; } = "Id";

        public bool IsAscending { get; set; } = true;
    }
}

