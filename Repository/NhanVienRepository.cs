using Contracts;
using Entities.Models;
using Entities.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class NhanVienRepository : INhanVienRepository
    {
        private readonly RepositoryContext _context;
        public NhanVienRepository(RepositoryContext context) { _context = context; }

        public async Task<NhanVien> CreateAsync(NhanVien nhanVien)
        {
            // validate email/phone trùng
            if (!string.IsNullOrWhiteSpace(nhanVien.Email))
            {
                var exists = await _context.NhanViens.AnyAsync(n => !n.IsDeleted && n.Email == nhanVien.Email);
                if (exists) throw new InvalidOperationException("Email đã tồn tại");
            }
            _context.NhanViens.Add(nhanVien);
            await _context.SaveChangesAsync();
            return nhanVien;
        }

        public async Task<NhanVien?> GetByIdAsync(int id) =>
            await _context.NhanViens.FirstOrDefaultAsync(n => n.Id == id);

        public async Task<List<NhanVien>> GetAllAsync()
        {
            return await _context.NhanViens.AsNoTracking().Where(n => !n.IsDeleted).ToListAsync();
        }

        public async Task<PagedResult<NhanVien>> GetPagedAsync(int page, int pageSize, string? ten = null, string? sdt = null, bool? isDeleted = null)
        {
            var query = _context.NhanViens.AsNoTracking();
            
            if (!string.IsNullOrWhiteSpace(ten)) query = query.Where(n => n.Ten.Contains(ten));
            if (!string.IsNullOrWhiteSpace(sdt)) query = query.Where(n => n.SoDienThoai.Contains(sdt));
            if (isDeleted.HasValue) query = query.Where(n => n.IsDeleted == isDeleted.Value);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<NhanVien>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };
        }

        public async Task UpdateAsync(NhanVien nhanVien)
        {
            _context.NhanViens.Update(nhanVien);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(NhanVien nhanVien)
        {
            nhanVien.IsDeleted = true;
            _context.NhanViens.Update(nhanVien);
            await _context.SaveChangesAsync();
        }

        public async Task RestoreAsync(NhanVien nhanVien)
        {
            nhanVien.IsDeleted = false;
            _context.NhanViens.Update(nhanVien);
            await _context.SaveChangesAsync();
        }
    }
}
