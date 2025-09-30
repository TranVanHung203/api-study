from sqlalchemy import Column, Integer, String, DateTime
from ..db.base import Base
from datetime import date

class NhanVien(Base):
    __tablename__ = "NhanVien"
    id = Column(Integer, primary_key=True, index=True)
    ho_ten = Column(String(200), nullable=False)
    email = Column(String(320), nullable=False)
    ngay_sinh = Column(DateTime, nullable=True)
    ma_nv = Column(String(50), nullable=True)
