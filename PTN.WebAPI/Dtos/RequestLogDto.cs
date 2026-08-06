using System;

namespace PTN.WebAPI.Dtos
{
    public class RequestLogDto
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string RequestParams { get; set; } = string.Empty;
        public string RequestBody { get; set; } = string.Empty;
        public string ResponseBody { get; set; } = string.Empty;
        public string Timing { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}