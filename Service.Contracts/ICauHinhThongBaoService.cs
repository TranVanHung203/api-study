using Entities.DTOs;

namespace Service.Contracts
{
    public interface ICauHinhThongBaoService
    {
        Task<CauHinhThongBaoDto?> GetConfigAsync();
    Task<CauHinhThongBaoDto> CreateConfigAsync(CreateCauHinhThongBaoDto dto);
        Task<CauHinhThongBaoDto> UpdateConfigAsync(UpdateCauHinhThongBaoDto dto);
        Task DeleteConfigAsync(int id);

        // new APIs
        Task<List<CauHinhThongBaoDto>> GetAllConfigsAsync();
        Task<CauHinhThongBaoDto?> GetConfigByIdAsync(int id);
        Task<CauHinhThongBaoDto?> ActivateConfigAsync(int id);
    Task<CauHinhThongBaoDto?> GetActiveOnlyAsync();

    // Run check and send notifications; returns number of emails sent
    Task<int> RunCheckAndSendAsync();
    
    // Run birthday check and send notifications; returns number of emails sent
    Task<int> RunBirthdayCheckAndSendAsync();
    }
}
