# CRM / Leads

## Purpose

A Lead is a prospect; a Member is a registered customer profile. Neither requires a
subscription or payment. The conceptual progression is Lead → Member → future
Subscriber. CRM performs prospect/contact/conversion work, not subscription purchase.

Lead.BranchId is the operational branch managing the prospect. Member.HomeBranchId
is the original registration branch. Conversion preserves the former and may choose
a different destination for the latter.

## Lead Intake

Individual creation supports phone-only Leads: FullName may be null until known;
conversion requires a complete Member name. Optional secondary phone, email, source
and notes are retained. Source is free text, not a new catalog/enum.

Lead.Phone stores display input; PhoneNormalized stores the shared PhoneNormalizer
result using operational Branch.CountryCallingCode. API contracts expose these as
primaryPhone/primaryPhoneNormalized. Persisted canonical values are not rewritten
when branch settings change. See [Member phones](MEMBER_MANAGEMENT.md).

Creation checks existing Member first, then existing Lead. Registration and phone
changes share a PostgreSQL transaction advisory lock for canonical phone identity;
per-table unique indexes remain final safeguards. Ordinary creation cannot bypass
an existing Lead's ownership. Legacy phone comparison is conservative, not a silent
backfill of ambiguous data.

Management with Leads.Manage plus Leads.Assign can create unassigned Leads. Eligible
Sales with Leads.Manage and Members.OwnSalesCustomers can create their own assigned
prospect without distribution permission. Scope still applies to the operational
branch. Bulk intake requires Leads.Assign and Leads.Manage, accepts 1–100 rows and
returns row-level Created, AlreadyMember, AlreadyLead or Invalid results. It is not
Excel parsing.

Phone lookup reports existence across branches while withholding inaccessible IDs,
profiles and history. Accessible duplicate details follow independent permissions.
An active operator with Leads.Convert and Members.Create may receive only an
ExistingLeadId conversion reference for a phone-selected inaccessible Lead. This
narrow reference does not grant general Lead browsing. The lookup endpoint itself
requires Leads.View and authorized active lookup-branch scope.

## Lead Assignment

LeadAssignments preserve Employee, AssignedAt, EndedAt, assigning user and reason.
One open assignment per Lead is protected by a partial unique index. Assignment and
reassignment lock the Lead, close the old row, insert a successor and audit atomically.
Interactions remain attributed to their original actors.

Leads.Assign authorizes distribution, within the actor's branch/global scope. A newly
selected owner must have active employment/account, no active account lockout,
Members.OwnSalesCustomers and a current primary assignment in the Lead's operational
branch. Management capability and ability to own customers are distinct.

Existing assignments survive Employee branch transfer. This does not allow old-branch
redistribution merely because the actor personally owns a Lead there.

## My Leads

`GET /api/leads/my` resolves the authenticated Employee; client SalesEmployeeId cannot
expand the queue. Ownership follows Employee identity across branches. After Ahmed
moves A → B, existing A Leads remain visible and actionable, and newly assigned B
Leads may appear alongside them.

Default My Leads excludes Lost, Converted and legacy Inactive records. Explicit
historical status filters are supported; converted Lead history remains visible to
the resulting Member's current owner. Management with Leads.Assign can browse their
branch's Leads, including unassigned redistribution work; global scope permits all.

Search/filter options include name/display or normalized phone, branch, status,
assigned Sales, unassigned-only, follow-up window and pagination (pageSize 1–100).
Branch filters narrow permitted results; they do not grant unrelated access.

## Interactions and Lifecycle

Interaction types are Call, WhatsApp, Visit and Other. Recording WhatsApp contact is
a history entry, not sending a message. Normal APIs append interactions and provide
no casual edit/delete endpoint. Each records Employee/user, contact/creation times,
notes, outcome and optional next follow-up.

Contact time defaults to now and cannot be future-dated. Next follow-up must be later
than contact; FollowUp status requires a date. Backdated contact history does not
replace a newer interaction's schedule/status.

Statuses are New, Contacted, Interested, FollowUp, Converted, Lost and legacy Inactive.
Normal status APIs do not directly set Converted or Inactive. Contacted/Interested
must reflect recorded contact; previously contacted Leads cannot be reset to New.
Lost preserves history, records LostAt/reason and clears the current schedule.
Reactivation requires Leads.Manage plus Leads.Assign and branch scope. Converted is
terminal in Lead APIs; Member edits use Member Management.

## Follow-Ups

Lead.NextFollowUpAt holds current scheduling; interaction history retains earlier
schedules. Lead lists and combined CRM queues support Today, Overdue and Upcoming.
These use configured gym-calendar boundaries: Overdue is before today's start and
Upcoming begins with the next day, not merely later than the current instant.
Lost/Converted/Inactive Leads are excluded from actionable scheduled queues.

Existing Member work uses MemberFollowUpTasks with an explicit Member FK, not a new
Member profile or untyped subject ID. Ownership is derived from current active
MemberSalesAssignment, not copied into each task. Reasons are RenewalDue,
ExpiredMembership, InactiveMember, Reactivation, ManagerAssigned and Other.
CompletedAt distinguishes completed history from open work.

One open Member/reason task is protected by a partial unique index and Member row
locking. Opening equivalent current work updates its date/notes; reopening after
completion creates a new historical row. Completion records user/time.

My Follow-ups contains owned Member tasks across registration branches and scheduled
owned Leads (the Lead portion additionally requires Leads.View). This does not expand
ordinary Member directory or financial access. No active Sales owner means House:
open tasks appear in the branch-scoped House management queue. Authorized distribution
calls existing Members.AssignSales, preserving history, then the task follows the
new owner. No fake House employee or random assignment exists.

## Lead → Member Conversion

```text
Ahmed owns Lead; Ali handles registration in B
→ Ali is CreatedBy / conversion audit actor
→ Ahmed is resulting Member Sales owner
→ Member registration branch and ClientCode come from B
```

An active linked Employee needs Leads.Convert and Members.Create, destination scope
and Members.CreateCrossBranch when registering outside their own current branch.
They need not be the Lead owner, a manager or have Members.AssignSales. Conversion
permission does not independently grant private Lead listing/details.

The transaction locks/reloads the active, unconverted Lead, validates matching phone,
resolves its current owner and calls the assembly-internal Member creation path.
Normal unassigned Leads must be assigned first. The current owner must remain eligible.
The converter cannot submit an arbitrary trusted Sales owner. An authorized
reassignment before conversion changes which owner is preserved.

Member creation uses destination ClientCode allocation and phone identity validation.
The stored Lead canonical phone is retained across countries rather than reinterpreted
with the destination calling code. A changed phone must first be explicitly updated
on the Lead. Success links ConvertedMemberId/ConvertedAt, marks Converted, clears
scheduling, closes the current Lead assignment and audits. Failure rolls back Member,
counter, assignment and audit changes; locking prevents double conversion.

Ahmed's working branch and the Lead's operational branch remain unchanged. Ali in B
may register Ahmed's A Lead into B without gaining A directory/finance access. CRM
creates no subscription, sale or payment. Future sales attribution remains separate
from employment branch and must use the actual transaction branch; CRM calculates
no revenue, target or commission.

## Freelance Conversion

Explicit freelancePlanId conversion is the House exception, additionally requiring
FreelancePlans.ManageEligibleMembers, destination scope and plan administrative scope.
Existing active plan/Coach/access-city rules apply. Even an assigned Lead converts to
a House Member with eligibility and no Member Sales assignment; the exception and
previous owner are audited. Merely entering data never makes the converter the owner.
No purchase or commission is performed. See [Membership Plans](MEMBERSHIP_PLANS.md).

## Employment Termination

Resigned/Suspended employment closes current Member and Lead ownership atomically.
Members become House; open Member tasks enter House queues. Leads retain operational
branch/status/schedule and become unassigned management redistribution work.
Assignment/contact history remains; nobody is randomly assigned. Reactivation does
not restore closed assignments. Transfer preserves ownership instead. Details are in
[Employee Management](EMPLOYEE_MANAGEMENT.md).

## Automatic Follow-Up Limitations

Automatic generation from expiry, attendance or Member inactivity is not connected.
Reason labels do not implement detection. Member status changes do not automatically
open tasks. Explicit management/owner opening exists, and an assembly-internal
OpenMemberFollowUpFromWorkflowAsync integration is available to future lifecycle
services. There is no operational Shift, notification-sending or fake attendance
integration in CRM.

## API Overview and Permissions

| Endpoint group | Purpose | Key permissions |
|---|---|---|
| GET /api/leads, /my, /{id}, /lookup | Lists, owner queue, details, phone lookup | Leads.View |
| POST /api/leads; PUT /api/leads/{id} | Intake and profile edits | Leads.Manage plus scope/ownership rules |
| POST /api/leads/bulk | Bounded row-level intake | Leads.Assign + Leads.Manage |
| POST /api/leads/{id}/assign | Assign/reassign | Leads.Assign |
| GET /api/leads/{id}/assignment, /assignment-history | Ownership history | Leads.View |
| POST / GET /api/leads/{id}/interactions | Append/read contact history | Leads.Manage / Leads.View |
| PATCH /api/leads/{id}/status | Lifecycle, Lost/reactivation | Leads.Manage; Leads.Assign for reactivation |
| POST /api/leads/{id}/convert | Register Member preserving owner | Leads.Convert + Members.Create plus workflow scope |
| GET /api/crm/my-follow-ups | Combined owned work | CRM.ViewMemberFollowUps |
| GET /api/crm/house-follow-ups | House distribution queue | CRM.ManageHouseFollowUps |
| POST /api/crm/members/{id}/follow-ups | Open/update Member work | CRM.ManageMemberFollowUps |
| POST /api/crm/member-follow-ups/{id}/complete | Complete work | CRM.ManageMemberFollowUps |
| POST /api/crm/house-follow-ups/{id}/assign | Distribute House Member | CRM.ManageHouseFollowUps + Members.AssignSales |

Opening/completing Member work requires current ownership, or Members.AssignSales
with Member branch scope. Profile edits do not change Lead branch/ownership/conversion
fields. Services enforce scope independently of controller policies.

## Audit

Lead.Created/Updated/Assigned/Reassigned/InteractionAdded/StatusChanged/MarkedLost/
Reactivated/Converted and employment-driven Lead.Unassigned use AuditLogs.
FollowUp.Created/Updated/Completed and HouseMember.AssignedForFollowUp record Member
work changes. Member creation/ownership audits are reused during conversion. Actors
are the users performing the action; creation actor is not a substitute for ownership.
