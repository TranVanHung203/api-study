namespace Entities.Models
{
    public class NhanVien
    {
        public int Id { get; set; }
        public string Ten { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChi { get; set; }
        public DateTime NgayVaoLam { get; set; }
        public DateTime NgaySinh { get; set; }
        public DateTime? NgayLamViecChinhThuc { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
