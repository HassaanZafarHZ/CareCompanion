namespace CareOS.Api.Helpers
{
    public class PaginatedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public long TotalRecords { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    public static class PaginationHelper
    {
        public static PaginatedResult<T> CreatePaginatedResult<T>(
            List<T> data,
            int page,
            int pageSize,
            long totalRecords)
        {
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            return new PaginatedResult<T>
            {
                Data = data,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalRecords = totalRecords,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            };
        }
    }
}