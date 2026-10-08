# CRM / Leads and follow-up management

Lead registration, Member registration and membership purchase are separate actions.
This feature registers prospects, preserves Sales ownership and schedules contact work.
It creates no subscriptions, payments, sales transactions or check-ins.

## Data and ownership

Existing `Leads`, `LeadAssignments` and `LeadInteractions` are reused. Guid IDs and
Restrict history relationships remain. Lead `BranchId` is the operational branch
managing the prospect; conversion does not change it. It is independent of the
resulting Member's original registration branch (`HomeBranchId`).

Lead `Phone` remains the display/original number. `PhoneNormalized` is the persisted
canonical value, exposed as `primaryPhoneNormalized`. Requests use `primaryPhone`.
The existing PhoneNormalizer uses the operational branch's CountryCallingCode for
local input and accepts international + input directly. Country changes do not
rewrite persisted numbers. Lead name is optional for phone-only manager intake;
unknown names are null, not invented. Full Member name is required at conversion.
Secondary phone, email, source and notes are optional. Source is the existing free
text field, not another source enum/table.

Leads have current next-follow-up time, LostAt/LostReason and ConvertedAt/Member link.
Existing statuses New, Contacted, Interested, FollowUp, Converted, Lost and Inactive
remain stored. Normal APIs do not set legacy Inactive or Converted directly.

LeadAssignments preserve assigned employee, start/end, assigning user and reason.
A partial unique LeadId index permits at most one row with EndedAt null. Distribution
locks the Lead, closes the previous row, saves, then inserts its successor. Targets
must have active employment/account, no active lockout, effective
Members.OwnSalesCustomers, and a current primary assignment in the Lead's branch.
No cross-branch Lead distribution policy is invented. Existing assignments survive employee branch transfers. Ownership follows the employee,
so My Leads and My Follow-ups include their previously assigned Leads from old branches.
New assignments still require the target's current operational branch; after A → B,
new B Leads may join their existing A Leads. Distribution/reactivation management
remains branch-scoped, even if an employee personally owns an old-branch Lead.

Changing employment to non-active (Resigned or Suspended) closes current Sales Member
and Lead assignments atomically, preserving rows and contact history. Members become
House; open Member tasks appear in the management House queue. Leads retain their
branch, status and schedule and appear in their branch's unassigned management list.
Nobody is assigned randomly, and reactivation does not restore closed ownership.
Future-dated assignments must be reconciled before this status change can complete.
Eligibility checks hold employee row locks until commit to serialize with termination.
Branch transfer never calls this ownership-closing workflow. Historical transactions
and Member registration branches remain unchanged. Future commission buckets use the
actual sale/payment branch plus employee, regardless of employment branch; no
commission calculation is implemented here.
Member ownership during conversion can cross branches and is not transferred with
an employee's operational assignment.

LeadInteractions retain employee, optional legacy-safe creating user/contact time,
interaction type (Call, WhatsApp, Visit, Other), notes, outcome, next follow-up and
creation time. Normal APIs only append interactions. New interactions require an
active linked employee. Contact time defaults to now and cannot be in the future.
Next follow-up must be after contact time; FollowUp status requires a date. A
backdated interaction cannot replace newer contact's scheduling/status. Legacy
interactions expose CreatedAt as contact time when ContactedAt is unknown.

MemberFollowUpTasks reference only Members with an explicit FK; Lead scheduling
stays on Lead itself. There is no untyped subject ID or redundant owner field.
Open/completed state derives from CompletedAt. Task fields include reason, DueAt,
notes, creating user/time and completing user/time. One open task per Member/reason
is protected by a partial unique index and Member row locking. Reopening completed
work creates another historical row; opening equivalent current work updates its
date/notes. Reasons: RenewalDue, ExpiredMembership, InactiveMember, Reactivation,
ManagerAssigned, Other. These are labels for work, not invented lifecycle detection.

## Manager and Sales flows

Manager -> create Leads (including phone-only bulk rows) -> assign to eligible Sales
-> Sales sees My Leads -> append contact -> schedule next follow-up.

Leads.Assign is management distribution authority, separate from ownership
capability. A manager creates an unassigned Lead and need not own customers.
An eligible Sales employee with Leads.Manage but without Leads.Assign creates a
self-assigned prospect. They cannot accept another owner from the request.
Bulk intake requires Leads.Assign and Leads.Manage, accepts 1–100 rows, and returns
Created, AlreadyMember, AlreadyLead or Invalid per row. Valid rows are not rolled
back because another row is invalid/duplicate. It is JSON intake, not Excel parsing.

My Leads always resolves the authenticated employee. Caller-supplied employee
filters can only narrow its results. Its default excludes Lost, Converted and
legacy Inactive; request an explicit status for historical work. Converted Lead
history is available to its resulting Member's current Sales owner or authorized
branch management. The old Lead assignment closes at conversion. Freelance House
conversion does not confer continuing Sales access through an ended Lead assignment.

General list/details are restricted to the current owner unless Leads.Assign permits
management visibility in the user's operational branch; Branches.AccessAll additionally
permits global management scope. Leads.View alone is not branch-wide browsing.
Filters: name/display or normalized phone search, operational branch, status,
Sales employee, unassigned state, follow-up Today/Overdue/Upcoming, page/pageSize.
Pagination is 1-based, size 1–100, deterministic, with totalCount before paging.
Contact history is separately paginated. Assignment history is preserved.

Today is the configured gym calendar day. Overdue means scheduled before its start;
Upcoming starts with the next calendar day. Lost/Converted/Inactive Leads are excluded
from actionable scheduled queues. No Shift integration or fake shift records are used.

Lost status preserves the Lead and history, records LostAt/reason and clears its
current schedule. Only management (Leads.Assign plus Leads.Manage) can retry a Lost
or legacy Inactive Lead. Retry can set FollowUp with a date; contacted prospects
cannot be reset to New to erase contact meaning. Converted is terminal in Lead APIs.
Profile update cannot change branch, ownership, normalized phone directly or conversion
fields. Converted profiles must use the existing Member profile workflow.

## Walk-ins and conversion

Phone lookup order: existing Member -> existing Lead -> NotFound. Canonical matching
also considers safely derivable legacy display phones without rewriting them.
Lookup requires branch scope and an active operational branch. It reports existence
across branches but exposes record IDs/details only when independently permitted.
An inaccessible result requires an authorized owner/manager to act; random Sales
cannot convert or take ownership merely because the customer visited their desk.

Existing Member -> use the existing profile; ownership is unchanged.
Existing Lead owned by Ahmed -> customer visits Mohamed -> authorized conversion
creates MemberSalesAssignment to Ahmed. Mohamed does not acquire ownership.
No Member/Lead -> eligible current Sales uses existing Member NormalSales registration.
Public Member creation and primary-phone changes reject conflicting Leads; this
prevents bypassing CRM ownership checks. Converted Lead's linked Member may retain
its own phone through legitimate display edits. Administrative reconciliation of
ambiguous/duplicate legacy data is separate from normal workflows.

Conversion flow:

1. Require Leads.Convert and Members.Create; only the assigned employee or a
   branch-authorized manager (Leads.Assign) can convert.
2. Start/join transaction and lock/reload Lead; check active, not already converted.
3. Resolve current Lead owner; normal unassigned Leads must be assigned first.
4. Validate complete Member profile, active destination branch, owner capability
   and cross-branch referral permission when destination differs from Lead branch.
5. Use assembly-internal MemberManagementService.CreateMemberFromLeadAsync. It accepts
   Lead ID and independently resolves/authenticates ownership, never an API-supplied
   trusted SalesEmployeeId. Broad Members.AssignSales is not needed for owner conversion.
6. Reuse Member creation/core validation, target branch atomic ClientCode generator,
   canonical phone uniqueness, ownership, Freelance eligibility and Member audits.
7. Preserve the Lead's stored canonical phone when destination uses another country
   code. Display phone remains as supplied. A different phone must first be updated
   explicitly on the Lead; destination configuration must not reinterpret its identity.
8. Link ConvertedMemberId/ConvertedAt, set Converted, clear next follow-up, close current
   Lead assignment and audit. Commit everything together. Failure rolls back Member,
   counter, assignment, conversion link and audit writes.

Lead/Member registration share a PostgreSQL transaction advisory lock for canonical
phone identity. Database unique indexes remain the final per-table protection. Lead
row locks prevent double conversion/reassignment, including independent connections.

Cross-branch example: Ahmed operates in A, Lead in A, customer chooses B -> Member
registration branch B -> ClientCode from B -> Sales owner Ahmed -> Ahmed remains in A.
Members.CreateCrossBranch is required for normal cross-branch conversion. This grants
no other-branch employee, Member, finance, vault or payroll browsing. Future paid revenue
belongs to B; future commission attribution is Ahmed+B. Branch target buckets must never
be combined. No revenue/commission calculations exist in this feature.

Explicit Freelance conversion uses freelancePlanId and additionally requires
FreelancePlans.ManageEligibleMembers and destination branch scope. The existing active
Freelance plan/coach/city rules apply. This explicitly authorized exception overrides
Lead ownership: Member has no Sales assignment, gains plan eligibility, and is House.
The previous owner and explicit exception are audited; the creating user is not the
Sales owner. No purchase or commission logic is added.

## Existing Member work and House distribution

Open work through POST /api/crm/members/{id}/follow-ups. Current MemberSalesAssignment
is always its owner; reassignment immediately changes whose queue contains the work.
Owned cross-branch Members appear in that owner's CRM queue with lightweight contact
information. This does not expand GET /api/members scope or financial access. Combined
My Follow-ups also includes scheduled owned Leads when the user has Leads.View.

No active Member Sales assignment -> House -> branch-scoped management queue.
Manager distributes an open House task through the CRM assign endpoint, which invokes
existing Members.AssignSales authorization/history workflow. The same tasks then appear
for the new Sales owner; no fake House employee is created. Completing work preserves
its row and actor/time. Member assignment and task completion lock the same Member;
House distribution rechecks the task remains open after obtaining the lock.

OpenMemberFollowUpFromWorkflowAsync is an assembly-internal BLL integration point for
future Subscription/CheckIn services. It permits an explicit system actor (null) and
serializes equivalent open work. The public opening method requires authorization;
there is no public system-actor bypass.

**Automatic generation is not wired:** changing Member.IsActive, future subscription
expiry/renewal or attendance does not yet create CRM tasks automatically. Existing
Member Management status behavior is preserved as requested. CRM provides explicit
manual opening and the internal lifecycle integration method. Inactive/expired work
returns to the current owner, or House management, once a task is opened. A background
scanner/job, status hook, notifications and Shift surfacing are future integration work.
No inactivity threshold or fake subscription/check-in data was invented.

Legacy import must preserve known old Sales ownership using MemberSalesAssignments;
legacy House remains House. CRM does not implement import or rewrite historical owners.

## API and permissions

All endpoints use existing authentication/policy infrastructure and thin controllers.
Service checks enforce authorization/scope independently. Successful operations return
200; current unassigned Lead assignment can return 204/null. Existing error conventions
are 401, 403, 404, 400 business validation and 409 concurrency conflicts.

| Method | Route | Permission / purpose |
| --- | --- | --- |
| GET | /api/leads | Leads.View; visible Leads/filter/pagination |
| GET | /api/leads/my | Leads.View; authenticated employee only |
| GET | /api/leads/lookup?branchId=&phone= | Leads.View; privacy-safe walk-in lookup |
| GET | /api/leads/{id} | Leads.View; scoped details |
| POST | /api/leads | Leads.Manage; create with management/eligible-self ownership rules |
| POST | /api/leads/bulk | Leads.Assign + Leads.Manage; bounded row results |
| PUT | /api/leads/{id} | Leads.Manage; editable profile |
| POST | /api/leads/{id}/assign | Leads.Assign; assign/reassign with history |
| GET | /api/leads/{id}/assignment | Leads.View; current assignment |
| GET | /api/leads/{id}/assignment-history | Leads.View; historical assignments |
| POST | /api/leads/{id}/interactions | Leads.Manage; append contact and schedule |
| GET | /api/leads/{id}/interactions | Leads.View; paged history |
| PATCH | /api/leads/{id}/status | Leads.Manage; lifecycle; reactivation also Leads.Assign |
| POST | /api/leads/{id}/convert | Leads.Convert + Members.Create; owner/manager and workflow guards |
| GET | /api/crm/my-follow-ups | CRM.ViewMemberFollowUps; owned Lead/Member work |
| GET | /api/crm/house-follow-ups | CRM.ManageHouseFollowUps; management House queue |
| POST | /api/crm/members/{id}/follow-ups | CRM.ManageMemberFollowUps; owner or scoped Members.AssignSales manager |
| POST | /api/crm/member-follow-ups/{id}/complete | CRM.ManageMemberFollowUps; owner or scoped Members.AssignSales manager |
| POST | /api/crm/house-follow-ups/{id}/assign | CRM.ManageHouseFollowUps + Members.AssignSales; distribute House work |

Leads.View/Manage already existed. Added Leads.Assign, Leads.Convert,
CRM.ViewMemberFollowUps, CRM.ManageMemberFollowUps, CRM.ManageHouseFollowUps.
Members.OwnSalesCustomers is eligibility, never branch access. Member creation,
referral, assignment, Freelance eligibility and Branches.AccessAll permissions are
reused. Runtime rules do not depend on role names.

BLL methods: CreateLeadAsync, CreateLeadsBulkAsync (typed and JSON row overloads),
LookupPhoneAsync, GetLeadsAsync, GetLeadByIdAsync, UpdateLeadAsync, AssignLeadAsync,
GetCurrentLeadAssignmentAsync, GetLeadAssignmentHistoryAsync, GetMyLeadsAsync,
AddLeadInteractionAsync, GetLeadInteractionsAsync, ChangeLeadStatusAsync,
ConvertLeadAsync, GetMyFollowUpsAsync, GetHouseFollowUpsAsync,
OpenMemberFollowUpAsync, CompleteMemberFollowUpAsync, AssignHouseFollowUpAsync.
Internal integrations: Member.CreateMemberFromLeadAsync and
CRM.OpenMemberFollowUpFromWorkflowAsync. CrmPermissionSetup.ConfigureAsync is explicit
setup; it never runs on normal API startup.

Audit actions: Lead.Created, Lead.Updated, Lead.Assigned, Lead.Reassigned,
Lead.InteractionAdded, Lead.StatusChanged, Lead.MarkedLost, Lead.Reactivated,
Lead.Converted, FollowUp.Created, FollowUp.Updated, FollowUp.Completed,
HouseMember.AssignedForFollowUp, existing Member registration/ownership/eligibility
events, and Permission.CrmAccessConfigured. Existing actor/branch/entity/old-new/time
context is reused. The current helper does not add HTTP IP/device metadata.

## Migration and rollout

- 20261008143406_AddCrmLeadManagement: seven Lead columns, two interaction columns,
  MemberFollowUpTasks and its checks/Restrict FKs; canonical phone, conversion,
  current-assignment and open-task uniqueness; scheduling/owner indexes.
- 20261008162710_AllowPhoneOnlyCrmLeads: only makes Leads.FullName nullable.
- Old migrations remain unchanged. Safe unique derivable Lead phones are backfilled;
  ambiguous/unconfigured values remain null. Existing dates/actors are not fabricated.
  Existing duplicate current assignments/conversion links must be reconciled before
  applying uniqueness to another database; the migration does not discard history.
- Both applied only to local campions_gym. Existing zero Leads/assignments/interactions/
  Members and two branches preserved. Production is untouched.
- Explicit local provisioning:
  `dotnet run --project CampionsSystem.API -- --configure-crm-permissions`.
  Guarded Development + localhost command creates missing codes and existing Owner
  bootstrap grants; preserves activation states and custom existing staff grants.
  Configure actual branch calling codes and appropriate staff permissions explicitly.
  Existing Sales/Manager role grants are not silently rewritten. New demo role defaults
  follow existing seeder conventions.
- Re-import the Postman collection for the CRM folders. Writes must target intended
  development records individually; examples are not an automatic fixture/import.

Tests include bulk/duplicate privacy, phone-only intake, normalization, assignment
eligibility/history, self-only lists, interactions/schedules, lifecycle, ownership-safe
cross-branch/Freelance conversion, rollback, no purchase side effects, Member/House
work, authorization/HTTP fields and independent PostgreSQL concurrency sessions.
See PROJECT_HISTORY.md for actual final build/full-test/drift results and commits.

Not implemented: purchases, renewals, freeze, payments/sale processing, commissions,
30K or 5%/7% logic, payroll, check-in processing, attendance/expiry automation,
WhatsApp/SMS sending, notifications, Excel parsing/import, frontend, campaigns or
complex analytics. No new infrastructure, repositories or messaging frameworks.

Final verification (2026-10-08): solution build passed with zero warnings/errors;
full suite 354 passed, zero failed/skipped (38 added CRM tests above baseline 316).
EF reports no pending model changes. Catalog verifies both CRM migrations, seven
CRM codes, 77 application + seven Identity tables (EF history is separate). Development
business counts are unchanged: zero Leads/assignments/interactions/tasks/Members,
two branches. Postman checks cover nineteen routes/twenty-one examples, all response
and pre-request scripts, ID capture and + phone encoding; previous folders preserved.
No live modifying Postman requests were executed. Automatic lifecycle generation and
production rollout remain separate integrations as described above.
