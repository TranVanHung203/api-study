using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Repository
{
    /// <summary>
    /// Interceptor tự động lưu lịch sử khi thay đổi hợp đồng nhân viên
    /// </summary>
    public class LichSuHopDongInterceptor : SaveChangesInterceptor
    {
        private List<(NhanVien Entity, string? GhiChu)> _pendingLogs = new();

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            _pendingLogs.Clear();
            CaptureContractChanges(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, 
            InterceptionResult<int> result, 
            CancellationToken cancellationToken = default)
        {
            _pendingLogs.Clear();
            CaptureContractChanges(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            SavePendingLogs(eventData.Context);
            return base.SavedChanges(eventData, result);
        }

        public override ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData, 
            int result, 
            CancellationToken cancellationToken = default)
        {
            SavePendingLogs(eventData.Context);
            return base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private void CaptureContractChanges(DbContext? context)
        {
            if (context == null) return;

            var entries = context.ChangeTracker.Entries<NhanVien>()
                .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added)
                .ToList();

            foreach (var entry in entries)
            {
                bool shouldLog = false;
                string? ghiChu = null;

                if (entry.State == EntityState.Added)
                {
                    // Nhân viên mới: Lưu hợp đồng ban đầu
                    shouldLog = true;
                    ghiChu = "Tạo hợp đồng ban đầu";
                }
                else if (entry.State == EntityState.Modified)
                {
                    // Kiểm tra xem LoaiHopDong hoặc SoThangHopDong có thay đổi không
                    var loaiHopDongChanged = entry.Property(nameof(NhanVien.LoaiHopDong)).IsModified;
                    var soThangChanged = entry.Property(nameof(NhanVien.SoThangHopDong)).IsModified;

                    if (loaiHopDongChanged || soThangChanged)
                    {
                        shouldLog = true;
                        
                        var oldLoai = entry.Property(nameof(NhanVien.LoaiHopDong)).OriginalValue?.ToString();
                        var newLoai = entry.Property(nameof(NhanVien.LoaiHopDong)).CurrentValue?.ToString();
                        var oldThang = entry.Property(nameof(NhanVien.SoThangHopDong)).OriginalValue;
                        var newThang = entry.Property(nameof(NhanVien.SoThangHopDong)).CurrentValue;

                        ghiChu = $"Thay đổi từ {oldLoai} ({oldThang} tháng) sang {newLoai} ({newThang} tháng)";
                    }
                }

                if (shouldLog)
                {
                    // Lưu vào pending list để xử lý SAU KHI SaveChanges (khi đã có Id)
                    _pendingLogs.Add((entry.Entity, ghiChu));
                }
            }
        }

        private void SavePendingLogs(DbContext? context)
        {
            if (context == null || !_pendingLogs.Any()) return;

            foreach (var (entity, ghiChu) in _pendingLogs)
            {
                // Bây giờ entity.Id đã có giá trị (sau khi SaveChanges)
                var lichSu = new LichSuHopDong
                {
                    NhanVienId = entity.Id,
                    LoaiHopDong = entity.LoaiHopDong ?? "1nam",
                    SoThangHopDong = entity.SoThangHopDong,
                    NgayThayDoi = DateTime.UtcNow,
                    GhiChu = ghiChu
                };

                context.Set<LichSuHopDong>().Add(lichSu);
            }

            // Lưu lịch sử vào database
            context.SaveChanges();
            _pendingLogs.Clear();
        }
    }
}
