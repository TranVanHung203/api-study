using Contracts;
using Entities.DTOs;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            if (dto == null) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Validate uniqueness
            var byUsername = await _repo.GetByUsernameAsync(dto.Username);
            if (byUsername != null) return Conflict(new { message = "Username already exists" });

            var byEmail = await _repo.GetByEmailAsync(dto.Email);
            if (byEmail != null) return Conflict(new { message = "Email already exists" });

            // Default role is Assistant. If the caller is Admin and provided a role, honor it.
            var role = "Assistant";
            if (!string.IsNullOrWhiteSpace(dto.Role) && User.IsInRole("Admin"))
            {
                role = dto.Role;
            }

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName ?? string.Empty,
                Role = role
            };

            var created = await _repo.CreateAsync(user);

            var outDto = new UserDto
            {
                Id = created.Id,
                Username = created.Username,
                Email = created.Email,
                FullName = created.FullName,
                Role = created.Role
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
                    Role = u.Role
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
            return Ok(new UserDto { Id = u.Id, Username = u.Username, Email = u.Email, FullName = u.FullName, Role = u.Role });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
        {
            if (dto == null) return BadRequest();
            if (id != dto.Id) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            // Role change only allowed by Admin
            if (!string.IsNullOrWhiteSpace(dto.Role) && dto.Role != existing.Role)
            {
                if (!User.IsInRole("Admin"))
                    return Forbid();

                existing.Role = dto.Role;
            }
            else
            {
                // Ensure non-admins cannot attempt to change role — silently ignore role changes from Assistants
                // (role already unchanged if not provided or same as existing)
            }

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

            return Ok(new UserDto { Id = existing.Id, Username = existing.Username, Email = existing.Email, FullName = existing.FullName, Role = existing.Role });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            await _repo.DeleteAsync(existing);
            return NoContent();
        }
    }
}
