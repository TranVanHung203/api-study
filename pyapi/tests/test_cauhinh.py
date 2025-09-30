from app.services.cauhinh_service import count_working_days
from datetime import datetime, date

def test_count_working_days_simple():
    start = datetime(2025,9,1)
    end = datetime(2025,9,5)
    holidays = set()
    days = count_working_days(start, end, holidays, exclude_saturday=False, exclude_sunday=False)
    # start=1 -> count from 2..5 inclusive => 4 days
    assert days == 4
