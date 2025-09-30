from fastapi import APIRouter, Depends, HTTPException, Query
from ..schemas.email import EmailCreate, EmailOut
from ..services.email_thongbao_service import EmailThongBaoService
from ..db.session import get_db
from ..schemas.thongbao import PagedResult

router = APIRouter()
service = EmailThongBaoService()


@router.post("/", response_model=EmailOut)
async def create(dto: EmailCreate, db=Depends(get_db)):
    try:
        entity = await service.create(db, dto)
        return entity
    except Exception as ex:
        raise HTTPException(status_code=400, detail=str(ex))


@router.get('/', response_model=PagedResult[EmailOut])
async def list_emails(page: int = Query(1, alias='page'), page_size: int = Query(20, alias='pageSize'), keyword: str | None = Query(None, alias='keyword'), db=Depends(get_db)):
    from ..repos.email_repo import EmailRepo
    repo = EmailRepo()
    paged = await repo.get_paged(db, page, page_size, keyword)
    items = paged.get('items', [])
    processed = []
    for it in items:
        try:
            d = it.__dict__.copy()
        except Exception:
            d = dict(it)
        processed.append(d)
    paged['items'] = processed
    return paged
