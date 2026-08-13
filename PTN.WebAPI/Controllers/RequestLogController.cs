
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTN.WebAPI.Dtos;
using PTN.WebAPI.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PTN.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestLogController : ControllerBase
    {
        private readonly IRequestService _requestService;

        public RequestLogController(IRequestService requestService)
        {
            _requestService = requestService;
        }

        /// <summary>
        /// Veritabanındaki tüm istek loglarını DTO formatında listelemek için kullanılan API.
        /// </summary>
        [HttpGet]
        public async Task<List<RequestLogDto>> GetAllLogsAsync(
            CancellationToken cancellationToken)
        {
            return await _requestService.GetAllLogsAsync(cancellationToken);
        }
        /// <summary>
        /// Veritabanına yeni bir istek log kaydı ekler (POST).
        /// </summary>
        [HttpPost]
        public async Task<RequestLogDto> CreateLogAsync(
            [FromBody] RequestLogCreateDto createDto)
        {
            return await _requestService.CreateLogAsync(createDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<bool> UpdateLogAsync(
            int id,
            [FromBody] RequestLogUpdateDto updateDto)
        {
            return await _requestService.UpdateLogAsync(id, updateDto);
        }

        /// <summary>
        /// İstek loglarını DTO parametrelerine göre siler. Sadece Admin yetkisi olanlar silebilir.
        /// </summary>
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<bool> DeleteLogsAsync(
            [FromBody] RequestLogDeleteDto deleteDto)
        {
            return await _requestService.DeleteLogsAsync(deleteDto);
        }
    }
}