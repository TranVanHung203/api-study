from ..models.thongbao import ThongBao
from sqlalchemy.future import select
from sqlalchemy import and_, func
from sqlalchemy.orm import joinedload
from datetime import datetime

class ThongBaoRepo:
    async def create(self, db, entity: ThongBao):
        db.add(entity)
        await db.commit()
        await db.refresh(entity)
        return entity

    async def exists_for(self, db, nhan_vien_id: int, ly_do: str, email_nhan: str):
        q = select(ThongBao).where(
            ThongBao.nhan_vien_id == nhan_vien_id,
            ThongBao.ly_do == ly_do,
            ThongBao.email_nhan == email_nhan
        )
        res = await db.execute(q)
        return res.scalars().first() is not None

    async def get_paged(self, db, page:int=1, page_size:int=20):
        # count total
        count_q = select(func.count(ThongBao.id))
        res_count = await db.execute(count_q)
        total = int(res_count.scalar() or 0)

        q = select(ThongBao).options(joinedload(ThongBao.nhan_vien)).order_by(ThongBao.ngay_gui.desc()).offset((page-1)*page_size).limit(page_size)
        res = await db.execute(q)
        items = res.scalars().all()
        total_pages = (total + page_size - 1) // page_size if page_size else 1
        return {
            'items': items,
            'page': page,
            'page_size': page_size,
            'total_items': total,
            'total_pages': total_pages
        }

    async def get_paged_filtered(self, db, page:int=1, page_size:int=20, nhan_vien_id: int | None = None, email_nhan: str | None = None, from_date: datetime | None = None, to_date: datetime | None = None):
        q = select(ThongBao)
        conditions = []
        if nhan_vien_id:
            conditions.append(ThongBao.nhan_vien_id == nhan_vien_id)
        if email_nhan:
            conditions.append(ThongBao.email_nhan.ilike(f"%{email_nhan}%"))
        if from_date:
            conditions.append(ThongBao.ngay_gui >= from_date)
        if to_date:
            conditions.append(ThongBao.ngay_gui <= to_date)
        if conditions:
            q = q.where(and_(*conditions))
        # count total using SQL COUNT
        count_q = select(func.count(ThongBao.id))
        if conditions:
            count_q = count_q.where(and_(*conditions))
        res_count = await db.execute(count_q)
        total = int(res_count.scalar() or 0)

        # fetch page with joined employee
        q = select(ThongBao)
        if conditions:
            q = q.where(and_(*conditions))
        q = q.order_by(ThongBao.ngay_gui.desc()).options(joinedload(ThongBao.nhan_vien)).offset((page-1)*page_size).limit(page_size)
        res = await db.execute(q)
        items = res.scalars().all()

        total_pages = (total + page_size - 1) // page_size if page_size else 1

        return {
            'items': items,
            'page': page,
            'page_size': page_size,
            'total_items': total,
            'total_pages': total_pages
        }

    async def get_by_id(self, db, id:int):
        return await db.get(ThongBao, id)

    async def delete(self, db, entity: ThongBao):
        await db.delete(entity)
        await db.commit()
