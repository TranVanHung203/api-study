from fastapi import APIRouter, Depends, HTTPException, Query
from ..schemas.ngayle import NgayLeOut, NgayLeBase
from ..db.session import AsyncSessionLocal
from ..models.ngayle import NgayLe
from ..repos.ngayle_repo import NgayLeRepo
from ..schemas.thongbao import PagedResult
from ..services.ngayle_service import NgayLeService

router = APIRouter()
repo = NgayLeRepo()
service = NgayLeService()


@router.get('/', response_model=PagedResult[NgayLeOut])
async def list_ngayle(page: int = Query(1), page_size: int = Query(50), ten: str | None = Query(None)):
    async with AsyncSessionLocal() as db:
        paged = await repo.get_paged(db, page, page_size, ten)
        items = paged.get('items', [])
        paged['items'] = [NgayLeOut.from_orm(i) for i in items]
        return paged


@router.post('/', response_model=NgayLeOut)
async def create_ngayle(payload: NgayLeBase):
    async with AsyncSessionLocal() as db:
        entity = await service.create(db, payload)
        return NgayLeOut.from_orm(entity)


@router.get('/{id}', response_model=NgayLeOut)
async def get_ngayle(id: int):
    async with AsyncSessionLocal() as db:
        ent = await service.get(db, id)
        if not ent:
            raise HTTPException(status_code=404, detail='Not found')
        return NgayLeOut.from_orm(ent)


@router.put('/{id}', response_model=NgayLeOut)
async def update_ngayle(id: int, payload: NgayLeBase):
    async with AsyncSessionLocal() as db:
        ent = await service.get(db, id)
        if not ent:
            raise HTTPException(status_code=404, detail='Not found')
        ent = await service.update(db, ent, payload)
        return NgayLeOut.from_orm(ent)


@router.delete('/{id}')
async def delete_ngayle(id: int):
    async with AsyncSessionLocal() as db:
        ent = await service.get(db, id)
        if not ent:
            raise HTTPException(status_code=404, detail='Not found')
        await service.delete(db, ent)
        return {'ok': True}
