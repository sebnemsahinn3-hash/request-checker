using AutoMapper;
using Microsoft.Extensions.Localization;
using PTN.WebAPI.Constants;
using PTN.WebAPI.Dtos;
using PTN.WebAPI.Entities;
using PTN.WebAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PTN.WebAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<UserService> _localizer;

        public UserService(
            IUserRepository repository, 
            IMapper mapper, 
            IStringLocalizer<UserService> localizer)
        {
            _repository = repository;
            _mapper = mapper;
            _localizer = localizer;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var entities = await _repository.GetAllUsersAsync();
            return _mapper.Map<List<UserDto>>(entities);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var entity = await _repository.GetUserByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<UserDto>(entity);
        }

        public async Task<UserDto> CreateUserAsync(UserCreateDto dto)
        {
            var existingUser = await _repository.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new Exception(_localizer[UserConstants.EmailAlreadyExists].Value);
            }

            var entity = _mapper.Map<UserEntity>(dto);
    
            // 🔐 Şifreyi SHA256 kriptografik hash ile şifreleyerek veritabanına kaydediyoruz
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(dto.Password));
                entity.PasswordHash = Convert.ToBase64String(bytes);
            }

            await _repository.AddUserAsync(entity);
            return _mapper.Map<UserDto>(entity);
        }

        public async Task<bool> UpdateUserAsync(int id, UserUpdateDto dto)
        {
            var entity = await _repository.GetUserByIdAsync(id);
            if (entity == null) return false;

            _mapper.Map(dto, entity);
            await _repository.UpdateUserAsync(entity);
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var entity = await _repository.GetUserByIdAsync(id);
            if (entity == null) return false;

            await _repository.DeleteUserAsync(entity);
            return true;
        }
    }
}