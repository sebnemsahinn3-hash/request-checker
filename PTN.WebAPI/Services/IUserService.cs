using PTN.WebAPI.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PTN.WebAPI.Services
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto> CreateUserAsync(UserCreateDto dto);
        Task<bool> UpdateUserAsync(int id, UserUpdateDto dto);
        Task<bool> DeleteUserAsync(int id);
    }
}