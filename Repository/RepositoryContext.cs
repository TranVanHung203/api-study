using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class RepositoryContext : DbContext
    {
        public RepositoryContext(DbContextOptions<RepositoryContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        // Các bảng khác
        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<NgayLe> NgayLes { get; set; }
        public DbSet<EmailThongBao> EmailThongBaos { get; set; }
        public DbSet<ThongBao> ThongBaos { get; set; }
        public DbSet<CauHinhThongBao> CauHinhThongBaos { get; set; }
        public DbSet<LichSuHopDong> LichSuHopDongs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Bảng User
            builder.Entity<User>().ToTable("Users");
            builder.Entity<RefreshToken>().ToTable("RefreshTokens");

            // Bảng nghiệp vụ
            builder.Entity<NhanVien>().ToTable("NhanVien");
            builder.Entity<NgayLe>().ToTable("NgayLe");
            builder.Entity<EmailThongBao>().ToTable("EmailThongBao");
            builder.Entity<ThongBao>().ToTable("ThongBao");
            builder.Entity<CauHinhThongBao>().ToTable("CauHinhThongBao");
            builder.Entity<LichSuHopDong>().ToTable("LichSuHopDong");
        }
    }
}
