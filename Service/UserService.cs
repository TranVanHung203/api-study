using Contracts;
using Entities.DTOs;
using Entities.Models;
using Service.Contracts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task<UserDto> CreateAsync(CreateUserDto dto, string currentUserRole)
        {
            // Validate uniqueness
            var byUsername = await _repo.GetByUsernameAsync(dto.Username);
            if (byUsername != null) throw new InvalidOperationException("Username already exists");

            var byEmail = await _repo.GetByEmailAsync(dto.Email);
            if (byEmail != null) throw new InvalidOperationException("Email already exists");

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

            return new UserDto
            {
                Id = created.Id,
                Username = created.Username,
                Email = created.Email,
                FullName = created.FullName,
                IsGuest = created.IsGuest,
                ExpiresAt = created.ExpiresAt
            };
        }

        public async Task<UserDto?> GetByIdAsync(Guid id)
        {
            var u = await _repo.GetByIdAsync(id);
            if (u == null) return null;

            return new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FullName = u.FullName,
                IsGuest = u.IsGuest,
                ExpiresAt = u.ExpiresAt
            };
        }

        public async Task<PagedResult<UserDto>> GetPagedAsync(int page, int pageSize, string? q = null)
        {
            var paged = await _repo.GetPagedAsync(page, pageSize, q);

            return new PagedResult<UserDto>
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
        }

        public async Task<UserDto> UpdateAsync(UpdateUserDto dto, string currentUserRole)
        {
            var existing = await _repo.GetByIdAsync(dto.Id);
            if (existing == null) throw new KeyNotFoundException("User not found");

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                existing.FullName = dto.FullName;

            // Allow updating email (must be unique)
            if (!string.IsNullOrWhiteSpace(dto.Email) && !string.Equals(dto.Email, existing.Email, StringComparison.OrdinalIgnoreCase))
            {
                var byEmail = await _repo.GetByEmailAsync(dto.Email);
                if (byEmail != null && byEmail.Id != existing.Id)
                    throw new InvalidOperationException("Email already exists");

                existing.Email = dto.Email;
            }

            await _repo.UpdateAsync(existing);

            return new UserDto
            {
                Id = existing.Id,
                Username = existing.Username,
                Email = existing.Email,
                FullName = existing.FullName,
                IsGuest = existing.IsGuest,
                ExpiresAt = existing.ExpiresAt
            };
        }

        public async Task DeleteAsync(Guid id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException("User not found");

            await _repo.DeleteAsync(existing);
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordDto dto)
        {
            var user = await _repo.GetByIdAsync(dto.UserId);
            if (user == null) throw new KeyNotFoundException("Không tìm thấy người dùng");

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            {
                throw new InvalidOperationException("Mật khẩu hiện tại không đúng");
            }

            // Hash new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            
            await _repo.UpdateAsync(user);
            
            return true;
        }
    }
}
