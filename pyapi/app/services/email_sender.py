import os
import aiosmtplib
from jinja2 import Environment, FileSystemLoader, select_autoescape
from ..core.config import settings
from typing import Optional, List, Tuple

TEMPLATE_DIR = os.path.join(os.path.dirname(__file__), '..', 'templates')
env = Environment(
    loader=FileSystemLoader(TEMPLATE_DIR),
    autoescape=select_autoescape(['html', 'xml'])
)

def render_template(template_name: str, context: dict) -> str:
    template = env.get_template(template_name)
    return template.render(**context)

async def send_email(to: str, subject: str, template_name: str, context: dict, attachments: Optional[List[Tuple[str, bytes]]] = None):
    html = render_template(template_name, context)

    host = settings.SMTP_HOST
    port = settings.SMTP_PORT or 25
    user = settings.SMTP_USER
    password = settings.SMTP_PASS
    sender = settings.SMTP_FROM or user

    message = aiosmtplib.helpers.build_message(
        subject=subject,
        sender=sender,
        recipients=[to],
        text=None,
        html=html
    )

    # attachments handling
    if attachments:
        for filename, data in attachments:
            from email.mime.base import MIMEBase
            from email import encoders
            part = MIMEBase('application', 'octet-stream')
            part.set_payload(data)
            encoders.encode_base64(part)
            part.add_header('Content-Disposition', f'attachment; filename="{filename}"')
            message.attach(part)

    await aiosmtplib.send(message, hostname=host, port=port, username=user, password=password, start_tls=False)
