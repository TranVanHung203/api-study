from fastapi import APIRouter, Depends, HTTPException, Query
from ..schemas.nhanvien import CreateNhanVien, NhanVienOut
from ..db.session import AsyncSessionLocal
from ..services.nhanvien_service import NhanVienService
from ..repos.nhanvien_repo import NhanVienRepo
from .auth import get_current_user
from ..schemas.thongbao import PagedResult

router = APIRouter()
service = NhanVienService()
repo = NhanVienRepo()


@router.get('/', response_model=PagedResult[NhanVienOut])
async def list_nhanvien(page: int = Query(1, alias='page'), page_size: int = Query(20, alias='pageSize'), ten: str | None = Query(None, alias='ten')):
    async with AsyncSessionLocal() as db:
        paged = await repo.get_paged(db, page, page_size, ten)
        items = paged.get('items', [])
        processed = [NhanVienOut.from_orm(i) for i in items]
        paged['items'] = processed
        return paged

@router.get('/{id}', response_model=NhanVienOut)
async def get_nhanvien(id: int):
    async with AsyncSessionLocal() as db:
        item = await service.get(db, id)
        if not item:
            raise HTTPException(status_code=404, detail='Not found')
        return NhanVienOut.from_orm(item)

@router.post('/', response_model=NhanVienOut)
async def create_nhanvien(payload: CreateNhanVien, current_user = Depends(get_current_user)):
    async with AsyncSessionLocal() as db:
        existing = await repo.get_by_email(db, payload.email)
        if existing:
            raise HTTPException(status_code=400, detail='Email already exists')
        nv = await service.create(db, payload)
        return NhanVienOut.from_orm(nv)

@router.put('/{id}', response_model=NhanVienOut)
async def update_nhanvien(id: int, payload: CreateNhanVien, current_user = Depends(get_current_user)):
    async with AsyncSessionLocal() as db:
        item = await service.get(db, id)
        if not item:
            raise HTTPException(status_code=404, detail='Not found')
        nv = await service.update(db, item, payload)
        return NhanVienOut.from_orm(nv)

@router.delete('/{id}')
async def delete_nhanvien(id: int, current_user = Depends(get_current_user)):
    async with AsyncSessionLocal() as db:
        item = await service.get(db, id)
        if not item:
            raise HTTPException(status_code=404, detail='Not found')
        await service.delete(db, item)
        return {'ok': True}
