using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTN.WebAPI.Dtos;
using PTN.WebAPI.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PTN.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
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
            if (user == null) throw new System.Collections.Generic.KeyNotFoundException("Kullanıcı bulunamadı.");
            return user;
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
            if (!success) throw new System.Collections.Generic.KeyNotFoundException("Güncellenecek kullanıcı bulunamadı.");
            return true;
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<bool> DeleteUser(int id)
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success) throw new System.Collections.Generic.KeyNotFoundException("Silinecek kullanıcı bulunamadı.");
            return true;
        }
    }
}