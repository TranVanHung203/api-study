namespace Entities.DTOs
{
    public class LichSuHopDongDto
    {
        public int Id { get; set; }
        public int NhanVienId { get; set; }
        public string TenNhanVien { get; set; } = string.Empty;
        public string LoaiHopDong { get; set; } = string.Empty;
        public int? SoThangHopDong { get; set; }
        public DateTime NgayThayDoi { get; set; }
        public string? GhiChu { get; set; }
    }
}
