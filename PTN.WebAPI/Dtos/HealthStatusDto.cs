using System;
using System.Collections.Generic;

namespace PTN.WebAPI.Dtos
{
    public class HealthStatusDto
    {
        public int TotalRequests { get; set; }
        public int Success200Count { get; set; }
        public int HealthyCount { get; set; }
        public int UnhealthyCount { get; set; }
        public double HealthPercentage { get; set; }
        public DateTime LastUpdate { get; set; }

        public List<string> RecentLogs { get; set; }
            = new();
    }
}