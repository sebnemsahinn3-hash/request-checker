using PTN.WebAPI.Dtos;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PTN.WebAPI.Services
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        Task<UserDto?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<UserDto> CreateUserAsync(UserCreateDto dto, CancellationToken cancellationToken = default);
        Task<bool> UpdateUserAsync(int id, UserUpdateDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default);
    }
}