using System;
using System.Collections.Generic;

namespace PTN.WebAPI
{
    public class HealthMetrics
    {
        public int TotalRequests { get; set; } = 0;
        public int Success200Count { get; set; } = 0;
        public int HealthyCount { get; set; } = 0;
        public int UnhealthyCount { get; set; } = 0;
        public double HealthPercentage => TotalRequests > 0 ? Math.Round(((double)HealthyCount / TotalRequests) * 100, 2) : 0;
        public DateTime LastUpdate { get; set; } = DateTime.Now;

        public List<string> RecentLogs { get; set; } = new List<string>();
    }
}