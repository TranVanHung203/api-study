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
                IsActive = c.IsActive
                ,
                ExcludeSaturday = c.ExcludeSaturday,
                ExcludeSunday = c.ExcludeSunday
            }).ToList();
        }

        public async Task<CauHinhThongBaoDto?> GetConfigByIdAsync(int id)
        {
            var c = await _cfgRepo.GetByIdAsync(id);
            if (c == null) return null;
            return new CauHinhThongBaoDto { Id = c.Id, SoNgayThongBao = c.SoNgayThongBao, DanhSachNamThongBao = c.DanhSachNamThongBao, IsActive = c.IsActive, ExcludeSaturday = c.ExcludeSaturday, ExcludeSunday = c.ExcludeSunday };
        }

        public async Task<CauHinhThongBaoDto?> ActivateConfigAsync(int id)
        {
            var cfg = await _cfgRepo.GetByIdAsync(id);
            if (cfg == null) return null;

            cfg.IsActive = true;
            await _cfgRepo.UpdateAsync(cfg);

            return new CauHinhThongBaoDto { Id = cfg.Id, SoNgayThongBao = cfg.SoNgayThongBao, DanhSachNamThongBao = cfg.DanhSachNamThongBao, IsActive = cfg.IsActive };
        }

        public async Task<CauHinhThongBaoDto?> GetActiveOnlyAsync()
        {
            // Use repository to fetch only explicitly active record; do NOT fallback to newest
            var all = await _cfgRepo.GetAllAsync();
            var active = all.FirstOrDefault(c => c.IsActive);
            if (active == null) return null;
            return new CauHinhThongBaoDto { Id = active.Id, SoNgayThongBao = active.SoNgayThongBao, DanhSachNamThongBao = active.DanhSachNamThongBao, IsActive = active.IsActive, ExcludeSaturday = active.ExcludeSaturday, ExcludeSunday = active.ExcludeSunday };
        }

        public async Task<CauHinhThongBaoDto?> GetConfigAsync()
        {
            var cfg = await _cfgRepo.GetAsync();
            if (cfg == null) return null;
            return new CauHinhThongBaoDto { Id = cfg.Id, SoNgayThongBao = cfg.SoNgayThongBao, ExcludeSaturday = cfg.ExcludeSaturday, ExcludeSunday = cfg.ExcludeSunday };
        }

        public async Task<CauHinhThongBaoDto> CreateConfigAsync(CreateCauHinhThongBaoDto dto)
        {
            var cfg = new CauHinhThongBao
            {
                SoNgayThongBao = dto.SoNgayThongBao,
                DanhSachNamThongBao = dto.DanhSachNamThongBao
                ,
                IsActive = dto.IsActive
                ,
                ExcludeSaturday = dto.ExcludeSaturday,
                ExcludeSunday = dto.ExcludeSunday
            };

            var created = await _cfgRepo.CreateAsync(cfg);
            return new CauHinhThongBaoDto { Id = created.Id, SoNgayThongBao = created.SoNgayThongBao, DanhSachNamThongBao = created.DanhSachNamThongBao, IsActive = created.IsActive, ExcludeSaturday = created.ExcludeSaturday, ExcludeSunday = created.ExcludeSunday };
        }

        public async Task<CauHinhThongBaoDto> UpdateConfigAsync(UpdateCauHinhThongBaoDto dto)
        {
            var cfg = await _cfgRepo.GetAsync();
            if (cfg == null)
            {
                cfg = new CauHinhThongBao { SoNgayThongBao = dto.SoNgayThongBao, DanhSachNamThongBao = dto.DanhSachNamThongBao, IsActive = dto.IsActive, ExcludeSaturday = dto.ExcludeSaturday, ExcludeSunday = dto.ExcludeSunday };
                var created = await _cfgRepo.CreateAsync(cfg);
                return new CauHinhThongBaoDto { Id = created.Id, SoNgayThongBao = created.SoNgayThongBao, DanhSachNamThongBao = created.DanhSachNamThongBao, IsActive = created.IsActive, ExcludeSaturday = created.ExcludeSaturday, ExcludeSunday = created.ExcludeSunday };
            }

            cfg.SoNgayThongBao = dto.SoNgayThongBao;
            cfg.DanhSachNamThongBao = dto.DanhSachNamThongBao;
            cfg.IsActive = dto.IsActive;
            cfg.ExcludeSaturday = dto.ExcludeSaturday;
            cfg.ExcludeSunday = dto.ExcludeSunday;
            await _cfgRepo.UpdateAsync(cfg);

            return new CauHinhThongBaoDto { Id = cfg.Id, SoNgayThongBao = cfg.SoNgayThongBao, DanhSachNamThongBao = cfg.DanhSachNamThongBao, IsActive = cfg.IsActive, ExcludeSaturday = cfg.ExcludeSaturday, ExcludeSunday = cfg.ExcludeSunday };
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

            // Load employees
            var employees = await _nvRepo.GetAllAsync();

            // Load holidays
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

                    //// Avoid sending if already sent today
                    //var alreadySent = await _thongBaoRepo.ExistsForNhanVienOnDateAsync(nv.Id, utcNow);
                    //if (alreadySent) continue;

                    var join = nv.NgayVaoLam.Date;

                    // Anniversary years from config (e.g. "1,2,3")
                    var years = new List<int>();
                    if (!string.IsNullOrWhiteSpace(cfg?.DanhSachNamThongBao))
                    {
                        years = cfg.DanhSachNamThongBao.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => { if (int.TryParse(s.Trim(), out var v)) return v; return 0; })
                            .Where(v => v > 0).ToList();
                    }

                    // Check anniversary years
                    bool anniversaryHandled = false;
                    var reasons = new List<string>();

                    foreach (var y in years)
                    {
                        var anniversaryDate = join.AddYears(y);
                        if (utcNow >= anniversaryDate)
                        {
                            reasons.Add($"Kỷ niệm {y} năm làm việc");
                        }
                    }

                    foreach (var reason in reasons)
                    {
                        toNotify.Add((nv.Id, nv.Email, reason));
                        anniversaryHandled = true;
                    }

                    if (anniversaryHandled) continue;

                    // Probation logic: measure working days (exclude Saturdays and holidays)
                    var workingDays = CountWorkingDays(join, utcNow, holidays, cfg?.ExcludeSaturday ?? true, cfg?.ExcludeSunday ?? true);
                    if (workingDays >= soNgay)
                    {
                        var reason = $"Đã đủ {soNgay} ngày làm việc (thử việc)";
                        // Tentatively add; final duplicate suppression will be done per recipient when sending
                        toNotify.Add((nv.Id, nv.Email, reason));
                    }
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
            var notifiedEmployees = new HashSet<int>();

            foreach (var to in recipients)
            {
                foreach (var item in toNotify)
                {
                    // Kiểm tra xem nhân viên này đã được thêm vào bảng chưa
                    if (notifiedEmployees.Contains(item.NhanVienId))
                    {
                        continue; // Bỏ qua nếu nhân viên đã được thêm
                    }

                    // Kiểm tra xem thông báo đã được gửi cho nhân viên này với lý do và recipient cụ thể chưa
                    var alreadySentForRecipient = await _thongBaoRepo.ExistsForNhanVienWithReasonAsync(item.NhanVienId, item.Reason, to);
                    if (!alreadySentForRecipient)
                    {
                        // Nếu chưa gửi, thêm vào bảng
                        nvMap.TryGetValue(item.NhanVienId, out var emp);
                        var name = emp?.Ten ?? "-";
                        var emailNv = item.Email ?? "-";
                        var rowStyle = rowIndex % 2 == 0 ? "background-color: #f9fafb;" : "";
                        sb.AppendLine($"<tr style=\"{rowStyle}\"><td style=\"padding: 12px; border: 1px solid #d1d5db;\">{item.NhanVienId}</td><td style=\"padding: 12px; border: 1px solid #d1d5db;\">{System.Net.WebUtility.HtmlEncode(name)}</td><td style=\"padding: 12px; border: 1px solid #d1d5db;\">{System.Net.WebUtility.HtmlEncode(emailNv)}</td><td style=\"padding: 12px; border: 1px solid #d1d5db;\">{System.Net.WebUtility.HtmlEncode(item.Reason)}</td></tr>");
                        rowIndex++;
                        // Đánh dấu nhân viên này đã được thêm vào bảng
                        notifiedEmployees.Add(item.NhanVienId);
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
                var addedEmployees = new HashSet<int>();

                foreach (var to in recipients)
                {
                    foreach (var item in toNotify)
                    {
                        // Kiểm tra xem nhân viên này đã được thêm vào bảng chưa
                        if (addedEmployees.Contains(item.NhanVienId))
                        {
                            continue; // Bỏ qua nếu nhân viên đã được thêm
                        }

                        // Kiểm tra xem thông báo đã được gửi cho nhân viên này với lý do và recipient cụ thể chưa
                        var alreadySentForRecipient = await _thongBaoRepo.ExistsForNhanVienWithReasonAsync(item.NhanVienId, item.Reason, to);
                        if (!alreadySentForRecipient)
                        {
                            // Nếu chưa gửi, thêm vào bảng
                            var emp = employees.FirstOrDefault(e => e.Id == item.NhanVienId);
                            ws.Cell(r, 1).Value = item.NhanVienId;
                            ws.Cell(r, 2).Value = emp?.Ten ?? "-";
                            ws.Cell(r, 3).Value = item.Email ?? "-";
                            ws.Cell(r, 4).Value = item.Reason;
                            r++;
                            // Đánh dấu nhân viên này đã được thêm vào bảng
                            addedEmployees.Add(item.NhanVienId);
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

    }
}
