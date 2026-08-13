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
        LogFileDto GetLogFile();
            CancellationToken cancellationToken = default);
    }
}