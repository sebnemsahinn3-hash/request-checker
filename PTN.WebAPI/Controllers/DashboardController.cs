using Microsoft.AspNetCore.Mvc;
using PTN.WebAPI.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PTN.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IRequestService _requestService;

        public DashboardController(IRequestService requestService)
        {
            _requestService = requestService;
        }

        /// <summary>
        /// GET /api/dashboard/summary?range=1h
        /// Returns aggregated APM metrics (Throughput, Error Rate, SLA %, Percentiles P50/P95/P99, Slowest Request, Problematic Endpoint)
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<object>> GetSummary([FromQuery] string range = "1h", CancellationToken cancellationToken = default)
        {
            var logs = await _requestService.GetAllLogsAsync(cancellationToken);
            var now = DateTime.UtcNow;

            var filteredLogs = range.ToLower() switch
            {
                "1h" => logs.Where(l => (now - l.CreatedAt).TotalHours <= 1).ToList(),
                "24h" => logs.Where(l => (now - l.CreatedAt).TotalHours <= 24).ToList(),
                _ => logs
            };

            var totalCount = filteredLogs.Count;
            var unhealthyCount = filteredLogs.Count(l => l.StatusCode >= 400 || l.StatusCode == 2 || l.StatusCode == 4 || l.StatusCode == 5);
            var errorRate = totalCount > 0 ? Math.Round((double)unhealthyCount / totalCount * 100, 1) : 0.0;
            var slaPercent = totalCount > 0 ? Math.Round(100 - errorRate, 1) : 100.0;

            var timings = filteredLogs
                .Select(l => int.TryParse(l.Timing?.Replace("ms", "").Trim(), out var t) ? t : 0)
                .Where(t => t > 0)
                .OrderBy(t => t)
                .ToList();

            var p50 = timings.Count > 0 ? timings[(int)(timings.Count * 0.50)] : 0;
            var p95 = timings.Count > 0 ? timings[Math.Min((int)(timings.Count * 0.95), timings.Count - 1)] : 0;
            var p99 = timings.Count > 0 ? timings[Math.Min((int)(timings.Count * 0.99), timings.Count - 1)] : 0;

            var slowest = filteredLogs.OrderByDescending(l => int.TryParse(l.Timing?.Replace("ms", "").Trim(), out var t) ? t : 0).FirstOrDefault();

            return Ok(new
            {
                range,
                totalRequests = totalCount,
                throughputRps = Math.Round((double)totalCount / 300, 2),
                unhealthyCount,
                errorRatePercent = errorRate,
                slaPercent,
                percentiles = new { p50, p95, p99 },
                slowestRequest = slowest != null ? new
                {
                    slowest.Id,
                    slowest.Url,
                    slowest.Timing,
                    slowest.CreatedAt
                } : null
            });
        }

        /// <summary>
        /// GET /api/dashboard/timeseries?range=1h&bucket=5m
        /// Returns timeseries data for traffic bar chart, error rate line, and P95 latency line
        /// </summary>
        [HttpGet("timeseries")]
        public async Task<ActionResult<object>> GetTimeseries([FromQuery] string range = "1h", [FromQuery] string bucket = "5m", CancellationToken cancellationToken = default)
        {
            var logs = await _requestService.GetAllLogsAsync(cancellationToken);
            var now = DateTime.UtcNow;

            var timeSeries = logs
                .GroupBy(l => l.CreatedAt.ToString("yyyy-MM-dd HH:mm"))
                .OrderBy(g => g.Key)
                .Take(12)
                .Select(g => new
                {
                    timestamp = g.Key,
                    count = g.Count(),
                    errorRate = Math.Round((double)g.Count(l => l.StatusCode >= 400) / g.Count() * 100, 1),
                    p95Latency = g.Select(l => int.TryParse(l.Timing?.Replace("ms", "").Trim(), out var t) ? t : 0).DefaultIfEmpty(0).Max()
                })
                .ToList();

            return Ok(timeSeries);
        }
    }
}
