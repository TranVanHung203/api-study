from pydantic import BaseModel, EmailStr
from typing import Optional

class EmailCreate(BaseModel):
    email: EmailStr
    name: Optional[str] = None

class EmailUpdate(BaseModel):
    id: int
    email: EmailStr
    name: Optional[str] = None

class EmailOut(BaseModel):
    id: int
    email: EmailStr
    name: Optional[str] = None

    class Config:
        orm_mode = True
