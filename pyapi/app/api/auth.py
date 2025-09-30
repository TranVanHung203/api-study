from fastapi import APIRouter, Depends, HTTPException, status
from ..schemas.user import UserCreate, UserOut, Token, TokenRefresh, UserLogin
from ..repos.user_repo import UserRepo
from ..repos.refresh_token_repo import RefreshTokenRepo
from ..models.user import User
from ..models.refresh_token import RefreshToken
from ..core.config import settings
from ..db.session import AsyncSessionLocal
from passlib.context import CryptContext
from datetime import datetime, timedelta
import jwt
from jwt import ExpiredSignatureError, InvalidTokenError
import uuid
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials

router = APIRouter(prefix="/auth", tags=["auth"]) 

pwd_context = CryptContext(schemes=["bcrypt"], deprecated="auto")
user_repo = UserRepo()
refresh_repo = RefreshTokenRepo()
security = HTTPBearer()

# Simple JWT helpers

def create_access_token(data: dict, expires_delta: timedelta | None = None):
    to_encode = data.copy()
    if expires_delta:
        expire = datetime.utcnow() + expires_delta
    else:
        expire = datetime.utcnow() + timedelta(minutes=settings.ACCESS_TOKEN_EXPIRE_MINUTES)
    # ensure exp is a UNIX timestamp (int)
    to_encode.update({"exp": int(expire.timestamp())})
    encoded_jwt = jwt.encode(to_encode, settings.SECRET_KEY, algorithm="HS256")
    return encoded_jwt


def verify_access_token(token: str):
    try:
        payload = jwt.decode(token, settings.SECRET_KEY, algorithms=["HS256"])
        return payload
    except ExpiredSignatureError:
        return None
    except InvalidTokenError:
        return None


async def get_current_user(credentials: HTTPAuthorizationCredentials = Depends(security)):
    token = credentials.credentials
    payload = verify_access_token(token)
    if not payload:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Invalid or expired token")
    user_id = int(payload.get("sub"))
    async with AsyncSessionLocal() as db:
        user = await user_repo.get_by_id(db, user_id)
        if not user:
            raise HTTPException(status_code=401, detail="User not found")
        return user

@router.post("/register", response_model=UserOut)
async def register(payload: UserCreate):
    async with AsyncSessionLocal() as db:
        existing = await user_repo.get_by_email(db, payload.email)
        if existing:
            raise HTTPException(status_code=400, detail="Email already registered")
        user = User(username=payload.username, email=payload.email, password_hash=pwd_context.hash(payload.password))
        await user_repo.create(db, user)
        return UserOut.from_orm(user)

@router.post("/login", response_model=Token)
async def login(payload: UserLogin):
    async with AsyncSessionLocal() as db:
        user = await user_repo.get_by_username(db, payload.username)
        if not user or not pwd_context.verify(payload.password, user.password_hash):
            raise HTTPException(status_code=401, detail="Invalid credentials")
        access_token = create_access_token({"sub": str(user.id)})
        refresh = str(uuid.uuid4())
        expires = datetime.utcnow() + timedelta(days=7)
        rt = RefreshToken(user_id=user.id, token=refresh, expires_at=expires)
        await refresh_repo.create(db, rt)
        return {"access_token": access_token, "token_type": "bearer", "refresh_token": refresh}

@router.post("/refresh", response_model=Token)
async def refresh_token(payload: TokenRefresh):
    async with AsyncSessionLocal() as db:
        r = await refresh_repo.get_by_token(db, payload.refresh_token)
        if not r or r.expires_at < datetime.utcnow():
            raise HTTPException(status_code=401, detail="Invalid refresh token")
        user = await user_repo.get_by_id(db, r.user_id)
        access_token = create_access_token({"sub": str(user.id)})
        return {"access_token": access_token, "token_type": "bearer", "refresh_token": r.token}

@router.post("/logout")
async def logout(payload: TokenRefresh):
    async with AsyncSessionLocal() as db:
        r = await refresh_repo.get_by_token(db, payload.refresh_token)
        if r:
            await refresh_repo.delete(db, r)
        return {"ok": True}
