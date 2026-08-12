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
namespace PTN.WebAPI.Services
{
    public class RequestService : IRequestService
    {
        private readonly IRequestRepository _repository;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<RequestService> _localizer;
        private readonly IDistributedCache _cache;

        public RequestService(
            IRequestRepository repository, 
            IMapper mapper, 
            IStringLocalizer<RequestService> localizer,
            IDistributedCache cache)
        {
            _repository = repository;
            _mapper = mapper;
            _localizer = localizer;
            _cache = cache;
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

        public async Task DeleteLogsAsync(RequestLogDeleteDto dto)
        {
            await _repository.DeleteLogsAsync(dto.IsAllDelete, dto.Count);
            await _cache.RemoveAsync(CacheConstants.RequestLogsCacheList);
        }
    }
}