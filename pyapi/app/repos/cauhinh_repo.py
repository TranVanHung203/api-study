from ..models.cauhinh import CauHinhThongBao
from ..db.session import AsyncSessionLocal
from sqlalchemy.future import select

class CauHinhRepo:
    async def create(self, db, entity: CauHinhThongBao):
        db.add(entity)
        await db.commit()
        await db.refresh(entity)
        return entity

    async def get_active(self, db):
        q = select(CauHinhThongBao).where(CauHinhThongBao.is_active == True)
        res = await db.execute(q)
        return res.scalars().first()

    async def update(self, db, entity: CauHinhThongBao):
        await db.commit()
        await db.refresh(entity)
        return entity

    async def get_all(self, db):
        res = await db.execute(select(CauHinhThongBao))
        return res.scalars().all()
