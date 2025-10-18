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
        public DateTime NgayVaoLam { get; set; }
        public DateTime NgaySinh { get; set; }
        public DateTime? NgayLamViecChinhThuc { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CreateNhanVienDto
    {
        [Required]
        [StringLength(200)]
        public string Ten { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? SoDienThoai { get; set; }

        [StringLength(500)]
        public string? DiaChi { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime NgayVaoLam { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime NgaySinh { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NgayLamViecChinhThuc { get; set; }
    }

    public class UpdateNhanVienDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Ten { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? SoDienThoai { get; set; }

        [StringLength(500)]
        public string? DiaChi { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime NgayVaoLam { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime NgaySinh { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NgayLamViecChinhThuc { get; set; }
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
