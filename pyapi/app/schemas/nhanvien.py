from pydantic import BaseModel
from typing import Optional
from datetime import date

class NhanVienBase(BaseModel):
    ho_ten: str
    email: str
    ngay_sinh: Optional[date] = None
    ma_nv: Optional[str] = None

class CreateNhanVien(NhanVienBase):
    pass

class NhanVienOut(NhanVienBase):
    id: int
    class Config:
        orm_mode = True
