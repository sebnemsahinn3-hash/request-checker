using Microsoft.AspNetCore.Mvc;
using PTN.WebAPI.Constants;
using PTN.WebAPI.Dtos;
using PTN.WebAPI.Services;
using System.Collections.Generic;
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
        /// API endpointlerinin sağlık durumlarını döndürür.
        /// </summary>
        [HttpGet("health")]
        public async Task<List<EndpointHealthDto>> GetEndpointsHealth(
            [FromQuery] string range =
                RequestConstants.TimeRange.OneHour,
            CancellationToken cancellationToken = default)
        {
            return await _requestService.GetEndpointsHealthAsync(
                range,
                cancellationToken);
        }
    }
}