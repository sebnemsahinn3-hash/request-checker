using PTN.WebAPI.Dtos;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PTN.WebAPI.Services
{
    public interface IHealthService
    {
        HealthStatusDto GetStatus();

        List<string> GetLogs();

        Task<LogFileDto> GetLogFileAsync(
            CancellationToken cancellationToken = default);

        Task<DatabaseHealthDto> CheckDatabaseAsync(
            CancellationToken cancellationToken = default);
    }
}