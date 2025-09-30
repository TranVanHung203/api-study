from fastapi import APIRouter, Depends, HTTPException
from ..schemas.cauhinh import CreateCauHinhThongBao, UpdateCauHinhThongBao, CauHinhThongBaoOut
from ..db.session import get_db
from ..services.cauhinh_service import get_active_config, create_config, update_config

router = APIRouter()

@router.get('/active', response_model=CauHinhThongBaoOut)
async def get_active(db=Depends(get_db)):
    cfg = await get_active_config(db)
    if not cfg:
        raise HTTPException(status_code=404, detail='No active config')
    return cfg

@router.post('/', response_model=CauHinhThongBaoOut)
async def create(dto: CreateCauHinhThongBao, db=Depends(get_db)):
    created = await create_config(db, dto)
    return created

@router.put('/', response_model=CauHinhThongBaoOut)
async def update(dto: UpdateCauHinhThongBao, db=Depends(get_db)):
    updated = await update_config(db, dto)
    return updated
