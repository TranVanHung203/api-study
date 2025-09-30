from ..repos.nhanvien_repo import NhanVienRepo
from ..models.nhanvien import NhanVien

class NhanVienService:
    def __init__(self):
        self.repo = NhanVienRepo()

    async def create(self, db, dto):
        nv = NhanVien(ho_ten=dto.ho_ten, email=dto.email, ngay_sinh=dto.ngay_sinh, ma_nv=dto.ma_nv)
        return await self.repo.create(db, nv)

    async def get(self, db, id: int):
        return await self.repo.get_by_id(db, id)

    async def list(self, db):
        return await self.repo.get_all(db)

    async def update(self, db, entity: NhanVien, dto):
        entity.ho_ten = dto.ho_ten
        entity.email = dto.email
        entity.ngay_sinh = dto.ngay_sinh
        entity.ma_nv = dto.ma_nv
        return await self.repo.update(db, entity)

    async def delete(self, db, entity: NhanVien):
        return await self.repo.delete(db, entity)
