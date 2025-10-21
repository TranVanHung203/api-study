using Contracts;
using Entities.DTOs;
using Entities.Models;
using Service.Contracts;
using ClosedXML.Excel;
using System.ComponentModel.DataAnnotations;

namespace Service
{
    public class NhanVienService : INhanVienService
    {
        private readonly INhanVienRepository _repo;
        public NhanVienService(INhanVienRepository repo) { _repo = repo; }

        public async Task<NhanVienDto> CreateAsync(CreateNhanVienDto dto)
        {
            // Xác định số tháng hợp đồng dựa trên loại
            int? soThang = dto.LoaiHopDong switch
            {
                "1nam" => 12,
                "vothoihan" => 999,
                "khac" => dto.SoThangHopDong, // sẽ được validate phía sau
                _ => null
            };

            if (dto.LoaiHopDong == "khac")
            {
                if (!dto.SoThangHopDong.HasValue || dto.SoThangHopDong.Value <= 0)
                    throw new ValidationException("SoThangHopDong phải > 0 khi LoaiHopDong = khac");
            }

            var entity = new NhanVien
            {
                Ten = dto.Ten,
                Email = dto.Email ?? string.Empty,
                SoDienThoai = dto.SoDienThoai ?? string.Empty,
                DiaChi = dto.DiaChi ?? string.Empty,
                NgayVaoLam = dto.NgayVaoLam,
                NgaySinh = dto.NgaySinh,
                NgayLamViecChinhThuc = dto.NgayLamViecChinhThuc,
                LoaiHopDong = dto.LoaiHopDong,
                SoThangHopDong = soThang
            };
            var created = await _repo.CreateAsync(entity);
            return new NhanVienDto
            {
                Id = created.Id,
                Ten = created.Ten,
                Email = created.Email,
                SoDienThoai = created.SoDienThoai,
                DiaChi = created.DiaChi,
                NgayVaoLam = created.NgayVaoLam,
                NgaySinh = created.NgaySinh,
                NgayLamViecChinhThuc = created.NgayLamViecChinhThuc,
                LoaiHopDong = created.LoaiHopDong,
                SoThangHopDong = created.SoThangHopDong,
                IsDeleted = created.IsDeleted
            };
        }

        public async Task<NhanVienDto?> GetByIdAsync(int id)
        {
            var nv = await _repo.GetByIdAsync(id);
            return nv == null ? null : new NhanVienDto
            {
                Id = nv.Id,
                Ten = nv.Ten,
                Email = nv.Email,
                SoDienThoai = nv.SoDienThoai,
                DiaChi = nv.DiaChi,
                NgayVaoLam = nv.NgayVaoLam,
                NgaySinh = nv.NgaySinh,
                NgayLamViecChinhThuc = nv.NgayLamViecChinhThuc,
                LoaiHopDong = nv.LoaiHopDong,
                SoThangHopDong = nv.SoThangHopDong,
                IsDeleted = nv.IsDeleted
            };
        }

        public async Task<PagedResult<NhanVienDto>> GetPagedAsync(int page, int pageSize, string? ten = null, string? sdt = null, bool? isDeleted = null)
        {
            var paged = await _repo.GetPagedAsync(page, pageSize, ten, sdt, isDeleted);
            return new PagedResult<NhanVienDto>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalItems = paged.TotalItems,
                TotalPages = paged.TotalPages,
                Items = paged.Items.Select(n => new NhanVienDto
                {
                    Id = n.Id,
                    Ten = n.Ten,
                    Email = n.Email,
                    SoDienThoai = n.SoDienThoai,
                    DiaChi = n.DiaChi,
                    NgayVaoLam = n.NgayVaoLam,
                    NgaySinh = n.NgaySinh,
                    NgayLamViecChinhThuc = n.NgayLamViecChinhThuc,
                    LoaiHopDong = n.LoaiHopDong,
                    SoThangHopDong = n.SoThangHopDong,
                    IsDeleted = n.IsDeleted
                })
            };
        }

        public async Task<NhanVienDto> UpdateAsync(UpdateNhanVienDto dto)
        {
            var existing = await _repo.GetByIdAsync(dto.Id);
            if (existing == null) throw new KeyNotFoundException("Không tìm thấy nhân viên");

            int? soThang = dto.LoaiHopDong switch
            {
                "1nam" => 12,
                "vothoihan" => 999,
                "khac" => dto.SoThangHopDong,
                _ => existing.SoThangHopDong // giữ nguyên nếu giá trị không hợp lệ (sẽ hiếm khi xảy ra do validation)
            };
            if (dto.LoaiHopDong == "khac")
            {
                if (!dto.SoThangHopDong.HasValue || dto.SoThangHopDong.Value <= 0)
                    throw new ValidationException("SoThangHopDong phải > 0 khi LoaiHopDong = khac");
            }

            existing.Ten = dto.Ten;
            existing.Email = dto.Email ?? existing.Email;
            existing.SoDienThoai = dto.SoDienThoai ?? existing.SoDienThoai;
            existing.DiaChi = dto.DiaChi ?? existing.DiaChi;
            existing.NgayVaoLam = dto.NgayVaoLam;
            existing.NgaySinh = dto.NgaySinh;
            existing.NgayLamViecChinhThuc = dto.NgayLamViecChinhThuc;
            existing.LoaiHopDong = dto.LoaiHopDong;
            existing.SoThangHopDong = soThang;

            await _repo.UpdateAsync(existing);

            return new NhanVienDto
            {
                Id = existing.Id,
                Ten = existing.Ten,
                Email = existing.Email,
                SoDienThoai = existing.SoDienThoai,
                DiaChi = existing.DiaChi,
                NgayVaoLam = existing.NgayVaoLam,
                NgaySinh = existing.NgaySinh,
                NgayLamViecChinhThuc = existing.NgayLamViecChinhThuc,
                LoaiHopDong = existing.LoaiHopDong,
                SoThangHopDong = existing.SoThangHopDong,
                IsDeleted = existing.IsDeleted
            };
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException("Không tìm thấy nhân viên");
            await _repo.DeleteAsync(existing);
        }

        public async Task<NhanVienDto> RestoreAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException("Không tìm thấy nhân viên");
            if (!existing.IsDeleted) throw new InvalidOperationException("Nhân viên chưa bị xóa");

            await _repo.RestoreAsync(existing);

            return new NhanVienDto
            {
                Id = existing.Id,
                Ten = existing.Ten,
                Email = existing.Email,
                SoDienThoai = existing.SoDienThoai,
                DiaChi = existing.DiaChi,
                NgayVaoLam = existing.NgayVaoLam,
                NgaySinh = existing.NgaySinh,
                NgayLamViecChinhThuc = existing.NgayLamViecChinhThuc,
                LoaiHopDong = existing.LoaiHopDong,
                SoThangHopDong = existing.SoThangHopDong,
                IsDeleted = existing.IsDeleted
            };
        }

        public async Task<ImportNhanVienResultDto> ImportFromExcelAsync(Stream excelStream)
        {
            var result = new ImportNhanVienResultDto();
            
            try
            {
                using var workbook = new XLWorkbook(excelStream);
                var worksheet = workbook.Worksheets.First();
                
                var usedRange = worksheet.RangeUsed();
                if (usedRange == null)
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        Row = 0,
                        Error = "File Excel trống hoặc không có dữ liệu",
                        Data = ""
                    });
                    return result;
                }
                
                var rows = usedRange.RowsUsed().Skip(1); // Skip header row
                result.TotalRows = rows.Count();

                foreach (var row in rows)
                {
                    var rowNumber = row.RowNumber();
                    
                    try
                    {
                        // Đọc dữ liệu từ các cột (thứ tự): Tên, Email, SĐT, Địa chỉ, Ngày vào làm, Ngày sinh, Ngày làm việc chính thức, LoaiHopDong, SoThangHopDong
                        var ten = row.Cell(1).GetString().Trim();
                        var email = row.Cell(2).GetString().Trim();
                        var soDienThoai = row.Cell(3).GetString().Trim();
                        var diaChi = row.Cell(4).GetString().Trim();
                        var ngayVaoLamStr = row.Cell(5).GetString().Trim();
                        var ngaySinhStr = row.Cell(6).GetString().Trim();
                        var ngayLamViecChinhThucStr = row.Cell(7).GetString().Trim();
                        var loaiHopDong = row.Cell(8).GetString().Trim();
                        var soThangHopDongStr = row.Cell(9).GetString().Trim();

                        // Validate required fields - CHỈ TÊN là bắt buộc
                        if (string.IsNullOrEmpty(ten))
                        {
                            result.Errors.Add(new ImportErrorDto
                            {
                                Row = rowNumber,
                                Error = "Tên không được để trống",
                                Data = $"Tên: {ten}"
                            });
                            result.FailedCount++;
                            continue;
                        }

                        // Parse dates với định dạng dd/MM/yyyy
                        var dateFormat = "dd/MM/yyyy";
                        var culture = System.Globalization.CultureInfo.InvariantCulture;
                        
                        if (!DateTime.TryParseExact(ngayVaoLamStr, dateFormat, culture, System.Globalization.DateTimeStyles.None, out var ngayVaoLam))
                        {
                            result.Errors.Add(new ImportErrorDto
                            {
                                Row = rowNumber,
                                Error = "Ngày thử việc không hợp lệ (định dạng yêu cầu: dd/MM/yyyy)",
                                Data = ngayVaoLamStr
                            });
                            result.FailedCount++;
                            continue;
                        }

                        if (!DateTime.TryParseExact(ngaySinhStr, dateFormat, culture, System.Globalization.DateTimeStyles.None, out var ngaySinh))
                        {
                            result.Errors.Add(new ImportErrorDto
                            {
                                Row = rowNumber,
                                Error = "Ngày sinh không hợp lệ (định dạng yêu cầu: dd/MM/yyyy)",
                                Data = ngaySinhStr
                            });
                            result.FailedCount++;
                            continue;
                        }

                        DateTime? ngayLamViecChinhThuc = null;
                        if (!string.IsNullOrEmpty(ngayLamViecChinhThucStr) && 
                            DateTime.TryParseExact(ngayLamViecChinhThucStr, dateFormat, culture, System.Globalization.DateTimeStyles.None, out var parsedNgayLamViecChinhThuc))
                        {
                            ngayLamViecChinhThuc = parsedNgayLamViecChinhThuc;
                        }

                        // Hợp đồng: validate và chuẩn hóa
                        if (string.IsNullOrEmpty(loaiHopDong)) loaiHopDong = "1nam"; // mặc định
                        if (loaiHopDong != "1nam" && loaiHopDong != "vothoihan" && loaiHopDong != "khac")
                        {
                            result.Errors.Add(new ImportErrorDto
                            {
                                Row = rowNumber,
                                Error = "LoaiHopDong không hợp lệ (chỉ: 1nam|vothoihan|khac)",
                                Data = loaiHopDong
                            });
                            result.FailedCount++;
                            continue;
                        }

                        int? soThangHopDong = null;
                        if (loaiHopDong == "khac")
                        {
                            if (string.IsNullOrEmpty(soThangHopDongStr) || !int.TryParse(soThangHopDongStr, out var parsedSoThang) || parsedSoThang <= 0)
                            {
                                result.Errors.Add(new ImportErrorDto
                                {
                                    Row = rowNumber,
                                    Error = "Số tháng hợp đồng phải là số > 0 khi LoaiHopDong = khac",
                                    Data = soThangHopDongStr
                                });
                                result.FailedCount++;
                                continue;
                            }
                            soThangHopDong = parsedSoThang;
                        }

                        // Create employee
                        var createDto = new CreateNhanVienDto
                        {
                            Ten = ten,
                            Email = string.IsNullOrEmpty(email) ? null : email,
                            SoDienThoai = string.IsNullOrEmpty(soDienThoai) ? null : soDienThoai,
                            DiaChi = string.IsNullOrEmpty(diaChi) ? null : diaChi,
                            NgayVaoLam = ngayVaoLam,
                            NgaySinh = ngaySinh,
                            NgayLamViecChinhThuc = ngayLamViecChinhThuc,
                            LoaiHopDong = loaiHopDong,
                            SoThangHopDong = soThangHopDong
                        };

                        var created = await CreateAsync(createDto);
                        result.ImportedEmployees.Add(created);
                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            Row = rowNumber,
                            Error = ex.Message,
                            Data = string.Join(", ", row.Cells().Select(c => c.GetString()))
                        });
                        result.FailedCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ImportErrorDto
                {
                    Row = 0,
                    Error = $"Lỗi đọc file Excel: {ex.Message}",
                    Data = ""
                });
                result.FailedCount = result.TotalRows;
            }

            return result;
        }
    }
}
