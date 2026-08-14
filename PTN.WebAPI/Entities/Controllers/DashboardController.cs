using Microsoft.AspNetCore.Mvc;
using PTN.WebAPI.Services;
using System.Threading;
using System.Threading.Tasks;
using PTN.WebAPI.Constants;
using PTN.WebAPI.Dtos;
using System.Collections.Generic;

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
        public async Task<DashboardSummaryDto> GetSummary(
            [FromQuery] string range =
                RequestConstants.TimeRange.OneHour,
            CancellationToken cancellationToken = default)
        {
            return await _requestService.GetDashboardSummaryAsync(
                range,
                cancellationToken);
        }

        /// <summary>
        /// GET /api/dashboard/timeseries?range=1h&amp;bucket=5m
        /// Returns timeseries data for traffic bar chart, error rate line, and P95 latency line
        /// </summary>
        [HttpGet("timeseries")]
        public async Task<List<DashboardTimeSeriesDto>> GetTimeseries(
            [FromQuery] string range =
                RequestConstants.TimeRange.OneHour,
            [FromQuery] string bucket =
                RequestConstants.Dashboard.DefaultBucket,
            CancellationToken cancellationToken = default)
        {
            return await _requestService.GetDashboardTimeSeriesAsync(
                range,
                bucket,
                cancellationToken);
        }
    }
}
