from sqlalchemy import Column, Integer, String, Boolean, DateTime
from ..db.base import Base
from datetime import datetime

class User(Base):
    __tablename__ = "User"
    id = Column(Integer, primary_key=True, index=True)
    username = Column(String(100), unique=True, nullable=False)
    email = Column(String(320), unique=True, nullable=False)
    password_hash = Column(String(200), nullable=False)
    is_active = Column(Boolean, default=True)
    created_at = Column(DateTime, default=datetime.utcnow)
