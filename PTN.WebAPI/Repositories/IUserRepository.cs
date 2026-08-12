using PTN.WebAPI.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PTN.WebAPI.Repositories
{
    public interface IUserRepository
    {
        Task<List<UserEntity>> GetAllUsersAsync();
        Task<UserEntity?> GetUserByIdAsync(int id);
        Task<UserEntity?> GetUserByEmailAsync(string email);
        Task AddUserAsync(UserEntity entity);
        Task UpdateUserAsync(UserEntity entity);
        Task DeleteUserAsync(UserEntity entity);
    }
}