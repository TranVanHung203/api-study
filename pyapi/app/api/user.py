from fastapi import APIRouter, Depends, HTTPException, Query
from ..db.session import get_db
from ..repos.user_repo import UserRepo
from ..schemas.user import UserOut
from ..schemas.thongbao import PagedResult
from ..schemas.user import UserCreate, UpdateUser
from ..services.user_service import UserService
from .auth import get_current_user

router = APIRouter(prefix='/users', tags=['users'])
service = UserService()


@router.get('/', response_model=PagedResult[UserOut])
async def list_users(page: int = Query(1, alias='page'), page_size: int = Query(20, alias='pageSize'), q: str | None = Query(None, alias='q'), db=Depends(get_db)):
    repo = UserRepo()
    paged = await repo.get_paged(db, page, page_size, q)
    # convert items to dicts for Pydantic
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


@router.get('/{id}', response_model=UserOut)
async def get_user(id: int, db=Depends(get_db)):
    repo = UserRepo()
    user = await repo.get_by_id(db, id)
    if not user:
        raise HTTPException(status_code=404, detail='User not found')
    return user


@router.post('/', response_model=UserOut)
async def create_user(payload: UserCreate, current_user = Depends(get_current_user)):
    async with get_db() as db:
        # only Admin may create users with specific roles; non-admins default
        cur_role = getattr(current_user, 'role', 'Assistant')
        created = await service.create(db, payload, current_user_role=cur_role)
        return created


@router.put('/{id}', response_model=UserOut)
async def update_user(id: int, payload: UpdateUser, current_user = Depends(get_current_user)):
    async with get_db() as db:
        existing = await service.get(db, id)
        if not existing:
            raise HTTPException(status_code=404, detail='User not found')
        cur_role = getattr(current_user, 'role', 'Assistant')
        updated = await service.update(db, existing, payload, current_user_role=cur_role)
        return updated


@router.delete('/{id}')
async def delete_user(id: int, current_user = Depends(get_current_user)):
    # only Admin can delete; check role
    if getattr(current_user, 'role', None) != 'Admin':
        raise HTTPException(status_code=403, detail='Forbidden')
    async with get_db() as db:
        await service.delete(db, id)
        return {'ok': True}
