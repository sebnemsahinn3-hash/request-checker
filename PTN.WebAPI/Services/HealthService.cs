using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using PTN.WebAPI.Constants;
using PTN.WebAPI.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Text;

namespace PTN.WebAPI.Services
{
    public class HealthService : IHealthService
    {
        private readonly AppDbContext _dbContext;

        private readonly IStringLocalizer<HealthService>
            _localizer;

        public HealthService(
            AppDbContext dbContext,
            IStringLocalizer<HealthService> localizer)
        {
            _dbContext = dbContext;
            _localizer = localizer;
        }

        public HealthStatusDto GetStatus()
        {
            var metrics =
                HealthCheckBackgroundService.Metrics;

            lock (metrics.RecentLogs)
            {
                return new HealthStatusDto
                {
                    TotalRequests = metrics.TotalRequests,
                    Success200Count = metrics.Success200Count,
                    HealthyCount = metrics.HealthyCount,
                    UnhealthyCount = metrics.UnhealthyCount,
                    HealthPercentage =
                        metrics.HealthPercentage,
                    LastUpdate = metrics.LastUpdate,
                    RecentLogs =
                        metrics.RecentLogs.ToList()
                };
            }
        }
        public async Task<LogFileDto> GetLogFileAsync(
            CancellationToken cancellationToken = default)
        {
            var recentLogs = GetLogs();

            var logContent = recentLogs.Count > 0
                ? string.Join(
                      Environment.NewLine,
                      recentLogs) +
                  Environment.NewLine
                : string.Empty;

            if (string.IsNullOrWhiteSpace(logContent))
            {
                var filePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    RequestConstants.LogFilePath);

                if (File.Exists(filePath))
                {
                    logContent = await File.ReadAllTextAsync(
                        filePath,
                        cancellationToken);
                }
            }

            return new LogFileDto
            {
                Content = Encoding.UTF8.GetBytes(logContent),
                ContentType = HealthConstants.TextContentType,
                FileName = RequestConstants.LogFilePath
            };
        }

        public List<string> GetLogs()
        {
            var recentLogs =
                HealthCheckBackgroundService
                    .Metrics
                    .RecentLogs;

            lock (recentLogs)
            {
                return recentLogs.ToList();
            }
        }

        public async Task<DatabaseHealthDto>
            CheckDatabaseAsync(
                CancellationToken cancellationToken = default)
        {
            var isConnected =
                await _dbContext.Database.CanConnectAsync(
                    cancellationToken);

            var totalLogCount = isConnected
                ? await _dbContext.RequestLogs.CountAsync(
                    cancellationToken)
                : 0;

            var message = isConnected
                ? _localizer[
                    HealthConstants.SuccessfullyConnected]
                : _localizer[
                    HealthConstants.NoConnection];

            return new DatabaseHealthDto
            {
                IsConnected = isConnected,
                TotalLogCount = totalLogCount,
                Message = message.Value
            };
        }
    }
}