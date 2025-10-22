using System.ComponentModel.DataAnnotations;

namespace Entities.DTOs
{
    public class CauHinhThongBaoDto
    {
        public int Id { get; set; }
        public string? SoNgayThongBao { get; set; }
        public string? DanhSachNamThongBao { get; set; }
        public bool IsActive { get; set; }
        public bool ExcludeSaturday { get; set; }
        public bool ExcludeSunday { get; set; }
        public string? SoNgayThongBaoTruoc { get; set; }
    }

    public class UpdateCauHinhThongBaoDto
    {
        [Required]
        public int Id { get; set; }

        // comma separated days, e.g. "60,90"
        public string? SoNgayThongBao { get; set; }
        // comma separated years, e.g. "1,2"
        public string? DanhSachNamThongBao { get; set; }
        public bool IsActive { get; set; }
        public bool ExcludeSaturday { get; set; }
        public bool ExcludeSunday { get; set; }
        
        // comma separated days, e.g. "30,60,90"
        public string? SoNgayThongBaoTruoc { get; set; } = "30";
    }

    public class CreateCauHinhThongBaoDto
    {
        // comma separated days, e.g. "60,90"
        public string? SoNgayThongBao { get; set; } = "60";
        public string? DanhSachNamThongBao { get; set; }
        public bool IsActive { get; set; } = false;
        public bool ExcludeSaturday { get; set; } = true;
        public bool ExcludeSunday { get; set; } = true;
        
        // comma separated days, e.g. "30,60,90"
        public string? SoNgayThongBaoTruoc { get; set; } = "30";
    }

    // SendCauHinhThongBaoDto removed - /send endpoint deprecated
}
