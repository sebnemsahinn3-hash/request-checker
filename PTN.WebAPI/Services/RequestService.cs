using AutoMapper;
using Microsoft.Extensions.Localization;
using PTN.WebAPI.Dtos;
using PTN.WebAPI.Entities;
using PTN.WebAPI.Repositories;
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

        public RequestService(IRequestRepository repository, IMapper mapper, IStringLocalizer<RequestService> localizer)
        {
            _repository = repository;
            _mapper = mapper;
            _localizer = localizer;
        }

        public async Task<List<RequestLogDto>> GetAllLogsAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _repository.GetAllLogsAsync();
            return _mapper.Map<List<RequestLogDto>>(entities, opt => opt.Items["Localizer"] = _localizer);
        }

        public async Task<RequestLogDto> CreateLogAsync(RequestLogCreateDto dto)
        {
            var entity = _mapper.Map<RequestLogEntity>(dto);
            await _repository.AddLogAsync(entity);
            return _mapper.Map<RequestLogDto>(entity, opt => opt.Items["Localizer"] = _localizer);
        }

        public async Task<bool> UpdateLogAsync(int id, RequestLogUpdateDto dto)
        {
            var entity = await _repository.GetLogByIdAsync(id);
            if (entity == null) return false;

            _mapper.Map(dto, entity);
            await _repository.UpdateLogAsync(entity);
            return true;
        }

        public async Task DeleteLogsAsync(RequestLogDeleteDto dto)
        {
            await _repository.DeleteLogsAsync(dto.IsAllDelete, dto.Count);
        }
    }
}