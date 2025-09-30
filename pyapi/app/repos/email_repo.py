from sqlalchemy.future import select
from ..models.email import EmailThongBao
from ..db.session import AsyncSessionLocal

class EmailRepo:
    def __init__(self):
        pass

    async def create(self, db, email: EmailThongBao):
        db.add(email)
        await db.commit()
        await db.refresh(email)
        return email

    async def get_by_id(self, db, id: int):
        return await db.get(EmailThongBao, id)

    async def get_paged(self, db, page:int=1, page_size:int=8, keyword: str|None=None):
        query = select(EmailThongBao)
        if keyword:
            kw = f"%{keyword.lower()}%"
            query = query.where(EmailThongBao.email.ilike(kw) | EmailThongBao.name.ilike(kw))
        # count total
        from sqlalchemy import func
        count_q = query.with_only_columns([func.count(EmailThongBao.id)])
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

    async def update(self, db, entity: EmailThongBao):
        await db.commit()
        await db.refresh(entity)
        return entity

    async def delete(self, db, entity: EmailThongBao):
        await db.delete(entity)
        await db.commit()
