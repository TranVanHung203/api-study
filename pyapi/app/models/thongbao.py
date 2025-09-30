from sqlalchemy import Column, Integer, String, DateTime, ForeignKey
from sqlalchemy.orm import relationship
from ..db.base import Base
from datetime import datetime

class ThongBao(Base):
    __tablename__ = "ThongBao"
    id = Column(Integer, primary_key=True, index=True)
    nhan_vien_id = Column(Integer, ForeignKey('NhanVien.id'), nullable=False)
    ly_do = Column(String(200), nullable=False)
    email_nhan = Column(String(320), nullable=False)
    noi_dung = Column(String, nullable=True)
    ngay_gui = Column(DateTime, nullable=False, default=datetime.utcnow)
    file_path = Column(String(500), nullable=True)
    # ORM relationship to NhanVien for convenient access
    nhan_vien = relationship('NhanVien', lazy='joined')
