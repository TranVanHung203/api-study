from sqlalchemy import Column, Integer, String
from ..db.base import Base

class EmailThongBao(Base):
    __tablename__ = "EmailThongBao"
    id = Column(Integer, primary_key=True, index=True)
    email = Column(String(320), nullable=False, unique=True)
    name = Column(String(200), nullable=True)
