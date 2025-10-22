using Contracts;
using Entities.DTOs;
using Service.Contracts;

namespace Service
{
    public class LichSuHopDongService : ILichSuHopDongService
    {
        private readonly ILichSuHopDongRepository _lichSuRepo;
        private readonly INhanVienRepository _nhanVienRepo;

        public LichSuHopDongService(ILichSuHopDongRepository lichSuRepo, INhanVienRepository nhanVienRepo)
        {
            _lichSuRepo = lichSuRepo;
            _nhanVienRepo = nhanVienRepo;
        }

        public async Task<List<LichSuHopDongDto>> GetAllAsync()
        {
            var lichSuList = await _lichSuRepo.GetAllAsync();

            return lichSuList.Select(ls => new LichSuHopDongDto
            {
                Id = ls.Id,
                NhanVienId = ls.NhanVienId,
                TenNhanVien = ls.NhanVien?.Ten ?? "N/A",
                LoaiHopDong = ls.LoaiHopDong,
                SoThangHopDong = ls.SoThangHopDong,
                NgayThayDoi = ls.NgayThayDoi,
                GhiChu = ls.GhiChu
            }).ToList();
        }

        public async Task<List<LichSuHopDongDto>> GetByNhanVienIdAsync(int nhanVienId)
        {
            var lichSuList = await _lichSuRepo.GetByNhanVienIdAsync(nhanVienId);
            var nhanVien = await _nhanVienRepo.GetByIdAsync(nhanVienId);

            return lichSuList.Select(ls => new LichSuHopDongDto
            {
                Id = ls.Id,
                NhanVienId = ls.NhanVienId,
                TenNhanVien = nhanVien?.Ten ?? "N/A",
                LoaiHopDong = ls.LoaiHopDong,
                SoThangHopDong = ls.SoThangHopDong,
                NgayThayDoi = ls.NgayThayDoi,
                GhiChu = ls.GhiChu
            }).ToList();
        }
    }
}
