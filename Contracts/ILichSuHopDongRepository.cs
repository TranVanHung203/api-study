using Entities.Models;

namespace Contracts
{
    public interface ILichSuHopDongRepository
    {
        Task<List<LichSuHopDong>> GetAllAsync();
        Task<List<LichSuHopDong>> GetByNhanVienIdAsync(int nhanVienId);
        Task<LichSuHopDong?> GetByIdAsync(int id);
    }
}
