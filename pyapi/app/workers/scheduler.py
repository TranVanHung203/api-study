from apscheduler.schedulers.asyncio import AsyncIOScheduler
from apscheduler.triggers.cron import CronTrigger
from .worker_service import run_daily_checks

scheduler = AsyncIOScheduler()

def start_scheduler():
    # run at 08:00 local time every day
    scheduler.add_job(run_daily_checks, CronTrigger(hour=8, minute=0))
    scheduler.start()
