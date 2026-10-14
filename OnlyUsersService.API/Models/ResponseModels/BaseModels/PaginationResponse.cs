namespace OnlyUsersService.API.Models.ResponseModels.BaseModels
{
    public class PaginationResponse
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalRecords { get; set; }
        public int TotalPages => PageSize > 0 ? (int)System.Math.Ceiling((double)TotalRecords / PageSize) : 0;
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
