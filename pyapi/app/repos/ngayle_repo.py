from ..models.ngayle import NgayLe
from sqlalchemy.future import select
from sqlalchemy import func

class NgayLeRepo:
    async def create(self, db, entity: NgayLe):
        db.add(entity)
        await db.commit()
        await db.refresh(entity)
        return entity

    async def get_by_id(self, db, id:int):
        return await db.get(NgayLe, id)

    async def get_all(self, db):
        res = await db.execute(select(NgayLe))
        return res.scalars().all()

    async def get_paged(self, db, page:int=1, page_size:int=20, ten: str|None=None):
        query = select(NgayLe)
        if ten:
            kw = f"%{ten.lower()}%"
            query = query.where(NgayLe.ten_ngay_le.ilike(kw))

        count_q = select(func.count(NgayLe.id))
        if ten:
            count_q = count_q.where(NgayLe.ten_ngay_le.ilike(kw))
        res_count = await db.execute(count_q)
        total = int(res_count.scalar() or 0)

        query = query.offset((page-1)*page_size).limit(page_size)
        res = await db.execute(query)
        items = res.scalars().all()
        total_pages = (total + page_size - 1) // page_size if page_size else 1
        return {'items': items, 'page': page, 'page_size': page_size, 'total_items': total, 'total_pages': total_pages}

    async def update(self, db, entity: NgayLe):
        await db.commit()
        await db.refresh(entity)
        return entity

    async def delete(self, db, entity: NgayLe):
        await db.delete(entity)
        await db.commit()
