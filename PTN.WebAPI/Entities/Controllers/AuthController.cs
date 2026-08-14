using Microsoft.AspNetCore.Mvc;
using PTN.WebAPI.Dtos;
using PTN.WebAPI.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using PTN.WebAPI.Constants;

namespace PTN.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IStringLocalizer<AuthController> _localizer;

        public AuthController(
            IAuthService authService,
            IStringLocalizer<AuthController> localizer)
        {
            _authService = authService;
            _localizer = localizer;
        }

        [HttpPost("login")]
        public async Task<TokenResponseDto> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (result == null)
            {
                throw new UnauthorizedAccessException(
                    _localizer[AuthConstants.InvalidCredentials].Value);            }
            return result;
        }
    }
}