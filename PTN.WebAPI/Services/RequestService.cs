using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using PTN.WebAPI.Dtos;
using PTN.WebAPI.Entities;
using PTN.WebAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Text.Json;
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
            const string cacheKey = "request_logs_cache_list";

            // 1. Önce Redis Cache'te var mı bakıyoruz
            var cachedJson = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (!string.IsNullOrEmpty(cachedJson))
            {
                // Redis'te bulundu! Doğrudan RAM'den ışık hızında döndürüyoruz
                var cacheModels = JsonSerializer.Deserialize<List<RequestCacheModel>>(cachedJson);
                return _mapper.Map<List<RequestLogDto>>(cacheModels, opt => opt.Items["Localizer"] = _localizer);
            }

            // 2. Redis'te yoksa Veritabanından (PostgreSQL) çekiyoruz
            var entities = await _repository.GetAllLogsAsync();

            // 3. Çekilen veriyi sonraki istekler hızlı olsun diye 10 Dakikalığına Redis'e yazıyoruz
            var modelsToCache = _mapper.Map<List<RequestCacheModel>>(entities);
            var jsonToCache = JsonSerializer.Serialize(modelsToCache);

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };

            await _cache.SetStringAsync(cacheKey, jsonToCache, cacheOptions, cancellationToken);

            return _mapper.Map<List<RequestLogDto>>(entities, opt => opt.Items["Localizer"] = _localizer);
        }

        public async Task<RequestLogDto> CreateLogAsync(RequestLogCreateDto dto)
        {
            var entity = _mapper.Map<RequestLogEntity>(dto);
            await _repository.AddLogAsync(entity);

            // Yeni log eklenince Redis Cache'ini temizliyoruz ki listemizde anında görünsün
            await _cache.RemoveAsync("request_logs_cache_list");

            return _mapper.Map<RequestLogDto>(entity, opt => opt.Items["Localizer"] = _localizer);
        }

        public async Task<bool> UpdateLogAsync(int id, RequestLogUpdateDto dto)
        {
            var entity = await _repository.GetLogByIdAsync(id);
            if (entity == null) return false;

            _mapper.Map(dto, entity);
            await _repository.UpdateLogAsync(entity);

            await _cache.RemoveAsync("request_logs_cache_list");
            return true;
        }

        public async Task DeleteLogsAsync(RequestLogDeleteDto dto)
        {
            await _repository.DeleteLogsAsync(dto.IsAllDelete, dto.Count);
            await _cache.RemoveAsync("request_logs_cache_list");
        }
    }
}