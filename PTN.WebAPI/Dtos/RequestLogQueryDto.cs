using PTN.WebAPI.Constants;

namespace PTN.WebAPI.Dtos
{
    public class RequestLogQueryDto
    {
        public string Status { get; set; } =
            RequestConstants.Query.AllStatus;

        public string? Query { get; set; }

        public string SortBy { get; set; } =
            RequestConstants.Query.SortById;

        public string SortDirection { get; set; } =
            RequestConstants.Query.Descending;

        public int Page { get; set; } =
            RequestConstants.Query.FirstPage;

        public int PageSize { get; set; } =
            RequestConstants.Query.DefaultPageSize;
    }
}