using System;

namespace PTN.WebAPI.Events
{
    public class CriticalHealthAlertEvent
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime AlertTime { get; set; } = DateTime.UtcNow;
    }
}