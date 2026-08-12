
using System;

namespace PTN.WebAPI.Dtos
{
    public class RequestCacheModel
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Timing { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
