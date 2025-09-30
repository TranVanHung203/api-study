from ..models.refresh_token import RefreshToken
from sqlalchemy.future import select

class RefreshTokenRepo:
    async def create(self, db, entity: RefreshToken):
        db.add(entity)
        await db.commit()
        await db.refresh(entity)
        return entity

    async def get_by_token(self, db, token: str):
        q = select(RefreshToken).where(RefreshToken.token == token)
        res = await db.execute(q)
        return res.scalars().first()

    async def delete(self, db, entity: RefreshToken):
        await db.delete(entity)
        await db.commit()
