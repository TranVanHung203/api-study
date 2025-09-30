from ..repos.ngayle_repo import NgayLeRepo
from ..models.ngayle import NgayLe

class NgayLeService:
    def __init__(self):
        self.repo = NgayLeRepo()

    async def create(self, db, dto):
        entity = NgayLe(ten_ngay_le=dto.ten_ngay_le, ngay_bat_dau=dto.ngay_bat_dau, ngay_ket_thuc=dto.ngay_ket_thuc)
        return await self.repo.create(db, entity)

    async def get(self, db, id: int):
        return await self.repo.get_by_id(db, id)

    async def get_paged(self, db, page:int=1, page_size:int=20, ten: str|None=None):
        return await self.repo.get_paged(db, page, page_size, ten)

    async def update(self, db, entity: NgayLe, dto):
        entity.ten_ngay_le = dto.ten_ngay_le
        entity.ngay_bat_dau = dto.ngay_bat_dau
        entity.ngay_ket_thuc = dto.ngay_ket_thuc
        return await self.repo.update(db, entity)

    async def delete(self, db, entity: NgayLe):
        return await self.repo.delete(db, entity)
