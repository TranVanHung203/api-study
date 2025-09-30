from sqlalchemy import Column, Integer, String, Boolean
from ..db.base import Base

class CauHinhThongBao(Base):
    __tablename__ = "CauHinhThongBao"
    id = Column(Integer, primary_key=True, index=True)
    so_ngay_thong_bao = Column(Integer, nullable=False, default=60)
    danh_sach_nam_thong_bao = Column(String(200), nullable=True)
    is_active = Column(Boolean, default=False)
    exclude_saturday = Column(Boolean, default=True)
    exclude_sunday = Column(Boolean, default=True)
