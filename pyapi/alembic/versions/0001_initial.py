"""initial

Revision ID: 0001
Revises: 
Create Date: 2025-09-29 00:00:00.000000
"""
from alembic import op
import sqlalchemy as sa

# revision identifiers, used by Alembic.
revision = '0001'
down_revision = None
branch_labels = None
depends_on = None


def upgrade():
    op.create_table(
        'EmailThongBao',
        sa.Column('id', sa.Integer, primary_key=True),
        sa.Column('email', sa.String(320), nullable=False, unique=True),
        sa.Column('name', sa.String(200), nullable=True),
    )

    op.create_table(
        'CauHinhThongBao',
        sa.Column('id', sa.Integer, primary_key=True),
        sa.Column('so_ngay_thong_bao', sa.Integer, nullable=False, server_default='60'),
        sa.Column('danh_sach_nam_thong_bao', sa.String(200), nullable=True),
        sa.Column('is_active', sa.Boolean, nullable=False, server_default='0'),
        sa.Column('exclude_saturday', sa.Boolean, nullable=False, server_default='1'),
        sa.Column('exclude_sunday', sa.Boolean, nullable=False, server_default='1'),
    )

    op.create_table(
        'NhanVien',
        sa.Column('id', sa.Integer, primary_key=True),
        sa.Column('ho_ten', sa.String(200), nullable=False),
        sa.Column('email', sa.String(320), nullable=False),
        sa.Column('ngay_sinh', sa.DateTime, nullable=True),
        sa.Column('ma_nv', sa.String(50), nullable=True),
    )

    op.create_table(
        'ThongBao',
        sa.Column('id', sa.Integer, primary_key=True),
        sa.Column('nhan_vien_id', sa.Integer, nullable=False),
        sa.Column('ly_do', sa.String(200), nullable=False),
        sa.Column('email_nhan', sa.String(320), nullable=False),
        sa.Column('noi_dung', sa.Text, nullable=True),
        sa.Column('ngay_gui', sa.DateTime, nullable=False),
        sa.Column('file_path', sa.String(500), nullable=True),
    )

    op.create_table(
        'User',
        sa.Column('id', sa.Integer, primary_key=True),
        sa.Column('username', sa.String(100), nullable=False, unique=True),
        sa.Column('email', sa.String(320), nullable=False, unique=True),
        sa.Column('password_hash', sa.String(200), nullable=False),
        sa.Column('is_active', sa.Boolean, nullable=False, server_default='1'),
        sa.Column('created_at', sa.DateTime, nullable=False),
    )

    op.create_table(
        'RefreshToken',
        sa.Column('id', sa.Integer, primary_key=True),
        sa.Column('user_id', sa.Integer, nullable=False),
        sa.Column('token', sa.String(500), nullable=False),
        sa.Column('expires_at', sa.DateTime, nullable=False),
        sa.Column('created_at', sa.DateTime, nullable=False),
    )

    op.create_table(
        'NgayLe',
        sa.Column('id', sa.Integer, primary_key=True),
        sa.Column('name', sa.String(200), nullable=False),
        sa.Column('date', sa.DateTime, nullable=False),
        sa.Column('created_at', sa.DateTime, nullable=False),
    )


def downgrade():
    op.drop_table('NgayLe')
    op.drop_table('RefreshToken')
    op.drop_table('User')
    op.drop_table('ThongBao')
    op.drop_table('NhanVien')
    op.drop_table('CauHinhThongBao')
    op.drop_table('EmailThongBao')
