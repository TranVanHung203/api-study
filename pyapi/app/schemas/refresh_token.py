from pydantic import BaseModel
from datetime import datetime

class TokenRefresh(BaseModel):
    refresh_token: str

class Token(BaseModel):
    access_token: str
    token_type: str

class RefreshTokenBase(BaseModel):
    user_id: int
    token: str
    expires_at: datetime

class RefreshTokenOut(RefreshTokenBase):
    id: int
    created_at: datetime
    class Config:
        orm_mode = True
