namespace PTN.WebAPI.Dtos
{
    public class DashboardTimeSeriesDto
    {
        public string Timestamp { get; set; } = string.Empty;
        public int Count { get; set; }
        public double ErrorRate { get; set; }
        public int P95Latency { get; set; }
    }
}