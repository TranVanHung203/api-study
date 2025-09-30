from pydantic import BaseModel, EmailStr
from typing import Optional
from datetime import datetime

class UserBase(BaseModel):
    username: str
    email: EmailStr

class UserCreate(UserBase):
    password: str

class UpdateUser(UserBase):
    id: int
    password: Optional[str] = None

class UserOut(UserBase):
    id: int
    is_active: bool
    created_at: datetime
    class Config:
        orm_mode = True

class UserLogin(BaseModel):
    username: str
    password: str
