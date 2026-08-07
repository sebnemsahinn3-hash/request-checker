using PTN.WebAPI.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PTN.WebAPI.Repositories
{
    public interface IRequestRepository
    {
        Task AddLogAsync(RequestLogEntity entity);
        Task<RequestLogEntity?> GetLogByIdAsync(int id);
        Task UpdateLogAsync(RequestLogEntity entity);
        Task<List<RequestLogEntity>> GetAllLogsAsync();
        Task DeleteLogsAsync(bool isAllDelete, int? count);
    }
}