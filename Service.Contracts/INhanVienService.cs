using Entities.DTOs;

namespace Service.Contracts
{
    public interface INhanVienService
    {
        Task<NhanVienDto> CreateAsync(CreateNhanVienDto dto);
        Task<NhanVienDto?> GetByIdAsync(int id);
        Task<PagedResult<NhanVienDto>> GetPagedAsync(int page, int pageSize, string? ten = null, string? sdt = null, bool? isDeleted = null);
        Task<NhanVienDto> UpdateAsync(UpdateNhanVienDto dto);
        Task DeleteAsync(int id);
        Task<NhanVienDto> RestoreAsync(int id);
        Task<ImportNhanVienResultDto> ImportFromExcelAsync(Stream excelStream);
    }
}
