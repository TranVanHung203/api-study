using System;
using System.ComponentModel.DataAnnotations;

namespace Entities.DTOs
{
    public class NhanVienDto
    {
        public int Id { get; set; }
        public string Ten { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
        public DateTime? NgayVaoLam { get; set; }
        public DateTime NgaySinh { get; set; }
        public DateTime? NgayLamViecChinhThuc { get; set; }
        public string? LoaiHopDong { get; set; }
        public int? SoThangHopDong { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CreateNhanVienDto
    {
        [Required]
        [StringLength(200)]
        public string Ten { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? SoDienThoai { get; set; }

        [StringLength(500)]
        public string? DiaChi { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NgayVaoLam { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime NgaySinh { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NgayLamViecChinhThuc { get; set; }

        // "1nam" | "vothoihan" | "khac" (có thể mở rộng thành enum sau)
        [Required]
        [RegularExpression("^(1nam|vothoihan|khac)$", ErrorMessage = "LoaiHopDong phải là 1nam, vothoihan hoặc khac")]
        public string LoaiHopDong { get; set; } = "1nam";

        // Nếu LoaiHopDong = khac thì bắt buộc nhập số tháng > 0, còn lại service sẽ tự gán (12 hoặc 999)
        public int? SoThangHopDong { get; set; }
    }

    public class UpdateNhanVienDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Ten { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? SoDienThoai { get; set; }

        [StringLength(500)]
        public string? DiaChi { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NgayVaoLam { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime NgaySinh { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NgayLamViecChinhThuc { get; set; }

        [Required]
        [RegularExpression("^(1nam|vothoihan|khac)$", ErrorMessage = "LoaiHopDong phải là 1nam, vothoihan hoặc khac")]
        public string LoaiHopDong { get; set; } = "1nam";

        public int? SoThangHopDong { get; set; }
    }

    public class ImportNhanVienResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<ImportErrorDto> Errors { get; set; } = new List<ImportErrorDto>();
        public List<NhanVienDto> ImportedEmployees { get; set; } = new List<NhanVienDto>();
    }

    public class ImportErrorDto
    {
        public int Row { get; set; }
        public string Error { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
    }
}
