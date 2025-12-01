using Contracts;
using Entities.DTOs;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace qlnv.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _repo;
        public UserController(IUserRepository repo)
        {
            _repo = repo;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            if (dto == null) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Validate uniqueness
            var byUsername = await _repo.GetByUsernameAsync(dto.Username);
            if (byUsername != null) return Conflict(new { message = "Username already exists" });

            var byEmail = await _repo.GetByEmailAsync(dto.Email);
            if (byEmail != null) return Conflict(new { message = "Email already exists" });

            // Create user with IsGuest flag
            var expiresAt = dto.IsGuest ? DateTime.UtcNow.AddHours(24) : (DateTime?)null;

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName ?? string.Empty,
                IsGuest = dto.IsGuest,
                ExpiresAt = expiresAt
            };

            var created = await _repo.CreateAsync(user);

            var outDto = new UserDto
            {
                Id = created.Id,
                Username = created.Username,
                Email = created.Email,
                FullName = created.FullName,
                IsGuest = created.IsGuest,
                ExpiresAt = created.ExpiresAt
            };

            return CreatedAtAction(nameof(GetById), new { id = outDto.Id }, outDto);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] string? q = null)
        {
            const int pageSize = 10;
            var paged = await _repo.GetPagedAsync(page, pageSize, q);

            var dto = new PagedResult<UserDto>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalItems = paged.TotalItems,
                TotalPages = paged.TotalPages,
                Items = paged.Items.Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FullName = u.FullName,
                    IsGuest = u.IsGuest,
                    ExpiresAt = u.ExpiresAt
                })
            };

            return Ok(dto);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            var u = await _repo.GetByIdAsync(id);
            if (u == null) return NotFound();
            return Ok(new UserDto { Id = u.Id, Username = u.Username, Email = u.Email, FullName = u.FullName, IsGuest = u.IsGuest, ExpiresAt = u.ExpiresAt });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
        {
            if (dto == null) return BadRequest();
            if (id != dto.Id) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.FullName)) existing.FullName = dto.FullName;

            // Allow updating email (must be unique)
            if (!string.IsNullOrWhiteSpace(dto.Email) && !string.Equals(dto.Email, existing.Email, StringComparison.OrdinalIgnoreCase))
            {
                var byEmail = await _repo.GetByEmailAsync(dto.Email);
                if (byEmail != null && byEmail.Id != existing.Id)
                    return Conflict(new { message = "Email already exists" });

                existing.Email = dto.Email;
            }

            await _repo.UpdateAsync(existing);

            return Ok(new UserDto { Id = existing.Id, Username = existing.Username, Email = existing.Email, FullName = existing.FullName, IsGuest = existing.IsGuest, ExpiresAt = existing.ExpiresAt });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            await _repo.DeleteAsync(existing);
            return NoContent();
        }

        [HttpPost("change-password")]
        [Authorize] // Yêu cầu đăng nhập
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Dữ liệu không hợp lệ" });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // Lấy UserId từ JWT claim (ASP.NET Core đã map "sub" thành ClaimTypes.NameIdentifier)
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                
                if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
                {
                    return BadRequest(new { message = "Không tìm thấy UserId trong token" });
                }

                if (!Guid.TryParse(userIdClaim.Value, out var currentUserGuid))
                    return BadRequest(new { message = "UserId không hợp lệ", userId = userIdClaim.Value });

                // Chỉ cho phép user đổi mật khẩu của chính mình, trừ Admin có thể đổi cho bất kỳ ai
                if (dto.UserId != currentUserGuid && !User.IsInRole("Admin"))
                {
                    return Forbid("Bạn chỉ có thể đổi mật khẩu của chính mình");
                }

                var user = await _repo.GetByIdAsync(dto.UserId);
                if (user == null) return NotFound(new { message = "Không tìm thấy người dùng" });

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                {
                    return BadRequest(new { message = "Mật khẩu hiện tại không đúng" });
                }

                // Hash new password and update
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                await _repo.UpdateAsync(user);

                return Ok(new { message = "Đổi mật khẩu thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", detail = ex.Message });
            }
        }
    }
}
