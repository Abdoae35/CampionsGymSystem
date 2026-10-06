"""Configure local-only auth demo secrets and ignored Postman environment; never prints secrets."""
import base64
import json
import os
from pathlib import Path
import secrets
import subprocess
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
project = ROOT / "CampionsSystem.API/CampionsSystem.csproj"
secret_id = ET.parse(project).findtext(".//UserSecretsId")
secret_root = Path(os.environ["APPDATA"]) / "Microsoft/UserSecrets" if os.environ.get("APPDATA") else Path.home() / ".microsoft/usersecrets"
secret_path = secret_root / secret_id / "secrets.json"
existing = json.loads(secret_path.read_text(encoding="utf-8-sig")) if secret_path.exists() else {}
if not existing.get("ConnectionStrings:DefaultConnection"):
    raise SystemExit("Configure the existing local database connection in user secrets first.")
updates = {}
if not existing.get("Jwt:Key"):
    updates["Jwt:Key"] = base64.b64encode(secrets.token_bytes(32)).decode()
if not existing.get("DemoAuth:Password"):
    updates["DemoAuth:Password"] = secrets.token_urlsafe(24) + "!aA9"
if updates:
    result = subprocess.run(["dotnet", "user-secrets", "set", "--project", str(project)],
                            input=json.dumps(updates), text=True, capture_output=True)
    if result.returncode:
        raise SystemExit("Secret storage failed; details suppressed to protect credentials.")
existing.update(updates)
values = {"baseUrl": "http://localhost:5236", "demoPassword": existing["DemoAuth:Password"],
          "ownerUserName": "dev.owner", "receptionUserName": "dev.reception",
          "noPermissionsUserName": "dev.no-permissions", "inactiveUserName": "dev.inactive",
          "suspendedUserName": "dev.suspended", "ownerToken": "", "receptionToken": "", "noPermissionsToken": ""}
environment = {"name": "Campions Auth - LOCAL PRIVATE", "values": [
    {"key": key, "value": value, "enabled": True, "type": "secret" if "Token" in key or key == "demoPassword" else "default"}
    for key, value in values.items()], "_postman_variable_scope": "environment"}
output = ROOT / "postman/Campions.Auth.local.postman_environment.json"
output.parent.mkdir(exist_ok=True)
# New credentials go only into this ignored local file. Do not sync/export them to Git.
fd = os.open(output, os.O_WRONLY | os.O_CREAT | os.O_TRUNC, 0o600)
with os.fdopen(fd, "w") as stream:
    json.dump(environment, stream, indent=2)
os.chmod(output, 0o600)
print("Demo secrets configured; private Postman environment written (values not displayed).")
print("Next: run the explicit --seed-auth-demo development command, then start the API.")
