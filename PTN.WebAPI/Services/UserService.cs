using AutoMapper;
using Microsoft.Extensions.Localization;
using PTN.WebAPI.Constants;
using PTN.WebAPI.Dtos;
using PTN.WebAPI.Entities;
using PTN.WebAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Threading;
namespace PTN.WebAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<UserService> _localizer;
        private readonly IPasswordHasher<UserEntity>
            _passwordHasher;

        public UserService(
            IUserRepository repository,
            IMapper mapper,
            IStringLocalizer<UserService> localizer,
            IPasswordHasher<UserEntity> passwordHasher)
        {
            _repository = repository;
            _mapper = mapper;
            _localizer = localizer;
            _passwordHasher = passwordHasher;
        }

        public async Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _repository.GetAllUsersAsync(cancellationToken);
            return _mapper.Map<List<UserDto>>(entities);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetUserByIdAsync(id, cancellationToken);
            if (entity == null) return null;
            return _mapper.Map<UserDto>(entity);
        }

        public async Task<UserDto> CreateUserAsync(UserCreateDto dto, CancellationToken cancellationToken = default)
        {
            dto.FullName = dto.FullName.Trim();
            dto.Email = dto.Email.Trim().ToLowerInvariant();

            var existingUser = await _repository.GetUserByEmailAsync(dto.Email, cancellationToken);
            if (existingUser != null)
            {
                throw new InvalidOperationException(
                    _localizer[UserConstants.EmailAlreadyExists].Value);
            }

            var entity = _mapper.Map<UserEntity>(dto);
    
            // Parolayı ASP.NET Core PasswordHasher ile güvenli biçimde hashliyoruz.
            entity.PasswordHash =
                _passwordHasher.HashPassword(
                    entity,
                    dto.Password);

            await _repository.AddUserAsync(entity, cancellationToken);
            return _mapper.Map<UserDto>(entity);
        }

        public async Task<bool> UpdateUserAsync(int id, UserUpdateDto dto, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetUserByIdAsync(id, cancellationToken);
            if (entity == null) return false;

            _mapper.Map(dto, entity);
            await _repository.UpdateUserAsync(entity, cancellationToken);
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetUserByIdAsync(id, cancellationToken);
            if (entity == null) return false;

            await _repository.DeleteUserAsync(entity, cancellationToken);
            return true;
        }
    }
}