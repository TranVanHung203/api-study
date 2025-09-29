using Contracts;
using Entities.Models;
using Entities.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class EmailThongBaoRepository : IEmailThongBaoRepository
    {
        private readonly RepositoryContext _context;
        public EmailThongBaoRepository(RepositoryContext context)
        {
            _context = context;
        }

        public async Task<EmailThongBao> CreateAsync(EmailThongBao email)
        {
            if (!string.IsNullOrWhiteSpace(email.Email))
            {
                var exists = await _context.EmailThongBaos
                    .AnyAsync(e => e.Email.ToLower() == email.Email.ToLower());
                if (exists)
                    throw new InvalidOperationException("Email đã tồn tại.");
            }

            _context.EmailThongBaos.Add(email);
            await _context.SaveChangesAsync();
            return email;
        }

        public async Task<EmailThongBao?> GetByIdAsync(int id)
        {
            return await _context.EmailThongBaos.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<PagedResult<EmailThongBao>> GetPagedAsync(int page, int pageSize, string? email = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 8;

            var query = _context.EmailThongBaos.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(email))
            {
                var keyword = email.Trim().ToLower();
                query = query.Where(e => e.Email.ToLower().Contains(keyword) || (e.Name != null && e.Name.ToLower().Contains(keyword)));
            }

            query = query.OrderBy(e => e.Id);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<EmailThongBao>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };
        }

        public async Task UpdateAsync(EmailThongBao email)
        {
            if (!string.IsNullOrWhiteSpace(email.Email))
            {
                var exists = await _context.EmailThongBaos
                    .AnyAsync(e => e.Id != email.Id && e.Email.ToLower() == email.Email.ToLower());
                if (exists)
                    throw new InvalidOperationException("Email đã tồn tại.");
            }

            _context.EmailThongBaos.Update(email);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(EmailThongBao email)
        {
            _context.EmailThongBaos.Remove(email); // xóa cứng
            await _context.SaveChangesAsync();
        }
    }
}
