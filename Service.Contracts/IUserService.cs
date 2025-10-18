using Entities.DTOs;
using System;
using System.Threading.Tasks;

namespace Service.Contracts
{
    public interface IUserService
    {
        Task<UserDto> CreateAsync(CreateUserDto dto, string currentUserRole);
        Task<UserDto?> GetByIdAsync(Guid id);
        Task<PagedResult<UserDto>> GetPagedAsync(int page, int pageSize, string? q = null);
        Task<UserDto> UpdateAsync(UpdateUserDto dto, string currentUserRole);
        Task DeleteAsync(Guid id);
        Task<bool> ChangePasswordAsync(ChangePasswordDto dto);
    }
}
