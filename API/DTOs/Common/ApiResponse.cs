namespace MasterBilling.API.DTOs.Common
{
    /// <summary>
    /// Standard API response wrapper for all endpoints
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }
        public int? TotalCount { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
        {
            return new ApiResponse<T> { Success = true, Message = message, Data = data };
        }

        public static ApiResponse<T> SuccessResponse(T data, string message, int totalCount, int page, int pageSize)
        {
            return new ApiResponse<T> { Success = true, Message = message, Data = data, TotalCount = totalCount, Page = page, PageSize = pageSize };
        }

        public static ApiResponse<T> FailResponse(string message, List<string>? errors = null)
        {
            return new ApiResponse<T> { Success = false, Message = message, Errors = errors };
        }
    }

    /// <summary>
    /// Paginated request parameters
    /// </summary>
    public class PaginationRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; } = "asc";
        public string? SearchQuery { get; set; }
    }

    /// <summary>
    /// Date range filter
    /// </summary>
    public class DateRangeFilter
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
