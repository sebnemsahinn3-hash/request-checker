using Microsoft.AspNetCore.Mvc;
using PTN.WebAPI.Dtos;
using PTN.WebAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PTN.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestsController : ControllerBase
    {
        private readonly IRequestService _requestService;

        public RequestsController(IRequestService requestService)
        {
            _requestService = requestService;
        }

        /// <summary>
        /// GET /api/requests?status=500&query=&page=1&pageSize=50
        /// Paginated, filtered request logs endpoint
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<object>> GetRequests(
            [FromQuery] string status = null,
            [FromQuery] string query = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            var logs = await _requestService.GetAllLogsAsync(cancellationToken);

            var filtered = logs.AsQueryable();

            if (!string.IsNullOrEmpty(status) && status != "ALL")
            {
                filtered = filtered.Where(l => l.StatusCode.ToString() == status || (status == "500" && l.StatusCode >= 500) || (status == "400" && l.StatusCode >= 400 && l.StatusCode < 500));
            }

            if (!string.IsNullOrEmpty(query))
            {
                var q = query.ToLower();
                filtered = filtered.Where(l =>
                    (l.Url != null && l.Url.ToLower().Contains(q)) ||
                    (l.Message != null && l.Message.ToLower().Contains(q)) ||
                    l.StatusCode.ToString().Contains(q));
            }

            var totalCount = filtered.Count();
            var items = filtered
                .OrderByDescending(l => l.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                items
            });
        }
    }
}
