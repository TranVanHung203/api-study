namespace Entities.Models
{
    public class CauHinhThongBao
    {
        public int Id { get; set; }
        // probation days separated by comma, e.g., "60,90" (thử việc)
        public string? SoNgayThongBao { get; set; } = "60";

        // comma separated list of anniversary years to notify, e.g. "1,2,3"
        public string? DanhSachNamThongBao { get; set; }
        
        // mark this configuration as the active one used by the system
        public bool IsActive { get; set; } = false;
        
        // If true, Saturdays are considered non-working days (i.e., excluded from working days count)
        public bool ExcludeSaturday { get; set; } = true;

        // If true, Sundays are considered non-working days (i.e., excluded from working days count)
        public bool ExcludeSunday { get; set; } = true;

        // Số ngày thông báo trước khi đến kỷ niệm, phân cách bằng dấu phẩy, e.g. "30,60,90"
        public string? SoNgayThongBaoTruoc { get; set; } = "30";
    }
}
