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
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace PTN.WebAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtSettings _jwtSettings;

        private readonly IPasswordHasher<UserEntity>
            _passwordHasher;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher<UserEntity> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtSettings = new JwtSettings();
        }

        public async Task<TokenResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetUserByEmailAsync(dto.Email);




            if (user == null || !user.IsActive)
            {
                return null;
            }

            PasswordVerificationResult verificationResult;

            try
            {
                verificationResult =
                    _passwordHasher.VerifyHashedPassword(
                        user,
                        user.PasswordHash,
                        dto.Password);
            }
            catch (FormatException)
            {
                // Eski düz metin veya SHA256 kayıtları
                // aşağıdaki legacy kontrolünde doğrulanacak.
                verificationResult =
                    PasswordVerificationResult.Failed;
            }
            var isLegacyPassword =
                verificationResult ==
                PasswordVerificationResult.Failed &&
                VerifyLegacyPassword(
                    user.PasswordHash,
                    dto.Password);

            if (verificationResult ==
                PasswordVerificationResult.Failed &&
                !isLegacyPassword)
            {
                return null;
            }

            if (isLegacyPassword ||
                verificationResult ==
                PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash =
                    _passwordHasher.HashPassword(
                        user,
                        dto.Password);

                await _userRepository.UpdateUserAsync(user);
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
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
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

        private static bool VerifyLegacyPassword(
            string storedPassword,
            string providedPassword)
        {
            // Geçmişte düz metin kaydedilmiş kayıtları
            // ilk başarılı girişte yeni hash'e yükseltir.
            if (string.Equals(
                    storedPassword,
                    providedPassword,
                    StringComparison.Ordinal))
            {
                return true;
            }

            // Geçmişte SHA256 ile kaydedilmiş kullanıcıları destekler.
            var passwordBytes =
                Encoding.UTF8.GetBytes(providedPassword);

            var hashBytes =
                SHA256.HashData(passwordBytes);

            var legacyHash =
                Convert.ToBase64String(hashBytes);

            return string.Equals(
                storedPassword,
                legacyHash,
                StringComparison.Ordinal);
        }
    }
}