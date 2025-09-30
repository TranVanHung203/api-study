from ..services.email_sender import send_email
from ..db.session import get_db
from ..repos.email_repo import EmailRepo
from ..services.cauhinh_service import get_active_config, is_within_notice, count_working_days
from ..repos.ngayle_repo import NgayLeRepo
from datetime import datetime, timedelta
from ..repos.nhanvien_repo import NhanVienRepo
from ..repos.thongbao_repo import ThongBaoRepo
from ..models.thongbao import ThongBao
from ..utils.excel import create_thongbao_excel


async def run_daily_checks():
    async for db in get_db():
        cfg = await get_active_config(db)
        if not cfg:
            return

        # build holidays set by expanding NgayLe ranges (ngay_bat_dau..ngay_ket_thuc)
        nrepo = NgayLeRepo()
        nholidays = await nrepo.get_all(db)
        holidays = set()
        for h in nholidays:
            try:
                sd = h.ngay_bat_dau.date() if hasattr(h.ngay_bat_dau, 'date') else h.ngay_bat_dau
                ed = h.ngay_ket_thuc.date() if hasattr(h.ngay_ket_thuc, 'date') else h.ngay_ket_thuc
                cur = sd
                while cur <= ed:
                    holidays.add(cur)
                    cur = cur + timedelta(days=1)
            except Exception:
                continue

        # load employees and email recipients
        nvrepo = NhanVienRepo()
        employees = await nvrepo.get_all(db)

        email_repo = EmailRepo()
        cfg_emails = await email_repo.get_paged(db, 1, 1000)
        recipients = [e.email for e in cfg_emails.get('items', [])]

        to_notify: list[tuple[int,str,str]] = []  # (NhanVienId, Email, Reason)
        utc_now = datetime.utcnow().date()

        # build notification candidates
        years_list = []
        if cfg.danh_sach_nam_thong_bao:
            try:
                years_list = [int(s.strip()) for s in cfg.danh_sach_nam_thong_bao.split(',') if s.strip().isdigit()]
            except Exception:
                years_list = []

        for nv in employees:
            try:
                if not nv.email: continue

                join_date = nv.ngay_sinh or None
                # Anniversary logic (if join_date is present use ngay_sinh fallback; original C# used NgayVaoLam - adjust if available)
                if getattr(nv, 'ngay_vao_lam', None):
                    join_date = nv.ngay_vao_lam

                # Anniversary checks
                handled = False
                reasons = []
                if join_date and years_list:
                    for y in years_list:
                        try:
                            anniversary = join_date.replace(year=(join_date.year + y))
                            if utc_now >= anniversary.date():
                                reasons.append(f"Kỷ niệm {y} năm làm việc")
                        except Exception:
                            continue

                if reasons:
                    for r in reasons:
                        to_notify.append((nv.id, nv.email, r))
                    handled = True
                if handled:
                    continue

                # Probation / working days logic
                # Count working days from join_date to utc_now
                if join_date:
                    from ..services.cauhinh_service import count_working_days
                    wd = count_working_days(join_date, datetime.utcnow(), holidays, cfg.exclude_saturday, cfg.exclude_sunday)
                    so_ngay = cfg.so_ngay_thong_bao or 60
                    if wd >= so_ngay:
                        reason = f"Đã đủ {so_ngay} ngày làm việc (thử việc)"
                        to_notify.append((nv.id, nv.email, reason))
            except Exception:
                continue

        if not to_notify:
            return 0

        # Build HTML table rows and Excel based on unique employees to be notified
        nv_map = {e.id: e for e in employees}
        row_html = []
        added_employees = set()
        for item in to_notify:
            if item[0] in added_employees:
                continue
            emp = nv_map.get(item[0])
            name = getattr(emp, 'ho_ten', '-') if emp else '-'
            email_nv = item[1] or '-'
            row_html.append((item[0], name, email_nv, item[2]))
            added_employees.add(item[0])

        # build HTML
        template = """<!DOCTYPE html>
<html lang="en">
<head><meta charset="UTF-8"><meta name="viewport" content="width=device-width, initial-scale=1.0"><title>Báo cáo thông báo nhân sự</title></head>
<body style="margin:0;padding:0;background-color:#f4f4f4;font-family:Arial,Helvetica,sans-serif;">
<table role="presentation" style="width:100%;max-width:600px;margin:20px auto;background-color:#ffffff;border-radius:8px;box-shadow:0 2px 4px rgba(0,0,0,0.1);">
<tr><td style="padding:20px;text-align:center;background-color:#1e3a8a;border-top-left-radius:8px;border-top-right-radius:8px;"><img src="https://atpro.com.vn/wp-content/uploads/2020/10/logo-cong-ty-1024x342-1024x342.png" alt="Company Logo" style="max-width:150px;height:auto;margin-bottom:10px;display:block;margin-left:auto;margin-right:auto;"><h1 style="color:#ffffff;font-size:24px;margin:0;">Báo cáo thông báo nhân sự</h1><p style="color:#e5e7eb;font-size:14px;margin:5px 0;">Ngày gửi: {date}</p></td></tr>
<tr><td style="padding:20px;"><table role="presentation" style="width:100%;border-collapse:collapse;"><thead><tr style="background-color:#3b82f6;color:#ffffff;"><th style="padding:12px;text-align:left;font-size:14px;border:1px solid #d1d5db;">Id</th><th style="padding:12px;text-align:left;font-size:14px;border:1px solid #d1d5db;">Họ tên</th><th style="padding:12px;text-align:left;font-size:14px;border:1px solid #d1d5db;">Email NV</th><th style="padding:12px;text-align:left;font-size:14px;border:1px solid #d1d5db;">Lý do</th></tr></thead><tbody>{rows}</tbody></table></td></tr>
<tr><td style="padding:20px;text-align:center;background-color:#f9fafb;border-bottom-left-radius:8px;border-bottom-right-radius:8px;"><p style="color:#4b5563;font-size:12px;margin:0;">Đây là email tự động từ hệ thống nhân sự. Vui lòng không trả lời trực tiếp email này.</p></td></tr></table></body></html>"""

        rows_html_str = ""
        for idx, (id_, name, email_nv, reason) in enumerate(row_html):
            style = "background-color:#f9fafb;" if idx % 2 == 0 else ""
            rows_html_str += f"<tr style='{style}'><td style='padding:12px;border:1px solid #d1d5db;'>{id_}</td><td style='padding:12px;border:1px solid #d1d5db;'>{name}</td><td style='padding:12px;border:1px solid #d1d5db;'>{email_nv}</td><td style='padding:12px;border:1px solid #d1d5db;'>{reason}</td></tr>"

        html_body = template.format(date=datetime.utcnow().strftime('%d/%m/%Y'), rows=rows_html_str)

        # build excel bytes
        excel_records = []
        for id_, name, email_nv, reason in row_html:
            excel_records.append({'id': id_, 'nhan_vien': name, 'email_nhan': email_nv, 'ly_do': reason, 'noi_dung': '' , 'ngay_gui': datetime.utcnow()})
        excel_bytes = create_thongbao_excel(excel_records, title='ThongBao')

        if not recipients:
            return 0

        thongbao_repo = ThongBaoRepo()
        sent = 0
        subject = 'Báo cáo thông báo nhân sự'
        attachments = [("ThongBao.xlsx", excel_bytes)]

        for to in recipients:
            try:
                # build items for this recipient where not already sent
                items_for_recipient = []
                for item in to_notify:
                    already = await thongbao_repo.exists_for(db, item[0], item[2], to)
                    if not already:
                        items_for_recipient.append(item)

                if not items_for_recipient:
                    continue

                await send_email(to, subject, html_body, {'recipient': to, 'name': '' , 'message_html': ''}, attachments=attachments)

                for item in items_for_recipient:
                    tb = ThongBao(nhan_vien_id=item[0], ly_do=item[2], email_nhan=to, ngay_gui=datetime.utcnow())
                    await thongbao_repo.create(db, tb)
                    sent += 1
            except Exception:
                continue

        return sent
