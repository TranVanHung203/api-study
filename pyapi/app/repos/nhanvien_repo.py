from ..models.nhanvien import NhanVien
from sqlalchemy.future import select
from sqlalchemy import func

class NhanVienRepo:
    async def create(self, db, entity: NhanVien):
        db.add(entity)
        await db.commit()
        await db.refresh(entity)
        return entity

    async def get_by_id(self, db, id:int):
        return await db.get(NhanVien, id)

    async def get_by_email(self, db, email:str):
        q = select(NhanVien).where(NhanVien.email.ilike(email))
        res = await db.execute(q)
        return res.scalars().first()

    async def get_all(self, db):
        res = await db.execute(select(NhanVien))
        return res.scalars().all()

    async def update(self, db, entity: NhanVien):
        await db.commit()
        await db.refresh(entity)
        return entity

    async def delete(self, db, entity: NhanVien):
        await db.delete(entity)
        await db.commit()

    async def get_paged(self, db, page:int=1, page_size:int=20, ten: str|None=None, sdt: str|None=None):
        query = select(NhanVien)
        if ten:
            kw = f"%{ten.lower()}%"
            query = query.where(NhanVien.ho_ten.ilike(kw))

        # count
        count_q = query.with_only_columns([func.count(NhanVien.id)])
        res_count = await db.execute(count_q)
        total = int(res_count.scalar() or 0)

        query = query.offset((page-1)*page_size).limit(page_size)
        res = await db.execute(query)
        items = res.scalars().all()
        total_pages = (total + page_size - 1) // page_size if page_size else 1
        return {
            'items': items,
            'page': page,
            'page_size': page_size,
            'total_items': total,
            'total_pages': total_pages
        }
