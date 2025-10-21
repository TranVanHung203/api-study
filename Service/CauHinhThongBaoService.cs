using Contracts;
using Entities.DTOs;
using Entities.Models;
using Service.Contracts;
using System.Net;

namespace Service
{
    public class CauHinhThongBaoService : ICauHinhThongBaoService
    {
        private readonly ICauHinhThongBaoRepository _cfgRepo;
        private readonly INhanVienRepository _nvRepo;
        private readonly INgayLeRepository _ngayLeRepo;
        private readonly IEmailSenderService _emailSender;
        private readonly IEmailThongBaoRepository _emailThongBaoRepo;
        private readonly IThongBaoRepository _thongBaoRepo;

        public CauHinhThongBaoService(ICauHinhThongBaoRepository cfgRepo,
            INhanVienRepository nvRepo,
            INgayLeRepository ngayLeRepo,
            IEmailSenderService emailSender,
            IThongBaoRepository thongBaoRepo,
            IEmailThongBaoRepository emailThongBaoRepo)
        {
            _cfgRepo = cfgRepo;
            _nvRepo = nvRepo;
            _ngayLeRepo = ngayLeRepo;
            _emailSender = emailSender;
            _thongBaoRepo = thongBaoRepo;
            _emailThongBaoRepo = emailThongBaoRepo;
        }

        public async Task<List<CauHinhThongBaoDto>> GetAllConfigsAsync()
        {
            var list = await _cfgRepo.GetAllAsync();
            return list.Select(c => new CauHinhThongBaoDto
            {
                Id = c.Id,
                SoNgayThongBao = c.SoNgayThongBao,
                DanhSachNamThongBao = c.DanhSachNamThongBao,
                IsActive = c.IsActive,
                ExcludeSaturday = c.ExcludeSaturday,
                ExcludeSunday = c.ExcludeSunday,
                SoNgayThongBaoTruoc = c.SoNgayThongBaoTruoc
            }).ToList();
        }

        public async Task<CauHinhThongBaoDto?> GetConfigByIdAsync(int id)
        {
            var c = await _cfgRepo.GetByIdAsync(id);
            if (c == null) return null;
            return new CauHinhThongBaoDto { Id = c.Id, SoNgayThongBao = c.SoNgayThongBao, DanhSachNamThongBao = c.DanhSachNamThongBao, IsActive = c.IsActive, ExcludeSaturday = c.ExcludeSaturday, ExcludeSunday = c.ExcludeSunday, SoNgayThongBaoTruoc = c.SoNgayThongBaoTruoc };
        }

        public async Task<CauHinhThongBaoDto?> ActivateConfigAsync(int id)
        {
            var cfg = await _cfgRepo.GetByIdAsync(id);
            if (cfg == null) return null;

            cfg.IsActive = true;
            await _cfgRepo.UpdateAsync(cfg);

            return new CauHinhThongBaoDto { Id = cfg.Id, SoNgayThongBao = cfg.SoNgayThongBao, DanhSachNamThongBao = cfg.DanhSachNamThongBao, IsActive = cfg.IsActive, ExcludeSaturday = cfg.ExcludeSaturday, ExcludeSunday = cfg.ExcludeSunday, SoNgayThongBaoTruoc = cfg.SoNgayThongBaoTruoc };
        }

        public async Task<CauHinhThongBaoDto?> GetActiveOnlyAsync()
        {
            // Use repository to fetch only explicitly active record; do NOT fallback to newest
            var all = await _cfgRepo.GetAllAsync();
            var active = all.FirstOrDefault(c => c.IsActive);
            if (active == null) return null;
            return new CauHinhThongBaoDto { Id = active.Id, SoNgayThongBao = active.SoNgayThongBao, DanhSachNamThongBao = active.DanhSachNamThongBao, IsActive = active.IsActive, ExcludeSaturday = active.ExcludeSaturday, ExcludeSunday = active.ExcludeSunday, SoNgayThongBaoTruoc = active.SoNgayThongBaoTruoc };
        }

        public async Task<CauHinhThongBaoDto?> GetConfigAsync()
        {
            var cfg = await _cfgRepo.GetAsync();
            if (cfg == null) return null;
            return new CauHinhThongBaoDto { Id = cfg.Id, SoNgayThongBao = cfg.SoNgayThongBao, DanhSachNamThongBao = cfg.DanhSachNamThongBao, IsActive = cfg.IsActive, ExcludeSaturday = cfg.ExcludeSaturday, ExcludeSunday = cfg.ExcludeSunday, SoNgayThongBaoTruoc = cfg.SoNgayThongBaoTruoc };
        }

        public async Task<CauHinhThongBaoDto> CreateConfigAsync(CreateCauHinhThongBaoDto dto)
        {
            var cfg = new CauHinhThongBao
            {
                SoNgayThongBao = dto.SoNgayThongBao,
                DanhSachNamThongBao = dto.DanhSachNamThongBao,
                IsActive = dto.IsActive,
                ExcludeSaturday = dto.ExcludeSaturday,
                ExcludeSunday = dto.ExcludeSunday,
                SoNgayThongBaoTruoc = dto.SoNgayThongBaoTruoc
            };

            var created = await _cfgRepo.CreateAsync(cfg);
            return new CauHinhThongBaoDto { Id = created.Id, SoNgayThongBao = created.SoNgayThongBao, DanhSachNamThongBao = created.DanhSachNamThongBao, IsActive = created.IsActive, ExcludeSaturday = created.ExcludeSaturday, ExcludeSunday = created.ExcludeSunday, SoNgayThongBaoTruoc = created.SoNgayThongBaoTruoc };
        }

        public async Task<CauHinhThongBaoDto> UpdateConfigAsync(UpdateCauHinhThongBaoDto dto)
        {
            var cfg = await _cfgRepo.GetAsync();
            if (cfg == null)
            {
                cfg = new CauHinhThongBao { SoNgayThongBao = dto.SoNgayThongBao, DanhSachNamThongBao = dto.DanhSachNamThongBao, IsActive = dto.IsActive, ExcludeSaturday = dto.ExcludeSaturday, ExcludeSunday = dto.ExcludeSunday, SoNgayThongBaoTruoc = dto.SoNgayThongBaoTruoc };
                var created = await _cfgRepo.CreateAsync(cfg);
                return new CauHinhThongBaoDto { Id = created.Id, SoNgayThongBao = created.SoNgayThongBao, DanhSachNamThongBao = created.DanhSachNamThongBao, IsActive = created.IsActive, ExcludeSaturday = created.ExcludeSaturday, ExcludeSunday = created.ExcludeSunday, SoNgayThongBaoTruoc = created.SoNgayThongBaoTruoc };
            }

            cfg.SoNgayThongBao = dto.SoNgayThongBao;
            cfg.DanhSachNamThongBao = dto.DanhSachNamThongBao;
            cfg.IsActive = dto.IsActive;
            cfg.ExcludeSaturday = dto.ExcludeSaturday;
            cfg.ExcludeSunday = dto.ExcludeSunday;
            cfg.SoNgayThongBaoTruoc = dto.SoNgayThongBaoTruoc;
            await _cfgRepo.UpdateAsync(cfg);

            return new CauHinhThongBaoDto { Id = cfg.Id, SoNgayThongBao = cfg.SoNgayThongBao, DanhSachNamThongBao = cfg.DanhSachNamThongBao, IsActive = cfg.IsActive, ExcludeSaturday = cfg.ExcludeSaturday, ExcludeSunday = cfg.ExcludeSunday, SoNgayThongBaoTruoc = cfg.SoNgayThongBaoTruoc };
        }
        public async Task DeleteConfigAsync(int id)
        {
            var entity = await _cfgRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("Không tìm thấy cấu hình thông báo.");

            await _cfgRepo.DeleteAsync(entity);
        }

        public async Task<int> RunCheckAndSendAsync()
        {
            // Load config
            var cfg = await _cfgRepo.GetAsync();
            int soNgay = cfg?.SoNgayThongBao ?? 60;
            int soNgayThongBaoTruoc = cfg?.SoNgayThongBaoTruoc ?? 30;

            // Load employees - Lấy nhân viên có loại hợp đồng "1nam" HOẶC "khac"
            var allEmployees = await _nvRepo.GetAllAsync();
            var employees = allEmployees.Where(nv => nv.LoaiHopDong == "1nam" || nv.LoaiHopDong == "khac").ToList();

            // Load holidays (chỉ để loại trừ ngày lễ, không loại trừ cuối tuần)
            var holidaysPaged = await _ngayLeRepo.GetPagedAsync(1, int.MaxValue);
            var holidays = holidaysPaged.Items.SelectMany(n =>
                Enumerable.Range(0, (int)(n.NgayKetThuc.Date - n.NgayBatDau.Date).TotalDays + 1)
                          .Select(i => n.NgayBatDau.Date.AddDays(i))).ToHashSet();

            var toNotify = new List<(int NhanVienId, string Email, string Reason)>();

            var utcNow = DateTime.UtcNow.Date;

            foreach (var nv in employees)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(nv.Email)) continue;

                    bool contractHandled = false;

                    // Xử lý thông báo hợp đồng dựa vào loại hợp đồng
                    if (nv.NgayLamViecChinhThuc.HasValue)
                    {
                        var officialDate = nv.NgayLamViecChinhThuc.Value.Date;

                        if (nv.LoaiHopDong == "1nam")
                        {
                            // Hợp đồng 1 năm: Thông báo MỌI năm (năm 1, 2, 3, 4,...)
                            int yearNum = 1;
                            while (true)
                            {
                                var anniversaryDate = officialDate.AddYears(yearNum);
                                var notificationDate = anniversaryDate.AddDays(-soNgayThongBaoTruoc);

                                // Nếu notificationDate chưa đến thì dừng loop
                                if (notificationDate > utcNow) break;

                                // Thông báo trong khoảng [notificationDate, anniversaryDate)
                                if (utcNow >= notificationDate && utcNow < anniversaryDate)
                                {
                                    var daysLeft = soNgayThongBaoTruoc;
                                    var reason = $"Ngày kí hợp đồng chính thức: {officialDate:dd/MM/yyyy}. Còn {daysLeft} ngày nữa là hết hạn hợp đồng năm lần {yearNum}";
                                    toNotify.Add((nv.Id, nv.Email, reason));
                                    contractHandled = true;
                                }
                                else if (soNgayThongBaoTruoc == 0 && utcNow == anniversaryDate)
                                {
                                    var reason = $"Ngày kí hợp đồng chính thức: {officialDate:dd/MM/yyyy}. Hôm nay hết hạn hợp đồng năm lần {yearNum}";
                                    toNotify.Add((nv.Id, nv.Email, reason));
                                    contractHandled = true;
                                }

                                yearNum++;
                            }
                        }
                        else if (nv.LoaiHopDong == "khac" && nv.SoThangHopDong.HasValue && nv.SoThangHopDong.Value > 0)
                        {
                            // Hợp đồng khác: Thông báo theo chu kỳ SoThangHopDong (lần 1, 2, 3,...)
                            int cycleNum = 1;
                            while (true)
                            {
                                var cycleEndDate = officialDate.AddMonths(nv.SoThangHopDong.Value * cycleNum);
                                var notificationDate = cycleEndDate.AddDays(-soNgayThongBaoTruoc);

                                // Nếu notificationDate chưa đến thì dừng loop
                                if (notificationDate > utcNow) break;

                                // Thông báo trong khoảng [notificationDate, cycleEndDate)
                                if (utcNow >= notificationDate && utcNow < cycleEndDate)
                                {
                                    var daysLeft = soNgayThongBaoTruoc;
                                    var reason = $"Ngày kí hợp đồng chính thức: {officialDate:dd/MM/yyyy}. Còn {daysLeft} ngày nữa là hết hạn hợp đồng lần {cycleNum}";
                                    toNotify.Add((nv.Id, nv.Email, reason));
                                    contractHandled = true;
                                }
                                else if (soNgayThongBaoTruoc == 0 && utcNow == cycleEndDate)
                                {
                                    var reason = $"Ngày kí hợp đồng chính thức: {officialDate:dd/MM/yyyy}. Hôm nay hết hạn hợp đồng lần {cycleNum}";
                                    toNotify.Add((nv.Id, nv.Email, reason));
                                    contractHandled = true;
                                }

                                cycleNum++;
                            }
                        }
                    }

                    

                    // Probation logic: Tính tổng số ngày (bao gồm cả cuối tuần, chỉ loại trừ ngày lễ)
                    // Áp dụng cho cả "1nam" và "khac"
                    if (nv.NgayVaoLam.HasValue)
                    {
                        var join = nv.NgayVaoLam.Value.Date;
                        var totalDays = CountTotalDays(join, utcNow, holidays);
                        if (totalDays >= soNgay)
                        {
                            var reason = $"Đủ {soNgay} ngày thử việc";
                            toNotify.Add((nv.Id, nv.Email, reason));
                        }
                    }

                    if (contractHandled) continue;
                }
                catch
                {
                    // swallow per-user errors to continue processing others
                }
            }

            // Send emails and log
            if (!toNotify.Any()) return 0;

            // Define the HTML email template with inline CSS and logo
            var template = @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Báo cáo thông báo nhân sự</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, Helvetica, sans-serif;"">
    <table role=""presentation"" style=""width: 100%; max-width: 600px; margin: 20px auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
        <tr>
            <td style=""padding: 20px; text-align: center; background-color: #1e3a8a; border-top-left-radius: 8px; border-top-right-radius: 8px;"">
                <img src=""https://atpro.com.vn/wp-content/uploads/2020/10/logo-cong-ty-1024x342-1024x342.png"" alt=""Company Logo"" style=""max-width: 150px; height: auto; margin-bottom: 10px; display: block; margin-left: auto; margin-right: auto;"">
                <h1 style=""color: #ffffff; font-size: 24px; margin: 0;"">Báo cáo thông báo nhân sự</h1>
                <p style=""color: #e5e7eb; font-size: 14px; margin: 5px 0;"">Ngày gửi: {0:dd/MM/yyyy}</p>
            </td>
        </tr>
        <tr>
            <td style=""padding: 20px;"">
                <table role=""presentation"" style=""width: 100%; border-collapse: collapse;"">
                    <thead>
                        <tr style=""background-color: #3b82f6; color: #ffffff;"">
                            <th style=""padding: 12px; text-align: left; font-size: 14px; border: 1px solid #d1d5db;"">Id</th>
                            <th style=""padding: 12px; text-align: left; font-size: 14px; border: 1px solid #d1d5db;"">Họ tên</th>
                            <th style=""padding: 12px; text-align: left; font-size: 14px; border: 1px solid #d1d5db;"">Email NV</th>
                            <th style=""padding: 12px; text-align: left; font-size: 14px; border: 1px solid #d1d5db;"">Lý do</th>
                        </tr>
                    </thead>
                    <tbody>
                        {1}
                    </tbody>
                </table>
            </td>
        </tr>
        <tr>
            <td style=""padding: 20px; text-align: center; background-color: #f9fafb; border-bottom-left-radius: 8px; border-bottom-right-radius: 8px;"">
                <p style=""color: #4b5563; font-size: 12px; margin: 0;"">Đây là email tự động từ hệ thống nhân sự. Vui lòng không trả lời trực tiếp email này.</p>
            </td>
        </tr>
    </table>
</body>
</html>";

            // Generate table rows with alternating background colors
            var sb = new System.Text.StringBuilder();
            var nvMap = employees.ToDictionary(e => e.Id);
            int rowIndex = 0;
            var cfgEmailsPaged = await _emailThongBaoRepo.GetPagedAsync(1, int.MaxValue);
            var recipients = cfgEmailsPaged.Items.Select(e => e.Email).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            // Tạo một danh sách để theo dõi các nhân viên đã được gửi thông báo
            var notifiedEmployees = new HashSet<(int NhanVienId, string Reason)>();


            foreach (var to in recipients)
            {
                foreach (var item in toNotify)
                {
                    var key = (item.NhanVienId, (item.Reason ?? string.Empty).Trim());

                    // Kiểm tra xem nhân viên này + lý do đã được thêm vào bảng chưa
                    if (notifiedEmployees.Contains(key))
                    {
                        continue; // Bỏ qua nếu đã thêm cùng id + lý do trước đó
                    }

                    // Kiểm tra xem thông báo đã được gửi cho nhân viên này với lý do và recipient cụ thể chưa
                    var alreadySentForRecipient = await _thongBaoRepo.ExistsForNhanVienWithReasonAsync(item.NhanVienId, item.Reason, to);
                    if (!alreadySentForRecipient)
                    {
                        nvMap.TryGetValue(item.NhanVienId, out var emp);
                        var name = emp?.Ten ?? "-";
                        var emailNv = item.Email ?? "-";
                        var rowStyle = rowIndex % 2 == 0 ? "background-color: #f9fafb;" : "";
                        sb.AppendLine($"<tr style=\"{rowStyle}\"><td style=\"padding: 12px; border: 1px solid #d1d5db;\">{item.NhanVienId}</td><td style=\"padding: 12px; border: 1px solid #d1d5db;\">{System.Net.WebUtility.HtmlEncode(name)}</td><td style=\"padding: 12px; border: 1px solid #d1d5db;\">{System.Net.WebUtility.HtmlEncode(emailNv)}</td><td style=\"padding: 12px; border: 1px solid #d1d5db;\">{System.Net.WebUtility.HtmlEncode(item.Reason)}</td></tr>");
                        rowIndex++;
                        // Đánh dấu đã thêm (id + lý do)
                        notifiedEmployees.Add(key);
                    }
                }
            }

            // Format the HTML body with the current date and table rows
            var htmlBody = string.Format(template, DateTime.UtcNow, sb.ToString());

           

            if (!recipients.Any())
            {
                return 0;
            }

            byte[] excelBytes;
            using (var wb = new ClosedXML.Excel.XLWorkbook())
            {
                var ws = wb.Worksheets.Add("ThongBao");
                ws.Cell(1, 1).Value = "Id";
                ws.Cell(1, 2).Value = "Ten";
                ws.Cell(1, 3).Value = "EmailNV";
                ws.Cell(1, 4).Value = "LyDo";
                int r = 2;
                // Tạo một HashSet để theo dõi các nhân viên đã được thêm vào bảng
                var addedEmployees = new HashSet<(int NhanVienId, string Reason)>();

                foreach (var to in recipients)
                {
                    foreach (var item in toNotify)
                    {
                        var key = (item.NhanVienId, (item.Reason ?? string.Empty).Trim());

                        if (addedEmployees.Contains(key))
                        {
                            continue;
                        }

                        var alreadySentForRecipient = await _thongBaoRepo.ExistsForNhanVienWithReasonAsync(item.NhanVienId, item.Reason, to);
                        if (!alreadySentForRecipient)
                        {
                            var emp = employees.FirstOrDefault(e => e.Id == item.NhanVienId);
                            ws.Cell(r, 1).Value = item.NhanVienId;
                            ws.Cell(r, 2).Value = emp?.Ten ?? "-";
                            ws.Cell(r, 3).Value = item.Email ?? "-";
                            ws.Cell(r, 4).Value = item.Reason;
                            r++;
                            addedEmployees.Add(key);
                        }
                    }
                }

                ws.Columns().AdjustToContents();

                using (var ms = new System.IO.MemoryStream())
                {
                    wb.SaveAs(ms);
                    excelBytes = ms.ToArray();
                }
            }

            int sent = 0;
            var subject = "Báo cáo thông báo nhân sự";
            var attachments = new List<(string FileName, byte[] Content)> { ("ThongBao.xlsx", excelBytes) };

            foreach (var to in recipients)
            {
                try
                {
                    // For each recipient, ensure we don't duplicate sends for the same employee/reason/email
                    var itemsForThisRecipient = new List<(int NhanVienId, string Email, string Reason)>();
                    foreach (var item in toNotify)
                    {
                        var alreadySentForRecipient = await _thongBaoRepo.ExistsForNhanVienWithReasonAsync(item.NhanVienId, item.Reason, to);
                        if (!alreadySentForRecipient)
                        {
                            itemsForThisRecipient.Add(item);
                        }
                    }

                    if (!itemsForThisRecipient.Any()) continue;

                    // Send email with the formatted HTML body
                    await _emailSender.SendEmailAsync(to, subject, htmlBody, attachments);

                    foreach (var item in itemsForThisRecipient)
                    {
                        await _thongBaoRepo.CreateAsync(new ThongBao
                        {
                            NhanVienId = item.NhanVienId,
                            EmailNhan = to,
                            NgayGui = DateTime.UtcNow,
                            LyDo = item.Reason
                        });
                        sent++;
                    }
                }
                catch
                {
                    // continue to next recipient
                }
            }

            return sent;
        }
        private int CountWorkingDays(DateTime startDate, DateTime endDate, HashSet<DateTime> holidays, bool excludeSaturday, bool excludeSunday)
        {
            if (endDate <= startDate) return 0; // chưa qua ngày nào thì = 0

            int count = 0;
            for (var d = startDate.Date.AddDays(1); d <= endDate.Date; d = d.AddDays(1))
            {
                if (d.DayOfWeek == DayOfWeek.Saturday && excludeSaturday) continue;
                if (d.DayOfWeek == DayOfWeek.Sunday && excludeSunday) continue;
                if (holidays.Contains(d.Date)) continue;
                count++;
            }

            return count;
        }

        private int CountTotalDays(DateTime startDate, DateTime endDate, HashSet<DateTime> holidays)
        {
            if (endDate <= startDate) return 0; // chưa qua ngày nào thì = 0

            int count = 0;
            for (var d = startDate.Date.AddDays(1); d <= endDate.Date; d = d.AddDays(1))
            {
                // Chỉ loại trừ ngày lễ, không loại trừ cuối tuần
                if (holidays.Contains(d.Date)) continue;
                count++;
            }

            return count;
        }

        public async Task<int> RunBirthdayCheckAndSendAsync()
        {
            // Load employees
            var employees = await _nvRepo.GetAllAsync();
            
            var utcNow = DateTime.UtcNow.Date;
            var currentMonth = utcNow.Month;
            var currentYear = utcNow.Year;

            // Lấy danh sách nhân viên có sinh nhật trong tháng hiện tại
            var birthdayEmployees = employees.Where(nv => 
                !string.IsNullOrWhiteSpace(nv.Email) && 
                nv.NgaySinh.Month == currentMonth)
                .Select(nv => new
                {
                    nv.Id,
                    nv.Ten,
                    nv.Email,
                    NgaySinh = nv.NgaySinh,
                    Age = currentYear - nv.NgaySinh.Year
                })
                .OrderBy(x => x.NgaySinh.Day)
                .ToList();

            if (!birthdayEmployees.Any()) return 0;

            // Lấy danh sách email để gửi thông báo
            var cfgEmailsPaged = await _emailThongBaoRepo.GetPagedAsync(1, int.MaxValue);
            var recipients = cfgEmailsPaged.Items.Select(e => e.Email).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            if (!recipients.Any()) return 0;

            // Tạo template HTML cho email sinh nhật
            var template = @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Báo cáo sinh nhật nhân viên tháng {2}</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, Helvetica, sans-serif;"">
    <table role=""presentation"" style=""width: 100%; max-width: 600px; margin: 20px auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
        <tr>
            <td style=""padding: 20px; text-align: center; background-color: #ff6b6b; border-top-left-radius: 8px; border-top-right-radius: 8px;"">
                <img src=""https://atpro.com.vn/wp-content/uploads/2020/10/logo-cong-ty-1024x342-1024x342.png"" alt=""Company Logo"" style=""max-width: 150px; height: auto; margin-bottom: 10px; display: block; margin-left: auto; margin-right: auto;"">
                <h1 style=""color: #ffffff; font-size: 24px; margin: 0;"">🎉 Sinh nhật nhân viên tháng {2}/{3} 🎂</h1>
                <p style=""color: #fff3cd; font-size: 14px; margin: 5px 0;"">Ngày gửi: {0:dd/MM/yyyy}</p>
            </td>
        </tr>
        <tr>
            <td style=""padding: 20px;"">
                <p style=""font-size: 16px; color: #333; text-align: center; margin-bottom: 20px;"">
                    Chúc mừng sinh nhật các nhân viên có sinh nhật trong tháng này! 🎈
                </p>
                <table role=""presentation"" style=""width: 100%; border-collapse: collapse;"">
                    <thead>
                        <tr style=""background-color: #ff6b6b; color: #ffffff;"">
                            <th style=""padding: 12px; text-align: left; font-size: 14px; border: 1px solid #d1d5db;"">Họ tên</th>
                            <th style=""padding: 12px; text-align: left; font-size: 14px; border: 1px solid #d1d5db;"">Ngày sinh</th>
                            <th style=""padding: 12px; text-align: left; font-size: 14px; border: 1px solid #d1d5db;"">Tuổi</th>
                            <th style=""padding: 12px; text-align: left; font-size: 14px; border: 1px solid #d1d5db;"">Email</th>
                        </tr>
                    </thead>
                    <tbody>
                        {1}
                    </tbody>
                </table>
            </td>
        </tr>
        <tr>
            <td style=""padding: 20px; text-align: center; background-color: #f9fafb; border-bottom-left-radius: 8px; border-bottom-right-radius: 8px;"">
                <p style=""color: #4b5563; font-size: 12px; margin: 0;"">Đây là email tự động từ hệ thống nhân sự. Vui lòng không trả lời trực tiếp email này.</p>
            </td>
        </tr>
    </table>
</body>
</html>";

            // Generate table rows
            var sb = new System.Text.StringBuilder();
            int rowIndex = 0;
            foreach (var emp in birthdayEmployees)
            {
                var rowStyle = rowIndex % 2 == 0 ? "background-color: #fff5f5;" : "background-color: #fef2f2;";
                sb.AppendLine($"<tr style=\"{rowStyle}\">");
                sb.AppendLine($"<td style=\"padding: 12px; border: 1px solid #d1d5db;\">{System.Net.WebUtility.HtmlEncode(emp.Ten)}</td>");
                sb.AppendLine($"<td style=\"padding: 12px; border: 1px solid #d1d5db;\">{emp.NgaySinh:dd/MM/yyyy}</td>");
                sb.AppendLine($"<td style=\"padding: 12px; border: 1px solid #d1d5db;\">{emp.Age} tuổi</td>");
                sb.AppendLine($"<td style=\"padding: 12px; border: 1px solid #d1d5db;\">{System.Net.WebUtility.HtmlEncode(emp.Email)}</td>");
                sb.AppendLine("</tr>");
                rowIndex++;
            }

            // Format HTML body
            var htmlBody = string.Format(template, DateTime.UtcNow, sb.ToString(), currentMonth, currentYear);

            // Create Excel file
            byte[] excelBytes;
            using (var wb = new ClosedXML.Excel.XLWorkbook())
            {
                var ws = wb.Worksheets.Add("SinhNhat");
                ws.Cell(1, 1).Value = "Họ tên";
                ws.Cell(1, 2).Value = "Ngày sinh";
                ws.Cell(1, 3).Value = "Tuổi";
                ws.Cell(1, 4).Value = "Email";
                
                int row = 2;
                foreach (var emp in birthdayEmployees)
                {
                    ws.Cell(row, 1).Value = emp.Ten;
                    ws.Cell(row, 2).Value = emp.NgaySinh.ToString("dd/MM/yyyy");
                    ws.Cell(row, 3).Value = emp.Age;
                    ws.Cell(row, 4).Value = emp.Email;
                    row++;
                }

                ws.Columns().AdjustToContents();

                using (var ms = new System.IO.MemoryStream())
                {
                    wb.SaveAs(ms);
                    excelBytes = ms.ToArray();
                }
            }

            // Send emails
            int sent = 0;
            var subject = $"🎉 Sinh nhật nhân viên tháng {currentMonth}/{currentYear}";
            var attachments = new List<(string FileName, byte[] Content)> { ($"SinhNhat_T{currentMonth}_{currentYear}.xlsx", excelBytes) };

            foreach (var to in recipients)
            {
                try
                {
                    await _emailSender.SendEmailAsync(to, subject, htmlBody, attachments);
                    sent++;
                }
                catch
                {
                    // continue to next recipient
                }
            }

            return sent * birthdayEmployees.Count;
        }

    }
}
