using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTN.WebAPI.Dtos;
using PTN.WebAPI.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using PTN.WebAPI.Constants;

namespace PTN.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public async Task<List<UserDto>> GetAllUsers()
        {
            return await _userService.GetAllUsersAsync();
        }

        [HttpGet("{id}")]
        public async Task<UserDto?> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
           throw new KeyNotFoundException(
                _localizer[UserConstants.UserNotFound].Value);
        }

        [HttpPost]
        public async Task<UserDto> CreateUser([FromBody] UserCreateDto dto)
        {
            return await _userService.CreateUserAsync(dto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<bool> UpdateUser(int id, [FromBody] UserUpdateDto dto)
        {
            var success = await _userService.UpdateUserAsync(id, dto);
           throw new KeyNotFoundException(
                _localizer[UserConstants.UserNotFound].Value);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<bool> DeleteUser(int id)
        {
            var success = await _userService.DeleteUserAsync(id);
           throw new KeyNotFoundException(
                _localizer[UserConstants.UserNotFound].Value);
        }
    }
}