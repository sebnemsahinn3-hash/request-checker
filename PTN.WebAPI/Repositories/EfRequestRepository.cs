using Microsoft.EntityFrameworkCore;
using PTN.WebAPI.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PTN.WebAPI.Repositories
{
    public class EfRequestRepository : IRequestRepository
    {
        private readonly AppDbContext _context;

        public EfRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddLogAsync(RequestLogEntity entity)
        {
            await _context.RequestLogs.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<RequestLogEntity>> GetAllLogsAsync()
        {
            return await _context.RequestLogs.ToListAsync();
        }

        // Mentörün istediği Akıllı Silme Mantığı
        public async Task DeleteLogsAsync(bool isAllDelete, int? count)
        {
            if (isAllDelete)
            {
                // isAllDelete true ise TÜM kayıtları siler
                _context.RequestLogs.RemoveRange(_context.RequestLogs);
            }
            else if (count.HasValue && count.Value > 0)
            {
                // count girilmişse İLK OLUŞAN (tarihe göre sıralı) o kadar kaydı siler
                var logsToDelete = await _context.RequestLogs
                    .OrderBy(x => x.CreatedAt)
                    .Take(count.Value)
                    .ToListAsync();

                _context.RequestLogs.RemoveRange(logsToDelete);
            }

            await _context.SaveChangesAsync();
        }
    }
}