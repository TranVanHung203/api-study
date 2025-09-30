from fastapi import APIRouter, BackgroundTasks
from ..workers.scheduler import start_scheduler
from ..services.worker_service import run_daily_checks

router = APIRouter()

@router.post('/run')
async def run_once(background_tasks: BackgroundTasks):
    # trigger a background run
    background_tasks.add_task(run_daily_checks)
    return {"status": "started"}

@router.post('/start-scheduler')
async def start():
    start_scheduler()
    return {"status": "scheduler started"}
