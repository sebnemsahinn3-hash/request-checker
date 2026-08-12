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
    public class EndpointsController : ControllerBase
    {
        private readonly IRequestService _requestService;

        public EndpointsController(IRequestService requestService)
        {
            _requestService = requestService;
        }

        /// <summary>
        /// GET /api/endpoints/health?range=1h
        /// Returns health status per API endpoint route
        /// </summary>
        [HttpGet("health")]
        public async Task<IActionResult> GetEndpointsHealth([FromQuery] string range = "1h", CancellationToken cancellationToken = default)
        {
            var logs = await _requestService.GetAllLogsAsync(cancellationToken);

            var endpointHealth = logs
                .GroupBy(l => l.Url ?? "GET /api/check")
                .Select(g => new
                {
                    endpoint = g.Key,
                    totalRequests = g.Count(),
                    successCount = g.Count(l => l.StatusCode == 200 || l.StatusCode == 1),
                    errorCount = g.Count(l => l.StatusCode >= 400 || l.StatusCode == 2 || l.StatusCode == 4 || l.StatusCode == 5),
                    avgLatencyMs = Math.Round(g.Average(l => int.TryParse(l.Timing?.Replace("ms", "").Trim(), out var t) ? t : 0), 1),
                    availabilityPercent = Math.Round((double)g.Count(l => l.StatusCode < 400) / g.Count() * 100, 1),
                    status = g.Any(l => l.StatusCode >= 500) ? "Degraded" : "Healthy"
                })
                .ToList();

            return Ok(endpointHealth);
        }
    }
}
