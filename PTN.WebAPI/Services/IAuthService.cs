using PTN.WebAPI.Dtos;
using System.Threading.Tasks;

namespace PTN.WebAPI.Services
{
    public interface IAuthService
    {
        Task<TokenResponseDto?> LoginAsync(LoginDto dto);
    }
}