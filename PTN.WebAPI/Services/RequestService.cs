using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using PTN.WebAPI.Constants;
using PTN.WebAPI.Dtos;
using PTN.WebAPI.Entities;
using PTN.WebAPI.Extensions;
using PTN.WebAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using FluentValidation;
namespace PTN.WebAPI.Services
{
    public class RequestService : IRequestService
    {
        private readonly IRequestRepository _repository;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<RequestService> _localizer;
        private readonly IDistributedCache _cache;
        private readonly IValidator<RequestLogDeleteDto> _deleteValidator;

        public RequestService(
            IRequestRepository repository, 
            IMapper mapper, 
            IStringLocalizer<RequestService> localizer,
            IDistributedCache cache,
            IValidator<RequestLogDeleteDto> deleteValidator)
        {
            _repository = repository;
            _mapper = mapper;
            _localizer = localizer;
            _cache = cache;
            _deleteValidator = deleteValidator;
        }

   
        public async Task<PagedResultDto<RequestLogDto>> GetPagedLogsAsync(
            RequestLogQueryDto query,
            CancellationToken cancellationToken = default)
        {
            var allLogs = await GetAllLogsAsync(cancellationToken);

            var filteredLogs = allLogs
                .AsQueryable()
                .ApplyDynamicFilter(query.Query);

            filteredLogs = ApplyStatusFilter(
                filteredLogs,
                query.Status);

            var page = Math.Max(
                query.Page,
                RequestConstants.Query.FirstPage);

            var pageSize = query.PageSize <= 0
                ? RequestConstants.Query.DefaultPageSize
                : Math.Min(
                    query.PageSize,
                    RequestConstants.Query.MaxPageSize);

            var totalCount = filteredLogs.Count();

            var items = filteredLogs
                .ApplyDynamicSorting(
                    query.SortBy,
                    query.SortDirection)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalPages = (int)Math.Ceiling(
                totalCount / (double)pageSize);

            return new PagedResultDto<RequestLogDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = items
            };
        }
        private static IQueryable<RequestLogDto> ApplyStatusFilter(
            IQueryable<RequestLogDto> logs,
            string? status)
        {
            if (string.IsNullOrWhiteSpace(status) ||
                string.Equals(
                    status,
                    RequestConstants.Query.AllStatus,
                    StringComparison.OrdinalIgnoreCase))
            {
                return logs;
            }

            var normalizedStatus = status.Trim();

            return normalizedStatus switch
            {
                RequestConstants.Query.SuccessStatus =>
                    logs.Where(log =>
                        (log.StatusCode >= 200 &&
                         log.StatusCode < 300) ||
                        log.StatusCode == 1),

                RequestConstants.Query.ClientErrorStatus =>
                    logs.Where(log =>
                        (log.StatusCode >= 400 &&
                         log.StatusCode < 500) ||
                        log.StatusCode == 2 ||
                        log.StatusCode == 3 ||
                        log.StatusCode == 4),

                RequestConstants.Query.ServerErrorStatus =>
                    logs.Where(log =>
                        (log.StatusCode >= 500 &&
                         log.StatusCode < 600) ||
                        log.StatusCode == 5),

                _ => logs.ApplyDynamicFilter(
                    normalizedStatus,
                    nameof(RequestLogDto.StatusCode))
            };
        }
public async Task<List<EndpointHealthDto>> GetEndpointsHealthAsync(
    string range,
    CancellationToken cancellationToken = default)
{
    var logs = await GetAllLogsAsync(cancellationToken);
    var now = DateTime.UtcNow;

    var filteredLogs = range.ToLowerInvariant() switch
    {
        RequestConstants.TimeRange.OneHour =>
            logs.Where(log =>
                (now - log.CreatedAt).TotalHours <= 1),

        RequestConstants.TimeRange.TwentyFourHours =>
            logs.Where(log =>
                (now - log.CreatedAt).TotalHours <= 24),

        _ => logs
    };

    return filteredLogs
        .GroupBy(log =>
            string.IsNullOrWhiteSpace(log.Url)
                ? RequestConstants.EndpointHealth.DefaultEndpoint
                : log.Url)
        .Select(group =>
        {
            var totalRequests = group.Count();

            var successCount = group.Count(log =>
                (log.StatusCode >= 200 && log.StatusCode < 300) ||
                log.StatusCode == 1);

            var errorCount = group.Count(log =>
                log.StatusCode >= 400 ||
                log.StatusCode is 2 or 3 or 4 or 5);

            var hasError = errorCount > 0;
            return new EndpointHealthDto
            {
                Endpoint = group.Key,
                TotalRequests = totalRequests,
                SuccessCount = successCount,
                ErrorCount = errorCount,

                AvgLatencyMs = Math.Round(
                    group.Average(log => ParseTiming(log.Timing)),
                    1),

                AvailabilityPercent = totalRequests > 0
                    ? Math.Round(
                        (double)successCount / totalRequests * 100,
                        1)
                    : 0,

                Status = hasError
                    ? RequestConstants.EndpointHealth.Degraded
                    : RequestConstants.EndpointHealth.Healthy
            };
        })
        .ToList();
}public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(
    string range,
    CancellationToken cancellationToken = default)
{
    var logs = await GetAllLogsAsync(cancellationToken);
    var now = DateTime.UtcNow;

    var normalizedRange = string.Equals(
        range,
        RequestConstants.TimeRange.TwentyFourHours,
        StringComparison.OrdinalIgnoreCase)
            ? RequestConstants.TimeRange.TwentyFourHours
            : RequestConstants.TimeRange.OneHour;

    var filteredLogs = normalizedRange switch
    {
        RequestConstants.TimeRange.TwentyFourHours =>
            logs.Where(log =>
                (now - log.CreatedAt).TotalHours <= 24)
                .ToList(),

        _ =>
            logs.Where(log =>
                (now - log.CreatedAt).TotalHours <= 1)
                .ToList()
    };

    var totalCount = filteredLogs.Count;

    var unhealthyCount = filteredLogs.Count(log =>
        IsErrorStatus(log.StatusCode));

    var errorRate = totalCount > 0
        ? Math.Round(
            (double)unhealthyCount / totalCount * 100,
            1)
        : 0;

    var slaPercent = totalCount > 0
        ? Math.Round(100 - errorRate, 1)
        : 100;

    var timings = filteredLogs
        .Select(log => ParseTiming(log.Timing))
        .Where(timing => timing > 0)
        .OrderBy(timing => timing)
        .ToList();

    var p50 = GetPercentile(timings, 0.50);
    var p95 = GetPercentile(timings, 0.95);
    var p99 = GetPercentile(timings, 0.99);

    var slowestLog = filteredLogs
        .OrderByDescending(log => ParseTiming(log.Timing))
        .FirstOrDefault();

    var durationSeconds =
        normalizedRange ==
        RequestConstants.TimeRange.TwentyFourHours
            ? 24 * 60 * 60
            : 60 * 60;

    return new DashboardSummaryDto
    {
        Range = normalizedRange,
        TotalRequests = totalCount,

        ThroughputRps = Math.Round(
            (double)totalCount / durationSeconds,
            2),

        UnhealthyCount = unhealthyCount,
        ErrorRatePercent = errorRate,
        SlaPercent = slaPercent,

        Percentiles = new DashboardPercentilesDto
        {
            P50 = p50,
            P95 = p95,
            P99 = p99
        },

        SlowestRequest = slowestLog == null
            ? null
            : new SlowestRequestDto
            {
                Id = slowestLog.Id,
                Url = slowestLog.Url,
                Timing = slowestLog.Timing,
                CreatedAt = slowestLog.CreatedAt
            }
    };
}public async Task<List<DashboardTimeSeriesDto>>
    GetDashboardTimeSeriesAsync(
        string range,
        string bucket,
        CancellationToken cancellationToken = default)
{
    var logs = await GetAllLogsAsync(cancellationToken);
    var now = DateTime.UtcNow;

    var normalizedRange = string.Equals(
        range,
        RequestConstants.TimeRange.TwentyFourHours,
        StringComparison.OrdinalIgnoreCase)
            ? RequestConstants.TimeRange.TwentyFourHours
            : RequestConstants.TimeRange.OneHour;

    var filteredLogs = normalizedRange switch
    {
        RequestConstants.TimeRange.TwentyFourHours =>
            logs.Where(log =>
                (now - log.CreatedAt).TotalHours <= 24),

        _ =>
            logs.Where(log =>
                (now - log.CreatedAt).TotalHours <= 1)
    };

    var bucketMinutes = ParseBucketMinutes(bucket);

    var groups = filteredLogs
        .GroupBy(log =>
            RoundDownToBucket(
                log.CreatedAt,
                bucketMinutes))
        .OrderByDescending(group => group.Key)
        .Take(RequestConstants.Dashboard.MaxTimeSeriesPoints)
        .OrderBy(group => group.Key);

    return groups
        .Select(group =>
        {
            var groupLogs = group.ToList();

            var timings = groupLogs
                .Select(log => ParseTiming(log.Timing))
                .Where(timing => timing > 0)
                .OrderBy(timing => timing)
                .ToList();

            var errorCount = groupLogs.Count(log =>
                IsErrorStatus(log.StatusCode));

            return new DashboardTimeSeriesDto
            {
                Timestamp = group.Key.ToString(
                    RequestConstants.Dashboard.TimestampFormat),

                Count = groupLogs.Count,

                ErrorRate = groupLogs.Count > 0
                    ? Math.Round(
                        (double)errorCount /
                        groupLogs.Count * 100,
                        1)
                    : 0,

                P95Latency = GetPercentile(
                    timings,
                    0.95)
            };
        })
        .ToList();
}
        public async Task<List<RequestLogDto>> GetAllLogsAsync(CancellationToken cancellationToken = default)
        {
            // Extension metodu kullanarak Cache-First (Önce Cache'e Bak) mantığını 1 satıra düşürdük!
            var cacheModels = await _cache.GetOrSetAsync(
                CacheConstants.RequestLogsCacheList,
                async () =>
                {
                    var entities = await _repository.GetAllLogsAsync();
                    return _mapper.Map<List<RequestCacheModel>>(entities);
                },
                TimeSpan.FromMinutes(10),
                cancellationToken
            );

            return _mapper.Map<List<RequestLogDto>>(cacheModels, opt => opt.Items["Localizer"] = _localizer);
        }

        public async Task<RequestLogDto> CreateLogAsync(RequestLogCreateDto dto)
        {
            var entity = _mapper.Map<RequestLogEntity>(dto);
            await _repository.AddLogAsync(entity);

            // Yeni log eklenince cache key'ini temizliyoruz
            await _cache.RemoveAsync(CacheConstants.RequestLogsCacheList);

            return _mapper.Map<RequestLogDto>(entity, opt => opt.Items["Localizer"] = _localizer);
        }

        public async Task<bool> UpdateLogAsync(int id, RequestLogUpdateDto dto)
        {
            var entity = await _repository.GetLogByIdAsync(id);
            if (entity == null) return false;

            _mapper.Map(dto, entity);
            await _repository.UpdateLogAsync(entity);

            await _cache.RemoveAsync(CacheConstants.RequestLogsCacheList);
            return true;
        }

        public async Task<bool> DeleteLogsAsync(RequestLogDeleteDto dto)
        {
            var validationResult = _deleteValidator.Validate(dto);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            await _repository.DeleteLogsAsync(dto.IsAllDelete, dto.Count);
            await _cache.RemoveAsync(CacheConstants.RequestLogsCacheList);

            return true;
        }
        private static bool IsErrorStatus(int statusCode)
        {
            return statusCode >= 400 ||
                   statusCode is 2 or 3 or 4 or 5;
        }

        private static int GetPercentile(
            List<int> values,
            double percentile)
        {
            if (values.Count == 0)
            {
                return 0;
            }

            var index = Math.Min(
                (int)(values.Count * percentile),
                values.Count - 1);

            return values[index];
        }
        private static int ParseBucketMinutes(string? bucket)
        {
            var normalizedBucket = bucket?
                .Trim()
                .TrimEnd('m', 'M');

            return int.TryParse(
                       normalizedBucket,
                       out var minutes) &&
                   minutes > 0
                ? minutes
                : RequestConstants.Dashboard.DefaultBucketMinutes;
        }

        private static DateTime RoundDownToBucket(
            DateTime dateTime,
            int bucketMinutes)
        {
            var roundedMinute =
                dateTime.Minute / bucketMinutes * bucketMinutes;

            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                dateTime.Hour,
                roundedMinute,
                0,
                dateTime.Kind);
        }
        private static int ParseTiming(string? timing)
        {
            var normalizedTiming = timing?
                .Replace(
                    RequestConstants.EndpointHealth.MillisecondSuffix,
                    string.Empty)
                .Trim();

            return int.TryParse(normalizedTiming, out var value)
                ? value
                : 0;
        }

    } // RequestService sınıfı
}     // namespace