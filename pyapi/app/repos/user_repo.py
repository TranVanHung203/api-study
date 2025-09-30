from ..models.user import User
from sqlalchemy.future import select
from sqlalchemy import func

class UserRepo:
    async def create(self, db, entity: User):
        db.add(entity)
        await db.commit()
        await db.refresh(entity)
        return entity

    async def get_by_id(self, db, id:int):
        return await db.get(User, id)

    async def get_by_email(self, db, email:str):
        q = select(User).where(User.email.ilike(email))
        res = await db.execute(q)
        return res.scalars().first()

    async def get_by_username(self, db, username:str):
        q = select(User).where(User.username == username)
        res = await db.execute(q)
        return res.scalars().first()

    async def update(self, db, entity: User):
        await db.commit()
        await db.refresh(entity)
        return entity

    async def delete(self, db, entity: User):
        await db.delete(entity)
        await db.commit()

    async def get_paged(self, db, page:int=1, page_size:int=20, q: str|None = None):
        query = select(User)
        if q:
            kw = f"%{q.lower()}%"
            query = query.where(User.username.ilike(kw) | User.email.ilike(kw))

        # count
        count_q = query.with_only_columns([func.count(User.id)])
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
