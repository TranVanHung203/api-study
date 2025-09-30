from ..repos.cauhinh_repo import CauHinhRepo

repo = CauHinhRepo()

async def get_active_config(db):
    return await repo.get_active(db)

async def create_config(db, dto):
    from ..models.cauhinh import CauHinhThongBao
    entity = CauHinhThongBao(so_ngay_thong_bao=dto.so_ngay_thong_bao, danh_sach_nam_thong_bao=dto.danh_sach_nam_thong_bao, is_active=dto.is_active, exclude_saturday=dto.exclude_saturday, exclude_sunday=dto.exclude_sunday)
    return await repo.create(db, entity)

async def update_config(db, dto):
    entity = await db.get_by_id(dto.id)
    entity.so_ngay_thong_bao = dto.so_ngay_thong_bao
    entity.danh_sach_nam_thong_bao = dto.danh_sach_nam_thong_bao
    entity.is_active = dto.is_active
    entity.exclude_saturday = dto.exclude_saturday
    entity.exclude_sunday = dto.exclude_sunday
    return await repo.update(db, entity)


def count_working_days(start_date, end_date, holidays: set, exclude_saturday: bool, exclude_sunday: bool) -> int:
    """Count working days from start_date (inclusive) to end_date (inclusive).
    holidays is a set of date objects (no time).
    exclude_saturday/exclude_sunday: when True, those weekdays are treated as non-working.
    """
    if start_date is None or end_date is None:
        return 0
    # ensure dates are date objects (not datetime) for comparison
    sd = start_date.date() if hasattr(start_date, 'date') else start_date
    ed = end_date.date() if hasattr(end_date, 'date') else end_date
    if sd > ed:
        return 0

    days = 0
    from datetime import timedelta
    # match C# semantics: start counting from the day after start_date up to end_date inclusive
    cur = sd + timedelta(days=1)
    while cur <= ed:
        weekday = cur.weekday()  # 0=Mon .. 6=Sun
        is_weekend = (weekday == 5 and exclude_saturday) or (weekday == 6 and exclude_sunday)
        if not is_weekend and cur not in holidays:
            days += 1
        cur = cur + timedelta(days=1)
    return days


def is_within_notice(target_date, today, cfg, holidays: set) -> bool:
    """Return True if number of working days from today to target_date is <= cfg.so_ngay_thong_bao"""
    # normalize
    from datetime import datetime
    if isinstance(today, datetime):
        now = today
    else:
        now = datetime.utcnow()
    # ensure target_date is a date
    td = target_date.date() if hasattr(target_date, 'date') else target_date
    wd = count_working_days(now, td, holidays, cfg.exclude_saturday, cfg.exclude_sunday)
    return wd <= (cfg.so_ngay_thong_bao or 0)
