# qlnv Python port

This is the FastAPI port of the original .NET `qlnv` application.

Quick start

1. Create a virtualenv and install requirements:

```powershell
python -m venv .venv
.\.venv\Scripts\activate
pip install -r ..\requirements.txt
```

2. Copy `.env.example` to `.env` and fill credentials.

3. Run Alembic migrations (configure `DATABASE_URL` in `.env`):

```powershell
alembic upgrade head
```

4. Start the app:

```powershell
uvicorn app.main:app --reload --host 0.0.0.0 --port 8000
```

Notes

- The scheduler runs a daily job at 08:00 local time. For production, consider moving scheduled jobs to a dedicated worker or using a distributed lock to avoid duplicate runs.
- Tests: run `pytest` from `pyapi/`.