namespace Entities.Models
{
    public class LichSuHopDong
    {
        public int Id { get; set; }
        public int NhanVienId { get; set; }
        public string LoaiHopDong { get; set; } = string.Empty;
        public int? SoThangHopDong { get; set; }
        public DateTime NgayThayDoi { get; set; }
        public string? GhiChu { get; set; }
        
        // Navigation property
        public NhanVien? NhanVien { get; set; }
    }
}
