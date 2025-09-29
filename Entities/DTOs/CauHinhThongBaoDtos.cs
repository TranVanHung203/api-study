using System.ComponentModel.DataAnnotations;

namespace Entities.DTOs
{
    public class CauHinhThongBaoDto
    {
        public int Id { get; set; }
        public int SoNgayThongBao { get; set; }
        public string? DanhSachNamThongBao { get; set; }
        public bool IsActive { get; set; }
        public bool ExcludeSaturday { get; set; }
        public bool ExcludeSunday { get; set; }
    }

    public class UpdateCauHinhThongBaoDto
    {
        [Required]
        public int Id { get; set; }

        [Range(1, 3650)]
        public int SoNgayThongBao { get; set; }
        // comma separated years, e.g. "1,2"
        public string? DanhSachNamThongBao { get; set; }
        public bool IsActive { get; set; }
        public bool ExcludeSaturday { get; set; }
        public bool ExcludeSunday { get; set; }
    }

    public class CreateCauHinhThongBaoDto
    {
        [Range(1, 3650)]
        public int SoNgayThongBao { get; set; } = 60;
        public string? DanhSachNamThongBao { get; set; }
        public bool IsActive { get; set; } = false;
        public bool ExcludeSaturday { get; set; } = true;
        public bool ExcludeSunday { get; set; } = true;
    }

    // SendCauHinhThongBaoDto removed - /send endpoint deprecated
}
