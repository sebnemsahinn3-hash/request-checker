using Microsoft.AspNetCore.Mvc;
using System.Threading;
using PTN.WebAPI.Dtos;
using PTN.WebAPI.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PTN.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IHealthService _healthService;

        public HealthController(IHealthService healthService)
        {
            _healthService = healthService;
        }
        /// <summary>
        /// Canlı sistem analiz istatistiklerini ve özet metrikleri döner.
        /// </summary>
        [HttpGet("status")]
        public HealthStatusDto GetStatus()
        {
            return _healthService.GetStatus();
        }
        /// <summary>
        /// Canlı sistem analiz istatistiklerini ve özet metrikleri döner.
        /// </summary>
        [HttpGet("logs")]
        public List<string> GetLogs()
        {
            return _healthService.GetLogs();
        }
        /// <summary>
        /// Analiz loglarını .txt metin dosyası olarak bilgisayara indirir.
        /// </summary>
        [HttpGet("download-log")]
        [HttpGet("DownloadLogFileAsync")]
        public FileContentResult DownloadLogFile()
        {
            var logFile = _healthService.GetLogFile();

            return File(
                logFile.Content,
                logFile.ContentType,
                logFile.FileName);
        }
        /// <summary>
        /// PostgreSQL veritabanı bağlantı durumunu ve toplam kayıt sayısını test eder.
        /// </summary>
        [HttpGet("db-check")]
        public async Task<DatabaseHealthDto> CheckDatabase(
            CancellationToken cancellationToken = default)
        {
            return await _healthService.CheckDatabaseAsync(
                cancellationToken);
        }
    }
}