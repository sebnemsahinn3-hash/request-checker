namespace PTN.WebAPI.Entities
{
    public class RequestLogEntity
    {
        public int Id { get; set; }
        public int? StatusCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Url { get; set; } = string.Empty;
        public string RequestParams { get; set; } = string.Empty;
        public string RequestBody { get; set; } = string.Empty;
        public string ResponseBody { get; set; } = string.Empty;
        public string Timing { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}