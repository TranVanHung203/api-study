from ..repos.email_repo import EmailRepo
from ..models.email import EmailThongBao

class EmailThongBaoService:
    def __init__(self):
        self.repo = EmailRepo()

    async def create(self, db, dto):
        entity = EmailThongBao(email=dto.email, name=dto.name)
        return await self.repo.create(db, entity)

    async def get_by_id(self, db, id: int):
        return await self.repo.get_by_id(db, id)

    async def get_paged(self, db, page:int=1, page_size:int=20, keyword: str|None=None):
        return await self.repo.get_paged(db, page, page_size, keyword)

    async def update(self, db, entity: EmailThongBao, dto):
        entity.email = dto.email
        entity.name = dto.name
        return await self.repo.update(db, entity)

    async def delete(self, db, entity: EmailThongBao):
        return await self.repo.delete(db, entity)
