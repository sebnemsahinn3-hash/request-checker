using Microsoft.IdentityModel.Tokens;
using PTN.WebAPI.Dtos;
using PTN.WebAPI.Entities;
using PTN.WebAPI.Models;
using PTN.WebAPI.Repositories;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PTN.WebAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtSettings _jwtSettings;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _jwtSettings = new JwtSettings();
        }

        public async Task<TokenResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetUserByEmailAsync(dto.Email);

            // Eğer veritabanında admin@ptn.com yoksa otomatik oluştur ve giriş izni ver
            if (user == null && dto.Email.Equals("admin@ptn.com", StringComparison.OrdinalIgnoreCase) && dto.Password == "Admin123!")
            {
                user = new UserEntity
                {
                    FullName = "System Admin",
                    Email = "admin@ptn.com",
                    PasswordHash = "Admin123!",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _userRepository.AddUserAsync(user);
            }

            if (user == null || user.PasswordHash != dto.Password || !user.IsActive)
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = expiration,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new TokenResponseDto
            {
                Token = tokenHandler.WriteToken(token),
                Expiration = expiration,
                FullName = user.FullName,
                Role = user.Role
            };
        }
    }
}