using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTN.WebAPI.Dtos;
using PTN.WebAPI.Services;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using PTN.WebAPI.Constants;

namespace PTN.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IStringLocalizer<UserController> _localizer;

        public UserController(
            IUserService userService,
            IStringLocalizer<UserController> localizer)
        {
            _userService = userService;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<List<UserDto>> GetAllUsers(CancellationToken cancellationToken)
        {
            return await _userService.GetAllUsersAsync(cancellationToken);
        }

        [HttpGet("{id}")]
        public async Task<UserDto> GetUserById(int id, CancellationToken cancellationToken)
        {
            var user = await _userService.GetUserByIdAsync(id, cancellationToken);
            if (user == null)
            {
                throw new KeyNotFoundException(
                    _localizer[UserConstants.UserNotFound].Value);
            }

            return user;
        }

        [HttpPost]
        public async Task<UserDto> CreateUser(
            [FromBody] UserCreateDto dto,
            CancellationToken cancellationToken)
        {
            return await _userService.CreateUserAsync(dto, cancellationToken);
        }

        [HttpPut("{id}")]
        public async Task<bool> UpdateUser(
            int id,
            [FromBody] UserUpdateDto dto,
            CancellationToken cancellationToken)
        {
            var success = await _userService.UpdateUserAsync(id, dto, cancellationToken);
            if (!success)
            {
                throw new KeyNotFoundException(
                    _localizer[UserConstants.UserNotFound].Value);
            }

            return true;
        }

        [HttpDelete("{id}")]
        public async Task<bool> DeleteUser(int id, CancellationToken cancellationToken)
        {
            var success = await _userService.DeleteUserAsync(id, cancellationToken);
            if (!success)
            {
                throw new KeyNotFoundException(
                    _localizer[UserConstants.UserNotFound].Value);
            }

            return true;
        }
    }
}