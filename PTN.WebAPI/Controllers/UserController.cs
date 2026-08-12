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
        public async Task<ActionResult<List<UserDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound(new { message = "Kullanıcı bulunamadı." });
            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] UserCreateDto dto)
        {
            var created = await _userService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetUserById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<bool>> UpdateUser(int id, [FromBody] UserUpdateDto dto)
        {
            var success = await _userService.UpdateUserAsync(id, dto);
            if (!success) return NotFound(new { message = "Güncellenecek kullanıcı bulunamadı." });
            return Ok(true);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<bool>> DeleteUser(int id)
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success) return NotFound(new { message = "Silinecek kullanıcı bulunamadı." });
            return Ok(true);
        }
    }
}