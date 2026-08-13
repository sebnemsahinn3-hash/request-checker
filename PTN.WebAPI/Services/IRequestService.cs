using PTN.WebAPI.Dtos;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PTN.WebAPI.Services
{
    public interface IRequestService
    {
        Task<PagedResultDto<RequestLogDto>> GetPagedLogsAsync(
            RequestLogQueryDto query,
            CancellationToken cancellationToken = default);
        Task<List<RequestLogDto>> GetAllLogsAsync(CancellationToken cancellationToken = default);
        Task<RequestLogDto> CreateLogAsync(RequestLogCreateDto dto);
        Task<bool> UpdateLogAsync(int id, RequestLogUpdateDto dto);
        Task<bool> DeleteLogsAsync(RequestLogDeleteDto dto);
        Task<List<EndpointHealthDto>> GetEndpointsHealthAsync(
            string range,
            CancellationToken cancellationToken = default);
        Task<DashboardSummaryDto> GetDashboardSummaryAsync(
            string range,
            CancellationToken cancellationToken = default);
        Task<List<DashboardTimeSeriesDto>>
            GetDashboardTimeSeriesAsync(
                string range,
                string bucket,
                CancellationToken cancellationToken = default);
        
    }
}