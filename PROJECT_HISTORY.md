# Project history and handoff

This is the portable record for the project. A new assistant, account, or provider
should read this file and `AGENTS.md` first, then inspect the relevant code and
`git status` before making changes. This summary reduces repeated explanation;
it does not replace checking the actual repository.

## Project overview

- Project: CampionsGymSystem, a gym system at an early development stage.
- Stack: C# / .NET 10 (`net10.0`).
- `CampionsSystem.API`: ASP.NET Core web API project; references
  `Microsoft.AspNetCore.OpenApi` version `10.0.8`.
- `CampionsSystem.BLL`: class library intended for business logic.
- `CampionsSystem.DAL`: class library intended for data access.
- `CampionsGymSystem.slnx`: solution file.
- Intended scope includes gym operations, transactions, payments, and leads.
  PostgreSQL is selected; the 26-module EF persistence schema is implemented.
  The current aligned model has 71 application tables plus seven Identity tables.
  Database rollout remains separate; no migration is applied by schema alignment.
  Payment provider and business workflows remain to be implemented.

## User preferences and workflow

- For the Authentication task (2026-10-06), commit and push each completed,
  verified small subtask to the existing Auth branch as explicitly requested.
- Work one user-selected task at a time; finish and verify it before advancing.
  Do not begin the next task or whole-project implementation without direction.

- The user is new to Codex and wants continuity across providers and accounts.
- Keep this file updated so future assistants can understand previous work
  without needing the full chat history.
- The user described the repository as private and wants generated files kept
  out of uploads.
- Keep summaries practical and concise; record outcomes, decisions, and checks.
- Explain proposed skills before installing any; wait for the user's explicit
  approval of the selection.
- After every project operation, update both history and the bounded current
  handoff in `AGENTS.md`; group related commands into a single concise entry.

## Current state

- Latest operation (2026-10-07): final schema-only alignment complete. Added direct
  Allow/Deny override storage, BusinessDays/event links, dynamic membership audiences,
  leave requests and shift replacement business history. Existing features preserved.
- Model: 71 application tables plus seven Identity tables (78). New migration
  20261007151235_FinalBusinessSchemaAlignment; reviewed forward SQL adds five tables
  and nine nullable FK columns. No destructive Up operation or historical backfill.
- Final build zero warnings/errors; 128 tests passed, zero failed/skipped; no model drift.
  Legacy preservation and constraints tested in isolated rolled-back PostgreSQL schemas.
- No API/BLL/auth/feature implementation changed. Direct Allow/Deny resolution and
  BusinessDay/leave/shift validation need their dedicated feature implementations.
- Existing Classes, Activities, Freelance and Private facts reused. Latest transferred
  Private work/expiry 50% rule documented for future BLL; no hardcoded DB rate/trigger.
  Class financial compensation remains not finalized; no offline sync implementation.
- Detailed gap report, exact paths and deployment notes: docs/database/FINAL_SCHEMA_ALIGNMENT.md.
  Schema/rules/data flows updated. Existing prior change is committed by user as c78d95a.
- Branch DataBaseInit; this update/checkpoint uncommitted/unpushed. No user development
  or production database migration applied. Review deployment separately. Ready for
  one user-selected vertical feature; do not start it automatically.

## Work history

### 2026-10-04 — Git ignore setup

Request: prevent generated files from being uploaded to the private repository.

Completed:
- Inspected project files and Git tracking; identified a .NET 10 project and
  tracked NuGet restore artifacts in the BLL and DAL `obj/` directories.
- Added `.gitignore` with rules appropriate to the existing .NET projects.
- Ran `git rm -r --cached` on both tracked `obj/` directories, keeping local
  copies. Updating the Git index required sandbox approval, which was granted.

Verification:
- `git check-ignore` confirmed rules cover `.DS_Store`, representative `bin/`
  output, `obj/` artifacts, `.env`, and `appsettings.Local.json`.
- `git ls-files` confirmed no `bin/` or `obj/` files remain tracked.
- File checks confirmed both local `project.assets.json` files remain on disk.
- `git status --short` showed ten staged removals and a new `.gitignore`.

### 2026-10-04 — Portable project memory

Request: create a persistent record of the work and workflow to help another
provider or account continue without reconstructing the conversation.

Completed:
- Created `PROJECT_HISTORY.md` with project context, preferences, current state,
  and dated work history.
- Created `AGENTS.md` to instruct future coding assistants to read and maintain
  this record after meaningful tasks.

Verification: reviewed repository state and checked that both documentation
files exist and are not ignored by Git. No commit or push was performed.

### 2026-10-04 — Skills research (no installation)

Request: find suitable GitHub skills for database design, secure .NET backend
development, payments, and the wider gym system; explain before installing.

Completed: inspected upstream skill manifests and repository documentation.
Shortlist:
- OpenAI `aspnet-core`: .NET backend, EF Core, security/identity, testing.
  https://github.com/openai/skills/tree/main/skills/.curated/aspnet-core
- OpenAI `security-threat-model`: repository-specific threats and mitigations.
  https://github.com/openai/skills/tree/main/skills/.curated/security-threat-model
- Community `sql-schema-designer`: relational modeling, constraints, indexes,
  exact money types; supports SQL Server and PostgreSQL among other engines.
  https://github.com/jaskirat1616/grok-skills/tree/main/skills/sql-schema-designer
- .NET team's `scaffold-dotnet-test-project`: establish test project wiring.
  https://github.com/dotnet/skills/tree/main/plugins/dotnet-test/skills/scaffold-dotnet-test-project
- Supabase `supabase-postgres-best-practices`: conditional on PostgreSQL choice.
  https://github.com/supabase/agent-skills/tree/main/skills/supabase-postgres-best-practices
- Stripe `stripe-best-practices`: conditional on Stripe provider choice.
  https://github.com/stripe/ai/tree/main/skills/stripe-best-practices

Not recommended for this C# backend: OpenAI `security-best-practices`, whose
declared supported languages are Python, JavaScript/TypeScript, and Go.

Limitations: this is a fit-based shortlist, not a claim of universal ranking or
a full audit of every repository/script. Skills guide work; they do not guarantee
security. Before installation, inspect the complete selected folders and record
their source revisions. No skills, plugins, dependencies, or services installed;
no commit or push performed. Await explicit selection/approval.

### 2026-10-04 — Approved skills installed

Request: download the four recommended skills and explain project usage and
token impact. User explicitly approved these four.

Completed:
- Resolved and pinned the GitHub source revisions, downloaded the four folders
  using skill-installer, inspected their contents, and installed reviewed copies
  in `~/.codex/skills/` with filesystem approval.
- Verified source and installed file contents match using SHA-256: 17 files for
  ASP.NET, 5 for threat modeling, and 1 each for schema design and test setup.
- Created `SKILLS_GUIDE.md` with examples, source revisions, portability notes,
  token implications, and packaging limitations; linked it from `AGENTS.md`.
- Consulted official OpenAI documentation on skill context loading.

Limitations: schema skill mentions supporting files not shipped upstream; no
validator exists in that folder. Skills become available on the next turn.
No application changes, database setup, test runs, commits, or pushes performed.

### 2026-10-04 — Mandatory history checkpoint skill

Request: create a skill requiring project history and AGENTS.md updates after
operations. This request explicitly authorized creating and installing it.

Completed:
- Created `skills/project-history-update/SKILL.md` as portable repository source
  and installed an identical copy in `~/.codex/skills/project-history-update`.
- Updated `AGENTS.md` to require the skill after project operations, including
  investigations and blocked attempts, and added a bounded current handoff.
- Added usage and portability notes to `SKILLS_GUIDE.md`.
- Group related tool calls into one operation; keep detailed history out of
  permanent agent instructions and avoid recursive documentation logging.

Verification: basic manifest/name/description checks passed; installed source
matches the repository copy byte-for-byte. The bundled `quick_validate.py` could
not run because Python lacks PyYAML (`ModuleNotFoundError: yaml`). No dependency
was installed just for validation. Reviewed documentation and Git status.

Limitations: instructions depend on assistant/host compliance, not an executable
enforcement hook. Local installation becomes available on the next turn; the
repository copy can be read immediately or by another provider. No application
code changes, commits, or pushes performed.

### 2026-10-05 — Local PostgreSQL and API connection

Request: set up PostgreSQL and localhost connectivity.

Completed:
- Reused running Homebrew PostgreSQL 14.20, listening on localhost port 5432.
- Created `campions_gym` and dedicated `campions_app` development login; no
  superuser, CREATEDB, or CREATEROLE permissions. Account owns only its new
  project database for local migrations; revoked public database access and
  public schema creation in that database.
- Added targeted SCRAM authentication rules for the project role to existing
  pg_hba.conf and backed up the original. Existing unrelated trust rules remain.
- Generated a random password without printing it; saved connection to .NET
  user secrets. No secret value recorded in Git or these history documents.
- Added DAL `CampionsDbContext`, Npgsql EF provider 10.0.3, API DAL reference,
  secret configuration, scoped context registration, and development-only
  `/health/database`. Added all three projects to the previously empty solution.
- Added one-time `scripts/setup-local-postgres.py` and `LOCAL_DATABASE.md` guide.
- Initial build exposed existing Microsoft.OpenApi 2.0.0 advisory
  GHSA-v5pm-xwqc-g5wc; pinned patched 2.7.5 and rebuilt successfully.

Verification:
- App account connected with its generated password; login without a password
  was rejected. Confirmed its administrative privilege flags are false.
- Solution build completed with zero warnings/errors after the OpenAPI fix.
- Development API returned HTTP 200 with healthy database status using real
  EF/Npgsql connectivity. API verification process stopped after checks.

Limitations/next: existing PostgreSQL server version was not upgraded. Dedicated
local account is a database owner for development, not the production permission
model. No tables, schema migrations, or business tests created. Define the gym
schema next. No commit or push performed; earlier staged generated-file removals
and other user/assistant changes preserved.

### 2026-10-05 — Connect the business layer

Request: include BLL with DAL after the database setup.

Completed: added BLL's project reference to DAL and API's reference to BLL.
Retained API's DAL reference for the existing DbContext registration and local
health diagnostic. Updated `LOCAL_DATABASE.md` and the agent handoff.

Verification: `dotnet build CampionsGymSystem.slnx` succeeded with zero warnings
and zero errors. No business services or database schema added. Existing user
deletions of placeholder Class1.cs files preserved. No commit or push performed.

### 2026-10-05 — Complete EF Code-First persistence layer

Request: implement the supplied 26-module Gym Management persistence model,
Guid Identity, Fluent configuration, PostgreSQL constraints/indexes, and initial
migration; preserve N-Tier and existing project names/framework.

Discovery: all three projects target net10.0; API references BLL and DAL, BLL
references DAL. Existing DAL CampionsDbContext was empty, with no prior entities,
Identity, or migrations. Existing PostgreSQL development database was confirmed
local and empty before update. No projects renamed or target framework upgraded.

Completed:
- Replaced empty context with DAL/Data/ApplicationDbContext.cs inheriting Guid
  IdentityDbContext. Added ApplicationUser and 61 application entity classes,
  stable string-backed enums, and 61 separate Fluent configuration classes.
- Configured explicit restricted business FKs, composite junction keys, named
  plural tables, required unique/indexes, four current-row partial unique indexes,
  check constraints, financial/rate precision, date/time types and audit jsonb.
- Added an ActivitySubscription AccessMode snapshot to preserve package semantics.
  Guid user IDs are initialized; tracked audit updates/deletes are guarded in
  synchronous and asynchronous SaveChanges. Raw/bulk SQL remains outside that guard.
- Added Identity EF 10.0.8 (DAL), EF Design 10.0.8 (private API startup dependency),
  and local dotnet-ef 10.0.8 tool manifest. Retained Npgsql EF 10.0.3.
- API registers scoped context and IdentityCore roles/stores. No JWT, controllers,
  business services, integrations, background jobs, or startup migrations added.
- Renamed the local secret key to DefaultConnection without exposing its value;
  updated setup-local-postgres.py and LOCAL_DATABASE.md to the same key/context.
- Generated DAL/Migrations/20261005185159_InitialCreate.cs, metadata/snapshot,
  and docs/database/InitialCreate.sql; applied only to local campions_gym.
- Created docs/database/SCHEMA.md, BUSINESS_RULES_NOT_ENFORCED_BY_DB.md,
  VALIDATION.md and reusable transactional validate-local.sql.
- No data seeds: roles/permissions stay dynamic, and default sales commission
  rules must be created when real branches are configured.

Verification: solution build zero warnings/errors; migration SQL inspected for
68 tables (61 application/7 Identity), restricted business deletes, financial
precision, jsonb and required partial indexes; EF reported no pending model
changes. Local migration history contains InitialCreate. PostgreSQL catalog
assertions found 61 application tables, 7 Identity, 0 business cascades, 4 partial
unique indexes, 2 audit jsonb columns and no leftover synthetic users. All 17
negative constraint tests passed, with positive split-payment/refund, historical
assignment, unlimited attendance, ledger, JSON and cross-branch fixtures rolled
back. Isolated C# model checks passed for no shadow FKs/duplicate entities,
precision and sync/async audit guards without connecting to a database.

Corrected issues: initial secret reader needed BOM support; initial validation
fixture omitted standard Identity non-null flags. Both corrected and verified.
Expected migration-history-not-found logs occurred before first schema creation.

Assumptions/limitations: unspecified date fields use DateOnly; same-day schedules
use DayOfWeek 0=Sunday; event offsets must be UTC. Standard Identity claim keys
remain integers; users/roles/business keys use Guid. Production runtime grants
must enforce append-only audit and differ from the local migration owner. BLL
owns cross-record authorization/branch consistency, borrow limits, access blocking,
coach transfers, commissions, refund effects, payroll calculations and audit
redaction. No production connection used and no assistant Git commit or push.

### 2026-10-05 — Task-by-task workflow

User requested working one task at a time rather than implementing the entire
project at once. Recorded the rule in AGENTS.md and preferences/current state
here. Each selected task includes relevant verification and a history checkpoint;
the next task requires user direction. Documentation only; no application tests
needed. Reviewed changes and Git status. No Linear changes, commit, or push.

### 2026-10-06 — Private session commission and Freelance plan rule update

Request: smallest schema/documentation update for session-based Private commission
basis, unrestricted Coach transfer, unused-session expiry settlement and dedicated
Freelance membership plans; create a new migration without touching InitialCreate
or updating the existing database.

Completed:
- Added Normal/Freelance PlanCategory, optional restricted Freelance Coach FK on
  MembershipPlan and category/Coach purchase snapshots with check constraints.
  Price is the single gym amount source; FreelanceAgreement remains metadata.
  No extra rate, client ownership or freelance offer tables were introduced.
- Added purchased-batch RemainingSessions, immutable tracked purchase facts and
  FIFO index; PrivateSession now links to its purchased batch and CommissionPeriod.
  Assignment history uses EndedAt and the existing current-row partial unique index;
  added lookup index. Deprecated legacy financial fields retained to avoid data loss.
- Added PrivateExpirySettlement with restricted FKs, unique purchased-batch FK,
  count/unit/eligible snapshots, checked calculation and period lookup.
- Generated DAL/Migrations/20261006014644_UpdatePrivateAndFreelanceBusinessRules.cs,
  designer/snapshot and docs/database/UpdatePrivateAndFreelanceBusinessRules.sql.
  Added safe preflight rejection of unlinked legacy sessions/inconsistent private
  counters and backfill of pristine unused batches. Legacy plans/purchases become
  Normal/null because the old model had no Freelance purchase identity.
- Added one net10.0 xUnit DAL test project and registered it in the actual .slnx;
  PostgreSQL tests run real migrations in transaction-scoped isolated schemas.
- Updated business rules/schema/local database guidance and added seven Mermaid
  flows plus rollout/reconciliation notes and changed-file inventory. Marked the
  old validate-local.sql as InitialCreate-only. No BLL services/controllers/triggers.

Verification:
- Pre-migration and post-migration solution builds succeeded with zero warnings
  and errors; final solution build also passed with zero warnings/errors.
- Direct test-project and solution-level discovery each ran 20 tests: 20 passed,
  0 failed, 0 skipped.
  Covered all requested scenarios plus FK deletes, immutable batch prices,
  amount checks, unused legacy backfill and safe legacy migration rejection.
  All test schema/data changes rolled back; existing gym database untouched.
- EF reported no pending model changes. Reviewed generated SQL scope: only five
  existing target tables altered and the one new settlement table created; no
  table/column drops or cascading deletes in Up. InitialCreate, its designer and
  archived SQL match HEAD byte-for-byte. Git diff whitespace check passed.
- Sandbox blocked local PostgreSQL access and stalled .NET tooling; approved
  out-of-sandbox build/migration/test tooling completed the verification. No
  credentials or connection strings were exposed or recorded.

Decisions/limitations/next:
- Existing legacy session batch/period links cannot be inferred safely. A rollout
  with such data must reconcile it explicitly; the migration refuses to guess.
  Deprecated assignment values are retained until an approved cleanup is safe.
- BLL still owns FIFO initialization/consumption, branch/member/Coach validation,
  locking/idempotency, immutable history/closed periods, Coach-at-expiry selection,
  zeroing live expiry counters, configurable CommissionRule calculations and gym-only
  Freelance payment/offer accounting. These schema tests do not implement services.
- DateOnly EndDate is retained; branch-local expiration and monetary rounding need
  explicit BLL policy. EF purchase immutability does not intercept raw/bulk SQL.
- No existing development/production database migration was applied. Obtain explicit
  user permission before applying to the existing development database. No commit,
  push, unrelated module redesign or next-task work performed.

### 2026-10-06 — Auth subtask 1: BLL identity context and permission queries

Inspected the solution, project references, Identity configuration/entities,
current branch and existing tests/persistence. Auth branch is clean at 6c305d2;
previous schema work was already committed by user. GitHub Auth access verified.
No JWT/cookies, service/DTO pattern or seeding mechanism exists. IsActive and
all employee/branch/permission persistence already exist; no schema change needed.

Plan: (1) BLL services/tests, (2) JWT API/tests, (3) dynamic permission policies/
HTTP tests, (4) documentation/final verification. User explicitly authorizes
commit/push after each completed verified small subtask; no unrelated endpoints.

Added BLL AuthenticationService, UserContextService, CurrentUserService,
PermissionService, DTOs/reusable abstractions and configurable business timezone.
Identity verifies passwords, counts failures and honors lockout; inactive users/
employees are rejected. Password-only login refuses MFA-enabled accounts because
MFA challenge support is out of scope. Employee is optional; branch uses date-valid
active-branch assignments, primary preferred, then latest start and Id tie-break.
Default business date uses Africa/Cairo, configurable because schema lacks branch
timezones. Permissions query live Identity role mappings; active codes are distinct.

Reused existing DAL.Tests project, adding only BLL reference and auth test fixture/
10 tests. All test schemas/data roll back. Build passed zero warnings/errors;
10 focused auth tests passed; all 30 solution tests passed (0 failures/skips).
The verified subtask is ready for the requested commit/push after this checkpoint. Remaining: API tokens/endpoints, policies and docs.
No database migration/update, real users, roles or privileges seeded.

### 2026-10-06 — Auth subtask 2: JWT login and current-user API

First auth subtask dc933b0 committed and successfully pushed to origin/Auth.
Added JwtBearer 10.0.8 to API and Mvc.Testing 10.0.8/API reference to existing tests.
Added JwtOptions/startup validation, JwtTokenIssuer, HTTP claims accessor, active
user JWT events and DI registration. JWT validates HS256 signature, issuer,
audience, expiry (30-second clock skew); key must decode from base64 to at least
32 bytes. Configured 30-minute default (allowed 1..60); committed key is empty.
No secret/token logging or stored tokens. Access tokens contain sub/name/role,
jti/iat and expiry metadata, never permission lists or financial data.

AuthController implements POST /api/auth/login and authenticated GET /api/auth/me.
Login returns generic 401, validation 400; /me uses BLL DB-backed context. Bearer
requests recheck active account/employee/lockout; role claims are refreshed from
current Identity membership. Added standard ProblemDetails/exception handling,
auth middleware and controllers while preserving existing diagnostics/template.
Connection configuration validation moved after host configuration is complete;
initial 13 HTTP tests failed at premature connection lookup, corrected run passed
all 13. Added seven invalid JWT settings tests. Build passed zero warnings/errors.

All 50 solution tests passed (0 failures/skips). JWT/API subtask is ready for
the requested commit/push. No schema changes or seed mechanism added. Existing optional-email Identity policy
retained; ambiguous legacy emails are refused safely. Requested commit/push follows this checkpoint, then permission policies and
documentation.

### 2026-10-06 — Auth subtask 3: dynamic permission authorization

JWT/API subtask e72ff74 committed and successfully pushed to origin/Auth after
all 50 solution tests passed. Added PermissionRequirement, PermissionPolicyProvider
and scoped PermissionAuthorizationHandler. Endpoints declare Permission:<code>;
provider delegates ordinary/default policies to the framework provider. Handler
queries current distinct active permission codes from Identity role mappings;
there is no per-role policy registration, Redis or persistent permission cache.

Added eight HTTP tests and a controller present only in the test assembly:
200/401/403, /me permission union/deduplication/inactive exclusion, permission
revocation/deactivation, role revocation with existing JWT and normal-policy fallback.
No production debug or business endpoint. Build passed zero warnings/errors.
All 21 HTTP authentication/authorization tests passed (0 failures/skips).
Requested commit/push follows this checkpoint. Remaining: documentation/bootstrap strategy and final regression checks.

### 2026-10-06 — Auth subtask 4: documentation and final verification

Permission policy subtask 24ee95b committed and successfully pushed to origin/Auth.
Created docs/authentication/AUTHENTICATION.md, AUTHORIZATION.md and
ADDING_A_PERMISSION.md. Added secure key setup, login/JWT/current context flows,
branch access boundaries, requested full permission catalog, conservative role
defaults, trusted Identity bootstrap strategy, exact permission attribute, test
commands and implementation-file inventory. Updated .http auth request examples.
No automatic seeding because there was no existing mechanism; no real accounts or
privilege grants were created. MFA challenge/refresh/branch switching stay out of
scope. Default Africa/Cairo business date and inclusive employment/assignment dates
are documented. Technical/Owner accounts may lack Employee/current branch.

Final build passed zero warnings/errors. All 58 solution tests passed, 0 failures/
skips (38 auth/policy/configuration and 20 prior schema tests). EF reported no
pending model changes; DAL source/migrations unchanged from task baseline. Fixtures
run real PostgreSQL migrations and HTTP JWT pipelines in isolated transaction-scoped
schemas, then roll back. All twelve requested cases and invalid JWT/configuration,
revocation, lockout and MFA safety are covered. No existing database migration/data
update or production connection. Whitespace check passed.

A concurrent user edit removing weatherforecast registration from Program.cs was
detected after the feature commits; preserved, not staged/published by assistant.
Final documentation commit a907b76 successfully pushed to origin/Auth, following
the three verified implementation commits dc933b0, e72ff74 and 24ee95b. This
publication-state checkpoint accompanies the final history commit/push. Stop at
authentication; no Members CRUD or next task. Only concurrent user Program.cs
weather-route removal remains outside assistant commits.

## 2026-10-06 — Persistent local authentication demo data

- Requested persistent API test data and Postman collection; completed the data subtask.
- Added DevelopmentAuthSeeder, explicit Development/localhost-only --seed-auth-demo command,
  secret setup script, ignored private Postman environment and local testing guide.
- Seeded local accounts for Owner, Reception, no permissions, inactive and suspended employee.
  Existing accounts/grants are preserved on rerun; no business workflows or migrations applied.
- Build: zero warnings/errors. All 61 tests passed, including three fixture/idempotence tests.
- Credentials/key stored only in user secrets/private ignored environment, never in Git.
- User weather cleanup is already committed as c3752e7. Preparing this verified subtask
  for commit/push to Auth as requested; next subtask is the collection and live API verification.

## 2026-10-06 — Postman collection and live API verification

- Created postman/Campions.Auth.postman_collection.json, blank shareable environment
  and README. Private local environment remains ignored; no secrets/tokens committed.
- Ran against the actual local seeded API: 12 requests, 18 assertions, zero failures.
  First run exposed a Postman sandbox variable collision; corrected and reran successfully.
- Verified successful logins, Owner without branch, employee/branch/permissions,
  no-permissions user, inactive/suspended rejection, invalid credentials/input/token,
  and unauthenticated /me. Business permission 200/403 remains covered by automated tests.
- No backend packages or schema changes added for Postman. Newman used through npx.
- Seed setup is committed/pushed as 8f73967. Preparing collection/checkpoint commit
  for push to Auth. API verification process stopped; start it using the README command.
- Authentication scope complete; await next user-selected task. All 61 solution tests
  passed and build had zero warnings/errors before collection verification.

## 2026-10-06 — Clarified demo login credentials

- User asked which usernames/password were created and what demoPassword means.
- Read the private local environment to provide the requested local demo credential
  directly to the user. No credential values recorded in project documentation.
- No application changes or new tests; previous build/test results remain unchanged.
- Collection/setup already pushed as 8f73967 and 3e8afee. This explanation checkpoint
  is documentation only; no additional commit/push performed.

## 2026-10-06 — Simplified Postman login and token forwarding

- User found many saved scenarios confusing and reported manual token pasting.
- Changed collection name to Campions Auth - Simple Login, exposing only Login and
  Current user at top level; moved original scenarios to optional folder.
- Login saves latest accessToken in collection variables; Current user explicitly
  sends that saved token, independently of the old account-specific token variables.
- README explains reimport, successful-login requirement and token refresh by login.
- Newman against user's running local API: 14 requests, 21 assertions, zero failures.
  No backend/schema changes; no build rerun needed for JSON/docs-only change.
- Preparing verified collection update for commit/push to Auth. Existing uncommitted
  clarification history was preserved; no credentials included. API left running.

## 2026-10-07 — Activities, Classes and Freelance business-rule update

Request: inspect existing persistence/auth implementation and apply the latest activity
coach, Class attendance and Freelance eligibility/House rules without redesigning the
system, editing applied migrations or applying destructive updates.

Findings: 62 application/7 Identity tables; ActivityCoachAssignment required an Employee;
activity purchases had no immutable coach/rate; no actual class occurrence existed;
Freelance plans/snapshots existed but eligibility/House purchase enforcement did not.
Generic CommissionRules allowed target/fixed/Class formulas but no business services
used them. Refund OriginalPaymentId was optional and ambiguous for installment attribution.

Changes:
- Four new tables: ActivityCoaches, ActivityCoachSettlements, ClassSessions,
  MembershipPlanEligibleMembers; new coach/session enums and restricted relationships.
- Extended activity assignments/subscriptions/batches, CheckIns and SaleTransactions.
  Preserved legacy Employee links and unknown historical values rather than inventing
  coaches, percentages, occurrences, eligibility or sales history.
- Added focused BLL management, initial/renewal purchase, attendance and reporting
  services; permission/branch checks and AuditLog entries; scoped DI and protected
  API endpoints. Kept N-Tier, existing identity, and response conventions. No packages.
- Activity percentages snapshot at purchase. Actual completed Payment counted once
  and checked against parts, minus refunds linked to original payments. Closed snapshots
  immutable; internal totals bridge existing employee summaries once, external totals
  remain separate amounts owed. Ambiguous legacy revenue refuses calculation.
- Class completed zero-attendee sessions count in average; cancelled/scheduled excluded;
  actual delivering coach receives stats. No Class financial calculation generated.
- Freelance eligibility required on every purchase/renewal. Sale snapshot House with no
  Sales employee/category; old Sales assignment ends and House appended atomically.
  Past purchases survive removal/plan edits. No global member-coach constraint.
- Generated 20261007000549_UpdateActivitiesClassesAndFreelanceRules and forward SQL.
  Four new tables, six nullable columns, old Employee FK made nullable, checks/indexes;
  no Up table/column drops. Eight permissions and existing Owner grants added by migration.
  Old migrations unchanged. No user development/production database update performed.
- Updated schema/rules/flows, local seed catalog count and detailed rollout/file manifest.

Verification: pre-migration and final builds passed with zero warnings/errors. First
expanded test build had an async-lambda syntax error, corrected before test execution.
28 initial business tests passed; expanded full suite passed 98; final regression suite
passed 103 (0 failed/skipped, 42 additional cases). PostgreSQL migration/constraint tests
and HTTP authorization passed, including legacy-data preservation. EF pending-model
check reports no drift; diff whitespace check clean. Test data/schema fully rolled back.

Decisions/limits: rate changes apply to future purchases, not old installments. Closed
period activity finalization is explicit; no whole-period close/reopen or payment engine
introduced. Reopening retains prior snapshot. Late-refund/correction workflow and Class
payout rules remain unresolved intentionally. No arbitrary mid-batch coach reassignment.

Commit/push state: clean repository at start; current user-selected branch DataBaseInit.
All new update/checkpoint files remain uncommitted/unpushed; earlier Auth-specific push
workflow was not used to publish this separate schema/business change to another branch.
Next: review/apply pending migrations explicitly after required legacy reconciliation;
then grant any additional non-Owner management permissions as intended. Stop here.

## 2026-10-07 — Final business schema alignment

Request: inspect the complete current schema, classify gaps against latest confirmed
rules and add only missing persistence support; no controllers/features, no previous
migration rewrites, no destructive data changes or offline architecture.

Inspection: all entities/configurations, enum/key/index/FK conventions, Guid Identity,
context, three existing migrations, snapshots and schema documentation. Existing
Class occurrences/unique attendance, internal/external Activity coaches/purchase rate
history, Freelance coach/eligibility/House snapshots and Private session/expiry facts
already supported the rules. Roles existed without direct user overrides; BusinessDay,
audience categories and leave were absent; ShiftEmployees lacked replacement/actor/reason.
Short gap report delivered before code and recorded in FINAL_SCHEMA_ALIGNMENT.md.

Changes:
- Added UserPermissionOverrides (composite Guid user/permission PK, checked Allow/Deny,
  optional creator); BusinessDays (date, timestamps/lifecycle and unique branch/date,
  partial unique open branch); MembershipPlanCategories (dynamic unique nonblank name);
  EmployeeLeaveRequests (dates/status/review metadata); ShiftEmployeeChanges (distinct
  original/replacement employee, shift, actor, time/reason). Five Fluent configurations,
  three string-stored enums and DbSets, all restricted FKs.
- Added nullable BusinessDayId to Shift, SaleTransaction, Payment, Refund, Expense,
  CheckIn, FinancialTransaction and ClassSession; nullable category FK on MembershipPlan.
  Existing rows stay unknown, not inferred from midnight timestamps or fake seeds.
- Membership/Private/Activity purchase batches derive day via SaleTransaction; parts
  via their parent. No duplicate BusinessDate or plan-city/branch access model added.
- Preserved all existing feature and identity code. No authorization resolution change:
  future feature must implement Roles + Allows - Denies; stored overrides are not yet
  effective at runtime. No leave/day/shift services or controllers implemented here.
- Verified existing private facts support latest transferred-client 50% work/expiry
  rule; updated conflicting earlier documentation, no rate hardcoded in schema/trigger.
- Generated 20261007151235_FinalBusinessSchemaAlignment and reviewed SQL: five tables,
  nine nullable FK columns with indexes/checks. No Up table/column drop, rename,
  alteration or data backfill. Previous migrations unchanged; no new permissions seeded.
- Updated SCHEMA, BUSINESS_RULES_NOT_ENFORCED_BY_DB, DATA_FLOWS, prior Private rollout
  clarification, detailed gap/file/verification report and handoff.

Verification: build before migration and final build zero warnings/errors. First full
suite passed 127; final suite with explicit legacy-sale/shift/plan upgrade case passed
128 (25 additional schema/migration cases), zero failed/skipped. Existing activity
migration test adapted to apply through latest before reading current EF optional
columns. PostgreSQL fixtures/DDL/data rolled back. EF has-pending-model-changes reports
no drift; git diff --check clean. API/BLL and old migration paths confirmed unchanged.
No user's development or production database migration applied.

Decisions/remaining: no ManagerId, EAV, duplicate class/activity/freelance concepts,
permanent member-coach constraint, payment tables, payroll redesign, sync/node/outbox
architecture or speculative per-table timestamps. Business rules cross-table checks
remain BLL work. Class payout and finalized activity adjustment rules remain unresolved
as before; storage alignment is complete and feature work can proceed vertically.

Commit state: repository started clean on DataBaseInit at user commit c78d95a; this
alignment and checkpoint remain uncommitted/unpushed. No branch switch or automatic
publication. Stop here and await next user-selected feature/deployment instruction.
