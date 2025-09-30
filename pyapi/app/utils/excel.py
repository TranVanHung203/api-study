from openpyxl import Workbook
from io import BytesIO
from datetime import datetime

def create_thongbao_excel(thongbao_records: list[dict], title: str = "ThongBao") -> bytes:
    wb = Workbook()
    ws = wb.active
    ws.title = title

    # header
    headers = ["ID", "NhanVien", "EmailNhan", "LyDo", "NoiDung", "NgayGui"]
    ws.append(headers)

    for r in thongbao_records:
        ws.append([
            r.get('id'),
            r.get('nhan_vien') or r.get('nhan_vien_id'),
            r.get('email_nhan'),
            r.get('ly_do'),
            r.get('noi_dung'),
            r.get('ngay_gui').isoformat() if isinstance(r.get('ngay_gui'), datetime) else r.get('ngay_gui')
        ])

    buf = BytesIO()
    wb.save(buf)
    buf.seek(0)
    return buf.read()
