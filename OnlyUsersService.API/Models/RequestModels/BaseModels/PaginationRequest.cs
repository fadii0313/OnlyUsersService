namespace OnlyUsersService.API.Models.RequestModels.BaseModels
{
    public class PaginationRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; }
        public string SortBy { get; set; } = "Id";
        public bool IsAscending { get; set; } = true;
    }
}
