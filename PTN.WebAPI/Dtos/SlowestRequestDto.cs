using System;

namespace PTN.WebAPI.Dtos
{
    public class SlowestRequestDto
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Timing { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}