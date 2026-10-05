# Local PostgreSQL

The existing Homebrew PostgreSQL 14.20 service is used for local development.

| Setting | Value |
| --- | --- |
| Host | localhost |
| Port | 5432 |
| Database | campions_gym |
| User | campions_app |
| Password | Generated locally; stored in .NET user secrets |

The account has no superuser, role-creation, or database-creation permissions.
It owns this development database so future local migrations can create tables.
Production should use separate migration and runtime permissions.

## Start the API

From the repository root:

```sh
brew services start postgresql@14
dotnet run --project CampionsSystem.API/CampionsSystem.csproj --launch-profile http
```

Open http://localhost:5236/health/database in development. A working connection
returns `{"status":"healthy"}`; connection failures return HTTP 503. This route
is not registered in production. The database currently has no gym tables.

## Manage the connection

The API reads `ConnectionStrings:CampionsDatabase` from .NET user secrets in
development. No password is stored in repository configuration.

To view the locally stored connection yourself (do not share its output):

```sh
dotnet user-secrets list --project CampionsSystem.API/CampionsSystem.csproj
```

Use the settings above and the stored password in pgAdmin or another database
client. To connect with psql and enter the password privately:

```sh
psql -h localhost -p 5432 -U campions_app -d campions_gym -W
```

User secrets are local development storage, not an encrypted production vault.
On another machine, create the database/account and set this secret locally.
Production must provide its own connection through a secure configuration source.

## Setup details

- `CampionsSystem.DAL/CampionsDbContext.cs` is the EF Core context.
- Npgsql EF Core provider is pinned to 10.0.3; its SQL compatibility is configured
  for the installed PostgreSQL 14 server.
- The solution includes API, BLL, and DAL. API references BLL, and BLL references
  DAL. API also references DAL directly for DbContext registration and the local
  connectivity diagnostic.
- `scripts/setup-local-postgres.py` performed the one-time setup for this Mac's
  existing cluster/admin account. Do not rerun it on this database: it refuses
  to overwrite existing project resources. Adapt its admin settings for another
  machine. No downloaded application code or database schema was executed.
- Authentication rules for `campions_app` require SCRAM passwords on loopback
  TCP and local sockets, ahead of existing trust rules. Other accounts were not
  changed. PostgreSQL listens on localhost.
- Original authentication config was backed up beside `pg_hba.conf` as
  `pg_hba.conf.campions-backup`. Do not blindly restore it after later changes.
- PostgreSQL was reused at its existing version; no server upgrade was performed.
  Keep server patching/upgrades as a separate maintenance task.

Do not run `EnsureCreated` or automatic migrations at startup. Next, agree the
gym data model and create reviewed EF Core migrations.
