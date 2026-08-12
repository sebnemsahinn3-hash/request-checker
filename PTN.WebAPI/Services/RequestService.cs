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