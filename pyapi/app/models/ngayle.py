from sqlalchemy import Column, Integer, String, DateTime
from ..db.base import Base
from datetime import datetime


class NgayLe(Base):
    __tablename__ = "NgayLe"
    id = Column(Integer, primary_key=True, index=True)
    ten_ngay_le = Column(String(200), nullable=False)
    ngay_bat_dau = Column(DateTime, nullable=False)
    ngay_ket_thuc = Column(DateTime, nullable=False)
    created_at = Column(DateTime, default=datetime.utcnow)
