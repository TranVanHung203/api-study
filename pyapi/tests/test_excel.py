from app.utils.excel import create_thongbao_excel
from datetime import datetime

def test_create_excel_bytes():
    records = [
        {'id':1, 'nhan_vien':'Nguyen A', 'email_nhan':'a@example.com', 'ly_do':'Test', 'noi_dung':'', 'ngay_gui': datetime.utcnow()}
    ]
    b = create_thongbao_excel(records)
    assert isinstance(b, (bytes, bytearray))
    assert len(b) > 0
