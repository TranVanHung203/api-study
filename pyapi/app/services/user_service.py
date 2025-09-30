from ..repos.user_repo import UserRepo
from ..models.user import User
import bcrypt

class UserService:
    def __init__(self):
        self.repo = UserRepo()

    async def create(self, db, dto, current_user_role: str = 'Assistant'):
        # uniqueness checks
        by_username = await self.repo.get_by_username(db, dto.username)
        if by_username:
            raise ValueError('Username already exists')
        by_email = await self.repo.get_by_email(db, dto.email)
        if by_email:
            raise ValueError('Email already exists')

        role = 'Assistant'
        if getattr(dto, 'role', None) and current_user_role == 'Admin':
            role = dto.role

        # bcrypt via passlib or bcrypt is acceptable; using bcrypt lib if available
        import hashlib
        password = getattr(dto, 'password', None) or ''
        # fallback to simple hashing if bcrypt not installed
        try:
            import bcrypt as _bcrypt
            hashed = _bcrypt.hashpw(password.encode('utf-8'), _bcrypt.gensalt()).decode('utf-8')
        except Exception:
            hashed = hashlib.sha256(password.encode('utf-8')).hexdigest()

        user = User(username=dto.username, email=dto.email, password_hash=hashed, full_name=getattr(dto, 'full_name', '') or '', role=role)
        created = await self.repo.create(db, user)
        return created

    async def get(self, db, id):
        return await self.repo.get_by_id(db, id)

    async def get_paged(self, db, page:int=1, page_size:int=20, q: str|None=None):
        return await self.repo.get_paged(db, page, page_size, q)

    async def update(self, db, entity: User, dto, current_user_role: str = 'Assistant'):
        if getattr(dto, 'role', None) and dto.role != entity.role:
            if current_user_role != 'Admin':
                raise PermissionError('Only Admin can change role')
            entity.role = dto.role
        if getattr(dto, 'full_name', None):
            entity.full_name = dto.full_name
        return await self.repo.update(db, entity)

    async def delete(self, db, id):
        ent = await self.repo.get_by_id(db, id)
        if not ent:
            raise KeyError('User not found')
        await self.repo.delete(db, ent)
