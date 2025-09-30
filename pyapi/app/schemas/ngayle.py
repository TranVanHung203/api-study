from pydantic import BaseModel
from datetime import datetime


class NgayLeBase(BaseModel):
    ten_ngay_le: str
    ngay_bat_dau: datetime
    ngay_ket_thuc: datetime


class NgayLeOut(NgayLeBase):
    id: int
    created_at: datetime

    class Config:
        orm_mode = True
