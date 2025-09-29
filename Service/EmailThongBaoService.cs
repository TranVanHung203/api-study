using Contracts;
using Entities.Models;
using Entities.DTOs;
using Service.Contracts;

namespace Service
{
    public class EmailThongBaoService : IEmailThongBaoService
    {
        private readonly IEmailThongBaoRepository _repo;
        public EmailThongBaoService(IEmailThongBaoRepository repo)
        {
            _repo = repo;
        }

        public async Task<EmailThongBaoDto> CreateAsync(CreateEmailThongBaoDto dto)
        {
            var entity = new EmailThongBao
            {
                Email = dto.Email,
                Name = dto.Name
            };

            var created = await _repo.CreateAsync(entity);

            return new EmailThongBaoDto
            {
                Id = created.Id,
                Email = created.Email,
                Name = created.Name
            };
        }

        public async Task<EmailThongBaoDto?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;

            return new EmailThongBaoDto
            {
                Id = entity.Id,
                Email = entity.Email,
                Name = entity.Name
            };
        }

        public async Task<PagedResult<EmailThongBaoDto>> GetPagedAsync(int page, int pageSize, string? email = null)
        {
            var paged = await _repo.GetPagedAsync(page, pageSize, email);

            return new PagedResult<EmailThongBaoDto>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalItems = paged.TotalItems,
                TotalPages = paged.TotalPages,
                Items = paged.Items.Select(e => new EmailThongBaoDto
                {
                    Id = e.Id,
                    Email = e.Email,
                    Name = e.Name
                })
            };
        }

        public async Task<EmailThongBaoDto> UpdateAsync(UpdateEmailThongBaoDto dto)
        {
            var entity = await _repo.GetByIdAsync(dto.Id);
            if (entity == null)
                throw new KeyNotFoundException("Không tìm thấy Email thông báo.");

            entity.Email = dto.Email;
            entity.Name = dto.Name;

            await _repo.UpdateAsync(entity);

            return new EmailThongBaoDto
            {
                Id = entity.Id,
                Email = entity.Email,
                Name = entity.Name
            };
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("Không tìm thấy Email thông báo.");

            await _repo.DeleteAsync(entity);
        }
    }
}
