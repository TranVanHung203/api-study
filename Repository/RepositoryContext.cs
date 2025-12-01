using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class RepositoryContext : DbContext
    {
        public RepositoryContext(DbContextOptions<RepositoryContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        // // Các bảng khác
        // public DbSet<NhanVien> NhanViens { get; set; }
        // public DbSet<NgayLe> NgayLes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Bảng User
            builder.Entity<User>().ToTable("Users");
            builder.Entity<RefreshToken>().ToTable("RefreshTokens");

            // // Bảng nghiệp vụ
            // builder.Entity<NhanVien>().ToTable("NhanVien");
            // builder.Entity<NgayLe>().ToTable("NgayLe");
   
        }
    }
}
