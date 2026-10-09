# Membership Plans Management

Plans are templates, not Member subscriptions. This feature configures future
products and grants; its commands never create subscriptions, sales or payments.
Existing purchase/operations code is preserved, not expanded by this feature.

## Ownership, access and visibility

`MembershipPlan.BranchId` is the offering/administrative owner; nullable only for
legacy/unmapped rows. New plans require an active owning branch. Branch managers
may read/manage only plans in their current active branch. `Branches.AccessAll`
permits any branch, together with the action permission. Same-city branches are
separate administrative scopes. Unmapped plans are global-only; global management
must explicitly map them before activation. Nothing infers ownership from CityId.

`CityId` keeps its existing entitlement/access meaning: the configured city for
future subscription use. No duplicate branch-access junction or administration-by-
city model was introduced. A Member's registration branch does not restrict
Freelance eligibility: a Member from A can use an offer administered by B.
Eligibility does not transfer that Member or change Sales ownership.

For example, A and B may share a city while B offers a plan A does not. A's manager
cannot manage B's plan. A Member originally registered in A may later visit B and
purchase B's offer when the general Subscription/Sales workflow is implemented;
Member.HomeBranchId need not equal Plan.BranchId. That future sale/payment belongs
to B, while Member Sales ownership remains separate. Plan management and eligibility
APIs do not execute that purchase/payment.

Global audience categories and benefits can be read by plan viewers, but only
`MembershipPlans.Manage` **and** `Branches.AccessAll` may mutate them. Branch
managers select existing active catalog entries for their own plans.

Employee plans are excluded from default lists and hidden from view-only details.
Administrators request `includeInternal=true` or `planType=Employee` explicitly.
`customerPurchasable` derives from non-Employee type, mapped ownership and active
state; it is metadata, not complete purchase authorization/eligibility validation.
There are no anonymous/public catalog endpoints.

## Template rules

- **Normal:** positive duration; no Freelance Coach. Price is gym price.
- **Freelance:** positive duration; active Freelance Employee required, with valid
  hire/termination dates. Price is the gym's fixed amount, not the coach's private
  customer payment or percentage share. Eligibility is an explicit current set.
- **Employee:** free, Unlimited, no session count, duration zero, no Freelance Coach.
  Internal employment-bound subscription lifecycle remains a separate workflow.
- Behavioral `PlanCategory` (API `planType`) is separate from optional dynamic
  `MembershipPlanCategory` (API `audienceCategoryId`). Behavioral type cannot be
  switched on an existing plan; create another template instead.
- Unlimited requires null sessions; SessionBased requires positive sessions.
- Price is nonnegative decimal(18,2), at most two fractional digits. Zero-price
  customer offers are allowed. Duration is positive except Employee duration zero.
- Freeze days are nonnegative; disabled freeze requires zero included days.
- Name is required, at most 256 characters. Existing schema has no unique plan-name
  rule, so names may repeat. Catalog code/name uniqueness follows existing exact
  database comparisons. No new case-insensitive uniqueness rule was invented.
- Selected access city must be active. Newly selected audience category must be
  active; an unchanged inactive classification may remain on an existing plan.
- Activation revalidates template/branch/coach rules. Deactivation preserves every
  subscription and history row. Global management may deactivate a plan belonging
  to an inactive branch. There is no plan hard-delete API.

## Benefits and historical safety

The shared benefit catalog has Code, Name, Description, IsActive and CreatedAt.
Quantity grants require a positive quantity; Access grants require null quantity.
PUT plan benefits replaces the current template set (maximum 100 distinct entries).
An empty set removes all template benefits. An inactive catalog benefit may be
retained unchanged or removed, but not newly selected or reconfigured.

`MembershipPlanBenefits` → future purchase snapshot → `MemberSubscriptionBenefits`.
Template price/duration edits and benefit changes never update subscriptions,
transaction prices, benefit snapshots or usages. Catalog rename/deactivation also
leaves stored snapshot code/name/quantity intact. Current template relationship
removal is safe because historical grants do not reference those relationship rows.

## Freelance eligibility

Only `FreelancePlans.ManageEligibleMembers` grants eligibility administration,
within the plan's branch scope. Adding requires an active Freelance plan, valid
active Coach and active Member. Registration branch is deliberately unrestricted.
Adds/removals are idempotent and return whether a relationship changed. Removal
works for inactive plans/coaches and leaves subscriptions/history intact. The
existing unique plan/member index prevents duplicates; plan row locking serializes
changes. Audits preserve relationship changes; no second eligibility/history model
was added. Eligibility never creates, closes or replaces MemberSalesAssignments.
Explicit FreelanceHouse registration remains House under Member Management. Its
existing eligibility insertion now share-locks and checks plan administrative
ownership too; a registration route cannot bypass another branch or unmapped plan
scope. Existing registration-city access validation remains separate.

Eligible-list search is paginated and searches only that plan's selected Members.
Candidate lookup requires exactly one exact Member ID, MemberCode or phone. Phone
uses shared PhoneNormalizer and the offering branch's configured calling code;
legacy display comparison is exact and only used when normalized phone is absent.
International input works without an implicit country. No broad cross-branch name
search is exposed. Use existing scoped Member search for normal directory browsing.

## API and permissions

All routes require authentication. Scope is also enforced in BLL independently.
`V` = MembershipPlans.View; `M` = MembershipPlans.Manage;
`E` = FreelancePlans.ManageEligibleMembers; `G` = Branches.AccessAll.

| Method | Route | Purpose | Permission |
|---|---|---|---|
| GET | /api/membership-plans | Search/filter/page templates | V |
| GET | /api/membership-plans/{id} | Lightweight details | V; M for Employee |
| POST | /api/membership-plans | Create template | M |
| PUT | /api/membership-plans/{id} | Update template | M |
| PATCH | /api/membership-plans/{id}/status | Activate/deactivate | M |
| GET | /api/membership-plans/{id}/benefits | Configured benefits | V |
| PUT | /api/membership-plans/{id}/benefits | Replace benefit configuration | M |
| GET | /api/membership-plans/{id}/eligible-members | Search/page selected Members | E |
| GET | /api/membership-plans/{id}/eligible-members/candidates | Exact Member lookup | E |
| POST | /api/membership-plans/{id}/eligible-members | Add eligibility | E |
| DELETE | /api/membership-plans/{id}/eligible-members/{memberId} | Remove eligibility | E |
| GET | /api/membership-plan-categories | List/search/page audience catalog | V |
| POST | /api/membership-plan-categories | Create category | M + G |
| PUT | /api/membership-plan-categories/{id} | Edit category | M + G |
| PATCH | /api/membership-plan-categories/{id}/status | Activate/deactivate category | M + G |
| GET | /api/membership-benefits | List/search/page benefit catalog | V |
| GET | /api/membership-benefits/{id} | Benefit details | V |
| POST | /api/membership-benefits | Create benefit | M + G |
| PUT | /api/membership-benefits/{id} | Edit benefit | M + G |
| PATCH | /api/membership-benefits/{id}/status | Activate/deactivate benefit | M + G |

Existing `/api/freelance-plans` POST/PUT definition routes retain
`FreelancePlans.Manage`, and PUT/DELETE eligibility routes retain E. They delegate
to this same secure core. Definition requests now require explicit BranchId;
legacy edit requests preserve audience/freeze fields they do not expose. Use the
canonical status endpoint when deactivating an invalid/inactive-coach template.
No new permissions were necessary; no role-name checks were introduced.

List filters: search/name, isActive, branchId, cityId, planType, audienceCategoryId,
mode, freelanceCoachEmployeeId, includeInternal, page and pageSize (1–100).
Details contain a Plan summary (Id, Name, PlanType, Branch, City, AudienceCategory,
Mode, Price, DurationDays, Sessions, AllowFreeze, FreezeDays, IsActive, FreelanceCoach,
CustomerPurchasable), CreatedAt, Benefits and EligibleMemberCount. No historical
subscriptions, financial records or CheckIns are loaded.

## BLL, audit and concurrency

One feature-based `MembershipPlanManagementService` uses the existing DbContext,
CurrentUserContext, branch authorization and OperationAccess audit helper. Public
methods: GetPlansAsync, GetPlanByIdAsync, CreatePlanAsync, UpdatePlanAsync,
ChangePlanStatusAsync, GetCategoriesAsync, CreateCategoryAsync, UpdateCategoryAsync,
ChangeCategoryStatusAsync, GetBenefitsAsync, GetBenefitByIdAsync, CreateBenefitAsync,
UpdateBenefitAsync, ChangeBenefitStatusAsync, GetPlanBenefitsAsync,
SetPlanBenefitsAsync, GetEligibleMembersAsync, FindEligibleMemberCandidatesAsync,
AddEligibleMemberAsync and RemoveEligibleMemberAsync.

Audit actions: MembershipPlan.Created/Updated/StatusChanged;
MembershipPlanCategory.Created/Updated/StatusChanged;
MembershipBenefit.Created/Updated/StatusChanged;
MembershipPlan.BenefitAdded/BenefitUpdated/BenefitRemoved;
FreelancePlan.MemberEligible/MemberEligibilityRemoved.
The compatibility save also retains FreelancePlan.Created/Edited audit names.

Changes run in transactions (savepoints when an ambient transaction exists).
Existing template rows are locked FOR UPDATE; selected branch/coach/catalog rows
are share-locked as appropriate. Benefit catalog locks use stable ID ordering.
Database unique indexes remain the final duplicate safeguard. Existing controller
error handling returns 400 business validation, 403 permission failure, 404 hidden
scope and 409 database concurrency/duplicate conflicts. No history cascades were
introduced.

## Migration and deferred integrations

`20261009013415_AddMembershipPlanBranchOwnership` adds only nullable BranchId,
IX_MembershipPlans_BranchId and restricted GymBranches FK. No ownership backfill.
Applied only to Development; inspected SQL is in database/AddMembershipPlanBranchOwnership.sql.
The Development database contained zero plans during the verified update.

Future purchase/grant must validate offering branch, active template and eligibility,
then snapshot commercial/benefit data. Future Check-In consumes city/access, session
and benefit entitlements. Future Employee lifecycle controls internal subscriptions.
Member transfer/financial settlement remains separate. No new subscription purchase,
renewal, freeze execution, payments, Check-In, benefit consumption, commission,
payroll, frontend, Excel, WhatsApp or SMS implementation was added here.

## Final verification (2026-10-09)

Solution build: zero warnings/errors. Full suite: 414 passed, zero failed/skipped
(44 additional cases over the 370-test baseline). Coverage includes branch FK and
legacy ownership, global catalogs, all plan types, mode/price/duration/freeze
validation, filters/pagination, snapshot safety, cross-registration eligibility,
unchanged Sales/House ownership, HTTP authorization and compatibility scope handling.
Existing Member/CRM/Employee/Branch/authorization tests remain passing.
EF model drift: none; local Development migration/FK/index verified.
Postman: all 20 canonical routes plus four compatibility routes, 26 requests including
two alternative create examples. JSON/body/variable and JavaScript syntax validated;
route coverage compared with controllers. No unattended Postman write runner was run.
