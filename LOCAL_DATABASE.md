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
is not registered in production. All eight migrations through `20261008012831_AddGuestInvitations` have been applied
locally (2026-10-08): 76 application tables, seven Identity tables and EF migration history.

## Manage the connection

The API reads `ConnectionStrings:DefaultConnection` from .NET user secrets in
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

- `CampionsSystem.DAL/Data/ApplicationDbContext.cs` is the Guid Identity EF context.
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

Do not run `EnsureCreated` or automatic migrations at startup. Future schema
changes need reviewed DAL migrations.

## Migrations and verification

```sh
dotnet tool restore
dotnet build CampionsGymSystem.slnx
dotnet ef migrations list --project CampionsSystem.DAL --startup-project CampionsSystem.API -- --environment Development
dotnet ef migrations has-pending-model-changes --project CampionsSystem.DAL --startup-project CampionsSystem.API -- --environment Development
```

For a freshly configured LOCAL database only:

```sh
dotnet ef database update --project CampionsSystem.DAL --startup-project CampionsSystem.API -- --environment Development
```

The reviewed SQL and transactional smoke checks are in `docs/database/`.
Run the smoke checks on development only, entering the password privately:

```sh
psql -h localhost -U campions_app -d campions_gym -W -v ON_ERROR_STOP=1 -f docs/database/validate-local.sql
```

Tests insert only synthetic fixtures and roll them back. See `SCHEMA.md` and
`BUSINESS_RULES_NOT_ENFORCED_BY_DB.md` for model assumptions and BLL obligations.

## Current migration deployment status

On 2026-10-07, after explicit user authorization, the local development database was
updated from InitialCreate through all four pending migrations, in order:

1. UpdatePrivateAndFreelanceBusinessRules
2. UpdateActivitiesClassesAndFreelanceRules
3. FinalBusinessSchemaAlignment
4. AddMembershipBenefits

At that checkpoint all five migrations were applied with no pending migrations. Before update,
a private local pg_dump archive was created and its archive listing verified. The
backup is outside the repository under `/private/tmp/gym-development-backups/`; it is
only a temporary recovery copy, not durable backup storage. Existing five Identity
users were preserved; aggregate member/sale/payment/Private counts remained unchanged.

Verification: full suite 156 passed, zero failed/skipped; solution compiled successfully.
A temporary API on a separate local port returned healthy database status and HTTP 401
for unauthenticated `/api/auth/me`, then was stopped. Production was not touched.

Prior schema reports describe their original code-only operations; this later authorized
deployment supersedes their pending-local-application status. Historical rows requiring
manual reconciliation remain subject to the documented migration guards on other databases.

### Member legacy code update

Later on 2026-10-07, the explicitly authorized development-only migration
`20261007182227_AddMemberClientCode` was applied successfully. The configured localhost
`campions_gym` Members table had zero rows before and after. ClientCode is bigint NOT
NULL, positive and unique, with no generated default. Guid Id and all member foreign
keys remain unchanged. No production database was updated. This brings local migration
history to six entries. See docs/database/SCHEMA.md before deploying to a database that
already has uncoded members; this required-column migration does not backfill codes.

### Branch-scoped legacy code correction

The latest confirmed rule supersedes global ClientCode uniqueness. New migration
20261007183558_ScopeMemberClientCodeByHomeBranch was applied to local development:
ClientCode is now nullable bigint, positive when present and unique only within
HomeBranchId (the original registration branch). It replaces the global index with
a partial composite index, contains no data backfill/renumbering, and leaves Guid
keys and MemberCode unchanged. Members count was zero before this correction.
Seven migrations are now applied locally; no old migration was rewritten.


### One-time guest invitations

On 2026-10-08, AddGuestInvitations was applied explicitly to localhost
campions_gym in Development. Catalog verification confirmed the migration history
entry, invitation indexes, history trigger and nullable CheckIns.MemberId.
Only Trial check-ins may omit MemberId; existing access types retain the member
requirement. No production update or existing-table data rewrite was performed.
The reviewed upgrade SQL is docs/database/AddGuestInvitations.sql.
