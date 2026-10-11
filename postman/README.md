# Simple manual API testing

1. Start the API: `dotnet run --project CampionsSystem.API --launch-profile http`.
2. Import `Campions.Auth.postman_collection.json`. Open **Campions API - Auth, Branches, Employees, Members and CRM**.
3. Select the **Campions Auth - LOCAL PRIVATE** environment (contains demoPassword).
4. Send **1. Login**. Default username is `dev.reception`; change it in Body for another account.
5. Send **2. Current user**. Login saves the token in this collection and Current user sends it automatically. No copying/pasting.

The authentication endpoints are POST /api/auth/login and GET /api/auth/me.
The collection also includes all eight implemented branch/employee assignment routes.
Leave **Optional automated test cases** collapsed. They are examples of successful/failed requests to the same endpoints, not separate APIs. Some intentionally return 401.

If you previously imported the old collection, use the new **Campions API - Auth, Branches, Employees, Members and CRM** collection. Your imported Postman copy does not update when repository files change.

Login must return 200 and run its Scripts / Post-response script. Current user sends the latest login token; logging in as another account replaces it. If token expires, send Login again. The collection starts with an empty token and passwords are never committed. Do not share/export a collection after login without clearing accessToken.

Private local environment is ignored by Git. Regenerate it with `python3 scripts/setup-local-auth-demo.py` if needed. See [local setup](../docs/authentication/LOCAL_API_TESTING.md).

Automated API permission/isolation tests remain in the .NET test suite; no debug endpoints were added.

## Branch feature requests

Re-import the updated collection file: existing Postman copies do not update automatically.
Login as an appropriately authorized account using **1. Login**; its saved accessToken is
used by every request in folders **3. Branch management** and **4. Employee branch
assignments and transfers**. Optional Owner login tests use their own ownerToken, so they
do not replace the primary accessToken; use 1. Login for management requests.

Set these **collection variables** (leave real passwords/tokens in private local storage):

| Variable | Source |
| --- | --- |
| cityId | Existing active City ID from your development database; no City CRUD endpoint yet |
| branchId | Branch being read/updated/assigned; Create branch saves its returned ID here |
| employeeId | Employee ID selected from GET /api/employees; Create employee saves its returned ID here |
| targetBranchId | Active destination for transfer, selected deliberately |
| branchName | Desired branch name for create/update |
| assignmentStartDate | YYYY-MM-DD, today or later in configured gym timezone |
| transferEffectiveDate | YYYY-MM-DD, today or later and after current assignment StartDate |

All eight routes have sample requests. PUT is a complete settings update; edit all desired
fields and set isActive false to deactivate. There is no hard-delete request. An employee
with an overlapping current assignment must use transfer, not initial assignment.
GET current assignment can return 204 for an unassigned employee queried globally.

Reception's existing demo grants do not include Branches.View/Manage or employee management.
Use an account with the relevant action permissions. Global operations additionally need
Branches.AccessAll. The existing Owner role needs the explicit local setup described in
[branch isolation](../docs/branches/BRANCH_ISOLATION.md); this collection does not seed
permissions, accounts, employees or cities. A 403 indicates missing permission or scope;
400 may indicate invalid date, inactive target, overlap or same-branch transfer.

Send modifying requests individually against your intended development records; do not run
the entire collection indiscriminately. No Postman requests were sent against your database
as part of adding these examples. The .NET integration tests verify endpoint behavior.

## Employee management requests

Folder **5. Employee management** contains list/search, branch employees, details,
create, profile update and status change. Re-import the collection after updating it.
Use primary Login with Employees.View for reads or Employees.Manage for writes;
global users also need Branches.AccessAll. The folder reuses accessToken, employeeId
and branchId. Set employeeHireDate to YYYY-MM-DD; PUT must retain the employee's real
hire date and other profile values. Create captures employeeId, but never creates a
login. Initial branch assignment is atomic with creation; global users may remove
initialBranchId to create an unassigned employee. Branch list totalCount counts all
matching current assignments before pagination (status=Active counts active only).
Status accepts Active, Suspended, Resigned; Resigned is the current ended-employment
name. No delete or account/role management requests are provided. Existing branch
assignment/transfer requests remain in folder 4. See
[employee management](../docs/employees/EMPLOYEE_MANAGEMENT.md) for complete rules.

## Member management requests

Folder **6. Member management** covers all nine Member endpoints with eleven requests:
list/search/filter, details, three alternative creation workflows, profile update,
status, current Sales assignment, Sales history, assign/reassign and return to House.
Re-import the updated collection; an existing imported copy does not update automatically.
Login first; every request uses the saved accessToken.

Set collection variables before sending:

| Variable | Purpose |
| --- | --- |
| memberId | Existing member; successful creation captures the returned ID |
| memberRegistrationBranchId | Active target registration branch or optional list filter |
| memberFullName | Profile name for create/update |
| memberPrimaryPhone | Valid unique display phone; retain current value during profile update |
| salesEmployeeId | Eligible owner for management creation/assignment or list filter |
| freelancePlanId | Eligible active Freelance plan for FreelanceHouse creation |
| memberSearch | Optional name, phone, MemberCode or ClientCode search |
| memberClientCode | Optional branch-local numeric ClientCode filter |

The list request has disabled optional query parameters: enable only desired filters
in Postman's Params tab. It includes search, registrationBranchId, clientCode,
isActive, salesEmployeeId and houseOnly, with page/pageSize enabled by default.
ClientCode alone is not globally unique; use registrationBranchId with clientCode
to identify a branch-local customer number.

NormalSales requires Members.Create and Members.OwnSalesCustomers; the current linked
active employee becomes owner. For a cross-branch referral, select the other branch
and grant Members.CreateCrossBranch. Ownership does not grant other-branch browsing
or financial access. ManagementCreated requires Members.AssignSales and an explicit
eligible salesEmployeeId. FreelanceHouse requires FreelancePlans.ManageEligibleMembers
and an eligible freelancePlanId; no Sales assignment is created.

Reads require Members.View, except history which needs Members.ViewSalesHistory.
Profile update needs Members.Edit, plus Members.ChangePrimaryPhone for a primary-phone
change. Status needs Members.ChangeStatus. Assignment and return to House need
Members.AssignSales. All operations retain existing branch scope rules. Owner eligibility
is Members.OwnSalesCustomers, separate from assignment authority.

Configure the branch's actual CountryCallingCode through branch settings before using
local phone input; do not guess a country code. International input starting with + is
accepted directly. PUT replaces the editable profile; retain real values and omit immutable
Id, ClientCode, registration branch and Sales ownership fields. None of these requests
creates a subscription/payment. Send writes individually against intended development
records; the three creation requests are alternatives, not a sequence. Examples were
validated without sending modifying requests to the development database. See
[Member Management](../docs/MEMBER_MANAGEMENT.md) for complete rules and setup.

## CRM / Leads requests

Re-import the updated collection. Folder **7. CRM Leads** covers all fourteen Lead
routes, with alternative Lost/reactivation and normal/Freelance conversion examples.
Folder **8. CRM Member follow-ups** covers all five Member follow-up routes. Login
first; all requests reuse accessToken. Send writes individually; conversion alternatives
must not both be sent to the same Lead. No live modifying Postman requests were sent
while creating these examples.

Set collection variables: crmOperationalBranchId (Lead branch), leadId (captured on
Created), leadFullName (optional), leadPrimaryPhone, leadSecondPhone (distinct bulk
example), leadNextFollowUpAt (future ISO8601), crmFollowUpDueAt (ISO8601), crmFollowUpId
(captured on task opening). Existing memberRegistrationBranchId is the conversion
destination; memberFullName is required for conversion even for an unnamed Lead.
Reuse memberId, salesEmployeeId and freelancePlanId. Conversion saves memberId.
Lookup's pre-request script encodes the phone, including +, automatically.

Lead list has disabled optional Params filters; enable the desired ones. My Leads
resolves the logged-in employee and normally excludes Lost/Converted; request status
for history or followUp=Today/Overdue/Upcoming. Combined work uses window for the same
date filters and can filter branchId. Calendar dates use the configured gym timezone.

Leads.View/Manage are reused; Leads.Assign is branch management/distribution authority,
Leads.Convert is conversion authority, and Members.OwnSalesCustomers is ownership
eligibility. Conversion also needs Members.Create, plus Members.CreateCrossBranch for
normal registration outside the conversion actor's current branch. An active employee
may convert another employee's Lead without Leads.Assign or Members.AssignSales;
phone lookup exposes only ExistingLeadId when private Lead details are inaccessible,
and captures it as leadId. The original Lead owner becomes Member owner and the converter remains the audit/
creation actor. This grants no general old-branch Lead browsing. Freelance conversion additionally requires
FreelancePlans.ManageEligibleMembers and destination scope; it is explicitly House.
Managers without ownership capability can assign to eligible Sales. Invalid/duplicate
bulk rows return row-level results; bounds are 1–100 rows.

CRM.ViewMemberFollowUps allows the employee's queue. CRM.ManageMemberFollowUps allows
owned work opening/completion (or branch-scoped management with Members.AssignSales).
CRM.ManageHouseFollowUps permits the management House queue; distribution also requires
Members.AssignSales. Cross-branch owned contact work does not grant wider Member or
financial access. Automatic inactivity/expiry/attendance work generation is not wired;
open tasks explicitly until future lifecycle integrations are connected.

Configure actual branch country calling codes and staff grants. Guarded local setup:
`dotnet run --project CampionsSystem.API -- --configure-crm-permissions`.
See [CRM documentation](../docs/CRM_LEADS.md) for complete contracts, flows and permissions.

## Membership Plans Management

Folders 9–12 include all 20 canonical plan/category/benefit routes and four existing
Freelance configuration compatibility routes. Normal/Freelance/Employee create
examples are alternatives: send writes individually, not as an unattended runner.
Set branchId (owning/offering branch), cityId (access city), freelanceCoachEmployeeId
and Member IDs. Creation scripts capture membershipPlanId, freelancePlanId,
planCategoryId and membershipBenefitId. Catalog writes require MembershipPlans.Manage
plus Branches.AccessAll. Branch managers can configure only their current active
branch plans, including eligibility for Members registered elsewhere. Candidate lookup
requires one exact ID/code/phone (encode international + as %2B). See
[Membership Plans](../docs/MEMBERSHIP_PLANS.md) for permissions and validation rules.
These new configuration requests do not purchase/grant subscriptions or process payments.

## Subscription lifecycle requests

Folder **13. Subscriptions and entitlement** includes all nine safe routes. Read details
first to save `subscriptionVersion`. Set `subscriptionOperationId` to a new UUID per
logical operation and reuse it only when retrying the identical operation. Refresh details
after any change. Set dates and durations deliberately; complete a freeze only after its
approved end. Medical extension needs Subscriptions.Extend; employee grants additionally
need Employees.Manage. Official holiday creation/application additionally require
Branches.AccessAll and use the explicitly selected originating branch, not CityId.
Set extendSubscriptions=false for operational closure without compensation.

Paid creation and renewal have no HTTP route until Sales/Payments is implemented.
Legacy freelance purchase returns 410. See [Subscriptions](../docs/SUBSCRIPTIONS.md).

## Check-In / Access Management

Folder **14. Check-In and access management** contains all seven routes, with separate
scanner, manual and explicit borrowing examples. `CheckIns.Create` authorizes lookup,
evaluation and creation; `CheckIns.View` authorizes entry/debt history. Branch scope
comes from authenticated operational context; only global-capability users can select
another branch. Configure the two permission catalog entries explicitly in local
Development with `dotnet run --project CampionsSystem.API -- --configure-checkin-permissions`.
Existing role grants and direct overrides are preserved; assign staff permissions
through the existing authorization workflow.

Set `scannedMemberCode`, `memberId`, and `checkInSearch` as appropriate. Encode search
query values, especially international `+` as `%2B`. Set `checkInOperationId` to a new
UUID for **each real entry**, including repeated same-day visits. Reuse it only for an
identical HTTP retry; successful requests capture `checkInId`. Evaluate expired access
first, then deliberately use the explicit borrow request after operator confirmation.
A failed normal scan never creates borrowed debt automatically.

Trial redemption requires an already issued Available `guestInvitationId` and matching
`guestPhone` at its issuing branch. It consumes one `GUEST` quantity snapshot unit at
redemption when linked to a Member benefit. No guest-token issuance or new Member
registration is provided. See [Check-In documentation](../docs/CHECK_INS.md).

## Sales / Payments

Folder **15. Sales and Payments** contains 32 deliberate examples. Choose one workflow
at a time; running the folder wholesale would perform real purchases and cancellations.
Use authorized Reception credentials for normal Sales/Payments and explicit management
capabilities for Offers, installment creation, refunds and Quick Cancel. Owner examples
select `branchId`; branch staff use their trusted current branch.

Provision named Cash/InstaPay accounts through global branch management, or select
existing active accounts. New accounts start with zero opening money. Quote the actual
plan/Offer price before changing the illustrative 1200/200/1000 amounts. Configure the
one-session plan and price for standalone visits; those visits neither settle debt nor
reset borrowing allowance and are excluded from commission/targets.

The folder initializes a UUID once per `saleOperationId`, `paymentOperationId`,
`refundOperationId` and `quickCancelOperationId`. Keep it for identical retries. Clear its
environment value before a different intentional operation. Set realistic activation and
installment dates and choose the actual obligation ID before allocating payment.

Numeric request enums follow the API: FullPayment=0, Reservation=1, Installment=2,
StandaloneSingleSession=3; Now=0, FirstCheckIn=1, SpecificDate=2; General=0, MemberOnly=1;
Cash=0, InstaPay=1. First-entry activation uses the existing Check-In workflow.

The explicit Development/localhost `--configure-sales-permissions` command creates the
capability catalog and preserves existing grants, inactive capabilities and overrides.
It does not infer existing staff permissions. Assign them through authorization management;
optional `Sales:ManagementRoleId` must identify a role with active `Branches.AccessAll`.
See [Sales / Payments documentation](../docs/SALES_PAYMENTS.md).

## Employee shifts, shared drawer, expenses and advances

Folder **16** contains 28 manual examples for actual routes documented in
[Shifts / Vault / Expenses](../docs/SHIFTS_VAULT_EXPENSES.md). Re-import the collection.
Initial account/drawer setup is explicit and global; new operational Sales/payments/
refunds and Member registration require your own open employee shift. Existing role
grants are preserved, so configure capabilities deliberately for current accounts.

Use real cash counts in drawerCount, cashAccountId/instaPayAccountId, expenseCategoryId,
advanceRecipientEmployeeId and actual receipt/reference variables. Each modifying
request initializes its operation variable only when empty. Preserve both that UUID
and the original body on retry; clear it for a new intentional operation. Opening
captures shiftId and shiftVersion; closing does not alter the original ExpectedVersion
used for an exact retry. A closed shift cannot be edited or reopened by replay.

Overlapping employee shifts share one drawer; never sum their opening counts or
interpret a closing shortage as employee liability. Cash returns require actual
returned funds. Advances are separate from expenses/payroll. Discretionary withdrawals
require both explicit Vault.Withdraw and Branches.AccessAll plus an own active shift.
Do not run the full folder blindly. No examples were sent against application data.
