# Architecture

## Technology Stack

- C# targeting .NET 10; ASP.NET Core controller-based Web API.
- Entity Framework Core 10 with Npgsql and PostgreSQL (provider configured for
  PostgreSQL 14 compatibility).
- ASP.NET Core Identity with Guid users/roles and JWT bearer authentication.
- Built-in dependency injection, configuration/options, ProblemDetails and OpenAPI.
- xUnit, WebApplicationFactory and real PostgreSQL fixtures for verification.

## Solution Structure

| Project | Responsibility |
|---|---|
| CampionsSystem.API | HTTP contracts/binding, controllers, JWT and permission policies, composition and middleware |
| CampionsSystem.BLL | Feature services, validation, authorization/scope, transactions and audit calls |
| CampionsSystem.DAL | Entities, ApplicationDbContext, EF mappings, constraints and migrations |
| CampionsSystem.DAL.Tests | Model, service, migration, concurrency and HTTP integration tests |

BLL code is grouped by feature: Authentication, Branches, Employees, Members, Crm,
Plans and Operations. Larger services use partial files within their feature.

## Request Flow

```text
API Controller → BLL Service → ApplicationDbContext / EF Core → PostgreSQL
```

Controllers apply permission policies and delegate work. BLL services also enforce
permissions and record scope; they use the existing DbContext directly. API supplies
HTTP identity through an interface, while BLL resolves the database-backed user,
Employee, permissions and operational branch. There is no generic repository,
CQRS/MediatR pipeline or separate service deployment per feature.

## Architecture Style

A modular monolith: one API process and shared relational model, with feature-based
business services and N-Tier project boundaries. Current infrastructure does not
introduce distributed queues, caches or synchronization services.

## Core Architectural Principles

- **Permission plus scope:** action capability does not independently grant access
  to every branch. Explicit CRM ownership exceptions are subject-specific.
- **Current branch:** effective Employee assignments resolve operational context;
  branch is not trusted from an arbitrary request or persisted in the JWT.
- **History preservation:** dated assignments are closed and replaced; business FKs
  commonly restrict deletion. Management endpoints generally deactivate records
  instead of deleting them. Current plan configuration relationships may be removed
  without deleting independent subscription snapshots.
- **Audit:** services reuse AuditLogs and OperationAccess for actor, branch, entity,
  old/new values and timestamps. Existing IP/device columns do not imply those
  values are captured by every service.
- **Transactions:** multi-row workflows use transactions/savepoints, PostgreSQL row
  locks, atomic updates and database constraints. Phone identity workflows also use
  transaction-scoped advisory locks.
- **Identity:** core domain entities use Guid technical IDs. Human identifiers such
  as MemberCode and branch-local ClientCode have separate purposes.

## Important Domain Relationships

| Relationship | Meaning |
|---|---|
| Employee → optional ApplicationUser / Member | Personnel, login and customer identities remain distinct |
| Employee → EmployeeBranchAssignments → GymBranch → City | Effective working branch with history |
| Member → MemberSalesAssignments → Employee | Current owner and ownership history |
| Lead → LeadAssignments / LeadInteractions | Distribution and append-only contact history |
| Lead → ConvertedMember | Conversion preserves the prospect's history |
| Member → MemberFollowUpTasks | Work ownership derives from current Sales assignment |
| MembershipPlan → Branch / City / audience category | Administration, access and audience are separate |
| MembershipPlan → benefits / eligible Members | Current product configuration |
| Subscription transaction → benefit snapshots → usages | Historical entitlements are separate from plan templates |

## Implemented Scope and Foundations

The implemented areas in the [index](README.md) have API/BLL/test support. DAL also
contains foundations for subscriptions, financial records, CheckIns, guest invitations,
commission and other operational areas; schema presence is not a complete workflow.
Employee gym access has model/constraint support, not an automatic grant/revoke service.

CoachingController implements Private and shared Classes purchases/attendance, independent
coach targets, finalized earnings and actual partial payouts using existing Sales and
Expense financial paths. Classes compensation remains disabled pending an Owner formula.
Legacy package-free class writes return 410; historical reports remain readable.
See [Private / Classes](PRIVATE_CLASSES.md). Existing OperationsController and services
retain limited activity purchases, activity-coach configuration and historical reports.
Those specific paths must not be confused with a complete general subscription,
payment-provider, renewal/freeze, attendance or commission/payroll feature. Member,
CRM and plan management commands do not purchase subscriptions or take payments.

## Runtime and Verification

Program.cs wires feature services, exception handling, HTTPS redirection,
authentication, authorization and controllers. OpenAPI and a database connectivity
endpoint are Development-only. Migrations and guarded local setup commands are
explicit operations, not automatic startup migrations.

The solution build and full test suite are the normal verification entry points.
PostgreSQL tests use isolated rollback schemas; concurrency fixtures may create/drop
private test schemas. They do not apply migrations to the configured gym application
database. EF model drift is checked with `migrations has-pending-model-changes`.
