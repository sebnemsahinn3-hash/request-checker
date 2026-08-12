using Microsoft.AspNetCore.Mvc;
using PTN.WebAPI.Dtos;
using PTN.WebAPI.Services;
using System.Threading.Tasks;

namespace PTN.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (result == null)
            {
                return Unauthorized(new { message = "Geçersiz e-posta veya şifre!" });
            }
            return Ok(result);
        }
    }
}