using Microsoft.AspNetCore.Mvc;
using PTN.WebAPI.Dtos;
using PTN.WebAPI.Services;
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
        /// Filtrelenmiş, sıralanmış ve sayfalanmış istek loglarını getirir.
        /// </summary>
        [HttpGet]
        public async Task<PagedResultDto<RequestLogDto>> GetRequests(
            [FromQuery] RequestLogQueryDto queryParameters,
            CancellationToken cancellationToken = default)
        {
            return await _requestService.GetPagedLogsAsync(
                queryParameters,
                cancellationToken);
        }
    }
}