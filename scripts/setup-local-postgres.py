"""One-time setup for the existing local Homebrew PostgreSQL 14 cluster.

Run with Python 3. Creates only new project resources; refuses to overwrite them.
Stores the generated password in .NET user secrets, never prints it.
"""
import json
import os
from pathlib import Path
import secrets
import subprocess

ROOT = Path(__file__).resolve().parents[1]
ADMIN = ["psql", "-X", "-w", "-h", "localhost", "-U", "macbookpro", "-d", "postgres", "-v", "ON_ERROR_STOP=1"]


def sql(statement, database=None):
    command = ADMIN.copy()
    if database:
        command[command.index("-d") + 1] = database
    result = subprocess.run(command + ["-At"], input=statement, text=True, capture_output=True)
    if result.returncode:
        raise RuntimeError("PostgreSQL setup command failed; inspect the local server. Details suppressed to protect credentials.")
    return result.stdout.strip()


if sql("SELECT 1 FROM pg_roles WHERE rolname='campions_app';") or sql(
    "SELECT 1 FROM pg_database WHERE datname='campions_gym';"
):
    raise SystemExit("Project database or role already exists; refusing to overwrite credentials or data.")

hba = Path(sql("SHOW hba_file;"))
original = hba.read_text()
backup = hba.with_name(hba.name + ".campions-backup")
if backup.exists():
    raise SystemExit("Authentication backup already exists; inspect prior setup before retrying.")
password = secrets.token_hex(32)
sql("SET password_encryption='scram-sha-256'; CREATE ROLE campions_app LOGIN "
    "NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION PASSWORD '" + password + "';")
sql("CREATE DATABASE campions_gym OWNER campions_app;")
sql("REVOKE ALL ON DATABASE campions_gym FROM PUBLIC; GRANT CONNECT ON DATABASE campions_gym TO campions_app;")
sql("REVOKE CREATE ON SCHEMA public FROM PUBLIC; GRANT USAGE, CREATE ON SCHEMA public TO campions_app;", "campions_gym")
backup.write_text(original)
os.chmod(backup, 0o600)
# Rules for this account precede existing trust rules; unrelated roles unchanged.
hba.write_text("# Campions gym local development account\n"
    "local all campions_app scram-sha-256\n"
    "host all campions_app 127.0.0.1/32 scram-sha-256\n"
    "host all campions_app ::1/128 scram-sha-256\n" + original)
if sql("SELECT pg_reload_conf();") != "t":
    raise RuntimeError("PostgreSQL configuration reload failed.")
connection = "Host=localhost;Port=5432;Database=campions_gym;Username=campions_app;Password=" + password
result = subprocess.run(["dotnet", "user-secrets", "set", "--project",
    str(ROOT / "CampionsSystem.API/CampionsSystem.csproj")],
    input=json.dumps({"ConnectionStrings:CampionsDatabase": connection}),
    text=True, capture_output=True)
if result.returncode:
    raise RuntimeError("Database created, but user-secret storage failed; credentials not printed. Manual recovery required.")
environment = os.environ.copy()
environment["PGPASSWORD"] = password
check = subprocess.run(["psql", "-X", "-w", "-h", "localhost", "-U", "campions_app",
    "-d", "campions_gym", "-At", "-c", "SELECT current_database(), current_user;"],
    env=environment, text=True, capture_output=True)
if check.returncode:
    raise RuntimeError("App account connection verification failed.")
print("Database and dedicated account created; password stored in .NET user secrets.")
print(check.stdout.strip())
