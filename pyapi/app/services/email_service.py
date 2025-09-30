from ..repos.email_repo import EmailRepo
from ..schemas.email import EmailCreate, EmailOut
from ..models.email import EmailThongBao
from ..db.session import AsyncSessionLocal

email_repo = EmailRepo()

async def create_email(db, dto: EmailCreate):
    entity = EmailThongBao(email=dto.email, name=dto.name)
    return await email_repo.create(db, entity)
