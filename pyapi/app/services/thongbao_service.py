from ..db.session import AsyncSessionLocal, get_db
from ..repos.thongbao_repo import ThongBaoRepo
from ..repos.nhanvien_repo import NhanVienRepo
from ..repos.email_repo import EmailRepo
from ..services.email_sender import send_email
from datetime import datetime
from ..utils.excel import create_thongbao_excel

thongbao_repo = ThongBaoRepo()
nhanvien_repo = NhanVienRepo()
email_repo = EmailRepo()

async def create_and_send(db, dto):
    # dto: CreateThongBao
    from ..models.thongbao import ThongBao
    tb = ThongBao(nhan_vien_id=dto.nhan_vien_id, ly_do=dto.ly_do, email_nhan=dto.email_nhan, noi_dung=dto.noi_dung, ngay_gui=datetime.utcnow())

    # dedupe per-email
    exists = await thongbao_repo.exists_for(db, dto.nhan_vien_id, dto.ly_do, dto.email_nhan)
    if exists:
        return None

    created = await thongbao_repo.create(db, tb)

    # send email
    recipient = dto.email_nhan
    # optional: use recipient name from employee or email list
    nv = await nhanvien_repo.get_by_id(db, dto.nhan_vien_id)
    name = nv.ho_ten if nv else recipient
    # Build a minimal excel with this single record
    record = {
        'id': created.id,
        'nhan_vien_id': created.nhan_vien_id,
        'email_nhan': created.email_nhan,
        'ly_do': created.ly_do,
        'noi_dung': created.noi_dung,
        'ngay_gui': created.ngay_gui
    }
    excel_bytes = create_thongbao_excel([record], title="ThongBao")
    await send_email(recipient, f"Thông báo: {dto.ly_do}", 'email_template.html', {'recipient': recipient, 'name': name, 'message_html': dto.noi_dung or ''}, attachments=[(f"thongbao_{created.id}.xlsx", excel_bytes)])
    return created
