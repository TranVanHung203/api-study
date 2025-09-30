from sqlalchemy import Column, Integer, String, DateTime
from ..db.base import Base
from datetime import datetime

class RefreshToken(Base):
    __tablename__ = "RefreshToken"
    id = Column(Integer, primary_key=True, index=True)
    user_id = Column(Integer, nullable=False)
    token = Column(String(500), nullable=False)
    expires_at = Column(DateTime, nullable=False)
    created_at = Column(DateTime, default=datetime.utcnow)
