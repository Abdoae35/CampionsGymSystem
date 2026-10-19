"""Start the local Testing profile without copying database or JWT secrets into Git."""

import json
import os
from pathlib import Path
import re
import subprocess
import sys
import xml.etree.ElementTree as ET


ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT / "CampionsSystem.API/CampionsSystem.csproj"
secret_id = ET.parse(PROJECT).findtext(".//UserSecretsId")
if not secret_id:
    raise SystemExit("The API project has no user-secrets ID.")

secret_root = (
    Path(os.environ["APPDATA"]) / "Microsoft/UserSecrets"
    if os.environ.get("APPDATA")
    else Path.home() / ".microsoft/usersecrets"
)
secret_path = secret_root / secret_id / "secrets.json"
if not secret_path.is_file():
    raise SystemExit("Configure the existing local API user secrets first.")
settings = json.loads(secret_path.read_text(encoding="utf-8-sig"))
source = settings.get("ConnectionStrings:DefaultConnection") or settings.get(
    "ConnectionStrings", {}
).get("DefaultConnection")
jwt_key = settings.get("Jwt:Key") or settings.get("Jwt", {}).get("Key")
if not source or not jwt_key:
    raise SystemExit("The existing local database connection and JWT key are required.")

parts = {}
for segment in source.split(";"):
    if "=" in segment:
        key, value = segment.split("=", 1)
        parts[key.strip().lower()] = value.strip()
if (
    parts.get("host") != "localhost"
    or parts.get("port", "5432") != "5432"
    or parts.get("database") != "campions_gym"
    or parts.get("username") != "campions_app"
    or not parts.get("password")
):
    raise SystemExit("The source user secret differs from the verified local Development connection.")

testing_connection, replacements = re.subn(
    r"(?i)(^|;)(\s*Database\s*=\s*)campions_gym(?=;|$)",
    lambda match: match.group(1) + match.group(2) + "campions_gym_testing",
    source,
    count=1,
)
if replacements != 1 or "application name" in parts:
    raise SystemExit("The local connection string cannot be safely adapted for Testing.")
testing_connection += ";Application Name=CampeonesTestingAPI"

preflight_env = os.environ.copy()
preflight_env.update(
    PGHOST="localhost",
    PGPORT="5432",
    PGDATABASE="campions_gym_testing",
    PGUSER=parts["username"],
    PGPASSWORD=parts["password"],
)
query = (
    "select current_database(), inet_server_port(), "
    "(select count(*) from \"__EFMigrationsHistory\"), "
    "(select count(*) from information_schema.tables "
    "where table_schema='public' and table_type='BASE TABLE')"
)
result = subprocess.run(
    ["psql", "-X", "-A", "-t", "-v", "ON_ERROR_STOP=1", "-c", query],
    env=preflight_env,
    capture_output=True,
    text=True,
)
if result.returncode or result.stdout.strip() != "campions_gym_testing|5432|31|114":
    raise SystemExit("Testing database identity or schema preflight failed; API was not started.")

api_env = os.environ.copy()
api_env["ConnectionStrings__DefaultConnection"] = testing_connection
api_env["Jwt__Key"] = jwt_key
api_env.pop("PGPASSWORD", None)
print("Verified localhost:5432/campions_gym_testing (31 migrations, 114 tables).", flush=True)
print("Starting Testing API at http://127.0.0.1:5290", flush=True)
try:
    subprocess.run(
        ["dotnet", "run", "--project", str(PROJECT), "--launch-profile", "Testing", "--no-build"],
        cwd=ROOT,
        env=api_env,
        check=True,
    )
except KeyboardInterrupt:
    sys.exit(130)
