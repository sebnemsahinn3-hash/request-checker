namespace PTN.WebAPI.Dtos
{
    public class RequestLogCreateDto
    {
        public string Url { get; set; } = string.Empty;
        public string RequestParams { get; set; } = string.Empty;
        public string RequestBody { get; set; } = string.Empty;
        public string ResponseBody { get; set; } = string.Empty;
        public string Timing { get; set; } = string.Empty;
        public int StatusCode { get; set; } = 200;
    }
}
