from fastapi import APIRouter
from .email import router as email_router
from .cauhinh import router as cauhinh_router
from .worker import router as worker_router
from .thongbao import router as thongbao_router
from .auth import router as auth_router
from .nhanvien import router as nhanvien_router
from .user import router as user_router

router = APIRouter()
router.include_router(email_router, prefix="/api/EmailThongBao", tags=["EmailThongBao"])
router.include_router(cauhinh_router, prefix="/api/CauHinhThongBao", tags=["CauHinhThongBao"])
router.include_router(thongbao_router, prefix="/api/ThongBao", tags=["ThongBao"])
router.include_router(auth_router, prefix="/api/auth", tags=["Auth"]) 
router.include_router(worker_router, prefix="/api/worker", tags=["Worker"])
router.include_router(nhanvien_router, prefix="/api/NhanVien", tags=["NhanVien"]) 
router.include_router(user_router, prefix="/api/User", tags=["User"]) 
