# Private and Freelance schema rollout

Current Private/Classes implementation (2026-10-12): [Private, Classes and coach earnings](../PRIVATE_CLASSES.md) supersedes earlier financial/workflow assumptions below. Original coaches earn on their delivered funded sources; incoming transferred assignments earn fixed 50%; expiry unused quantity goes to the coach assigned at expiry without attendance or zeroing actual remaining counters. Earnings and actual partial payouts are separate.

Latest clarification (2026-10-07): transferred-client Private work and expiry use the
fixed 50% rule in future BLL. Earlier configurable-rate guidance below applies to
ordinary non-transferred work; historical basis/coach/session facts stay unchanged.
See [final alignment](FINAL_SCHEMA_ALIGNMENT.md).


This update extends the applied InitialCreate with
UpdatePrivateAndFreelanceBusinessRules. InitialCreate and its archived SQL are
unchanged. No existing database was updated as part of this task.

## Legacy data safety

PrivateSessions previously had neither purchase-batch nor commission-period FK.
Those links cannot be reliably inferred, especially across renewals with different
prices. The new migration refuses to apply if legacy PrivateSessions exist or a
subscription's total/remaining counters differ from its purchased session totals.
Reconcile those records explicitly before rollout; do not invent links or erase
history merely to pass the guard. With no prior consumption and consistent totals,
each existing purchased batch is backfilled RemainingSessions=SessionsAdded.

PrivateCoachAssignment legacy SessionsAllocated, SessionsConsumed and
CommissionBasisAmount remain stored and are marked Obsolete. New rows use zero
defaults. These fields no longer determine financial ownership; remove them in a
later explicitly approved migration after reconciliation confirms it is safe.

Old membership plans and purchase transactions are backfilled Normal with a null
freelance Coach. The prior model had no category or coach-specific purchase link,
so legacy freelance classification cannot be reconstructed from FreelanceAgreement.
Review historical classification explicitly if external records indicate otherwise.
New purchases must snapshot category and coach from the selected plan.

No table is dropped in Up. All new history relationships use Restrict. Down drops
the new settlement table and columns and therefore loses new financial history;
never roll back after real usage without a reviewed data recovery plan. The unchanged
InitialCreate migration remains applied to the existing development database; validate-local.sql is an InitialCreate-only historical smoke script, not the
current-schema test suite. Use the DAL integration tests for the updated schema.

## Validation commands

From the repository root:

```sh
dotnet build CampionsGymSystem.slnx
dotnet ef migrations script InitialCreate UpdatePrivateAndFreelanceBusinessRules \
  --project CampionsSystem.DAL --startup-project CampionsSystem.API --no-build \
  --output docs/database/UpdatePrivateAndFreelanceBusinessRules.sql
dotnet ef migrations has-pending-model-changes \
  --project CampionsSystem.DAL --startup-project CampionsSystem.API --no-build
```

Review SQL, backup/reconcile meaningful legacy data and obtain explicit permission
before applying to the existing development database. Do not update production.

## PostgreSQL integration tests

CampionsSystem.DAL.Tests is registered in CampionsGymSystem.slnx and references DAL.
Each test opens a PostgreSQL transaction, creates a uniquely named schema, uses
SET LOCAL search_path, runs the real migration scripts there, then rolls everything
back. Existing schemas/tables/migration history are untouched. Use a disposable
local test database/login with CREATE SCHEMA privileges. Do not use production.

The default test connection uses localhost, database postgres and the current OS
username (for this workstation's local trusted development login). Override with
GYM_SCHEMA_TEST_CONNECTION from a secure local environment when needed; do not
commit credentials. No server or Docker container is started by tests.

```sh
dotnet test CampionsSystem.DAL.Tests/CampionsSystem.DAL.Tests.csproj
dotnet test CampionsGymSystem.slnx
```

Tests exercise category checks, different Coach prices, immutable purchase
snapshots, single-current assignment, transfer after closed period, retained old
Coach/period, new Coach sessions, batch links, duplicate settlement rejection,
current-Coach settlement, checked amount calculation, restricted deletes and
tracked immutable batch price with mutable counters, unused-batch migration
backfill and safe rejection of ambiguous legacy sessions/counters. Tests construct the documented
operations directly; they do not claim that future BLL validation is implemented.

DateOnly EndDate is retained; BLL must define the branch-local final expiration
instant. No service, trigger, payment provider, external coach revenue, extra pricing
source or special freelance Offer/client table is introduced.

## Changed files

- Entities under `CampionsSystem.DAL/Entities/`: MembershipPlan.cs,
  MembershipSubscriptionTransaction.cs, PrivateCoachAssignment.cs,
  PrivateSubscription.cs, PrivateSubscriptionTransaction.cs, PrivateSession.cs;
  new PrivateExpirySettlement.cs.
- Configurations under `CampionsSystem.DAL/Configurations/`:
  MembershipPlanConfiguration.cs, MembershipSubscriptionTransactionConfiguration.cs,
  PrivateCoachAssignmentConfiguration.cs, PrivateSubscriptionTransactionConfiguration.cs,
  PrivateSessionConfiguration.cs; new PrivateExpirySettlementConfiguration.cs.
- `CampionsSystem.DAL/Enums/PlanCategory.cs` (new),
  `CampionsSystem.DAL/Data/ApplicationDbContext.cs`.
- Migrations: `20261006014644_UpdatePrivateAndFreelanceBusinessRules.cs`, matching
  `.Designer.cs` (new), and `ApplicationDbContextModelSnapshot.cs`.
- `CampionsGymSystem.slnx`; new `CampionsSystem.DAL.Tests/CampionsSystem.DAL.Tests.csproj`
  and `CampionsSystem.DAL.Tests/PrivateAndFreelanceSchemaTests.cs`.
- Documentation: `docs/database/BUSINESS_RULES_NOT_ENFORCED_BY_DB.md`, `SCHEMA.md`,
  `VALIDATION.md`, baseline-only notice in `validate-local.sql`; new `DATA_FLOWS.md`,
  this rollout note and `UpdatePrivateAndFreelanceBusinessRules.sql`.
- `LOCAL_DATABASE.md`, `PROJECT_HISTORY.md`, `AGENTS.md` handoff checkpoint.
