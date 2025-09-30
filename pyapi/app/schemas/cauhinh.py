from pydantic import BaseModel
from typing import Optional

class CauHinhThongBaoBase(BaseModel):
    so_ngay_thong_bao: int = 60
    danh_sach_nam_thong_bao: Optional[str] = None
    is_active: bool = False
    exclude_saturday: bool = True
    exclude_sunday: bool = True

class CreateCauHinhThongBao(CauHinhThongBaoBase):
    pass

class UpdateCauHinhThongBao(CauHinhThongBaoBase):
    id: int

class CauHinhThongBaoOut(CauHinhThongBaoBase):
    id: int
    class Config:
        orm_mode = True
