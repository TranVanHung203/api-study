using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class LichSuHopDongRepository : ILichSuHopDongRepository
    {
        private readonly RepositoryContext _context;

        public LichSuHopDongRepository(RepositoryContext context)
        {
            _context = context;
        }

        public async Task<List<LichSuHopDong>> GetAllAsync()
        {
            return await _context.LichSuHopDongs
                .Include(ls => ls.NhanVien)
                .OrderByDescending(ls => ls.NgayThayDoi)
                .ToListAsync();
        }

        public async Task<List<LichSuHopDong>> GetByNhanVienIdAsync(int nhanVienId)
        {
            return await _context.LichSuHopDongs
                .Where(ls => ls.NhanVienId == nhanVienId)
                .OrderByDescending(ls => ls.NgayThayDoi)
                .ToListAsync();
        }

        public async Task<LichSuHopDong?> GetByIdAsync(int id)
        {
            return await _context.LichSuHopDongs.FindAsync(id);
        }
    }
}
