# Subscriptions / Renewals / Freeze

Implemented 2026-10-09 in the existing API → BLL → EF Core → PostgreSQL modular
monolith. This feature provides entitlement lifecycle services and safe management
endpoints. Sales/Payments and Check-In remain separate future integrations.

## Authoritative model and history

`MembershipPlan` is the current product template. `MemberSubscription` is one Member's
entitlement period, with originating `PurchasedAtBranchId`, access `CityId`, mode,
start/end, nullable session balances, optional Employee link, version and optional
`FollowsSubscriptionId`. The original source plan stays associated with the period;
renewal into another compatible plan is represented in grant history.

Existing `MembershipSubscriptionTransaction` records are append-only grants. Each
new grant snapshots source plan ID/name/category, owning branch, access city, mode,
duration, sessions, freeze allowance, plan/net price metadata, Freelance Coach,
effective grant start, old/new end, previous remaining sessions, source period,
BusinessDay, operation identity, actor and time. Prices are metadata; no money is
collected or recorded by this feature. Same-mode early renewal appends another grant
to the same period. Expired renewal and incompatible/mode-changing early purchases
create a new period and reference the prior period in their immutable grant.

Existing `MemberSubscriptionBenefit` records belong to each grant and snapshot
benefit code/name/type and granted quantity. Quantity benefits store their positive
grant; Access benefits have null quantity. Existing `MemberBenefitUsage` remains
available for future consumers; consumption is not implemented here. Plan/benefit
edits do not change historical grants, benefit snapshots or access scope.

Existing `SubscriptionFreeze` holds Normal and Medical history. New
`SubscriptionHolidayExtension` records protect one subscription/business date once,
even when declared closures overlap. EF rejects editing/deleting grant/benefit/holiday
facts; a Normal freeze allows only its first completion actor/time update.
Existing historical financial records and optional sale links are preserved.

Legacy facts that were never captured remain null rather than being reconstructed
from today's plan. Details expose them honestly. Renewal without historical category,
freeze without snapshot allowance/complete dated history, and ambiguous legacy
closure configuration fail with a reconciliation message. A legacy Frozen flag with
no dated history denies current access. Reconciliation requires a deliberate future
administration/data operation; no automatic migration invents historical facts.

## Dates, status and current access

Customer activation/renewal uses the originating branch's open `BusinessDay.BusinessDate`.
If none exists, it uses the configured gym timezone calendar date (Africa/Cairo by
default). An overnight open business day therefore remains authoritative after
midnight. Its ID is recorded when available. Employee validity follows employment
calendar dates rather than an overnight paid business day.

Both StartDate and EndDate are inclusive: a 30-day grant starting October 10 ends
November 8. It expires November 9. Unlimited stores null TotalSessions and
RemainingSessions, never a fake large session count. SessionBased starts with its
positive snapshot quantity and expires after EndDate even with unused sessions.
Zero remaining sessions also deny entitlement. No attendance deduction is provided.

Status uses dates, dated Normal freezes, Member activation and active linked employment,
while preserving existing Cancelled/SuspendedForInstallment states. Supported effective
states are Scheduled, Active, Frozen, Expired, Cancelled and the existing installment
suspension state. No installment workflow is added. Employee periods have no fake
far-future expiry and closed employee periods remain non-entitled.

`GetCurrentEntitlementAsync` answers access for the caller's selected operational
branch (current branch by default). It requires an active branch/city and the
subscription's single-city snapshot. Frozen entitlement can be returned with
HasAccess=false; Scheduled periods never grant access early. An applicable operational
gym closure denies access at the selected branch. Multiple conflicting current
periods fail closed for reconciliation. This query does not execute Check-In.

## Renewal and scheduling

| Renewal | Dates | Sessions |
| --- | --- | --- |
| Active Unlimited → compatible Unlimited | Add new duration after current inclusive end | Null |
| Active SessionBased → compatible SessionBased | Add new duration after current inclusive end | Remaining old + configured new |
| Expired customer period | New period starts on new effective BusinessDate | Only configured new; old unused balance remains in old history |
| Active Unlimited ↔ SessionBased | Separate Scheduled period starts the day after current effective end | Each period retains only its own sessions/mode |

An active 4-session balance plus a new 12-session grant becomes 16. After expiry,
the new cycle receives 12 and the old row still shows the expired 4.
Compatible renewal may use another PlanId; compatibility requires unchanged mode,
access city, originating branch and behavioral category. A change to another scope
or category also uses a separate scheduled period to protect old entitlement facts.

One unstarted successor is supported per current period, protected by member locks,
version checks and a unique predecessor link. Another conflicting pending period is
rejected. A Scheduled period cannot be renewed before it starts. Its effective status
becomes Active from authoritative dates without a background scheduler. Legitimate
renewal/freeze/medical/holiday extension shifts an unstarted successor's start and
end together, preserving duration and append-only purchase facts and recording a
ScheduleShifted audit. Already-started successor history is never moved backward or
rewritten; conflicting retrospective changes require reconciliation.

## Normal freeze

A current customer entitlement can book a positive, non-overlapping inclusive freeze
interval, starting no earlier than the effective business date and within remaining
entitlement. Normal allowance is the sum of its immutable grant allowances, less
booked Normal DaysApplied. Later plan changes cannot alter it. Employee entitlement,
expired periods, unknown legacy allowance and insufficient allowance are rejected.

Booking consumes the approved days and extends EndDate by those days, excluding any
same-day compensation already granted for a holiday. `DaysAlreadyCompensated` records
that exclusion separately from booked allowance. Any unstarted successor moves with
the effective end. Frozen access is derived only during the approved Normal interval.

Completion is an idempotent administrative acknowledgment after the inclusive freeze
end. It preserves dates, consumed allowance, extension and history. Ending a freeze
early/cancelling its approved days is not supplied by this workflow. Access resumes
from dates without waiting for the completion acknowledgment.

## Medical/custom extension

`Subscriptions.Extend` is the elevated capability. A positive duration, operation
identity, expected version and nonblank reason are required, within originating
branch scope. The existing Medical distinction records an immediately completed
exceptional extension with actor/time, reason and old/new end. It extends a current
Active/Frozen customer period and shifts an unstarted successor. It does not consume
Normal allowance or create an access-blocking Normal freeze. It does not revive an
expired period or add artificial duration to employment-bound entitlement.

## Owner-controlled official holidays

Creating/applying subscription-affecting closure requires both `Subscriptions.Extend`
and `Branches.AccessAll`, without a runtime role-name check. Owner/global management
selects the branch and inclusive date range (up to 366 days), reason and
`ExtendSubscriptions`. Existing `BranchClosure` is reused:

- AppliesToGym=true identifies a physical gym closure.
- ExtensionDays=0 means compensation disabled, even though the branch closes.
- ExtensionDays equal to the inclusive range length enables one protected day per date.

The service never infers compensation from CityId, whether another branch is open,
or city-wide availability. For multiple branches, declare each selected branch closure.
Affected customer subscriptions use their originating PurchasedAtBranchId, not Member
HomeBranch or access CityId. Their date period must include the holiday date; future,
already-expired-before-that-date, cancelled/suspended and non-entitled periods are
excluded. Employment-bound free entitlement needs no date compensation.

A frozen holiday day records DaysApplied=0; another entitled day records 1 and extends
EndDate by one. Seven freeze days including one holiday protect seven days, not eight.
The reverse application order also gives seven days total. A unique subscription/date
key protects retries and overlapping closures, and history includes zero-day records.

Creation applies only dates already reached in the branch's effective business day.
Future closure declarations need the protected apply endpoint when their dates arrive;
there is no scheduler. Internal customer grants/renewals also apply due, already-declared
Owner policy for that Member. An attempted retrospective extension conflicting with an
already-started successor fails transactionally instead of rewriting started history.

## Normal, Freelance and Employee rules

Normal grants require an active Normal plan and Member, offered by the originating
branch. Member registration at A does not prevent subscribing to B's plan. Neither
HomeBranch nor Sales ownership changes.

Freelance additionally requires the plan's active Freelance Coach and explicit eligible
Member. The plan's price is the gym's fixed future commercial amount. Sales-owned
Members retain the exact assignment; House Members stay House. No private Coach
payment, SaleTransaction, Payment, ledger or ownership change is created. Legacy
`PurchaseFreelanceAsync` is disabled and its authorized HTTP route returns 410; existing
historical financial reports remain available for their old records.

Employee grants require both Subscriptions.Manage and Employees.Manage, active eligible
employment, an explicit existing Member link (or protected first link), active Employee
plan, zero price and Unlimited mode. The current employment branch must own the plan.
The grant is free/internal, with null EndDate and no sale. Normal branch transfer keeps
employment and entitlement active; access follows the immutable city scope. Existing
employment-status transactions close entitlement on suspension/resignation/termination.
Reactivation/rehire requires a new explicit grant after the prior closed period, preserving
old IDs and history. Same-day overlapping closed/new periods are rejected.

## Branch and authorization separation

Registration/HomeBranch, plan owning BranchId, subscription originating PurchasedAtBranchId,
access CityId and Sales owner's branch have different purposes. Administration/history
use origin scope; current access can consume a subscription from another same-city branch
within the caller's own operational branch. Sales ownership provides no subscription
administration privilege. Tanta's snapshot does not grant Cairo access or multi-city access.
Plan edits and employee branch transfers do not rewrite the old city snapshot.

Four capabilities are added: Subscriptions.View, Manage, Freeze and Extend. Global access
uses existing Branches.AccessAll. Customer grant/renewal integration requires Manage plus
global capability and has no HTTP route before Finance. Freeze requires Freeze; exceptional
extension requires Extend; Employee start additionally requires Employees.Manage.

An explicit Development/localhost-only catalog command is available:

```sh
dotnet run --project CampionsSystem.API --no-launch-profile -- --environment Development --configure-subscription-permissions
```

It preserves existing activation states, role grants and user overrides. By default it
creates only the catalog. An explicitly configured `Subscriptions:ManagementRoleId` may
grant the capabilities to an existing role already carrying active Branches.AccessAll.
No role is inferred by name and ordinary staff get no automatic new grants. Existing role
administration can assign appropriate narrower capabilities. New explicit demo bootstrap
includes the codes; setup never runs automatically at startup.

## Safe API

All paths below have the `/api` prefix; POST mutations use strict JSON payloads.

| Method | Route | Purpose | Capability |
| --- | --- | --- | --- |
| GET | /members/{memberId}/subscriptions | Paginated origin-scoped history and filters | View |
| GET | /members/{memberId}/subscriptions/current?branchId=... | Operational branch access lookup | View |
| GET | /subscriptions/{id} | Snapshot, grants, freezes and holiday details | View |
| POST | /subscriptions/{id}/freeze | Book approved Normal freeze | Freeze |
| POST | /subscriptions/{id}/freezes/{freezeId}/complete | Acknowledge completed approved interval | Freeze |
| POST | /subscriptions/{id}/custom-extension | Reasoned medical/custom extension | Extend |
| POST | /employees/{id}/membership-entitlement | Link/start free employment-bound entitlement | Manage + Employees.Manage |
| POST | /subscriptions/official-holidays | Declare scoped closure/compensation | Extend + Branches.AccessAll |
| POST | /subscriptions/official-holidays/{id}/apply | Apply due unprotected closure days | Extend + Branches.AccessAll |

View/Manage/Freeze/Extend in this table mean Subscriptions.*. History supports page,
pageSize (1..100), BranchId, effective Status, PlanType and start-date From/To.
Details expose Version. Freeze/extension payloads supply a nonempty OperationId and
ExpectedVersion. Retry identical logical operations with the same identity; fresh changes
use a fresh identity and latest version. Stale writes return 409; invalid rules 400,
unauthorized scope 403 and missing resources 404. Owner without current branch selects
branchId explicitly for access lookup. Controllers remain thin.

## BLL integration, concurrency and audit

`SubscriptionService` provides GetHistoryAsync, GetDetailsAsync, GetCurrentEntitlementAsync,
EffectiveBusinessDateAsync, CreateCustomerGrantAsync, RenewCustomerAsync,
StartEmployeeEntitlementAsync, StartFreezeAsync, CompleteFreezeAsync,
GrantCustomExtensionAsync, CreateOfficialHolidayAsync and ApplyOfficialHolidayAsync.
Employment closure invokes its internal CloseEmployeeEntitlementsAsync.

Paid creation/renewal are protected BLL integration foundations. A future Sales/Payments
service must validate its financial workflow before calling them transactionally; no
ordinary-staff paid HTTP grant is provided. Existing optional sale FK remains available;
future sale-link/net-price validation is part of Finance integration, not a payment bypass
implemented here. No fake sale/payment is created to satisfy a required FK.

Transactions participate in the caller's EF transaction with savepoints, or own a local
transaction. PostgreSQL Member/plan/employment row locks, immutable operation IDs,
subscription Version concurrency tokens and unique successor/protected-day keys prevent
duplicate grants, conflicting future periods, allowance over-consumption and duplicate
benefit snapshots. Shared business-day/catalog locks stabilize snapshot inputs. Failed
operations roll back their savepoint/transaction and clear tracked failed changes.
Independent-connection PostgreSQL tests cover renewal, scheduling, freeze and holiday races.

Existing AuditLogs record Subscription.Created, Renewed, ScheduleShifted, FreezeStarted,
FreezeCompleted, CustomExtensionGranted, HolidayExtended, EmployeeEntitlementStarted and
EmployeeEntitlementEnded; BranchClosure.OfficialHolidayCreated and
BranchClosure.SubscriptionCompensationApplied record policy/application. Relevant Member,
period, branch, actor/time, old/new facts and reasons are preserved. Permission catalog
setup also records its existing AuditLog family. No second audit system is introduced.

## Database and verification

New migrations only: `20261009152545_PrepareSubscriptionLifecycle` and
`20261009164635_PreventDuplicateFreezeHolidayCompensation`. They add minimal snapshots,
operation IDs, version/successor link, freeze completion/end history, the protected holiday
record and reverse-order compensation count. They make the grant's sale link and final end
nullable for non-financial/free employment grants, with employee-specific constraints.
Unique identities/links/dates, interval/history indexes and restrictive snapshot/history FKs
support service invariants. Existing migrations remain unchanged. SQL exports are in
docs/database. Development migration/catalog verification uses localhost only; production
was untouched. Tests use disposable isolated local PostgreSQL schemas.

Verification covers immutable plan/benefit/freeze/access snapshots, inclusive date boundaries,
overnight BusinessDate, session/date expiry, both renewal modes and expired cycles, scheduled
activation/shifts, history, Employee closure/transfer/rehire, Freelance ownership preservation,
allowance and overlap, reason/permissions, holidays in both orders, branch isolation, financial
absence and independent concurrent requests. Existing Auth/Branch/Employee/Member/CRM/Plans
regressions remain part of the full suite.

## Deferred integrations

No SaleTransaction/Payment/PaymentPart workflow, Cash/InstaPay, installments/refunds,
commissions/payroll, Check-In/session deduction, borrowed-session execution, frontend,
messaging or Excel import was added. Existing borrowed-session debt foundation is preserved;
its eventual at-most-two-per-expired-cycle attendance and renewal debt settlement belong to
Check-In/Finance integration and are not executed by these services. Member branch transfer
with remaining paid value still needs the future A→B request, real financial settlement and
B acceptance; no fake settlement or Sales ownership transfer is supplied here.
