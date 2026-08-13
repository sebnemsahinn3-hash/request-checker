namespace PTN.WebAPI.Dtos
{
    public class DashboardSummaryDto
    {
        public string Range { get; set; } = string.Empty;
        public int TotalRequests { get; set; }
        public double ThroughputRps { get; set; }
        public int UnhealthyCount { get; set; }
        public double ErrorRatePercent { get; set; }
        public double SlaPercent { get; set; }

        public DashboardPercentilesDto Percentiles { get; set; }
            = new();

        public SlowestRequestDto? SlowestRequest { get; set; }
    }
}