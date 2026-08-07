using PTN.WebAPI.Dtos;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PTN.WebAPI.Services
{
    public interface IRequestService
    {
        Task<List<RequestLogDto>> GetAllLogsAsync(CancellationToken cancellationToken = default);
        Task<RequestLogDto> CreateLogAsync(RequestLogCreateDto dto);
        Task<bool> UpdateLogAsync(int id, RequestLogUpdateDto dto);
        Task DeleteLogsAsync(RequestLogDeleteDto dto);
    }
}