using PTN.WebAPI.Dtos;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PTN.WebAPI.Services
{
    public interface IRequestService
    {
        Task<List<RequestLogDto>> GetAllLogsAsync(CancellationToken cancellationToken = default);
        Task DeleteLogsAsync(RequestLogDeleteDto dto); // Mentörün istediği DTO alan servis
    }
}