from fastapi import FastAPI
from .api import router
from .core.config import settings
from .workers.scheduler import start_scheduler, scheduler

app = FastAPI(title=settings.PROJECT_NAME)

app.include_router(router)


@app.on_event("startup")
async def startup_event():
    start_scheduler()


@app.on_event("shutdown")
async def shutdown_event():
    try:
        scheduler.shutdown(wait=False)
    except Exception:
        pass


@app.get("/health")
async def health():
    return {"status": "ok"}
