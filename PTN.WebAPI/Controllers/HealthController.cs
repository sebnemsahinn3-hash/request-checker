using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PTN.WebAPI.Constants;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace PTN.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IStringLocalizer<HealthController> _localizer;

        // Constructor'da Localizer enjekte ediyoruz
        public HealthController(IStringLocalizer<HealthController> localizer)
        {
            _localizer = localizer;
        }
        /// <summary>
        /// Canlı sistem analiz istatistiklerini ve özet metrikleri döner.
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            var metrics = HealthCheckBackgroundService.Metrics;
            return Ok(metrics);
        }
        /// <summary>
        /// Canlı sistem analiz istatistiklerini ve özet metrikleri döner.
        /// </summary>
        [HttpGet("logs")]
        public IActionResult GetLogs()
        {
            var logs = HealthCheckBackgroundService.Metrics.RecentLogs;
            return Ok(logs);
        }
        /// <summary>
        /// Analiz loglarını .txt metin dosyası olarak bilgisayara indirir.
        /// </summary>
        [HttpGet("download-log")]
        [HttpGet("DownloadLogFileAsync")]
        public IActionResult DownloadLogFileAsync(CancellationToken cancellationToken = default)
        {
            string logContent = "";

            lock (HealthCheckBackgroundService.Metrics.RecentLogs)
            {
                if (HealthCheckBackgroundService.Metrics.RecentLogs.Count > 0)
                {
                    logContent = string.Join(Environment.NewLine, HealthCheckBackgroundService.Metrics.RecentLogs) + Environment.NewLine;
                }
            }

            if (string.IsNullOrEmpty(logContent))
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), RequestConstants.LogFilePath);
                if (System.IO.File.Exists(filePath))
                {
                    logContent = System.IO.File.ReadAllText(filePath);
                }
            }

            byte[] fileBytes = Encoding.UTF8.GetBytes(logContent);
            return File(fileBytes, "text/plain", RequestConstants.LogFilePath);
        }
        /// <summary>
        /// PostgreSQL veritabanı bağlantı durumunu ve toplam kayıt sayısını test eder.
        /// </summary>
        [HttpGet("db-check")]
        public IActionResult CheckDatabase([FromServices] AppDbContext dbContext)
        {
            bool isConnected = dbContext.Database.CanConnect();
            int totalLogCount = dbContext.RequestLogs.Count();

            // Koda sabit metin yazmıyoruz! Dil dosyasındaki key'den çekiyoruz:
            string message = isConnected
                ? _localizer["SuccessfullyConnected"]
                : _localizer["NoConnection"];

            return Ok(new
            {
                IsConnected = isConnected,
                TotalLogCount = totalLogCount,
                Message = message
            });
        }
    }
}