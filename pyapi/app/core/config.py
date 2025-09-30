from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    PROJECT_NAME: str = "qlnv-python"

    DATABASE_URL: str = "mysql+aiomysql://root:100100@localhost:3307/qlnv"

    SMTP_HOST: str | None = None
    SMTP_PORT: int | None = None
    SMTP_USER: str | None = None
    SMTP_PASS: str | None = None
    SMTP_FROM: str | None = None

    SECRET_KEY: str = "change_me"
    ACCESS_TOKEN_EXPIRE_MINUTES: int = 60

    # cấu hình cho pydantic-settings
    model_config = SettingsConfigDict(
        env_file=".env", env_file_encoding="utf-8"
    )


settings = Settings()
