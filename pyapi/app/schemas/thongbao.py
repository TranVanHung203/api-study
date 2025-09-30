from pydantic import BaseModel
from typing import Optional, Generic, TypeVar, List
from pydantic.generics import GenericModel
from datetime import datetime


class ThongBaoBase(BaseModel):
    nhan_vien_id: int
    ly_do: str
    email_nhan: str
    noi_dung: Optional[str] = None


class CreateThongBao(ThongBaoBase):
    pass


class ThongBaoOut(ThongBaoBase):
    id: int
    ngay_gui: datetime
    file_path: Optional[str] = None
    ten_nhan_vien: Optional[str]

    class Config:
        orm_mode = True


T = TypeVar('T')


class PagedResult(GenericModel, Generic[T]):
    items: List[T]
    page: int
    page_size: int
    total_items: int
    total_pages: int
