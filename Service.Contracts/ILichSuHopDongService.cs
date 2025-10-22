using Entities.DTOs;

namespace Service.Contracts
{
    public interface ILichSuHopDongService
    {
        Task<List<LichSuHopDongDto>> GetAllAsync();
        Task<List<LichSuHopDongDto>> GetByNhanVienIdAsync(int nhanVienId);
    }
}
