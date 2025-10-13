namespace Travel.Application.DTOs
{
    public class PagedResponse<T>
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string>? Errors { get; set; }
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public PaginationInfo Pagination { get; set; }

        public static PagedResponse<T> Success(IEnumerable<T> data, int currentPage, int pageSize, int totalItems, string message = "")
        {
            return new PagedResponse<T>
            {
                Succeeded = true,
                Message = message,
                Data = data,
                Pagination = new PaginationInfo
                {
                    CurrentPage = currentPage,
                    PageSize = pageSize,
                    TotalItems = totalItems
                }
            };
        }

        public static PagedResponse<T> Fail(string message, List<string>? errors = null)
        {
            return new PagedResponse<T>
            {
                Succeeded = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}
