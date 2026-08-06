using FluentValidation;
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
        /// İstek loglarını DTO parametrelerine göre siler (FluentValidation kontrollü).
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteLogsAsync([FromBody] RequestLogDeleteDto deleteDto)
        {
            // FluentValidation Senkron Kontrolü
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