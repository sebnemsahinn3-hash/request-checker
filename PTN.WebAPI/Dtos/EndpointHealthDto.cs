namespace PTN.WebAPI.Dtos
{
    public class EndpointHealthDto
    {
        public string Endpoint { get; set; } = string.Empty;
        public int TotalRequests { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public double AvgLatencyMs { get; set; }
        public double AvailabilityPercent { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}