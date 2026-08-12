using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTN.WebAPI.Dtos;
using PTN.WebAPI.Services;
using PTN.WebAPI.Validators;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<ActionResult<List<RequestLogDto>>> GetAllLogsAsync(CancellationToken cancellationToken)
        {
            var logs = await _requestService.GetAllLogsAsync(cancellationToken);
            return Ok(logs);
        }

        /// <summary>
        /// Veritabanına yeni bir istek log kaydı ekler (POST).
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<RequestLogDto>> CreateLogAsync([FromBody] RequestLogCreateDto createDto)
        {
            var result = await _requestService.CreateLogAsync(createDto);
            return CreatedAtAction(nameof(GetAllLogsAsync), new { id = result.Id }, result);
        }

        /// <summary>
        /// Belirtilen ID'ye sahip istek logunu günceller (PUT). Sadece Admin yetkisi olanlar değiştirebilir.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateLogAsync(int id, [FromBody] RequestLogUpdateDto updateDto)
        {
            var success = await _requestService.UpdateLogAsync(id, updateDto);
            if (!success)
            {
                return NotFound(new { message = "Güncellenecek kayıt bulunamadı." });
            }
            return Ok(new { message = "İstek logu başarıyla güncellendi." });
        }

        /// <summary>
        /// İstek loglarını DTO parametrelerine göre siler. Sadece Admin yetkisi olanlar silebilir.
        /// </summary>
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteLogsAsync([FromBody] RequestLogDeleteDto deleteDto)
        {
            var validator = new RequestLogDeleteDtoValidator();
            var validationResult = validator.Validate(deleteDto);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    message = "Validasyon Hatası!",
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            await _requestService.DeleteLogsAsync(deleteDto);
            return Ok(new { message = "Silme işlemi başarıyla gerçekleştirildi." });
        }
    }
}