from fastapi import APIRouter, Depends, HTTPException, Query
from ..schemas.thongbao import CreateThongBao, ThongBaoOut, PagedResult
from ..db.session import get_db
from ..services.thongbao_service import create_and_send
from datetime import datetime

router = APIRouter()


@router.post('/', response_model=ThongBaoOut)
async def create(dto: CreateThongBao, db=Depends(get_db)):
    created = await create_and_send(db, dto)
    if created is None:
        raise HTTPException(status_code=409, detail='Already sent')
    return created


@router.get('/', response_model=PagedResult[ThongBaoOut])
async def list_all(
    page: int = Query(1, alias='page'),
    page_size: int = Query(20, alias='pageSize'),
    nhan_vien_id: int | None = Query(None, alias='nhanVienId'),
    email_nhan: str | None = Query(None, alias='emailNhan'),
    from_date: datetime | None = Query(None, alias='from'),
    to_date: datetime | None = Query(None, alias='to'),
    db=Depends(get_db)
):
    from ..repos.thongbao_repo import ThongBaoRepo
    repo = ThongBaoRepo()
    paged = await repo.get_paged_filtered(db, page, page_size, nhan_vien_id, email_nhan, from_date, to_date)
    # ensure ten_nhan_vien is populated from related nhan_vien relationship
    items = paged.get('items', [])
    processed = []
    for it in items:
        try:
            d = it.__dict__.copy()
        except Exception:
            # already a dict-like
            d = dict(it)
        nv = getattr(it, 'nhan_vien', None)
        d['ten_nhan_vien'] = getattr(nv, 'ho_ten', None) if nv else None
        processed.append(d)

    paged['items'] = processed
    return paged
