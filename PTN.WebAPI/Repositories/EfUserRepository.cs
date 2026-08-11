using Microsoft.EntityFrameworkCore;
using PTN.WebAPI.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PTN.WebAPI.Repositories
{
    public class EfUserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public EfUserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserEntity>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<UserEntity?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<UserEntity?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddUserAsync(UserEntity entity)
        {
            await _context.Users.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(UserEntity entity)
        {
            _context.Users.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(UserEntity entity)
        {
            _context.Users.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}