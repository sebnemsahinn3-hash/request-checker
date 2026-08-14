using PTN.WebAPI.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PTN.WebAPI.Repositories
{
    public interface IUserRepository
    {
        Task<List<UserEntity>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        Task<UserEntity?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<UserEntity?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task AddUserAsync(UserEntity entity, CancellationToken cancellationToken = default);
        Task UpdateUserAsync(UserEntity entity, CancellationToken cancellationToken = default);
        Task DeleteUserAsync(UserEntity entity, CancellationToken cancellationToken = default);
    }
}