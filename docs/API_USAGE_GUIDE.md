# Campeones API usage guide

This guide identifies the current registration APIs and explains every application route. The source is `master` at `661ed5af2d0d0764a50645e9fe73e759186021b2`, cross-checked against the controllers and checked-in OpenAPI contract. It covers 231 application operations in 29 controller modules; the Development-only `/health/database` diagnostic is separate.

## Where to register a new person

| Purpose | Endpoint | Eligibility | Source |
|---|---|---|---|
| Register one gym customer, including a Freelance Coach customer | `POST /api/members` | Effective `Members.Create`, eligible branch and open operator Shift; additional creation-type rules below | [MembersController.Create](../CampionsSystem.API/Controllers/MembersController.cs#L15) |
| Register one prospective customer (Lead) | `POST /api/leads` | Effective `Leads.Manage` and permitted branch | [LeadsController.Create](../CampionsSystem.API/Controllers/LeadsController.cs#L12) |
| Convert an existing Lead to a Member | `POST /api/leads/{id}/convert` | Effective `Leads.Convert`, conversion checks and open Shift | [LeadsController.Convert](../CampionsSystem.API/Controllers/LeadsController.cs#L31) |
| Register an employee record | `POST /api/employees` | Effective `Employees.Manage` and employee/branch rules | [EmployeesController.Create](../CampionsSystem.API/Controllers/EmployeesController.cs#L14) |
| Register several new Leads or Members | Excel template → preview → confirm under `/api/excel-imports` | Same effective permissions as the corresponding manual creation; rechecked at confirmation | [ExcelImportsController](../CampionsSystem.API/Controllers/ExcelImportsController.cs) |

For `POST /api/members`, send `targetRegistrationBranchId` (nonempty UUID), `creationType`, `fullName`, and `primaryPhone`. Optional profile fields are `secondaryPhone`, `dateOfBirth`, `gender`, and `notes`. `NormalSales` requires `Members.OwnSalesCustomers` and assigns the current employee as Sales owner. `ManagementCreated` requires `Members.AssignSales` and an eligible `salesEmployeeId`. `FreelanceHouse` requires `FreelancePlans.ManageEligibleMembers` and an eligible `freelancePlanId`, without a Sales owner. An existing Lead phone must use conversion. Member registration itself creates no subscription, sale, payment or ledger entry. See the [request DTO](../CampionsSystem.BLL/Members/MemberContracts.cs#L52) and [service validation](../CampionsSystem.BLL/Members/MemberManagementService.Create.cs#L87).

```http
POST /api/members
Authorization: Bearer <token>
Content-Type: application/json

{"targetRegistrationBranchId":"11111111-1111-4111-8111-111111111111","creationType":"NormalSales","fullName":"Example Member","primaryPhone":"01000000000"}
```

The UUID and phone above are illustrative. For `POST /api/leads`, send `branchId` and `primaryPhone`; optional fields are `fullName`, `secondaryPhone`, `email`, `source`, and `notes`. Lead creation does not require an open reception Shift. Existing phone matches are reported instead of silently creating duplicates. See [CreateLeadRequest](../CampionsSystem.BLL/Crm/CrmContracts.cs#L20). If the person is already a Lead, use the conversion endpoint to preserve their assignment.

Excel import supports `kind=leads`, `members`, or `freelance-customers`. Download a template, upload an `.xlsx` file to preview, then explicitly confirm. Preview does not create business records; confirmation rechecks access, branch and Shift. See the [Excel workflow](frontend-handoff/EXCEL_BULK_IMPORT.md).

## How to use the route reference

- A role title alone is insufficient. An authenticated account needs each listed **effective** capability. A per-user Deny overrides role grants. Owner-only endpoints also check Owner membership. Services enforce branch/city scope and employee status.
- “Required” in the request field list means required by the generated OpenAPI schema. Some non-nullable IDs and values are required by business services even if OpenAPI omits that marker. Do not send empty UUIDs. JSON fields use camelCase. For nested types and exact enums, see [OpenAPI](frontend-handoff/openapi.json) and the [DTO reference](frontend-handoff/DTO_AND_ENUM_REFERENCE.md).
- Reception financial mutations and Member registration may require the authenticated operator’s open Shift. Owner administrative operations follow distinct service rules. The scope note below is a summary, not a replacement for service validation.
- Responses have no universal wrapper. Common errors are `400` validation/business rule, `401` unauthenticated, `403` capability/scope, `404` scoped resource missing, `409` concurrency/uniqueness, and `410` disabled legacy route. A null success may return `204`. See the [error guide](frontend-handoff/API_ERRORS.md).
- Postman examples are in [the current collection](../postman/Campions.Auth.postman_collection.json) and the [frontend handoff](frontend-handoff/postman/README.md). Replace saved example IDs with valid IDs from the selected branch. No request in this guide was executed against a database.

## Module navigation

- [AccessAdministration](#accessadministration) — 9 operations.
- [Auth](#auth) — 2 operations.
- [Branches](#branches) — 4 operations.
- [BusinessDays](#businessdays) — 3 operations.
- [CheckIns](#checkins) — 7 operations.
- [Coaching](#coaching) — 36 operations.
- [Commissions](#commissions) — 9 operations.
- [Crm](#crm) — 5 operations.
- [EmployeeAdvances](#employeeadvances) — 3 operations.
- [EmployeeBranches](#employeebranches) — 4 operations.
- [EmployeeSalaries](#employeesalaries) — 2 operations.
- [Employees](#employees) — 6 operations.
- [ExcelImports](#excelimports) — 4 operations.
- [Expenses](#expenses) — 8 operations.
- [FinancialAccounts](#financialaccounts) — 2 operations.
- [Leads](#leads) — 14 operations.
- [MaintenancePlans](#maintenanceplans) — 5 operations.
- [Members](#members) — 9 operations.
- [MembershipBenefits](#membershipbenefits) — 5 operations.
- [MembershipPlanCategories](#membershipplancategories) — 4 operations.
- [MembershipPlans](#membershipplans) — 11 operations.
- [MonthlyFinance](#monthlyfinance) — 13 operations.
- [Offers](#offers) — 9 operations.
- [Operations](#operations) — 17 operations.
- [Payroll](#payroll) — 4 operations.
- [Sales](#sales) — 10 operations.
- [Shifts](#shifts) — 9 operations.
- [Subscriptions](#subscriptions) — 9 operations.
- [Vault](#vault) — 8 operations.

## Every application operation

### AccessAdministration

#### `GET /api/access-administration/history`

- Purpose: Retrieve history data in AccessAdministration.
- Eligible: Authenticated; `OwnerAccessAdministration` and `Permission:Users.View` and `Permission:Roles.View`.
- Branch/record scope: Owner global inventory; target account/role checks.
- URL inputs: `Page` (query; integer/string; optional; minimum=1, maximum=2147483647); `PageSize` (query; integer/string; optional; minimum=1, maximum=100); `TargetUserId` (query; UUID; optional); `RoleId` (query; UUID; optional); `ActorUserId` (query; UUID; optional); `OperationId` (query; UUID; optional).
- Body: none.
- Returns: `AccessPage<AccessAuditDetails>`.
- Errors: documented statuses 200, 400, 401, 403, 404, 409; standard service validation and authorization errors may also occur.
- Source: [`AccessAdministrationService.HistoryAsync`](../CampionsSystem.API/Controllers/AccessAdministrationController.cs#L32). Static test mapping: `AccessAdministrationApiTests.cs`.

#### `GET /api/access-administration/permissions`

- Purpose: Retrieve catalog data in AccessAdministration.
- Eligible: Authenticated; `OwnerAccessAdministration` and `Permission:Roles.View`.
- Branch/record scope: Owner global inventory; target account/role checks.
- URL inputs: none.
- Body: none.
- Returns: `PermissionModule[]`.
- Errors: documented statuses 200, 400, 401, 403, 404, 409; standard service validation and authorization errors may also occur.
- Source: [`AccessAdministrationService.CatalogAsync`](../CampionsSystem.API/Controllers/AccessAdministrationController.cs#L17). Static test mapping: `AccessAdministrationApiTests.cs`.

#### `GET /api/access-administration/roles`

- Purpose: Retrieve roles data in AccessAdministration.
- Eligible: Authenticated; `OwnerAccessAdministration` and `Permission:Roles.View`.
- Branch/record scope: Owner global inventory; target account/role checks.
- URL inputs: none.
- Body: none.
- Returns: `AccessRole[]`.
- Errors: documented statuses 200, 400, 401, 403, 404, 409; standard service validation and authorization errors may also occur.
- Source: [`AccessAdministrationService.RolesAsync`](../CampionsSystem.API/Controllers/AccessAdministrationController.cs#L20). Static test mapping: `AccessAdministrationApiTests.cs`.

#### `GET /api/access-administration/roles/{roleId}`

- Purpose: Retrieve role data in AccessAdministration.
- Eligible: Authenticated; `OwnerAccessAdministration` and `Permission:Roles.View`.
- Branch/record scope: Owner global inventory; target account/role checks.
- URL inputs: `roleId` (path; UUID; required).
- Body: none.
- Returns: `RoleAccessDetails`.
- Errors: documented statuses 200, 400, 401, 403, 404, 409; standard service validation and authorization errors may also occur.
- Source: [`AccessAdministrationService.RoleAsync`](../CampionsSystem.API/Controllers/AccessAdministrationController.cs#L23). Static test mapping: `AccessAdministrationApiTests.cs`.

#### `PUT /api/access-administration/roles/{roleId}/permissions/{permissionId}`

- Purpose: Grant the relevant AccessAdministration record or workflow.
- Eligible: Authenticated; `OwnerAccessAdministration` and `Permission:Roles.Manage`.
- Branch/record scope: Owner global inventory; target account/role checks.
- URL inputs: `roleId` (path; UUID; required); `permissionId` (path; UUID; required).
- Body: `RoleGrantRequest` (`application/json`).
  - Top-level fields: `granted` **required** (boolean); `operationId` optional (UUID); `expectedVersion` **required** (string; minLength=64, maxLength=64); `reason` **required** (string; maxLength=2000).
- Returns: `AccessChangeResult`.
- Errors: documented statuses 200, 400, 401, 403, 404, 409; standard service validation and authorization errors may also occur.
- Source: [`AccessAdministrationService.GrantAsync`](../CampionsSystem.API/Controllers/AccessAdministrationController.cs#L39). Static test mapping: `AccessAdministrationApiTests.cs`.

#### `GET /api/access-administration/users`

- Purpose: Retrieve users data in AccessAdministration.
- Eligible: Authenticated; `OwnerAccessAdministration` and `Permission:Users.View`.
- Branch/record scope: Owner global inventory; target account/role checks.
- URL inputs: `Page` (query; integer/string; optional; minimum=1, maximum=2147483647); `PageSize` (query; integer/string; optional; minimum=1, maximum=100); `Search` (query; string; optional; maxLength=256); `RoleId` (query; UUID; optional); `BranchId` (query; UUID; optional); `IsActive` (query; boolean; optional).
- Body: none.
- Returns: `AccessPage<AccessUserItem>`.
- Errors: documented statuses 200, 400, 401, 403, 404, 409; standard service validation and authorization errors may also occur.
- Source: [`AccessAdministrationService.UsersAsync`](../CampionsSystem.API/Controllers/AccessAdministrationController.cs#L26). Static test mapping: `AccessAdministrationApiTests.cs`.

#### `GET /api/access-administration/users/{userId}`

- Purpose: Retrieve user access data in AccessAdministration.
- Eligible: Authenticated; `OwnerAccessAdministration` and `Permission:Users.View`.
- Branch/record scope: Owner global inventory; target account/role checks.
- URL inputs: `userId` (path; UUID; required).
- Body: none.
- Returns: `UserAccessDetails`.
- Errors: documented statuses 200, 400, 401, 403, 404, 409; standard service validation and authorization errors may also occur.
- Source: [`AccessAdministrationService.UserAsync`](../CampionsSystem.API/Controllers/AccessAdministrationController.cs#L29). Static test mapping: `AccessAdministrationApiTests.cs`.

#### `PUT /api/access-administration/users/{userId}/permissions/{permissionId}`

- Purpose: Override the relevant AccessAdministration record or workflow.
- Eligible: Authenticated; `OwnerAccessAdministration` and `Permission:Users.Manage`.
- Branch/record scope: Owner global inventory; target account/role checks.
- URL inputs: `userId` (path; UUID; required); `permissionId` (path; UUID; required).
- Body: `PermissionOverrideRequest` (`application/json`).
  - Top-level fields: `effect` **required** (string); `operationId` optional (UUID); `expectedVersion` **required** (string; minLength=64, maxLength=64); `reason` **required** (string; maxLength=2000).
- Returns: `AccessChangeResult`.
- Errors: documented statuses 200, 400, 401, 403, 404, 409; standard service validation and authorization errors may also occur.
- Source: [`AccessAdministrationService.OverrideAsync`](../CampionsSystem.API/Controllers/AccessAdministrationController.cs#L43). Static test mapping: `AccessAdministrationApiTests.cs`.

#### `PUT /api/access-administration/users/{userId}/roles/{roleId}`

- Purpose: Membership the relevant AccessAdministration record or workflow.
- Eligible: Authenticated; `OwnerAccessAdministration` and `Permission:Users.Manage`.
- Branch/record scope: Owner global inventory; target account/role checks.
- URL inputs: `userId` (path; UUID; required); `roleId` (path; UUID; required).
- Body: `RoleMembershipRequest` (`application/json`).
  - Top-level fields: `assigned` **required** (boolean); `operationId` optional (UUID); `expectedVersion` **required** (string; minLength=64, maxLength=64); `reason` **required** (string; maxLength=2000).
- Returns: `AccessChangeResult`.
- Errors: documented statuses 200, 400, 401, 403, 404, 409; standard service validation and authorization errors may also occur.
- Source: [`AccessAdministrationService.MembershipAsync`](../CampionsSystem.API/Controllers/AccessAdministrationController.cs#L35). Static test mapping: `AccessAdministrationApiTests.cs`.

### Auth

#### `POST /api/auth/login`

- Purpose: Validate credentials and return a bearer token.
- Eligible: Public login route; credentials checked by the service.
- Branch/record scope: identity context; no selected branch.
- URL inputs: none.
- Body: `LoginRequest` (`application/json`).
  - Top-level fields: `userNameOrEmail` **required** (string; maxLength=256); `password` **required** (string; maxLength=256).
- Returns: `CampionsSystem.BLL.Authentication.LoginResponse`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`IAuthenticationService.ValidateCredentialsAsync`](../CampionsSystem.API/Controllers/AuthController.cs#L13). Static test mapping: `AuthenticationApiTests.cs`.

#### `GET /api/auth/me`

- Purpose: Return current account, branch and effective permissions.
- Eligible: Authenticated; service-level permission checks.
- Branch/record scope: identity context; no selected branch.
- URL inputs: none.
- Body: none.
- Returns: `CampionsSystem.BLL.Authentication.CurrentUserContext`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ICurrentUserService.GetAsync`](../CampionsSystem.API/Controllers/AuthController.cs#L22). Static test mapping: `AuthenticationApiTests.cs`.

### Branches

#### `GET /api/branches`

- Purpose: Retrieve list data in Branches.
- Eligible: Authenticated; `Permission:Branches.View`.
- Branch/record scope: selected/current branch; global scope when granted.
- URL inputs: none.
- Body: none.
- Returns: `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Branches.BranchDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`BranchManagementService.ListAsync`](../CampionsSystem.API/Controllers/BranchesController.cs#L11). Static test mapping: `BranchManagementTests.cs`.

#### `POST /api/branches`

- Purpose: Create an active gym branch with its configured location and business settings.
- Eligible: Authenticated; `Permission:Branches.Manage`.
- Branch/record scope: selected/current branch; global scope when granted.
- URL inputs: none.
- Body: `SaveBranchRequest` (`application/json`).
  - Top-level fields: `name` **required** (string); `cityId` **required** (UUID); `address` optional (string); `phone` optional (string); `isActive` optional (boolean; default=True); `maxBorrowedGymSessions` optional (integer/string; default=2); `quickRefundWindowHours` optional (integer/string; default=24); `countryCallingCode` optional (string).
- Returns: `CampionsSystem.BLL.Branches.BranchDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`BranchManagementService.SaveAsync`](../CampionsSystem.API/Controllers/BranchesController.cs#L17). Static test mapping: `BranchManagementTests.cs`.

#### `GET /api/branches/{id}`

- Purpose: Retrieve get data in Branches.
- Eligible: Authenticated; `Permission:Branches.View`.
- Branch/record scope: selected/current branch; global scope when granted.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.Branches.BranchDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`BranchManagementService.GetAsync`](../CampionsSystem.API/Controllers/BranchesController.cs#L14). Static test mapping: `BranchManagementTests.cs`.

#### `PUT /api/branches/{id}`

- Purpose: Update the selected gym branch profile/settings.
- Eligible: Authenticated; `Permission:Branches.Manage`.
- Branch/record scope: selected/current branch; global scope when granted.
- URL inputs: `id` (path; UUID; required).
- Body: `SaveBranchRequest` (`application/json`).
  - Top-level fields: `name` **required** (string); `cityId` **required** (UUID); `address` optional (string); `phone` optional (string); `isActive` optional (boolean; default=True); `maxBorrowedGymSessions` optional (integer/string; default=2); `quickRefundWindowHours` optional (integer/string; default=24); `countryCallingCode` optional (string).
- Returns: `CampionsSystem.BLL.Branches.BranchDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`BranchManagementService.SaveAsync`](../CampionsSystem.API/Controllers/BranchesController.cs#L20). Static test mapping: `BranchManagementTests.cs`.

### BusinessDays

#### `POST /api/business-days`

- Purpose: Open the relevant BusinessDays record or workflow.
- Eligible: Authenticated; `Permission:Shifts.Manage`.
- Branch/record scope: BusinessDay branch.
- URL inputs: none.
- Body: `OpenBusinessDayRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `businessDate` **required** (date YYYY-MM-DD); `branchId` optional (UUID).
- Returns: `CampionsSystem.BLL.Shifts.BusinessDayDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ShiftService.DayDetails, ShiftService.OpenBusinessDayAsync`](../CampionsSystem.API/Controllers/ShiftsController.cs#L34). Static test mapping: `ShiftDomainTests.cs`.

#### `GET /api/business-days/current`

- Purpose: Retrieve current data in BusinessDays.
- Eligible: Authenticated; `Permission:Shifts.View`.
- Branch/record scope: BusinessDay branch.
- URL inputs: `branchId` (query; UUID; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.BusinessDayDetails?`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ShiftService.CurrentBusinessDayAsync`](../CampionsSystem.API/Controllers/ShiftsController.cs#L32). Static test mapping: `ShiftDomainTests.cs`.

#### `POST /api/business-days/{id}/close`

- Purpose: Close the relevant BusinessDays record or workflow.
- Eligible: Authenticated; `Permission:Shifts.Manage`.
- Branch/record scope: BusinessDay branch.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.BusinessDayDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ShiftService.DayDetails, ShiftService.CloseBusinessDayAsync`](../CampionsSystem.API/Controllers/ShiftsController.cs#L36). Static test mapping: `ShiftDomainTests.cs`.

### CheckIns

#### `GET /api/check-ins`

- Purpose: Retrieve history data in CheckIns.
- Eligible: Authenticated; `Permission:CheckIns.View`.
- Branch/record scope: entry branch, BusinessDate and city network.
- URL inputs: `BranchId` (query; UUID; optional); `MemberId` (query; UUID; optional); `From` (query; date YYYY-MM-DD; optional); `To` (query; date YYYY-MM-DD; optional); `Type` (query; CheckInType; optional); `Method` (query; CheckInMethod; optional); `Page` (query; integer/string; optional); `PageSize` (query; integer/string; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.CheckIns.CheckInHistory`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CheckInService.GetHistoryAsync`](../CampionsSystem.API/Controllers/CheckInsController.cs#L12). Static test mapping: `CheckInApiTests.cs`.

#### `POST /api/check-ins`

- Purpose: Register one eligible gym check-in; this is the attendance write.
- Eligible: Authenticated; `Permission:CheckIns.Create`.
- Branch/record scope: entry branch, BusinessDate and city network.
- URL inputs: none.
- Body: `CreateCheckInRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `method` **required** (CheckInMethod); `memberId` optional (UUID); `memberCode` optional (string); `confirmBorrow` optional (boolean; default=False); `branchId` optional (UUID).
- Returns: `CampionsSystem.BLL.CheckIns.CheckInResult`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CheckInService.CreateAsync`](../CampionsSystem.API/Controllers/CheckInsController.cs#L8). Static test mapping: `CheckInApiTests.cs`.

#### `POST /api/check-ins/evaluate`

- Purpose: Evaluate the relevant CheckIns record or workflow.
- Eligible: Authenticated; `Permission:CheckIns.Create`.
- Branch/record scope: entry branch, BusinessDate and city network.
- URL inputs: none.
- Body: `AccessRequest` (`application/json`).
  - Top-level fields: `memberId` **required** (UUID); `memberCode` **required** (string); `branchId` optional (UUID).
- Returns: `CampionsSystem.BLL.CheckIns.AccessDecision`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CheckInService.EvaluateAsync`](../CampionsSystem.API/Controllers/CheckInsController.cs#L10). Static test mapping: `CheckInApiTests.cs`.

#### `GET /api/check-ins/members`

- Purpose: Retrieve lookup data in CheckIns.
- Eligible: Authenticated; `Permission:CheckIns.Create`.
- Branch/record scope: entry branch, BusinessDate and city network.
- URL inputs: `Search` (query; string; optional); `BranchId` (query; UUID; optional); `Page` (query; integer/string; optional); `PageSize` (query; integer/string; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.CheckIns.CheckInMemberMatches`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CheckInService.FindMembersAsync`](../CampionsSystem.API/Controllers/CheckInsController.cs#L14). Static test mapping: `CheckInApiTests.cs`.

#### `POST /api/check-ins/trials`

- Purpose: Trial the relevant CheckIns record or workflow.
- Eligible: Authenticated; `Permission:CheckIns.Create`.
- Branch/record scope: entry branch, BusinessDate and city network.
- URL inputs: none.
- Body: `RedeemTrialRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `invitationId` **required** (UUID); `phone` **required** (string); `branchId` optional (UUID).
- Returns: `CampionsSystem.BLL.CheckIns.TrialCheckInResult`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CheckInService.RedeemTrialAsync`](../CampionsSystem.API/Controllers/CheckInsController.cs#L21). Static test mapping: `CheckInApiTests.cs`.

#### `GET /api/members/{memberId}/borrowed-gym-sessions`

- Purpose: Retrieve debts data in CheckIns.
- Eligible: Authenticated; `Permission:CheckIns.View`.
- Branch/record scope: entry branch, BusinessDate and city network.
- URL inputs: `memberId` (path; UUID; required); `BranchId` (query; UUID; optional); `ExpiredSubscriptionId` (query; UUID; optional); `Page` (query; integer/string; optional); `PageSize` (query; integer/string; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.CheckIns.BorrowedDebtHistory`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CheckInService.GetBorrowedDebtHistoryAsync`](../CampionsSystem.API/Controllers/CheckInsController.cs#L19). Static test mapping: `CheckInApiTests.cs`.

#### `GET /api/members/{memberId}/check-ins/current-business-day`

- Purpose: Retrieve today data in CheckIns.
- Eligible: Authenticated; `Permission:CheckIns.View`.
- Branch/record scope: entry branch, BusinessDate and city network.
- URL inputs: `memberId` (path; UUID; required); `branchId` (query; UUID; optional); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.CheckIns.CheckInHistory`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CheckInService.GetCurrentBusinessDayAsync`](../CampionsSystem.API/Controllers/CheckInsController.cs#L16). Static test mapping: `CheckInApiTests.cs`.

### Coaching

#### `GET /api/coaching/classes/coaches/{coach}/statistics`

- Purpose: Retrieve class statistics data in Coaching.
- Eligible: Authenticated; `Permission:Classes.ViewAttendanceReport`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `coach` (path; UUID; required); `branchId` (query; UUID; optional); `from` (query; date YYYY-MM-DD; optional); `to` (query; date YYYY-MM-DD; optional); `periodId` (query; UUID; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.Coaching.ClassCoachStatistics`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.ClassStatisticsAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L67). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `GET /api/coaching/classes/members`

- Purpose: Retrieve class members data in Coaching.
- Eligible: Authenticated; `Permission:Classes.View`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `branchId` (query; UUID; optional); `search` (query; string; optional); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.CoachingMemberResult>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.SearchAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L45). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `GET /api/coaching/classes/members/{member}/attendance`

- Purpose: Retrieve class member attendance data in Coaching.
- Eligible: Authenticated; `Permission:Classes.View`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `member` (path; UUID; required); `branchId` (query; UUID; optional); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.ClassAttendanceDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.ClassMemberHistoryAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L49). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `GET /api/coaching/classes/occurrences`

- Purpose: Retrieve occurrences data in Coaching.
- Eligible: Authenticated; `Permission:Classes.View`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `branchId` (query; UUID; optional); `from` (query; date YYYY-MM-DD; optional); `to` (query; date YYYY-MM-DD; optional); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.ClassOccurrenceDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.OccurrencesAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L57). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `POST /api/coaching/classes/occurrences`

- Purpose: Create occurrence in Coaching.
- Eligible: Authenticated; `Permission:Classes.Manage`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: none.
- Body: `ClassOccurrenceRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `classTypeId` **required** (UUID); `coachEmployeeId` **required** (UUID); `startsAt` **required** (timestamp).
- Returns: `CampionsSystem.BLL.Coaching.ClassOccurrenceDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.CreateOccurrenceAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L59). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `GET /api/coaching/classes/occurrences/{id}/attendance`

- Purpose: Retrieve class attendance data in Coaching.
- Eligible: Authenticated; `Permission:Classes.View`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `id` (path; UUID; required); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.ClassAttendanceDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.ClassAttendanceHistoryAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L65). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `POST /api/coaching/classes/occurrences/{id}/attendance`

- Purpose: Register class the relevant Coaching record or workflow.
- Eligible: Authenticated; `Permission:Classes.RegisterAttendance`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `id` (path; UUID; required).
- Body: `ClassAttendanceRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `memberId` **required** (UUID); `classesSubscriptionId` optional (UUID).
- Returns: `CampionsSystem.BLL.Coaching.ClassAttendanceDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.RegisterClassAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L63). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `POST /api/coaching/classes/occurrences/{id}/complete`

- Purpose: Complete occurrence the relevant Coaching record or workflow.
- Eligible: Authenticated; `Permission:Classes.Manage`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `id` (path; UUID; required).
- Body: `CompleteClassRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `expectedVersion` **required** (integer/string); `cancelled` optional (boolean; default=False).
- Returns: `CampionsSystem.BLL.Coaching.ClassOccurrenceDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.CompleteOccurrenceAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L61). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `GET /api/coaching/classes/packages`

- Purpose: Retrieve classes packages data in Coaching.
- Eligible: Authenticated; `Permission:Classes.View`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `branchId` (query; UUID; optional); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.CoachingPackageDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.PackagesAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L37). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `POST /api/coaching/classes/packages`

- Purpose: Create classes package in Coaching.
- Eligible: Authenticated; `Permission:Classes.Manage`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: none.
- Body: `CoachingPackageRequest` (`application/json`).
  - Top-level fields: `branchId` **required** (UUID); `name` **required** (string); `sessionCount` **required** (integer/string); `durationDays` **required** (integer/string); `price` **required** (number/string); `isActive` optional (boolean; default=True).
- Returns: `CampionsSystem.BLL.Coaching.CoachingPackageDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.SavePackageAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L39). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `PUT /api/coaching/classes/packages/{id}`

- Purpose: Update classes package in Coaching.
- Eligible: Authenticated; `Permission:Classes.Manage`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `id` (path; UUID; required).
- Body: `CoachingPackageRequest` (`application/json`).
  - Top-level fields: `branchId` **required** (UUID); `name` **required** (string); `sessionCount` **required** (integer/string); `durationDays` **required** (integer/string); `price` **required** (number/string); `isActive` optional (boolean; default=True).
- Returns: `CampionsSystem.BLL.Coaching.CoachingPackageDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.SavePackageAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L41). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `POST /api/coaching/classes/purchases`

- Purpose: Purchase classes the relevant Coaching record or workflow.
- Eligible: Authenticated; `Permission:Classes.Manage` and `Permission:Payments.Create`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: none.
- Body: `CoachingPurchaseRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `memberId` **required** (UUID); `packageId` **required** (UUID); `paymentParts` **required** (array of PaymentPartInput); `coachEmployeeId` optional (UUID); `purchaseMode` optional (PurchaseMode); `installments` optional (array of InstallmentLineInput).
- Returns: `CampionsSystem.BLL.Coaching.CoachingPurchaseReceipt`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SalesService.PurchaseCoachingAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L43). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `GET /api/coaching/classes/subscriptions`

- Purpose: Retrieve class subscriptions data in Coaching.
- Eligible: Authenticated; `Permission:Classes.View`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `branchId` (query; UUID; optional); `memberId` (query; UUID; optional); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.ClassesSubscriptionDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.ClassesSubscriptionsAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L47). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `GET /api/coaching/classes/types`

- Purpose: Retrieve class types data in Coaching.
- Eligible: Authenticated; `Permission:Classes.View`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `branchId` (query; UUID; optional); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.ClassTypeDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.ClassTypesAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L51). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `POST /api/coaching/classes/types`

- Purpose: Create class type in Coaching.
- Eligible: Authenticated; `Permission:Classes.Manage`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: none.
- Body: `ClassTypeRequest` (`application/json`).
  - Top-level fields: `branchId` **required** (UUID); `name` **required** (string); `coachEmployeeId` **required** (UUID); `isActive` optional (boolean; default=True); `description` optional (string).
- Returns: `CampionsSystem.BLL.Coaching.ClassTypeDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.SaveClassTypeAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L53). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `PUT /api/coaching/classes/types/{id}`

- Purpose: Update class type in Coaching.
- Eligible: Authenticated; `Permission:Classes.Manage`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `id` (path; UUID; required).
- Body: `ClassTypeRequest` (`application/json`).
  - Top-level fields: `branchId` **required** (UUID); `name` **required** (string); `coachEmployeeId` **required** (UUID); `isActive` optional (boolean; default=True); `description` optional (string).
- Returns: `CampionsSystem.BLL.Coaching.ClassTypeDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.SaveClassTypeAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L55). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `GET /api/coaching/earnings/coaches/{coach}/balance`

- Purpose: Retrieve balance data in Coaching.
- Eligible: Authenticated; `Permission:CoachEarnings.View`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `coach` (path; UUID; required); `branchId` (query; UUID; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.Coaching.CoachBalance`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.BalanceAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L78). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `GET /api/coaching/earnings/coaches/{coach}/history`

- Purpose: Retrieve earnings history data in Coaching.
- Eligible: Authenticated; `Permission:CoachEarnings.View`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `coach` (path; UUID; required); `branchId` (query; UUID; optional); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.CoachEarningDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.EarningsHistoryAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L80). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `GET /api/coaching/earnings/coaches/{coach}/payouts`

- Purpose: Retrieve payout history data in Coaching.
- Eligible: Authenticated; `Permission:CoachEarnings.View`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `coach` (path; UUID; required); `branchId` (query; UUID; optional); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.CoachPayoutDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.PayoutHistoryAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L84). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `POST /api/coaching/earnings/payouts`

- Purpose: Payout the relevant Coaching record or workflow.
- Eligible: Authenticated; `Permission:CoachEarnings.Pay`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: none.
- Body: `CoachPayoutRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `branchId` **required** (UUID); `coachEmployeeId` **required** (UUID); `amount` **required** (number/string); `financialAccountId` **required** (UUID); `expenseCategoryId` **required** (UUID); `reason` **required** (string); `receiptReference` optional (string).
- Returns: `CampionsSystem.BLL.Coaching.CoachPayoutDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.PayAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L82). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `GET /api/coaching/earnings/periods/{period}/progress`

- Purpose: Retrieve progress data in Coaching.
- Eligible: Authenticated; `Permission:CoachEarnings.View`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `period` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.Coaching.CoachEarningsProgress`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.ProgressAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L74). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `GET /api/coaching/earnings/periods/{period}/targets`

- Purpose: Retrieve target history data in Coaching.
- Eligible: Authenticated; `Permission:CoachEarnings.View`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `period` (path; UUID; required).
- Body: none.
- Returns: `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Coaching.CoachTargetDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.TargetsAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L72). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `POST /api/coaching/earnings/recognitions`

- Purpose: Recognize the relevant Coaching record or workflow.
- Eligible: Authenticated; `Permission:CoachEarnings.Recognize`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: none.
- Body: `RecognizeCoachEarningsRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `commissionPeriodId` **required** (UUID).
- Returns: `CampionsSystem.BLL.Coaching.CoachEarningsProgress`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.RecognizeAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L76). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `POST /api/coaching/earnings/targets`

- Purpose: Targets the relevant Coaching record or workflow.
- Eligible: Authenticated; `Permission:CoachEarnings.ManageTargets`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: none.
- Body: `CoachTargetRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `commissionPeriodId` **required** (UUID); `coachEmployeeId` **required** (UUID); `privateTarget` **required** (number/string); `classesAverageTarget` **required** (number/string).
- Returns: `CampionsSystem.BLL.Coaching.CoachTargetDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.ConfigureTargetsAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L70). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `GET /api/coaching/private/members`

- Purpose: Retrieve private members data in Coaching.
- Eligible: Authenticated; `Permission:Private.View`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `branchId` (query; UUID; optional); `search` (query; string; optional); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.CoachingMemberResult>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.SearchAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L20). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `GET /api/coaching/private/packages`

- Purpose: Retrieve private packages data in Coaching.
- Eligible: Authenticated; `Permission:Private.View`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `branchId` (query; UUID; optional); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.CoachingPackageDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.PackagesAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L12). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `POST /api/coaching/private/packages`

- Purpose: Create private package in Coaching.
- Eligible: Authenticated; `Permission:Private.Manage`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: none.
- Body: `CoachingPackageRequest` (`application/json`).
  - Top-level fields: `branchId` **required** (UUID); `name` **required** (string); `sessionCount` **required** (integer/string); `durationDays` **required** (integer/string); `price` **required** (number/string); `isActive` optional (boolean; default=True).
- Returns: `CampionsSystem.BLL.Coaching.CoachingPackageDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.SavePackageAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L14). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `PUT /api/coaching/private/packages/{id}`

- Purpose: Update private package in Coaching.
- Eligible: Authenticated; `Permission:Private.Manage`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `id` (path; UUID; required).
- Body: `CoachingPackageRequest` (`application/json`).
  - Top-level fields: `branchId` **required** (UUID); `name` **required** (string); `sessionCount` **required** (integer/string); `durationDays` **required** (integer/string); `price` **required** (number/string); `isActive` optional (boolean; default=True).
- Returns: `CampionsSystem.BLL.Coaching.CoachingPackageDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.SavePackageAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L16). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `POST /api/coaching/private/purchases`

- Purpose: Purchase private the relevant Coaching record or workflow.
- Eligible: Authenticated; `Permission:Private.Manage` and `Permission:Payments.Create`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: none.
- Body: `CoachingPurchaseRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `memberId` **required** (UUID); `packageId` **required** (UUID); `paymentParts` **required** (array of PaymentPartInput); `coachEmployeeId` optional (UUID); `purchaseMode` optional (PurchaseMode); `installments` optional (array of InstallmentLineInput).
- Returns: `CampionsSystem.BLL.Coaching.CoachingPurchaseReceipt`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SalesService.PurchaseCoachingAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L18). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `GET /api/coaching/private/subscriptions`

- Purpose: Retrieve private subscriptions data in Coaching.
- Eligible: Authenticated; `Permission:Private.View`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `branchId` (query; UUID; optional); `memberId` (query; UUID; optional); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.PrivateDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.PrivateSubscriptionsAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L22). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `GET /api/coaching/private/subscriptions/{id}`

- Purpose: Retrieve private subscription data in Coaching.
- Eligible: Authenticated; `Permission:Private.View`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.Coaching.PrivateDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.PrivateDetailsAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L24). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `GET /api/coaching/private/subscriptions/{id}/expiry-settlements`

- Purpose: Retrieve expiry history data in Coaching.
- Eligible: Authenticated; `Permission:Private.View`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `id` (path; UUID; required); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.PrivateExpiryDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.ExpiryHistoryAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L34). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `POST /api/coaching/private/subscriptions/{id}/expiry-settlements`

- Purpose: Settle expiry the relevant Coaching record or workflow.
- Eligible: Authenticated; `Permission:Private.SettleExpiry`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `id` (path; UUID; required).
- Body: `PrivateExpiryRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `commissionPeriodId` **required** (UUID); `reason` **required** (string).
- Returns: `CampionsSystem.BLL.Coaching.PrivateExpiryDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.SettleExpiryAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L32). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `GET /api/coaching/private/subscriptions/{id}/sessions`

- Purpose: Retrieve private sessions data in Coaching.
- Eligible: Authenticated; `Permission:Private.View`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `id` (path; UUID; required); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.PrivateAttendanceDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.PrivateHistoryAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L28). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `POST /api/coaching/private/subscriptions/{id}/sessions`

- Purpose: Register private the relevant Coaching record or workflow.
- Eligible: Authenticated; `Permission:Private.RegisterSession`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `id` (path; UUID; required).
- Body: `PrivateAttendanceRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `gymCheckInId` **required** (UUID); `notes` optional (string).
- Returns: `CampionsSystem.BLL.Coaching.PrivateAttendanceDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.RegisterPrivateAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L26). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

#### `POST /api/coaching/private/subscriptions/{id}/transfers`

- Purpose: Transfer the relevant Coaching record or workflow.
- Eligible: Authenticated; `Permission:Private.Transfer`.
- Branch/record scope: package/occurrence branch; coach assignment.
- URL inputs: `id` (path; UUID; required).
- Body: `PrivateTransferRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `coachEmployeeId` **required** (UUID); `expectedVersion` **required** (integer/string); `reason` **required** (string).
- Returns: `CampionsSystem.BLL.Coaching.PrivateAssignmentDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachingService.TransferAsync`](../CampionsSystem.API/Controllers/CoachingController.cs#L30). Static test mapping: `CoachingApiTests.cs; CoachingClassesPayoutTests.cs`.

### Commissions

#### `GET /api/commissions/periods`

- Purpose: Retrieve history data in Commissions.
- Eligible: Authenticated; `Permission:Commission.View`.
- Branch/record scope: commission period branch.
- URL inputs: `branchId` (query; UUID; optional); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Commissions.CommissionPage<CampionsSystem.BLL.Commissions.CommissionPeriodDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CommissionService.PeriodsAsync`](../CampionsSystem.API/Controllers/CommissionsController.cs#L15). Static test mapping: `CommissionDomainTests.cs`.

#### `POST /api/commissions/periods`

- Purpose: Open the relevant Commissions record or workflow.
- Eligible: Authenticated; `Permission:Commission.ClosePeriod`.
- Branch/record scope: commission period branch.
- URL inputs: none.
- Body: `OpenCommissionPeriodRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `name` **required** (string); `startDate` **required** (date YYYY-MM-DD); `endDate` **required** (date YYYY-MM-DD); `branchId` optional (UUID).
- Returns: `CampionsSystem.BLL.Commissions.CommissionPeriodDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CommissionService.OpenAsync`](../CampionsSystem.API/Controllers/CommissionsController.cs#L13). Static test mapping: `CommissionDomainTests.cs`.

#### `GET /api/commissions/periods/{id}`

- Purpose: Retrieve progress data in Commissions.
- Eligible: Authenticated; `Permission:Commission.View`.
- Branch/record scope: commission period branch.
- URL inputs: `id` (path; UUID; required); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Commissions.CommissionProgress`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CommissionService.ProgressAsync`](../CampionsSystem.API/Controllers/CommissionsController.cs#L20). Static test mapping: `CommissionDomainTests.cs`.

#### `GET /api/commissions/periods/{id}/employees/{employeeId}`

- Purpose: Retrieve employee data in Commissions.
- Eligible: Authenticated; `Permission:Commission.View`.
- Branch/record scope: commission period branch.
- URL inputs: `id` (path; UUID; required); `employeeId` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.Commissions.EmployeeCommissionStatement`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CommissionService.EmployeeStatementAsync`](../CampionsSystem.API/Controllers/CommissionsController.cs#L23). Static test mapping: `CommissionDomainTests.cs`.

#### `POST /api/commissions/periods/{id}/finalize`

- Purpose: Finalize the relevant Commissions record or workflow.
- Eligible: Authenticated; `Permission:Commission.ClosePeriod`.
- Branch/record scope: commission period branch.
- URL inputs: `id` (path; UUID; required).
- Body: `FinalizeCommissionPeriodRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `expectedVersion` **required** (integer/string).
- Returns: `CampionsSystem.BLL.Commissions.CommissionPeriodDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CommissionService.FinalizeAsync`](../CampionsSystem.API/Controllers/CommissionsController.cs#L18). Static test mapping: `CommissionDomainTests.cs`.

#### `GET /api/commissions/periods/{id}/refund-allocations`

- Purpose: Retrieve refund allocations data in Commissions.
- Eligible: Authenticated; `Permission:Commission.View`.
- Branch/record scope: commission period branch.
- URL inputs: `id` (path; UUID; required); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Commissions.CommissionPage<CampionsSystem.BLL.Commissions.CommissionRefundSource>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CommissionService.RefundSourcesAsync`](../CampionsSystem.API/Controllers/CommissionsController.cs#L28). Static test mapping: `CommissionDomainTests.cs`.

#### `GET /api/commissions/periods/{id}/sources`

- Purpose: Retrieve sources data in Commissions.
- Eligible: Authenticated; `Permission:Commission.View`.
- Branch/record scope: commission period branch.
- URL inputs: `id` (path; UUID; required); `employeeId` (query; UUID; optional); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Commissions.CommissionPage<CampionsSystem.BLL.Commissions.CommissionSourceLine>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CommissionService.SourcesAsync`](../CampionsSystem.API/Controllers/CommissionsController.cs#L25). Static test mapping: `CommissionDomainTests.cs`.

#### `GET /api/commissions/rules`

- Purpose: Retrieve rules data in Commissions.
- Eligible: Authenticated; `Permission:Commission.View`.
- Branch/record scope: commission period branch.
- URL inputs: `employeeId` (query; UUID; optional); `branchId` (query; UUID; optional); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Commissions.CommissionPage<CampionsSystem.BLL.Commissions.CommissionRuleDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CommissionService.RulesAsync`](../CampionsSystem.API/Controllers/CommissionsController.cs#L10). Static test mapping: `CommissionDomainTests.cs`.

#### `POST /api/commissions/rules`

- Purpose: Configure the relevant Commissions record or workflow.
- Eligible: Authenticated; `Permission:Commission.ManageRules`.
- Branch/record scope: commission period branch.
- URL inputs: none.
- Body: `ConfigureCommissionRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `employeeId` **required** (UUID); `effectiveFrom` **required** (date YYYY-MM-DD); `newClients` **required** (CategoryRuleInput); `renewals` **required** (CategoryRuleInput); `branchId` optional (UUID).
- Returns: `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Commissions.CommissionRuleDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CommissionService.ConfigureAsync`](../CampionsSystem.API/Controllers/CommissionsController.cs#L8). Static test mapping: `CommissionDomainTests.cs`.

### Crm

#### `GET /api/crm/house-follow-ups`

- Purpose: Retrieve house data in Crm.
- Eligible: Authenticated; `Permission:CRM.ManageHouseFollowUps`.
- Branch/record scope: member/lead owner and branch scope.
- URL inputs: `Window` (query; FollowUpWindow {Overdue, Today, Upcoming}; optional); `BranchId` (query; UUID; optional); `Page` (query; integer/string; optional; minimum=1, maximum=2147483647); `PageSize` (query; integer/string; optional; minimum=1, maximum=100).
- Body: none.
- Returns: `CampionsSystem.BLL.Crm.FollowUpListResult`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`LeadManagementService.GetHouseFollowUpsAsync`](../CampionsSystem.API/Controllers/CrmController.cs#L11). Static test mapping: `CrmManagementTests.cs`.

#### `POST /api/crm/house-follow-ups/{id}/assign`

- Purpose: Assign the relevant Crm record or workflow.
- Eligible: Authenticated; `Permission:CRM.ManageHouseFollowUps`.
- Branch/record scope: member/lead owner and branch scope.
- URL inputs: `id` (path; UUID; required).
- Body: `SalesAssignmentRequest` (`application/json`).
  - Top-level fields: `salesEmployeeId` optional (UUID); `reason` optional (string; maxLength=2048).
- Returns: `CampionsSystem.BLL.Members.MemberDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`LeadManagementService.AssignHouseFollowUpAsync`](../CampionsSystem.API/Controllers/CrmController.cs#L17). Static test mapping: `CrmManagementTests.cs`.

#### `POST /api/crm/member-follow-ups/{id}/complete`

- Purpose: Complete the relevant Crm record or workflow.
- Eligible: Authenticated; `Permission:CRM.ManageMemberFollowUps`.
- Branch/record scope: member/lead owner and branch scope.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `System.Guid`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`LeadManagementService.CompleteMemberFollowUpAsync`](../CampionsSystem.API/Controllers/CrmController.cs#L15). Static test mapping: `CrmManagementTests.cs`.

#### `POST /api/crm/members/{id}/follow-ups`

- Purpose: Open the relevant Crm record or workflow.
- Eligible: Authenticated; `Permission:CRM.ManageMemberFollowUps`.
- Branch/record scope: member/lead owner and branch scope.
- URL inputs: `id` (path; UUID; required).
- Body: `MemberFollowUpRequest` (`application/json`).
  - Top-level fields: `reason` **required** (string); `dueAt` optional (timestamp); `notes` optional (string; maxLength=4096).
- Returns: `System.Guid`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`LeadManagementService.OpenMemberFollowUpAsync`](../CampionsSystem.API/Controllers/CrmController.cs#L13). Static test mapping: `CrmManagementTests.cs`.

#### `GET /api/crm/my-follow-ups`

- Purpose: Retrieve mine data in Crm.
- Eligible: Authenticated; `Permission:CRM.ViewMemberFollowUps`.
- Branch/record scope: member/lead owner and branch scope.
- URL inputs: `Window` (query; FollowUpWindow {Overdue, Today, Upcoming}; optional); `BranchId` (query; UUID; optional); `Page` (query; integer/string; optional; minimum=1, maximum=2147483647); `PageSize` (query; integer/string; optional; minimum=1, maximum=100).
- Body: none.
- Returns: `CampionsSystem.BLL.Crm.FollowUpListResult`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`LeadManagementService.GetMyFollowUpsAsync`](../CampionsSystem.API/Controllers/CrmController.cs#L9). Static test mapping: `CrmManagementTests.cs`.

### EmployeeAdvances

#### `GET /api/employee-advances`

- Purpose: Retrieve history data in EmployeeAdvances.
- Eligible: Authenticated; `Permission:Advances.View`.
- Branch/record scope: recipient/account/Shift branch.
- URL inputs: `BranchId` (query; UUID; optional); `AccountId` (query; UUID; optional); `From` (query; date YYYY-MM-DD; optional); `To` (query; date YYYY-MM-DD; optional); `Page` (query; integer/string; optional); `PageSize` (query; integer/string; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.AdvanceDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ExpenseService.AdvancesAsync`](../CampionsSystem.API/Controllers/ExpensesController.cs#L30). Static test mapping: `EmployeeAdvanceTests.cs`.

#### `POST /api/employee-advances`

- Purpose: Paid the relevant EmployeeAdvances record or workflow.
- Eligible: Authenticated; `Permission:Advances.Create`.
- Branch/record scope: recipient/account/Shift branch.
- URL inputs: none.
- Body: `AdvanceRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `receivingEmployeeId` **required** (UUID); `financialAccountId` **required** (UUID); `amount` **required** (number/string); `reason` **required** (string); `reference` optional (string); `branchId` optional (UUID).
- Returns: `CampionsSystem.BLL.Shifts.AdvanceDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ExpenseService.RecordAdvanceAsync`](../CampionsSystem.API/Controllers/ExpensesController.cs#L28). Static test mapping: `EmployeeAdvanceTests.cs`.

#### `GET /api/employee-advances/{id}`

- Purpose: Retrieve get data in EmployeeAdvances.
- Eligible: Authenticated; `Permission:Advances.View`.
- Branch/record scope: recipient/account/Shift branch.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.AdvanceDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ExpenseService.GetAdvanceAsync`](../CampionsSystem.API/Controllers/ExpensesController.cs#L32). Static test mapping: `EmployeeAdvanceTests.cs`.

### EmployeeBranches

#### `POST /api/employees/{employeeId}/assign-branch`

- Purpose: Assign the relevant EmployeeBranches record or workflow.
- Eligible: Authenticated; `Permission:Employees.Manage`.
- Branch/record scope: employee source/destination assignment checks.
- URL inputs: `employeeId` (path; UUID; required).
- Body: `AssignBranchRequest` (`application/json`).
  - Top-level fields: `branchId` **required** (UUID); `startDate` **required** (date YYYY-MM-DD).
- Returns: `CampionsSystem.BLL.Branches.BranchAssignmentDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`EmployeeBranchAssignmentService.AssignAsync`](../CampionsSystem.API/Controllers/EmployeeBranchesController.cs#L17). Static test mapping: `EmployeeBranchAssignmentTests.cs`.

#### `GET /api/employees/{employeeId}/branch-assignment`

- Purpose: Retrieve current data in EmployeeBranches.
- Eligible: Authenticated; `Permission:Employees.View`.
- Branch/record scope: employee source/destination assignment checks.
- URL inputs: `employeeId` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.Branches.BranchAssignmentDetails?`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`EmployeeBranchAssignmentService.GetCurrentAsync`](../CampionsSystem.API/Controllers/EmployeeBranchesController.cs#L11). Static test mapping: `EmployeeBranchAssignmentTests.cs`.

#### `GET /api/employees/{employeeId}/branch-history`

- Purpose: Retrieve history data in EmployeeBranches.
- Eligible: Authenticated; `Permission:Employees.View`.
- Branch/record scope: employee source/destination assignment checks.
- URL inputs: `employeeId` (path; UUID; required).
- Body: none.
- Returns: `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Branches.BranchAssignmentDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`EmployeeBranchAssignmentService.HistoryAsync`](../CampionsSystem.API/Controllers/EmployeeBranchesController.cs#L14). Static test mapping: `EmployeeBranchAssignmentTests.cs`.

#### `POST /api/employees/{employeeId}/transfer-branch`

- Purpose: Transfer the relevant EmployeeBranches record or workflow.
- Eligible: Authenticated; `Permission:Employees.Transfer`.
- Branch/record scope: employee source/destination assignment checks.
- URL inputs: `employeeId` (path; UUID; required).
- Body: `TransferBranchRequest` (`application/json`).
  - Top-level fields: `targetBranchId` **required** (UUID); `effectiveDate` **required** (date YYYY-MM-DD).
- Returns: `CampionsSystem.BLL.Branches.BranchAssignmentDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`EmployeeBranchAssignmentService.TransferAsync`](../CampionsSystem.API/Controllers/EmployeeBranchesController.cs#L19). Static test mapping: `EmployeeBranchAssignmentTests.cs`.

### EmployeeSalaries

#### `GET /api/employee-salaries/{employeeId}/settings`

- Purpose: Retrieve history data in EmployeeSalaries.
- Eligible: Authenticated; `Permission:Salaries.View`.
- Branch/record scope: employee/branch salary setting.
- URL inputs: `employeeId` (path; UUID; required); `branchId` (query; UUID; optional).
- Body: none.
- Returns: `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.MonthlyFinance.SalarySettingDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SalaryService.HistoryAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L46). Static test mapping: `MonthlyFinanceSalaryTests.cs`.

#### `POST /api/employee-salaries/{employeeId}/settings`

- Purpose: Configure the relevant EmployeeSalaries record or workflow.
- Eligible: Authenticated; `Permission:Salaries.Manage`.
- Branch/record scope: employee/branch salary setting.
- URL inputs: `employeeId` (path; UUID; required).
- Body: `SetSalaryRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `basicSalary` **required** (number/string); `effectiveFrom` **required** (date YYYY-MM-DD); `reason` **required** (string).
- Returns: `CampionsSystem.BLL.MonthlyFinance.SalarySettingDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SalaryService.ConfigureAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L44). Static test mapping: `MonthlyFinanceSalaryTests.cs`.

### Employees

#### `GET /api/branches/{branchId}/employees`

- Purpose: Retrieve branch employees data in Employees.
- Eligible: Authenticated; `Permission:Employees.View`.
- Branch/record scope: employee branch/current scope; global when granted.
- URL inputs: `branchId` (path; UUID; required); `Search` (query; string; optional; maxLength=256); `BranchId` (query; UUID; optional); `Status` (query; EmploymentStatus; optional); `EmploymentType` (query; EmploymentType; optional); `Page` (query; integer/string; optional; minimum=1, maximum=2147483647); `PageSize` (query; integer/string; optional; minimum=1, maximum=100).
- Body: none.
- Returns: `CampionsSystem.BLL.Employees.EmployeeListResult`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`EmployeeManagementService.ListAsync`](../CampionsSystem.API/Controllers/EmployeesController.cs#L26). Static test mapping: `EmployeeManagementTests.cs`.

#### `GET /api/employees`

- Purpose: Retrieve list data in Employees.
- Eligible: Authenticated; `Permission:Employees.View`.
- Branch/record scope: employee branch/current scope; global when granted.
- URL inputs: `Search` (query; string; optional; maxLength=256); `BranchId` (query; UUID; optional); `Status` (query; EmploymentStatus; optional); `EmploymentType` (query; EmploymentType; optional); `Page` (query; integer/string; optional; minimum=1, maximum=2147483647); `PageSize` (query; integer/string; optional; minimum=1, maximum=100).
- Body: none.
- Returns: `CampionsSystem.BLL.Employees.EmployeeListResult`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`EmployeeManagementService.ListAsync`](../CampionsSystem.API/Controllers/EmployeesController.cs#L11). Static test mapping: `EmployeeManagementTests.cs`.

#### `POST /api/employees`

- Purpose: Create an employee record; Identity login linkage is a separate concern.
- Eligible: Authenticated; `Permission:Employees.Manage`.
- Branch/record scope: employee branch/current scope; global when granted.
- URL inputs: none.
- Body: `CreateEmployeeRequest` (`application/json`).
  - Top-level fields: `initialBranchId` optional (UUID); `fullName` **required** (string; maxLength=256); `phone` **required** (string; maxLength=64); `jobTitle` optional (string; maxLength=256); `employmentType` optional (EmploymentType); `hireDate` optional (date YYYY-MM-DD).
- Returns: `CampionsSystem.BLL.Employees.EmployeeDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`EmployeeManagementService.CreateAsync`](../CampionsSystem.API/Controllers/EmployeesController.cs#L14). Static test mapping: `EmployeeManagementTests.cs`.

#### `GET /api/employees/{id}`

- Purpose: Retrieve get data in Employees.
- Eligible: Authenticated; `Permission:Employees.View`.
- Branch/record scope: employee branch/current scope; global when granted.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.Employees.EmployeeDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`EmployeeManagementService.GetAsync`](../CampionsSystem.API/Controllers/EmployeesController.cs#L17). Static test mapping: `EmployeeManagementTests.cs`.

#### `PUT /api/employees/{id}`

- Purpose: Update the selected employee profile.
- Eligible: Authenticated; `Permission:Employees.Manage`.
- Branch/record scope: employee branch/current scope; global when granted.
- URL inputs: `id` (path; UUID; required).
- Body: `EmployeeProfileRequest` (`application/json`).
  - Top-level fields: `fullName` **required** (string; maxLength=256); `phone` **required** (string; maxLength=64); `jobTitle` optional (string; maxLength=256); `employmentType` optional (EmploymentType); `hireDate` optional (date YYYY-MM-DD).
- Returns: `CampionsSystem.BLL.Employees.EmployeeDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`EmployeeManagementService.UpdateAsync`](../CampionsSystem.API/Controllers/EmployeesController.cs#L20). Static test mapping: `EmployeeManagementTests.cs`.

#### `PATCH /api/employees/{id}/status`

- Purpose: Status the relevant Employees record or workflow.
- Eligible: Authenticated; `Permission:Employees.Manage`.
- Branch/record scope: employee branch/current scope; global when granted.
- URL inputs: `id` (path; UUID; required).
- Body: `ChangeEmployeeStatusRequest` (`application/json`).
  - Top-level fields: `status` **required** (EmploymentStatus); `reason` optional (string; maxLength=2048).
- Returns: `CampionsSystem.BLL.Employees.EmployeeDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`EmployeeManagementService.ChangeStatusAsync`](../CampionsSystem.API/Controllers/EmployeesController.cs#L23). Static test mapping: `EmployeeManagementTests.cs`.

### ExcelImports

#### `GET /api/excel-imports/{id}`

- Purpose: Retrieve preview/confirmation results.
- Eligible: Authenticated; `kind-dependent: Leads.Manage / Members.Create; FreelancePlans.ManageEligibleMembers for freelance`. `leads` needs `Leads.Manage`; `members` needs `Members.Create`; `freelance-customers` needs `Members.Create` and `FreelancePlans.ManageEligibleMembers`.
- Branch/record scope: creator-bound batch; each row branch rechecked.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `ExcelImportResult`.
- Errors: documented statuses 200, 403, 404; standard service validation and authorization errors may also occur.
- Source: [`ExcelImportService.GetAsync`](../CampionsSystem.API/Controllers/ExcelImportsController.cs#L55). Static test mapping: `ExcelImportTests.cs`.

#### `POST /api/excel-imports/{id}/confirm`

- Purpose: Revalidate and import eligible preview rows.
- Eligible: Authenticated; `kind-dependent: Leads.Manage / Members.Create; FreelancePlans.ManageEligibleMembers for freelance`. `leads` needs `Leads.Manage`; `members` needs `Members.Create`; `freelance-customers` needs `Members.Create` and `FreelancePlans.ManageEligibleMembers`.
- Branch/record scope: creator-bound batch; each row branch rechecked.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `ExcelImportResult`.
- Errors: documented statuses 200, 400, 403, 404; standard service validation and authorization errors may also occur.
- Source: [`ExcelImportService.ConfirmAsync`](../CampionsSystem.API/Controllers/ExcelImportsController.cs#L49). Static test mapping: `ExcelImportTests.cs`.

#### `POST /api/excel-imports/{kind}/preview`

- Purpose: Validate XLSX rows and save a short-lived preview without creating customer records.
- Eligible: Authenticated; `Leads.Manage`. `leads` needs `Leads.Manage`; `members` needs `Members.Create`; `freelance-customers` needs `Members.Create` and `FreelancePlans.ManageEligibleMembers`.
- Branch/record scope: creator-bound batch; each row branch rechecked.
- URL inputs: `kind` (path; string; required).
- Body: `multipart/form-data` field `File` containing `.xlsx` (1 MiB maximum).
- Returns: `ExcelImportResult`.
- Errors: documented statuses 200, 400, 403; standard service validation and authorization errors may also occur.
- Source: [`ExcelImportService.PreviewAsync`](../CampionsSystem.API/Controllers/ExcelImportsController.cs#L33). Static test mapping: `ExcelImportTests.cs`.

#### `GET /api/excel-imports/{kind}/template`

- Purpose: Download the selected blank XLSX template.
- Eligible: Authenticated; `Leads.Manage`. `leads` needs `Leads.Manage`; `members` needs `Members.Create`; `freelance-customers` needs `Members.Create` and `FreelancePlans.ManageEligibleMembers`.
- Branch/record scope: creator-bound batch; each row branch rechecked.
- URL inputs: `kind` (path; string; required).
- Body: none.
- Returns: `XLSX bytes`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ExcelImportService.TemplateAsync`](../CampionsSystem.API/Controllers/ExcelImportsController.cs#L19). Static test mapping: `ExcelImportTests.cs`.

### Expenses

#### `GET /api/expenses`

- Purpose: Retrieve history data in Expenses.
- Eligible: Authenticated; `Permission:Expenses.View`.
- Branch/record scope: expense/account branch; actor Shift except Owner paid expense.
- URL inputs: `BranchId` (query; UUID; optional); `AccountId` (query; UUID; optional); `From` (query; date YYYY-MM-DD; optional); `To` (query; date YYYY-MM-DD; optional); `Page` (query; integer/string; optional); `PageSize` (query; integer/string; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.ExpenseDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ExpenseService.HistoryAsync`](../CampionsSystem.API/Controllers/ExpensesController.cs#L10). Static test mapping: `OperationalExpenseTests.cs`.

#### `POST /api/expenses`

- Purpose: Paid the relevant Expenses record or workflow.
- Eligible: Authenticated; `Permission:Expenses.Create`.
- Branch/record scope: expense/account branch; actor Shift except Owner paid expense.
- URL inputs: none.
- Body: `PaidExpenseRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `expenseCategoryId` **required** (UUID); `financialAccountId` **required** (UUID); `amount` **required** (number/string); `description` **required** (string); `receiptReference` optional (string); `branchId` optional (UUID).
- Returns: `CampionsSystem.BLL.Shifts.ExpenseDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ExpenseService.RecordPaidAsync`](../CampionsSystem.API/Controllers/ExpensesController.cs#L8). Static test mapping: `OperationalExpenseTests.cs`.

#### `GET /api/expenses/categories`

- Purpose: Retrieve categories data in Expenses.
- Eligible: Authenticated; `Permission:Expenses.View`.
- Branch/record scope: expense/account branch; actor Shift except Owner paid expense.
- URL inputs: `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.ExpenseCategoryDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ExpenseService.CategoriesAsync`](../CampionsSystem.API/Controllers/ExpensesController.cs#L18). Static test mapping: `OperationalExpenseTests.cs`.

#### `POST /api/expenses/categories`

- Purpose: Category the relevant Expenses record or workflow.
- Eligible: Authenticated; `Permission:Expenses.Manage`.
- Branch/record scope: expense/account branch; actor Shift except Owner paid expense.
- URL inputs: none.
- Body: `ExpenseCategoryRequest` (`application/json`).
  - Top-level fields: `name` **required** (string); `isActive` optional (boolean; default=True).
- Returns: `CampionsSystem.BLL.Shifts.ExpenseCategoryDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ExpenseService.SetCategoryAsync`](../CampionsSystem.API/Controllers/ExpensesController.cs#L20). Static test mapping: `OperationalExpenseTests.cs`.

#### `PUT /api/expenses/categories/{id}`

- Purpose: Category the relevant Expenses record or workflow.
- Eligible: Authenticated; `Permission:Expenses.Manage`.
- Branch/record scope: expense/account branch; actor Shift except Owner paid expense.
- URL inputs: `id` (path; UUID; required).
- Body: `ExpenseCategoryRequest` (`application/json`).
  - Top-level fields: `name` **required** (string); `isActive` optional (boolean; default=True).
- Returns: `CampionsSystem.BLL.Shifts.ExpenseCategoryDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ExpenseService.SetCategoryAsync`](../CampionsSystem.API/Controllers/ExpensesController.cs#L22). Static test mapping: `OperationalExpenseTests.cs`.

#### `GET /api/expenses/{id}`

- Purpose: Retrieve get data in Expenses.
- Eligible: Authenticated; `Permission:Expenses.View`.
- Branch/record scope: expense/account branch; actor Shift except Owner paid expense.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.ExpenseDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ExpenseService.GetAsync`](../CampionsSystem.API/Controllers/ExpensesController.cs#L12). Static test mapping: `OperationalExpenseTests.cs`.

#### `GET /api/expenses/{id}/returns`

- Purpose: Retrieve returns data in Expenses.
- Eligible: Authenticated; `Permission:Expenses.View`.
- Branch/record scope: expense/account branch; actor Shift except Owner paid expense.
- URL inputs: `id` (path; UUID; required); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.ExpenseReversalDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ExpenseService.ReturnsAsync`](../CampionsSystem.API/Controllers/ExpensesController.cs#L16). Static test mapping: `OperationalExpenseTests.cs`.

#### `POST /api/expenses/{id}/returns`

- Purpose: Return the relevant Expenses record or workflow.
- Eligible: Authenticated; `Permission:Expenses.Reverse`.
- Branch/record scope: expense/account branch; actor Shift except Owner paid expense.
- URL inputs: `id` (path; UUID; required).
- Body: `ReverseExpenseRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `returnedAmount` **required** (number/string); `fundsActuallyReturned` **required** (boolean); `reason` **required** (string); `returnReference` optional (string).
- Returns: `CampionsSystem.BLL.Shifts.ExpenseReversalDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ExpenseService.ReverseAsync`](../CampionsSystem.API/Controllers/ExpensesController.cs#L14). Static test mapping: `OperationalExpenseTests.cs`.

### FinancialAccounts

#### `GET /api/financial-accounts`

- Purpose: Retrieve accounts data in FinancialAccounts.
- Eligible: Authenticated; `Permission:Payments.View`.
- Branch/record scope: account branch; active account selection.
- URL inputs: `branchId` (query; UUID; optional).
- Body: none.
- Returns: `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Sales.FinancialAccountDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SalesService.GetAccountsAsync`](../CampionsSystem.API/Controllers/FinancialAccountsController.cs#L8). Static test mapping: `SalesPaymentTests.cs`.

#### `POST /api/financial-accounts`

- Purpose: Create a branch financial account such as Cash or InstaPay.
- Eligible: Authenticated; `Permission:Branches.Manage` and `Permission:Branches.AccessAll`.
- Branch/record scope: account branch; active account selection.
- URL inputs: none.
- Body: `CreateFinancialAccountRequest` (`application/json`).
  - Top-level fields: `name` **required** (string); `method` **required** (FinancialAccountType); `branchId` optional (UUID).
- Returns: `CampionsSystem.BLL.Sales.FinancialAccountDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SalesService.CreateAccountAsync`](../CampionsSystem.API/Controllers/FinancialAccountsController.cs#L10). Static test mapping: `SalesPaymentTests.cs`.

### Leads

#### `GET /api/leads`

- Purpose: Retrieve list data in Leads.
- Eligible: Authenticated; `Permission:Leads.View`.
- Branch/record scope: Lead branch/current branch; global only when granted.
- URL inputs: `Search` (query; string; optional; maxLength=256); `BranchId` (query; UUID; optional); `SalesEmployeeId` (query; UUID; optional); `UnassignedOnly` (query; boolean; optional); `Status` (query; LeadStatus; optional); `FollowUp` (query; FollowUpWindow {Overdue, Today, Upcoming}; optional); `Page` (query; integer/string; optional; minimum=1, maximum=2147483647); `PageSize` (query; integer/string; optional; minimum=1, maximum=100).
- Body: none.
- Returns: `CampionsSystem.BLL.Crm.LeadListResult`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`LeadManagementService.GetLeadsAsync`](../CampionsSystem.API/Controllers/LeadsController.cs#L9). Static test mapping: `CrmApiTests.cs`.

#### `POST /api/leads`

- Purpose: Register one prospective customer.
- Eligible: Authenticated; `Permission:Leads.Manage`.
- Branch/record scope: Lead branch/current branch; global only when granted.
- URL inputs: none.
- Body: `CreateLeadRequest` (`application/json`).
  - Top-level fields: `branchId` optional (UUID); `fullName` optional (string; maxLength=256); `primaryPhone` **required** (string; maxLength=64); `secondaryPhone` optional (string; maxLength=64); `email` optional (string; maxLength=320); `source` optional (string; maxLength=256); `notes` optional (string; maxLength=4096).
- Returns: `CampionsSystem.BLL.Crm.LeadCreationResult`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`LeadManagementService.CreateLeadAsync`](../CampionsSystem.API/Controllers/LeadsController.cs#L17). Static test mapping: `CrmApiTests.cs`.

#### `POST /api/leads/bulk`

- Purpose: Bulk the relevant Leads record or workflow.
- Eligible: Authenticated; `Permission:Leads.Assign`.
- Branch/record scope: Lead branch/current branch; global only when granted.
- URL inputs: none.
- Body: JSON array of object; consult OpenAPI for element structure.
- Returns: `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Crm.LeadCreationResult>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`LeadManagementService.CreateLeadsBulkAsync`](../CampionsSystem.API/Controllers/LeadsController.cs#L19). Static test mapping: `CrmApiTests.cs`.

#### `GET /api/leads/lookup`

- Purpose: Retrieve lookup data in Leads.
- Eligible: Authenticated; `Permission:Leads.View`.
- Branch/record scope: Lead branch/current branch; global only when granted.
- URL inputs: `branchId` (query; UUID; optional); `phone` (query; string; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.Crm.LeadCreationResult`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`LeadManagementService.LookupPhoneAsync`](../CampionsSystem.API/Controllers/LeadsController.cs#L13). Static test mapping: `CrmApiTests.cs`.

#### `GET /api/leads/my`

- Purpose: Retrieve mine data in Leads.
- Eligible: Authenticated; `Permission:Leads.View`.
- Branch/record scope: Lead branch/current branch; global only when granted.
- URL inputs: `Search` (query; string; optional; maxLength=256); `BranchId` (query; UUID; optional); `SalesEmployeeId` (query; UUID; optional); `UnassignedOnly` (query; boolean; optional); `Status` (query; LeadStatus; optional); `FollowUp` (query; FollowUpWindow {Overdue, Today, Upcoming}; optional); `Page` (query; integer/string; optional; minimum=1, maximum=2147483647); `PageSize` (query; integer/string; optional; minimum=1, maximum=100).
- Body: none.
- Returns: `CampionsSystem.BLL.Crm.LeadListResult`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`LeadManagementService.GetMyLeadsAsync`](../CampionsSystem.API/Controllers/LeadsController.cs#L11). Static test mapping: `CrmApiTests.cs`.

#### `GET /api/leads/{id}`

- Purpose: Retrieve get data in Leads.
- Eligible: Authenticated; `Permission:Leads.View`.
- Branch/record scope: Lead branch/current branch; global only when granted.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.Crm.LeadDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`LeadManagementService.GetLeadByIdAsync`](../CampionsSystem.API/Controllers/LeadsController.cs#L15). Static test mapping: `CrmApiTests.cs`.

#### `PUT /api/leads/{id}`

- Purpose: Update an existing Lead profile.
- Eligible: Authenticated; `Permission:Leads.Manage`.
- Branch/record scope: Lead branch/current branch; global only when granted.
- URL inputs: `id` (path; UUID; required).
- Body: `UpdateLeadRequest` (`application/json`).
  - Top-level fields: `fullName` optional (string; maxLength=256); `primaryPhone` **required** (string; maxLength=64); `secondaryPhone` optional (string; maxLength=64); `email` optional (string; maxLength=320); `source` optional (string; maxLength=256); `notes` optional (string; maxLength=4096).
- Returns: `CampionsSystem.BLL.Crm.LeadDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`LeadManagementService.UpdateLeadAsync`](../CampionsSystem.API/Controllers/LeadsController.cs#L21). Static test mapping: `CrmApiTests.cs`.

#### `POST /api/leads/{id}/assign`

- Purpose: Assign the relevant Leads record or workflow.
- Eligible: Authenticated; `Permission:Leads.Assign`.
- Branch/record scope: Lead branch/current branch; global only when granted.
- URL inputs: `id` (path; UUID; required).
- Body: `LeadAssignmentRequest` (`application/json`).
  - Top-level fields: `salesEmployeeId` optional (UUID); `reason` optional (string; maxLength=2048).
- Returns: `CampionsSystem.BLL.Crm.LeadDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`LeadManagementService.AssignLeadAsync`](../CampionsSystem.API/Controllers/LeadsController.cs#L23). Static test mapping: `CrmApiTests.cs`.

#### `GET /api/leads/{id}/assignment`

- Purpose: Retrieve assignment data in Leads.
- Eligible: Authenticated; `Permission:Leads.View`.
- Branch/record scope: Lead branch/current branch; global only when granted.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.Crm.LeadAssignmentDetails?`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`LeadManagementService.GetCurrentLeadAssignmentAsync`](../CampionsSystem.API/Controllers/LeadsController.cs#L25). Static test mapping: `CrmApiTests.cs`.

#### `GET /api/leads/{id}/assignment-history`

- Purpose: Retrieve assignment history data in Leads.
- Eligible: Authenticated; `Permission:Leads.View`.
- Branch/record scope: Lead branch/current branch; global only when granted.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Crm.LeadAssignmentDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`LeadManagementService.GetLeadAssignmentHistoryAsync`](../CampionsSystem.API/Controllers/LeadsController.cs#L27). Static test mapping: `CrmApiTests.cs`.

#### `POST /api/leads/{id}/convert`

- Purpose: Convert an eligible Lead to a Member, preserving ownership.
- Eligible: Authenticated; `Permission:Leads.Convert`.
- Branch/record scope: Lead branch/current branch; global only when granted.
- URL inputs: `id` (path; UUID; required).
- Body: `ConvertLeadRequest` (`application/json`).
  - Top-level fields: `targetRegistrationBranchId` optional (UUID); `freelancePlanId` optional (UUID); `fullName` **required** (string; maxLength=256); `primaryPhone` **required** (string; maxLength=64); `secondaryPhone` optional (string; maxLength=64); `dateOfBirth` optional (date YYYY-MM-DD); `gender` optional (string; maxLength=32); `notes` optional (string; maxLength=4096).
- Returns: `CampionsSystem.BLL.Members.MemberDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`LeadManagementService.ConvertLeadAsync`](../CampionsSystem.API/Controllers/LeadsController.cs#L35). Static test mapping: `CrmApiTests.cs`.

#### `GET /api/leads/{id}/interactions`

- Purpose: Retrieve contacts data in Leads.
- Eligible: Authenticated; `Permission:Leads.View`.
- Branch/record scope: Lead branch/current branch; global only when granted.
- URL inputs: `id` (path; UUID; required); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Crm.LeadInteractionDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`LeadManagementService.GetLeadInteractionsAsync`](../CampionsSystem.API/Controllers/LeadsController.cs#L31). Static test mapping: `CrmApiTests.cs`.

#### `POST /api/leads/{id}/interactions`

- Purpose: Contact the relevant Leads record or workflow.
- Eligible: Authenticated; `Permission:Leads.Manage`.
- Branch/record scope: Lead branch/current branch; global only when granted.
- URL inputs: `id` (path; UUID; required).
- Body: `LeadInteractionRequest` (`application/json`).
  - Top-level fields: `interactionType` optional (InteractionType {Call, WhatsApp, Visit, Other}); `notes` **required** (string; maxLength=4096); `outcome` optional (string; maxLength=256); `contactedAt` optional (timestamp); `nextFollowUpAt` optional (timestamp); `status` optional (LeadStatus).
- Returns: `CampionsSystem.BLL.Crm.LeadDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`LeadManagementService.AddLeadInteractionAsync`](../CampionsSystem.API/Controllers/LeadsController.cs#L29). Static test mapping: `CrmApiTests.cs`.

#### `PATCH /api/leads/{id}/status`

- Purpose: Status the relevant Leads record or workflow.
- Eligible: Authenticated; `Permission:Leads.Manage`.
- Branch/record scope: Lead branch/current branch; global only when granted.
- URL inputs: `id` (path; UUID; required).
- Body: `LeadStatusRequest` (`application/json`).
  - Top-level fields: `status` **required** (LeadStatus); `reason` optional (string; maxLength=2048); `nextFollowUpAt` optional (timestamp).
- Returns: `CampionsSystem.BLL.Crm.LeadDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`LeadManagementService.ChangeLeadStatusAsync`](../CampionsSystem.API/Controllers/LeadsController.cs#L33). Static test mapping: `CrmApiTests.cs`.

### MaintenancePlans

#### `GET /api/maintenance-plans`

- Purpose: Retrieve plans data in MaintenancePlans.
- Eligible: Authenticated; `Permission:Maintenance.View`.
- Branch/record scope: plan branch; Owner global writes.
- URL inputs: `branchId` (query; UUID; optional); `asOf` (query; date YYYY-MM-DD; optional); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.MonthlyFinance.MaintenanceForecast>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MaintenanceService.PlansAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L66). Static test mapping: `MonthlyFinanceTests.cs`.

#### `POST /api/maintenance-plans`

- Purpose: Create or revise an informational plan without moving cash.
- Eligible: Authenticated; `Permission:Maintenance.Manage`.
- Branch/record scope: plan branch; Owner global writes.
- URL inputs: none.
- Body: `MaintenancePlanRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `branchId` **required** (UUID); `name` **required** (string); `expectedCost` **required** (number/string); `frequencyMonths` **required** (integer/string); `firstDueDate` **required** (date YYYY-MM-DD); `reason` **required** (string); `planId` optional (UUID); `expectedRevision` optional (integer/string; default=0); `isActive` optional (boolean; default=True); `description` optional (string); `nextExpectedDueDate` optional (date YYYY-MM-DD).
- Returns: `CampionsSystem.BLL.MonthlyFinance.MaintenanceForecast`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MaintenanceService.ConfigureAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L64). Static test mapping: `MonthlyFinanceTests.cs`.

#### `GET /api/maintenance-plans/reminders`

- Purpose: Retrieve reminders data in MaintenancePlans.
- Eligible: Authenticated; `Permission:Maintenance.View`.
- Branch/record scope: plan branch; Owner global writes.
- URL inputs: `branchId` (query; UUID; optional); `asOf` (query; date YYYY-MM-DD; optional); `horizonDays` (query; integer/string; optional; default=90); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.MonthlyFinance.MaintenanceForecast>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MaintenanceService.RemindersAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L71). Static test mapping: `MonthlyFinanceTests.cs`.

#### `GET /api/maintenance-plans/summary`

- Purpose: Retrieve summary data in MaintenancePlans.
- Eligible: Authenticated; `Permission:Maintenance.View`.
- Branch/record scope: plan branch; Owner global writes.
- URL inputs: `branchId` (query; UUID; optional); `asOf` (query; date YYYY-MM-DD; optional); `horizonDays` (query; integer/string; optional; default=90).
- Body: none.
- Returns: `CampionsSystem.BLL.MonthlyFinance.MaintenanceBranchSummary`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MaintenanceService.SummaryAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L68). Static test mapping: `MonthlyFinanceTests.cs`.

#### `GET /api/maintenance-plans/{id}/history`

- Purpose: Retrieve history data in MaintenancePlans.
- Eligible: Authenticated; `Permission:Maintenance.View`.
- Branch/record scope: plan branch; Owner global writes.
- URL inputs: `id` (path; UUID; required); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.MonthlyFinance.MaintenanceForecast>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MaintenanceService.HistoryAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L75). Static test mapping: `MonthlyFinanceTests.cs`.

### Members

#### `GET /api/members`

- Purpose: Retrieve list data in Members.
- Eligible: Authenticated; `Permission:Members.View`.
- Branch/record scope: home/registration branch; cross-branch needs dedicated capability.
- URL inputs: `Search` (query; string; optional; maxLength=256); `RegistrationBranchId` (query; UUID; optional); `ClientCode` (query; integer/string; optional); `IsActive` (query; boolean; optional); `SalesEmployeeId` (query; UUID; optional); `HouseOnly` (query; boolean; optional); `Page` (query; integer/string; optional; minimum=1, maximum=2147483647); `PageSize` (query; integer/string; optional; minimum=1, maximum=100).
- Body: none.
- Returns: `CampionsSystem.BLL.Members.MemberListResult`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MemberManagementService.GetMembersAsync`](../CampionsSystem.API/Controllers/MembersController.cs#L11). Static test mapping: `MemberManagementTests.cs`.

#### `POST /api/members`

- Purpose: Register one gym customer without selling a plan or collecting money.
- Eligible: Authenticated; `Permission:Members.Create`.
- Branch/record scope: home/registration branch; cross-branch needs dedicated capability.
- URL inputs: none.
- Body: `CreateMemberRequest` (`application/json`).
  - Top-level fields: `targetRegistrationBranchId` optional (UUID); `creationType` **required** (MemberCreationType {NormalSales, FreelanceHouse, ManagementCreated}); `salesEmployeeId` optional (UUID); `freelancePlanId` optional (UUID); `fullName` **required** (string; maxLength=256); `primaryPhone` **required** (string; maxLength=64); `secondaryPhone` optional (string; maxLength=64); `dateOfBirth` optional (date YYYY-MM-DD); `gender` optional (string; maxLength=32); `notes` optional (string; maxLength=4096).
- Returns: `CampionsSystem.BLL.Members.MemberDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MemberManagementService.CreateMemberAsync`](../CampionsSystem.API/Controllers/MembersController.cs#L15). Static test mapping: `MemberManagementTests.cs`.

#### `GET /api/members/{id}`

- Purpose: Retrieve get data in Members.
- Eligible: Authenticated; `Permission:Members.View`.
- Branch/record scope: home/registration branch; cross-branch needs dedicated capability.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.Members.MemberDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MemberManagementService.GetMemberByIdAsync`](../CampionsSystem.API/Controllers/MembersController.cs#L13). Static test mapping: `MemberManagementTests.cs`.

#### `PUT /api/members/{id}`

- Purpose: Update an existing gym customer profile.
- Eligible: Authenticated; `Permission:Members.Edit`.
- Branch/record scope: home/registration branch; cross-branch needs dedicated capability.
- URL inputs: `id` (path; UUID; required).
- Body: `UpdateMemberRequest` (`application/json`).
  - Top-level fields: `fullName` **required** (string; maxLength=256); `primaryPhone` **required** (string; maxLength=64); `secondaryPhone` optional (string; maxLength=64); `dateOfBirth` optional (date YYYY-MM-DD); `gender` optional (string; maxLength=32); `notes` optional (string; maxLength=4096).
- Returns: `CampionsSystem.BLL.Members.MemberDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MemberManagementService.UpdateMemberAsync`](../CampionsSystem.API/Controllers/MembersController.cs#L17). Static test mapping: `MemberManagementTests.cs`.

#### `POST /api/members/{id}/assign-sales`

- Purpose: Assign sales the relevant Members record or workflow.
- Eligible: Authenticated; `Permission:Members.AssignSales`.
- Branch/record scope: home/registration branch; cross-branch needs dedicated capability.
- URL inputs: `id` (path; UUID; required).
- Body: `SalesAssignmentRequest` (`application/json`).
  - Top-level fields: `salesEmployeeId` optional (UUID); `reason` optional (string; maxLength=2048).
- Returns: `CampionsSystem.BLL.Members.MemberDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MemberManagementService.AssignSalesAsync`](../CampionsSystem.API/Controllers/MembersController.cs#L25). Static test mapping: `MemberManagementTests.cs`.

#### `POST /api/members/{id}/return-to-house`

- Purpose: Return to house the relevant Members record or workflow.
- Eligible: Authenticated; `Permission:Members.AssignSales`.
- Branch/record scope: home/registration branch; cross-branch needs dedicated capability.
- URL inputs: `id` (path; UUID; required).
- Body: `ReturnToHouseRequest` (`application/json`).
  - Top-level fields: `reason` optional (string; maxLength=2048).
- Returns: `CampionsSystem.BLL.Members.MemberDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MemberManagementService.ReturnMemberToHouseAsync`](../CampionsSystem.API/Controllers/MembersController.cs#L27). Static test mapping: `MemberManagementTests.cs`.

#### `GET /api/members/{id}/sales-assignment`

- Purpose: Retrieve current sales data in Members.
- Eligible: Authenticated; `Permission:Members.View`.
- Branch/record scope: home/registration branch; cross-branch needs dedicated capability.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.Members.SalesAssignmentDetails?`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MemberManagementService.GetCurrentSalesAssignmentAsync`](../CampionsSystem.API/Controllers/MembersController.cs#L21). Static test mapping: `MemberManagementTests.cs`.

#### `GET /api/members/{id}/sales-history`

- Purpose: Retrieve sales history data in Members.
- Eligible: Authenticated; `Permission:Members.ViewSalesHistory`.
- Branch/record scope: home/registration branch; cross-branch needs dedicated capability.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Members.SalesAssignmentHistoryItem>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MemberManagementService.GetSalesAssignmentHistoryAsync`](../CampionsSystem.API/Controllers/MembersController.cs#L23). Static test mapping: `MemberManagementTests.cs`.

#### `PATCH /api/members/{id}/status`

- Purpose: Status the relevant Members record or workflow.
- Eligible: Authenticated; `Permission:Members.ChangeStatus`.
- Branch/record scope: home/registration branch; cross-branch needs dedicated capability.
- URL inputs: `id` (path; UUID; required).
- Body: `ChangeMemberStatusRequest` (`application/json`).
  - Top-level fields: `isActive` **required** (boolean); `reason` optional (string; maxLength=2048).
- Returns: `CampionsSystem.BLL.Members.MemberDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MemberManagementService.ChangeMemberStatusAsync`](../CampionsSystem.API/Controllers/MembersController.cs#L19). Static test mapping: `MemberManagementTests.cs`.

### MembershipBenefits

#### `GET /api/membership-benefits`

- Purpose: Retrieve list data in MembershipBenefits.
- Eligible: Authenticated; `Permission:MembershipPlans.View`.
- Branch/record scope: benefit catalog branch scope.
- URL inputs: `Search` (query; string; optional); `IsActive` (query; boolean; optional); `Page` (query; integer/string; optional); `PageSize` (query; integer/string; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.Plans.CatalogListResult`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MembershipPlanManagementService.GetBenefitsAsync`](../CampionsSystem.API/Controllers/MembershipBenefitsController.cs#L8). Static test mapping: `MembershipBenefitsTests.cs`.

#### `POST /api/membership-benefits`

- Purpose: Create a reusable membership benefit definition.
- Eligible: Authenticated; `Permission:MembershipPlans.Manage`.
- Branch/record scope: benefit catalog branch scope.
- URL inputs: none.
- Body: `SaveBenefitRequest` (`application/json`).
  - Top-level fields: `code` **required** (string; maxLength=128); `name` **required** (string; maxLength=256); `description` optional (string; maxLength=4096).
- Returns: `CampionsSystem.BLL.Plans.CatalogDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MembershipPlanManagementService.CreateBenefitAsync`](../CampionsSystem.API/Controllers/MembershipBenefitsController.cs#L12). Static test mapping: `MembershipBenefitsTests.cs`.

#### `GET /api/membership-benefits/{id}`

- Purpose: Retrieve get data in MembershipBenefits.
- Eligible: Authenticated; `Permission:MembershipPlans.View`.
- Branch/record scope: benefit catalog branch scope.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.Plans.CatalogDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MembershipPlanManagementService.GetBenefitByIdAsync`](../CampionsSystem.API/Controllers/MembershipBenefitsController.cs#L10). Static test mapping: `MembershipBenefitsTests.cs`.

#### `PUT /api/membership-benefits/{id}`

- Purpose: Update a membership benefit definition.
- Eligible: Authenticated; `Permission:MembershipPlans.Manage`.
- Branch/record scope: benefit catalog branch scope.
- URL inputs: `id` (path; UUID; required).
- Body: `SaveBenefitRequest` (`application/json`).
  - Top-level fields: `code` **required** (string; maxLength=128); `name` **required** (string; maxLength=256); `description` optional (string; maxLength=4096).
- Returns: `CampionsSystem.BLL.Plans.CatalogDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MembershipPlanManagementService.UpdateBenefitAsync`](../CampionsSystem.API/Controllers/MembershipBenefitsController.cs#L14). Static test mapping: `MembershipBenefitsTests.cs`.

#### `PATCH /api/membership-benefits/{id}/status`

- Purpose: Status the relevant MembershipBenefits record or workflow.
- Eligible: Authenticated; `Permission:MembershipPlans.Manage`.
- Branch/record scope: benefit catalog branch scope.
- URL inputs: `id` (path; UUID; required).
- Body: `ChangePlanStatusRequest` (`application/json`).
  - Top-level fields: `isActive` **required** (boolean).
- Returns: `CampionsSystem.BLL.Plans.CatalogDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MembershipPlanManagementService.ChangeBenefitStatusAsync`](../CampionsSystem.API/Controllers/MembershipBenefitsController.cs#L16). Static test mapping: `MembershipBenefitsTests.cs`.

### MembershipPlanCategories

#### `GET /api/membership-plan-categories`

- Purpose: Retrieve list data in MembershipPlanCategories.
- Eligible: Authenticated; `Permission:MembershipPlans.View`.
- Branch/record scope: configured branch/category scope.
- URL inputs: `Search` (query; string; optional); `IsActive` (query; boolean; optional); `Page` (query; integer/string; optional); `PageSize` (query; integer/string; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.Plans.CatalogListResult`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MembershipPlanManagementService.GetCategoriesAsync`](../CampionsSystem.API/Controllers/MembershipPlanCategoriesController.cs#L8). Static test mapping: `PlanCatalogTests.cs`.

#### `POST /api/membership-plan-categories`

- Purpose: Create a membership plan category.
- Eligible: Authenticated; `Permission:MembershipPlans.Manage`.
- Branch/record scope: configured branch/category scope.
- URL inputs: none.
- Body: `SaveCategoryRequest` (`application/json`).
  - Top-level fields: `name` **required** (string; maxLength=256); `description` optional (string; maxLength=4096).
- Returns: `CampionsSystem.BLL.Plans.CatalogDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MembershipPlanManagementService.CreateCategoryAsync`](../CampionsSystem.API/Controllers/MembershipPlanCategoriesController.cs#L10). Static test mapping: `PlanCatalogTests.cs`.

#### `PUT /api/membership-plan-categories/{id}`

- Purpose: Update a membership plan category.
- Eligible: Authenticated; `Permission:MembershipPlans.Manage`.
- Branch/record scope: configured branch/category scope.
- URL inputs: `id` (path; UUID; required).
- Body: `SaveCategoryRequest` (`application/json`).
  - Top-level fields: `name` **required** (string; maxLength=256); `description` optional (string; maxLength=4096).
- Returns: `CampionsSystem.BLL.Plans.CatalogDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MembershipPlanManagementService.UpdateCategoryAsync`](../CampionsSystem.API/Controllers/MembershipPlanCategoriesController.cs#L12). Static test mapping: `PlanCatalogTests.cs`.

#### `PATCH /api/membership-plan-categories/{id}/status`

- Purpose: Status the relevant MembershipPlanCategories record or workflow.
- Eligible: Authenticated; `Permission:MembershipPlans.Manage`.
- Branch/record scope: configured branch/category scope.
- URL inputs: `id` (path; UUID; required).
- Body: `ChangePlanStatusRequest` (`application/json`).
  - Top-level fields: `isActive` **required** (boolean).
- Returns: `CampionsSystem.BLL.Plans.CatalogDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MembershipPlanManagementService.ChangeCategoryStatusAsync`](../CampionsSystem.API/Controllers/MembershipPlanCategoriesController.cs#L14). Static test mapping: `PlanCatalogTests.cs`.

### MembershipPlans

#### `GET /api/membership-plans`

- Purpose: Retrieve list data in MembershipPlans.
- Eligible: Authenticated; `Permission:MembershipPlans.View`.
- Branch/record scope: plan-owned branch/city and eligible member.
- URL inputs: `BranchId` (query; UUID; optional); `CityId` (query; UUID; optional); `PlanType` (query; PlanCategory; optional); `AudienceCategoryId` (query; UUID; optional); `Mode` (query; AccessMode; optional); `FreelanceCoachEmployeeId` (query; UUID; optional); `IncludeInternal` (query; boolean; optional); `Search` (query; string; optional); `IsActive` (query; boolean; optional); `Page` (query; integer/string; optional); `PageSize` (query; integer/string; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.Plans.MembershipPlanListResult`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MembershipPlanManagementService.GetPlansAsync`](../CampionsSystem.API/Controllers/MembershipPlansController.cs#L8). Static test mapping: `PlanManagementApiTests.cs`.

#### `POST /api/membership-plans`

- Purpose: Create a membership plan with its rules and price.
- Eligible: Authenticated; `Permission:MembershipPlans.Manage`.
- Branch/record scope: plan-owned branch/city and eligible member.
- URL inputs: none.
- Body: `CreateMembershipPlanRequest` (`application/json`).
  - Top-level fields: `branchId` optional (UUID); `cityId` optional (UUID); `name` **required** (string; maxLength=256); `planType` **required** (PlanCategory); `audienceCategoryId` optional (UUID); `mode` **required** (AccessMode); `durationDays` optional (integer/string); `sessions` optional (integer/string); `price` optional (number/string); `allowFreeze` optional (boolean); `freezeDays` optional (integer/string); `freelanceCoachEmployeeId` optional (UUID).
- Returns: `CampionsSystem.BLL.Plans.MembershipPlanDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MembershipPlanManagementService.CreatePlanAsync`](../CampionsSystem.API/Controllers/MembershipPlansController.cs#L12). Static test mapping: `PlanManagementApiTests.cs`.

#### `GET /api/membership-plans/{id}`

- Purpose: Retrieve get data in MembershipPlans.
- Eligible: Authenticated; `Permission:MembershipPlans.View`.
- Branch/record scope: plan-owned branch/city and eligible member.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.Plans.MembershipPlanDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MembershipPlanManagementService.GetPlanByIdAsync`](../CampionsSystem.API/Controllers/MembershipPlansController.cs#L10). Static test mapping: `PlanManagementApiTests.cs`.

#### `PUT /api/membership-plans/{id}`

- Purpose: Update an existing membership plan.
- Eligible: Authenticated; `Permission:MembershipPlans.Manage`.
- Branch/record scope: plan-owned branch/city and eligible member.
- URL inputs: `id` (path; UUID; required).
- Body: `UpdateMembershipPlanRequest` (`application/json`).
  - Top-level fields: `branchId` optional (UUID); `cityId` optional (UUID); `name` **required** (string; maxLength=256); `planType` **required** (PlanCategory); `audienceCategoryId` optional (UUID); `mode` **required** (AccessMode); `durationDays` optional (integer/string); `sessions` optional (integer/string); `price` optional (number/string); `allowFreeze` optional (boolean); `freezeDays` optional (integer/string); `freelanceCoachEmployeeId` optional (UUID).
- Returns: `CampionsSystem.BLL.Plans.MembershipPlanDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MembershipPlanManagementService.UpdatePlanAsync`](../CampionsSystem.API/Controllers/MembershipPlansController.cs#L14). Static test mapping: `PlanManagementApiTests.cs`.

#### `GET /api/membership-plans/{id}/benefits`

- Purpose: Retrieve benefits data in MembershipPlans.
- Eligible: Authenticated; `Permission:MembershipPlans.View`.
- Branch/record scope: plan-owned branch/city and eligible member.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Plans.PlanBenefitDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MembershipPlanManagementService.GetPlanBenefitsAsync`](../CampionsSystem.API/Controllers/MembershipPlansController.cs#L18). Static test mapping: `PlanManagementApiTests.cs`.

#### `PUT /api/membership-plans/{id}/benefits`

- Purpose: Set benefits the relevant MembershipPlans record or workflow.
- Eligible: Authenticated; `Permission:MembershipPlans.Manage`.
- Branch/record scope: plan-owned branch/city and eligible member.
- URL inputs: `id` (path; UUID; required).
- Body: `SetPlanBenefitsRequest` (`application/json`).
  - Top-level fields: `benefits` optional (array of PlanBenefitRequest).
- Returns: `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Plans.PlanBenefitDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MembershipPlanManagementService.SetPlanBenefitsAsync`](../CampionsSystem.API/Controllers/MembershipPlansController.cs#L20). Static test mapping: `PlanManagementApiTests.cs`.

#### `GET /api/membership-plans/{id}/eligible-members`

- Purpose: Retrieve eligible data in MembershipPlans.
- Eligible: Authenticated; `Permission:FreelancePlans.ManageEligibleMembers`.
- Branch/record scope: plan-owned branch/city and eligible member.
- URL inputs: `id` (path; UUID; required); `Search` (query; string; optional); `IsActive` (query; boolean; optional); `Page` (query; integer/string; optional); `PageSize` (query; integer/string; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.Plans.EligibleMemberListResult`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MembershipPlanManagementService.GetEligibleMembersAsync`](../CampionsSystem.API/Controllers/MembershipPlansController.cs#L22). Static test mapping: `PlanManagementApiTests.cs`.

#### `POST /api/membership-plans/{id}/eligible-members`

- Purpose: Add eligible the relevant MembershipPlans record or workflow.
- Eligible: Authenticated; `Permission:FreelancePlans.ManageEligibleMembers`.
- Branch/record scope: plan-owned branch/city and eligible member.
- URL inputs: `id` (path; UUID; required).
- Body: `EligibleMemberRequest` (`application/json`).
  - Top-level fields: `memberId` optional (UUID).
- Returns: `bool`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MembershipPlanManagementService.AddEligibleMemberAsync`](../CampionsSystem.API/Controllers/MembershipPlansController.cs#L26). Static test mapping: `PlanManagementApiTests.cs`.

#### `GET /api/membership-plans/{id}/eligible-members/candidates`

- Purpose: Retrieve candidates data in MembershipPlans.
- Eligible: Authenticated; `Permission:FreelancePlans.ManageEligibleMembers`.
- Branch/record scope: plan-owned branch/city and eligible member.
- URL inputs: `id` (path; UUID; required); `MemberId` (query; UUID; optional); `Phone` (query; string; optional; maxLength=64); `MemberCode` (query; string; optional; maxLength=256).
- Body: none.
- Returns: `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Plans.EligibleMemberDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MembershipPlanManagementService.FindEligibleMemberCandidatesAsync`](../CampionsSystem.API/Controllers/MembershipPlansController.cs#L24). Static test mapping: `PlanManagementApiTests.cs`.

#### `DELETE /api/membership-plans/{id}/eligible-members/{memberId}`

- Purpose: Remove eligible the relevant MembershipPlans record or workflow.
- Eligible: Authenticated; `Permission:FreelancePlans.ManageEligibleMembers`.
- Branch/record scope: plan-owned branch/city and eligible member.
- URL inputs: `id` (path; UUID; required); `memberId` (path; UUID; required).
- Body: none.
- Returns: `bool`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MembershipPlanManagementService.RemoveEligibleMemberAsync`](../CampionsSystem.API/Controllers/MembershipPlansController.cs#L28). Static test mapping: `PlanManagementApiTests.cs`.

#### `PATCH /api/membership-plans/{id}/status`

- Purpose: Status the relevant MembershipPlans record or workflow.
- Eligible: Authenticated; `Permission:MembershipPlans.Manage`.
- Branch/record scope: plan-owned branch/city and eligible member.
- URL inputs: `id` (path; UUID; required).
- Body: `ChangePlanStatusRequest` (`application/json`).
  - Top-level fields: `isActive` **required** (boolean).
- Returns: `CampionsSystem.BLL.Plans.MembershipPlanDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MembershipPlanManagementService.ChangePlanStatusAsync`](../CampionsSystem.API/Controllers/MembershipPlansController.cs#L16). Static test mapping: `PlanManagementApiTests.cs`.

### MonthlyFinance

#### `GET /api/financial-months`

- Purpose: Retrieve history data in MonthlyFinance.
- Eligible: Authenticated; `Permission:FinancialMonths.View`.
- Branch/record scope: financial month branch; consolidation global.
- URL inputs: `branchId` (query; UUID; optional); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.MonthlyFinance.FinancialMonthDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MonthlyFinanceService.HistoryAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L11). Static test mapping: `MonthlyFinanceApiTests.cs; MonthlyFinanceIntegrationTests.cs`.

#### `POST /api/financial-months`

- Purpose: Open the relevant MonthlyFinance record or workflow.
- Eligible: Authenticated; `Permission:FinancialMonths.Manage`.
- Branch/record scope: financial month branch; consolidation global.
- URL inputs: none.
- Body: `OpenFinancialMonthRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `name` **required** (string); `startDate` **required** (date YYYY-MM-DD); `endDate` **required** (date YYYY-MM-DD); `reason` **required** (string); `branchId` optional (UUID).
- Returns: `CampionsSystem.BLL.MonthlyFinance.FinancialMonthDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MonthlyFinanceService.OpenAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L9). Static test mapping: `MonthlyFinanceApiTests.cs; MonthlyFinanceIntegrationTests.cs`.

#### `GET /api/financial-months/consolidated`

- Purpose: Retrieve consolidated data in MonthlyFinance.
- Eligible: Authenticated; `Permission:FinancialMonths.ViewConsolidated`.
- Branch/record scope: financial month branch; consolidation global.
- URL inputs: `monthIds` (query; array of UUID; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.MonthlyFinance.ConsolidatedMonthSummary`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MonthlyFinanceService.ConsolidatedAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L36). Static test mapping: `MonthlyFinanceApiTests.cs; MonthlyFinanceIntegrationTests.cs`.

#### `POST /api/financial-months/transactions/{ledgerId}/original-occurrence`

- Purpose: Occurrence the relevant MonthlyFinance record or workflow.
- Eligible: Authenticated; `Permission:FinancialMonths.Manage`.
- Branch/record scope: financial month branch; consolidation global.
- URL inputs: `ledgerId` (path; UUID; required).
- Body: `RecordOccurrenceRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `originalOccurredAt` **required** (timestamp); `reason` **required** (string).
- Returns: `System.Guid`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MonthlyFinanceService.RecordOccurrenceAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L38). Static test mapping: `MonthlyFinanceApiTests.cs; MonthlyFinanceIntegrationTests.cs`.

#### `GET /api/financial-months/{id}`

- Purpose: Retrieve get data in MonthlyFinance.
- Eligible: Authenticated; `Permission:FinancialMonths.View`.
- Branch/record scope: financial month branch; consolidation global.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.MonthlyFinance.FinancialMonthDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MonthlyFinanceService.GetAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L13). Static test mapping: `MonthlyFinanceApiTests.cs; MonthlyFinanceIntegrationTests.cs`.

#### `POST /api/financial-months/{id}/close`

- Purpose: Close the relevant MonthlyFinance record or workflow.
- Eligible: Authenticated; `Permission:FinancialMonths.Close`.
- Branch/record scope: financial month branch; consolidation global.
- URL inputs: `id` (path; UUID; required).
- Body: `CloseFinancialMonthRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `expectedVersion` **required** (integer/string); `reason` **required** (string); `countedCash` optional (number/string).
- Returns: `CampionsSystem.BLL.MonthlyFinance.FinancialMonthDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MonthlyFinanceService.CloseAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L15). Static test mapping: `MonthlyFinanceApiTests.cs; MonthlyFinanceIntegrationTests.cs`.

#### `GET /api/financial-months/{id}/employees/{employeeId}/salary`

- Purpose: Retrieve salary data in MonthlyFinance.
- Eligible: Authenticated; `Permission:Salaries.View`.
- Branch/record scope: financial month branch; consolidation global.
- URL inputs: `id` (path; UUID; required); `employeeId` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.MonthlyFinance.SalaryEntitlement`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SalaryService.PreviewAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L34). Static test mapping: `MonthlyFinanceApiTests.cs; MonthlyFinanceIntegrationTests.cs`.

#### `GET /api/financial-months/{id}/events`

- Purpose: Retrieve events data in MonthlyFinance.
- Eligible: Authenticated; `Permission:FinancialMonths.View`.
- Branch/record scope: financial month branch; consolidation global.
- URL inputs: `id` (path; UUID; required); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.MonthlyFinance.MonthEventDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MonthlyFinanceService.EventsAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L25). Static test mapping: `MonthlyFinanceApiTests.cs; MonthlyFinanceIntegrationTests.cs`.

#### `GET /api/financial-months/{id}/export`

- Purpose: Download the six-sheet monthly XLSX workbook.
- Eligible: Authenticated; `Permission:FinancialMonths.Export`.
- Branch/record scope: financial month branch; consolidation global.
- URL inputs: `id` (path; UUID; required); `revision` (query; integer/string; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.MonthlyFinance.WorkbookDownload`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MonthlyFinanceService.ExportAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L27). Static test mapping: `MonthlyFinanceApiTests.cs; MonthlyFinanceIntegrationTests.cs`.

#### `POST /api/financial-months/{id}/reopen`

- Purpose: Reopen the relevant MonthlyFinance record or workflow.
- Eligible: Authenticated; `Permission:FinancialMonths.Reopen`.
- Branch/record scope: financial month branch; consolidation global.
- URL inputs: `id` (path; UUID; required).
- Body: `ReopenFinancialMonthRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `expectedVersion` **required** (integer/string); `reason` **required** (string).
- Returns: `CampionsSystem.BLL.MonthlyFinance.FinancialMonthDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MonthlyFinanceService.ReopenAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L17). Static test mapping: `MonthlyFinanceApiTests.cs; MonthlyFinanceIntegrationTests.cs`.

#### `GET /api/financial-months/{id}/report`

- Purpose: Retrieve report data in MonthlyFinance.
- Eligible: Authenticated; `Permission:FinancialMonths.View`.
- Branch/record scope: financial month branch; consolidation global.
- URL inputs: `id` (path; UUID; required); `revision` (query; integer/string; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.MonthlyFinance.MonthlyReport`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MonthlyFinanceService.ReportAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L19). Static test mapping: `MonthlyFinanceApiTests.cs; MonthlyFinanceIntegrationTests.cs`.

#### `GET /api/financial-months/{id}/revisions`

- Purpose: Retrieve revisions data in MonthlyFinance.
- Eligible: Authenticated; `Permission:FinancialMonths.View`.
- Branch/record scope: financial month branch; consolidation global.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.MonthlyFinance.RevisionDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MonthlyFinanceService.RevisionsAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L23). Static test mapping: `MonthlyFinanceApiTests.cs; MonthlyFinanceIntegrationTests.cs`.

#### `GET /api/financial-months/{id}/transactions`

- Purpose: Retrieve transactions data in MonthlyFinance.
- Eligible: Authenticated; `Permission:FinancialMonths.View`.
- Branch/record scope: financial month branch; consolidation global.
- URL inputs: `id` (path; UUID; required); `revision` (query; integer/string; optional); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.MonthlyFinance.FinancialTransactionReportRow>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`MonthlyFinanceService.TransactionsAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L21). Static test mapping: `MonthlyFinanceApiTests.cs; MonthlyFinanceIntegrationTests.cs`.

### Offers

#### `PUT /api/members/{memberId}/audience-categories`

- Purpose: Audiences the relevant Offers record or workflow.
- Eligible: Authenticated; `Permission:Members.Manage`.
- Branch/record scope: offer/plan/member branch eligibility.
- URL inputs: `memberId` (path; UUID; required).
- Body: `SetMemberAudienceRequest` (`application/json`).
  - Top-level fields: `audienceCategoryIds` **required** (array of UUID).
- Returns: `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Sales.MemberAudienceDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`OfferService.SetMemberAudiencesAsync`](../CampionsSystem.API/Controllers/OffersController.cs#L24). Static test mapping: `SalesOfferTests.cs`.

#### `GET /api/offers`

- Purpose: Retrieve eligible data in Offers.
- Eligible: Authenticated; `Permission:Payments.Create`.
- Branch/record scope: offer/plan/member branch eligibility.
- URL inputs: `memberId` (query; UUID; optional); `membershipPlanId` (query; UUID; optional); `branchId` (query; UUID; optional).
- Body: none.
- Returns: `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Sales.OfferDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`OfferService.GetEligibleAsync`](../CampionsSystem.API/Controllers/OffersController.cs#L8). Static test mapping: `SalesOfferTests.cs`.

#### `POST /api/offers`

- Purpose: Create an offer using its configured audience/rules.
- Eligible: Authenticated; `Permission:Offers.Manage`.
- Branch/record scope: offer/plan/member branch eligibility.
- URL inputs: none.
- Body: `OfferRequest` (`application/json`).
  - Top-level fields: `name` **required** (string); `type` **required** (OfferType); `value` **required** (number/string); `membershipPlanIds` **required** (array of UUID); `eligibilityMode` **required** (OfferEligibilityMode); `audienceCategoryId` optional (UUID); `startDate` optional (date YYYY-MM-DD); `endDate` optional (date YYYY-MM-DD); `isPermanent` optional (boolean; default=False); `isActive` optional (boolean; default=True); `description` optional (string); `branchId` optional (UUID).
- Returns: `CampionsSystem.BLL.Sales.OfferDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`OfferService.CreateAsync`](../CampionsSystem.API/Controllers/OffersController.cs#L16). Static test mapping: `SalesOfferTests.cs`.

#### `GET /api/offers/management`

- Purpose: Retrieve management data in Offers.
- Eligible: Authenticated; `Permission:Offers.Manage`.
- Branch/record scope: offer/plan/member branch eligibility.
- URL inputs: `branchId` (query; UUID; optional); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Sales.OfferHistory`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`OfferService.GetManagementHistoryAsync`](../CampionsSystem.API/Controllers/OffersController.cs#L12). Static test mapping: `SalesOfferTests.cs`.

#### `GET /api/offers/quote`

- Purpose: Retrieve quote data in Offers.
- Eligible: Authenticated; `Permission:Payments.Create`.
- Branch/record scope: offer/plan/member branch eligibility.
- URL inputs: `memberId` (query; UUID; optional); `membershipPlanId` (query; UUID; optional); `offerId` (query; UUID; optional); `branchId` (query; UUID; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.Sales.PricingResult`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`OfferService.QuoteAsync`](../CampionsSystem.API/Controllers/OffersController.cs#L10). Static test mapping: `SalesOfferTests.cs`.

#### `GET /api/offers/{id}`

- Purpose: Retrieve details data in Offers.
- Eligible: Authenticated; `Permission:Offers.Manage`.
- Branch/record scope: offer/plan/member branch eligibility.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.Sales.OfferDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`OfferService.GetAsync`](../CampionsSystem.API/Controllers/OffersController.cs#L14). Static test mapping: `SalesOfferTests.cs`.

#### `PUT /api/offers/{id}`

- Purpose: Update an existing offer.
- Eligible: Authenticated; `Permission:Offers.Manage`.
- Branch/record scope: offer/plan/member branch eligibility.
- URL inputs: `id` (path; UUID; required).
- Body: `OfferRequest` (`application/json`).
  - Top-level fields: `name` **required** (string); `type` **required** (OfferType); `value` **required** (number/string); `membershipPlanIds` **required** (array of UUID); `eligibilityMode` **required** (OfferEligibilityMode); `audienceCategoryId` optional (UUID); `startDate` optional (date YYYY-MM-DD); `endDate` optional (date YYYY-MM-DD); `isPermanent` optional (boolean; default=False); `isActive` optional (boolean; default=True); `description` optional (string); `branchId` optional (UUID).
- Returns: `CampionsSystem.BLL.Sales.OfferDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`OfferService.UpdateAsync`](../CampionsSystem.API/Controllers/OffersController.cs#L18). Static test mapping: `SalesOfferTests.cs`.

#### `POST /api/offers/{id}/member-entitlements`

- Purpose: Grant the relevant Offers record or workflow.
- Eligible: Authenticated; `Permission:Offers.Manage`.
- Branch/record scope: offer/plan/member branch eligibility.
- URL inputs: `id` (path; UUID; required).
- Body: `GrantMemberOfferRequest` (`application/json`).
  - Top-level fields: `memberId` **required** (UUID); `startDate` optional (date YYYY-MM-DD); `endDate` optional (date YYYY-MM-DD).
- Returns: `CampionsSystem.BLL.Sales.MemberOfferGrantDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`OfferService.GrantMemberAsync`](../CampionsSystem.API/Controllers/OffersController.cs#L20). Static test mapping: `SalesOfferTests.cs`.

#### `DELETE /api/offers/{id}/member-entitlements/{grantId}`

- Purpose: Revoke the relevant Offers record or workflow.
- Eligible: Authenticated; `Permission:Offers.Manage`.
- Branch/record scope: offer/plan/member branch eligibility.
- URL inputs: `id` (path; UUID; required); `grantId` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.Sales.MemberOfferGrantDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`OfferService.RevokeMemberAsync`](../CampionsSystem.API/Controllers/OffersController.cs#L22). Static test mapping: `SalesOfferTests.cs`.

### Operations

#### `DELETE /api/activities/coach-assignments/{id}`

- Purpose: Unassign the relevant Operations record or workflow.
- Eligible: Authenticated; `Permission:Activities.ManageCoaches`.
- Branch/record scope: activity/freelance branch; 410 routes disabled.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `<anonymous type: bool Updated>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ActivityManagementService.UnassignAsync`](../CampionsSystem.API/Controllers/OperationsController.cs#L31). Static test mapping: `ActivitiesClassesFreelanceTests.cs`.

#### `GET /api/activities/coach-revenue/{periodId}`

- Purpose: Retrieve activity revenue data in Operations.
- Eligible: Authenticated; `Permission:Activities.ViewCoachRevenue`.
- Branch/record scope: activity/freelance branch; 410 routes disabled.
- URL inputs: `periodId` (path; UUID; required).
- Body: none.
- Returns: `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Operations.ActivityCoachReport>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachReportService.ActivitiesAsync`](../CampionsSystem.API/Controllers/OperationsController.cs#L41). Static test mapping: `ActivitiesClassesFreelanceTests.cs`.

#### `POST /api/activities/coach-revenue/{periodId}/finalize`

- Purpose: Finalize activity the relevant Operations record or workflow.
- Eligible: Authenticated; `Permission:Commission.ClosePeriod`.
- Branch/record scope: activity/freelance branch; 410 routes disabled.
- URL inputs: `periodId` (path; UUID; required).
- Body: none.
- Returns: `<anonymous type: bool Finalized>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachReportService.FinalizeActivitiesAsync`](../CampionsSystem.API/Controllers/OperationsController.cs#L45). Static test mapping: `ActivitiesClassesFreelanceTests.cs`.

#### `POST /api/activities/coaches`

- Purpose: Create coach in Operations.
- Eligible: Authenticated; `Permission:Activities.ManageCoaches`.
- Branch/record scope: activity/freelance branch; 410 routes disabled.
- URL inputs: none.
- Body: `CreateActivityCoachRequest` (`application/json`).
  - Top-level fields: `coachType` **required** (ActivityCoachType); `employeeId` **required** (UUID); `externalName` **required** (string); `externalPhone` **required** (string); `percentage` **required** (number/string).
- Returns: `<anonymous type: System.Guid Id>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ActivityManagementService.CreateCoachAsync`](../CampionsSystem.API/Controllers/OperationsController.cs#L16). Static test mapping: `ActivitiesClassesFreelanceTests.cs`.

#### `PUT /api/activities/coaches/{id}/percentage`

- Purpose: Percentage the relevant Operations record or workflow.
- Eligible: Authenticated; `Permission:Activities.ManageCoaches`.
- Branch/record scope: activity/freelance branch; 410 routes disabled.
- URL inputs: `id` (path; UUID; required).
- Body: `PercentageRequest` (`application/json`).
  - Top-level fields: `percentage` **required** (number/string).
- Returns: `<anonymous type: bool Updated>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ActivityManagementService.SetPercentageAsync`](../CampionsSystem.API/Controllers/OperationsController.cs#L21). Static test mapping: `ActivitiesClassesFreelanceTests.cs`.

#### `POST /api/activities/purchases`

- Purpose: Purchase activity the relevant Operations record or workflow.
- Eligible: Authenticated; `Permission:Activities.Manage`.
- Branch/record scope: activity/freelance branch; 410 routes disabled.
- URL inputs: none.
- Body: `ActivityPurchaseRequest` (`application/json`).
  - Top-level fields: `memberId` **required** (UUID); `activityPackageId` **required** (UUID); `activityCoachId` **required** (UUID); `existingSubscriptionId` optional (UUID).
- Returns: `CampionsSystem.BLL.Operations.PurchaseResult`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SubscriptionPurchaseService.PurchaseActivityAsync`](../CampionsSystem.API/Controllers/OperationsController.cs#L36). Static test mapping: `ActivitiesClassesFreelanceTests.cs`.

#### `POST /api/activities/{id}/coach-assignments`

- Purpose: Assign the relevant Operations record or workflow.
- Eligible: Authenticated; `Permission:Activities.ManageCoaches`.
- Branch/record scope: activity/freelance branch; 410 routes disabled.
- URL inputs: `id` (path; UUID; required).
- Body: `AssignmentRequest` (`application/json`).
  - Top-level fields: `activityCoachId` **required** (UUID).
- Returns: `<anonymous type: System.Guid Id>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ActivityManagementService.AssignAsync`](../CampionsSystem.API/Controllers/OperationsController.cs#L26). Static test mapping: `ActivitiesClassesFreelanceTests.cs`.

#### `GET /api/classes/attendance-report`

- Purpose: Retrieve class report data in Operations.
- Eligible: Authenticated; `Permission:Classes.ViewAttendanceReport`.
- Branch/record scope: activity/freelance branch; 410 routes disabled.
- URL inputs: `branchId` (query; UUID; optional); `from` (query; timestamp; optional); `until` (query; timestamp; optional).
- Body: none.
- Returns: `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Operations.ClassCoachReport>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachReportService.ClassesAsync`](../CampionsSystem.API/Controllers/OperationsController.cs#L65). Static test mapping: `ActivitiesClassesFreelanceTests.cs`.

#### `POST /api/classes/sessions`

- Purpose: Disabled compatibility endpoint; HTTP 410.
- Eligible: Authenticated; `Permission:Classes.Manage`.
- Branch/record scope: activity/freelance branch; 410 routes disabled.
- URL inputs: none.
- Body: `SaveClassSessionRequest` (`application/json`).
  - Top-level fields: `groupClassId` **required** (UUID); `classScheduleId` **required** (UUID); `coachEmployeeId` **required** (UUID); `startedAt` **required** (timestamp); `endedAt` **required** (timestamp); `status` **required** (ClassSessionStatus).
- Returns: `ProblemDetails (410 Gone)` (HTTP 410).
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`controller inline 410`](../CampionsSystem.API/Controllers/OperationsController.cs#L50). Static test mapping: `ActivitiesClassesFreelanceTests.cs`.

#### `PUT /api/classes/sessions/{id}`

- Purpose: Disabled compatibility endpoint; HTTP 410.
- Eligible: Authenticated; `Permission:Classes.Manage`.
- Branch/record scope: activity/freelance branch; 410 routes disabled.
- URL inputs: `id` (path; UUID; required).
- Body: `SaveClassSessionRequest` (`application/json`).
  - Top-level fields: `groupClassId` **required** (UUID); `classScheduleId` **required** (UUID); `coachEmployeeId` **required** (UUID); `startedAt` **required** (timestamp); `endedAt` **required** (timestamp); `status` **required** (ClassSessionStatus).
- Returns: `ProblemDetails (410 Gone)` (HTTP 410).
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`controller inline 410`](../CampionsSystem.API/Controllers/OperationsController.cs#L55). Static test mapping: `ActivitiesClassesFreelanceTests.cs`.

#### `POST /api/classes/sessions/{id}/attendance/{memberId}`

- Purpose: Disabled compatibility endpoint; HTTP 410.
- Eligible: Authenticated; `Permission:Classes.Manage`.
- Branch/record scope: activity/freelance branch; 410 routes disabled.
- URL inputs: `id` (path; UUID; required); `memberId` (path; UUID; required).
- Body: none.
- Returns: `ProblemDetails (410 Gone)` (HTTP 410).
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`controller inline 410`](../CampionsSystem.API/Controllers/OperationsController.cs#L60). Static test mapping: `ActivitiesClassesFreelanceTests.cs`.

#### `POST /api/freelance-plans`

- Purpose: Create plan in Operations.
- Eligible: Authenticated; `Permission:FreelancePlans.Manage`.
- Branch/record scope: activity/freelance branch; 410 routes disabled.
- URL inputs: none.
- Body: `SaveFreelancePlanRequest` (`application/json`).
  - Top-level fields: `cityId` **required** (UUID); `coachEmployeeId` **required** (UUID); `name` **required** (string); `price` **required** (number/string); `durationDays` **required** (integer/string); `accessMode` **required** (AccessMode); `sessionCount` **required** (integer/string); `isActive` optional (boolean; default=True); `branchId` optional (UUID).
- Returns: `<anonymous type: System.Guid Id>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`FreelancePlanService.SaveAsync`](../CampionsSystem.API/Controllers/OperationsController.cs#L70). Static test mapping: `ActivitiesClassesFreelanceTests.cs`.

#### `POST /api/freelance-plans/purchases`

- Purpose: Disabled compatibility endpoint; HTTP 410.
- Eligible: Authenticated; `Permission:FreelancePlans.Manage`.
- Branch/record scope: activity/freelance branch; 410 routes disabled.
- URL inputs: none.
- Body: `FreelancePurchaseRequest` (`application/json`).
  - Top-level fields: `memberId` **required** (UUID); `membershipPlanId` **required** (UUID); `branchId` **required** (UUID); `existingSubscriptionId` optional (UUID).
- Returns: `ProblemDetails (410 Gone)` (HTTP 410).
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`controller inline 410`](../CampionsSystem.API/Controllers/OperationsController.cs#L90). Static test mapping: `ActivitiesClassesFreelanceTests.cs`.

#### `PUT /api/freelance-plans/{id}`

- Purpose: Update plan in Operations.
- Eligible: Authenticated; `Permission:FreelancePlans.Manage`.
- Branch/record scope: activity/freelance branch; 410 routes disabled.
- URL inputs: `id` (path; UUID; required).
- Body: `SaveFreelancePlanRequest` (`application/json`).
  - Top-level fields: `cityId` **required** (UUID); `coachEmployeeId` **required** (UUID); `name` **required** (string); `price` **required** (number/string); `durationDays` **required** (integer/string); `accessMode` **required** (AccessMode); `sessionCount` **required** (integer/string); `isActive` optional (boolean; default=True); `branchId` optional (UUID).
- Returns: `<anonymous type: System.Guid Id>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`FreelancePlanService.SaveAsync`](../CampionsSystem.API/Controllers/OperationsController.cs#L75). Static test mapping: `ActivitiesClassesFreelanceTests.cs`.

#### `DELETE /api/freelance-plans/{id}/eligible-members/{memberId}`

- Purpose: Remove eligible the relevant Operations record or workflow.
- Eligible: Authenticated; `Permission:FreelancePlans.ManageEligibleMembers`.
- Branch/record scope: activity/freelance branch; 410 routes disabled.
- URL inputs: `id` (path; UUID; required); `memberId` (path; UUID; required).
- Body: none.
- Returns: `<anonymous type: bool Eligible>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`FreelancePlanService.SetEligibilityAsync`](../CampionsSystem.API/Controllers/OperationsController.cs#L85). Static test mapping: `ActivitiesClassesFreelanceTests.cs`.

#### `PUT /api/freelance-plans/{id}/eligible-members/{memberId}`

- Purpose: Add eligible the relevant Operations record or workflow.
- Eligible: Authenticated; `Permission:FreelancePlans.ManageEligibleMembers`.
- Branch/record scope: activity/freelance branch; 410 routes disabled.
- URL inputs: `id` (path; UUID; required); `memberId` (path; UUID; required).
- Body: none.
- Returns: `<anonymous type: bool Eligible>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`FreelancePlanService.SetEligibilityAsync`](../CampionsSystem.API/Controllers/OperationsController.cs#L80). Static test mapping: `ActivitiesClassesFreelanceTests.cs`.

#### `GET /api/freelance-plans/{id}/report`

- Purpose: Retrieve freelance report data in Operations.
- Eligible: Authenticated; `Permission:FreelancePlans.View`.
- Branch/record scope: activity/freelance branch; 410 routes disabled.
- URL inputs: `id` (path; UUID; required); `branchId` (query; UUID; optional); `from` (query; timestamp; optional); `until` (query; timestamp; optional).
- Body: none.
- Returns: `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Operations.FreelancePlanReport>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`CoachReportService.FreelanceAsync`](../CampionsSystem.API/Controllers/OperationsController.cs#L95). Static test mapping: `ActivitiesClassesFreelanceTests.cs`.

### Payroll

#### `GET /api/payroll`

- Purpose: Retrieve history data in Payroll.
- Eligible: Authenticated; `Permission:Payroll.View`.
- Branch/record scope: payroll branch; privileged payment writes.
- URL inputs: `branchId` (query; UUID; optional); `employeeId` (query; UUID; optional); `monthId` (query; UUID; optional); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.MonthlyFinance.PayrollDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`PayrollService.HistoryAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L52). Static test mapping: `MonthlyFinanceSalaryTests.cs`.

#### `POST /api/payroll/commission-recognitions`

- Purpose: Recognize the relevant Payroll record or workflow.
- Eligible: Authenticated; `Permission:Payroll.Manage`.
- Branch/record scope: payroll branch; privileged payment writes.
- URL inputs: none.
- Body: `RecognizePayrollCommissionsRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `commissionPeriodId` **required** (UUID).
- Returns: `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.MonthlyFinance.PayrollDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`PayrollService.RecognizeCommissionPeriodAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L54). Static test mapping: `MonthlyFinanceSalaryTests.cs`.

#### `GET /api/payroll/{id}/payments`

- Purpose: Retrieve payments data in Payroll.
- Eligible: Authenticated; `Permission:Payroll.View`.
- Branch/record scope: payroll branch; privileged payment writes.
- URL inputs: `id` (path; UUID; required); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.MonthlyFinance.PayrollPaymentDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`PayrollService.PaymentsAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L58). Static test mapping: `MonthlyFinanceSalaryTests.cs`.

#### `POST /api/payroll/{id}/payments`

- Purpose: Pay the relevant Payroll record or workflow.
- Eligible: Authenticated; `Permission:Payroll.RecordPayment`.
- Branch/record scope: payroll branch; privileged payment writes.
- URL inputs: `id` (path; UUID; required).
- Body: `RecordPayrollPaymentRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `amount` **required** (number/string); `paidAt` **required** (timestamp); `reason` **required** (string); `reference` optional (string).
- Returns: `CampionsSystem.BLL.MonthlyFinance.PayrollPaymentDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`PayrollService.RecordPaymentAsync`](../CampionsSystem.API/Controllers/MonthlyFinanceController.cs#L56). Static test mapping: `MonthlyFinanceSalaryTests.cs`.

### Sales

#### `GET /api/sales`

- Purpose: Retrieve history data in Sales.
- Eligible: Authenticated; `Permission:Payments.View`.
- Branch/record scope: Sale branch; actor Shift for reception money.
- URL inputs: `BranchId` (query; UUID; optional); `MemberId` (query; UUID; optional); `PurchaseMode` (query; PurchaseMode; optional); `From` (query; date YYYY-MM-DD; optional); `To` (query; date YYYY-MM-DD; optional); `Page` (query; integer/string; optional); `PageSize` (query; integer/string; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.Sales.SaleHistory`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SalesService.GetHistoryAsync`](../CampionsSystem.API/Controllers/SalesController.cs#L10). Static test mapping: `SalesApiTests.cs; SalesQuickCancelTests.cs`.

#### `POST /api/sales`

- Purpose: Create a customer Sale or purchase through the existing financial workflow.
- Eligible: Authenticated; `Permission:Payments.Create`.
- Branch/record scope: Sale branch; actor Shift for reception money.
- URL inputs: none.
- Body: `CreateSaleRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `memberId` **required** (UUID); `membershipPlanId` **required** (UUID); `purchaseMode` **required** (PurchaseMode); `activationMode` **required** (SubscriptionActivationMode); `paymentParts` **required** (array of PaymentPartInput); `offerId` optional (UUID); `startDate` optional (date YYYY-MM-DD); `reservationDueDate` optional (date YYYY-MM-DD); `installments` optional (array of InstallmentLineInput); `branchId` optional (UUID); `initialPaymentRequired` optional (number/string).
- Returns: `CampionsSystem.BLL.Sales.SaleReceipt`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SalesService.CreateAsync`](../CampionsSystem.API/Controllers/SalesController.cs#L8). Static test mapping: `SalesApiTests.cs; SalesQuickCancelTests.cs`.

#### `GET /api/sales/branch-totals`

- Purpose: Retrieve totals data in Sales.
- Eligible: Authenticated; `Permission:Payments.View`.
- Branch/record scope: Sale branch; actor Shift for reception money.
- URL inputs: `branchId` (query; UUID; optional); `from` (query; date YYYY-MM-DD; optional); `to` (query; date YYYY-MM-DD; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.Sales.BranchFinancialTotals`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SalesService.GetBranchTotalsAsync`](../CampionsSystem.API/Controllers/SalesController.cs#L26). Static test mapping: `SalesApiTests.cs; SalesQuickCancelTests.cs`.

#### `GET /api/sales/{id}`

- Purpose: Retrieve details data in Sales.
- Eligible: Authenticated; `Permission:Payments.View`.
- Branch/record scope: Sale branch; actor Shift for reception money.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.Sales.SaleDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SalesService.GetDetailsAsync`](../CampionsSystem.API/Controllers/SalesController.cs#L12). Static test mapping: `SalesApiTests.cs; SalesQuickCancelTests.cs`.

#### `GET /api/sales/{id}/installments`

- Purpose: Retrieve installments data in Sales.
- Eligible: Authenticated; `Permission:Payments.View`.
- Branch/record scope: Sale branch; actor Shift for reception money.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Sales.InstallmentDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SalesService.GetInstallmentsAsync`](../CampionsSystem.API/Controllers/SalesController.cs#L18). Static test mapping: `SalesApiTests.cs; SalesQuickCancelTests.cs`.

#### `GET /api/sales/{id}/payments`

- Purpose: Retrieve payments data in Sales.
- Eligible: Authenticated; `Permission:Payments.View`.
- Branch/record scope: Sale branch; actor Shift for reception money.
- URL inputs: `id` (path; UUID; required); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Sales.PaymentHistory`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SalesService.GetPaymentsAsync`](../CampionsSystem.API/Controllers/SalesController.cs#L16). Static test mapping: `SalesApiTests.cs; SalesQuickCancelTests.cs`.

#### `POST /api/sales/{id}/payments`

- Purpose: Pay the relevant Sales record or workflow.
- Eligible: Authenticated; `Permission:Payments.Create`.
- Branch/record scope: Sale branch; actor Shift for reception money.
- URL inputs: `id` (path; UUID; required).
- Body: `RecordPaymentRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `amount` **required** (number/string); `parts` **required** (array of PaymentPartInput); `allocations` optional (array of InstallmentAllocationInput).
- Returns: `CampionsSystem.BLL.Sales.PaymentDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SalesService.RecordPaymentAsync`](../CampionsSystem.API/Controllers/SalesController.cs#L14). Static test mapping: `SalesApiTests.cs; SalesQuickCancelTests.cs`.

#### `POST /api/sales/{id}/quick-cancel`

- Purpose: Cancel a recent purchase with the manually selected refund amount.
- Eligible: Authenticated; `Permission:Sales.QuickCancel`.
- Branch/record scope: Sale branch; actor Shift for reception money.
- URL inputs: `id` (path; UUID; required).
- Body: `QuickCancelRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `reason` **required** (string); `amount` **required** (number/string); `parts` optional (array of PaymentPartInput).
- Returns: `CampionsSystem.BLL.Sales.RefundDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SalesService.QuickCancelAsync`](../CampionsSystem.API/Controllers/SalesController.cs#L24). Static test mapping: `SalesApiTests.cs; SalesQuickCancelTests.cs`.

#### `GET /api/sales/{id}/refunds`

- Purpose: Retrieve refunds data in Sales.
- Eligible: Authenticated; `Permission:Payments.View`.
- Branch/record scope: Sale branch; actor Shift for reception money.
- URL inputs: `id` (path; UUID; required); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Sales.RefundHistory`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SalesService.GetRefundsAsync`](../CampionsSystem.API/Controllers/SalesController.cs#L22). Static test mapping: `SalesApiTests.cs; SalesQuickCancelTests.cs`.

#### `POST /api/sales/{id}/refunds`

- Purpose: Refund the relevant Sales record or workflow.
- Eligible: Authenticated; `Permission:Payments.Refund`.
- Branch/record scope: Sale branch; actor Shift for reception money.
- URL inputs: `id` (path; UUID; required).
- Body: `RefundRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `amount` **required** (number/string); `parts` **required** (array of PaymentPartInput); `reason` **required** (string).
- Returns: `CampionsSystem.BLL.Sales.RefundDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SalesService.RefundAsync`](../CampionsSystem.API/Controllers/SalesController.cs#L20). Static test mapping: `SalesApiTests.cs; SalesQuickCancelTests.cs`.

### Shifts

#### `GET /api/shifts`

- Purpose: Retrieve history data in Shifts.
- Eligible: Authenticated; `Permission:Shifts.View`.
- Branch/record scope: Shift branch; own or in-scope override.
- URL inputs: `BranchId` (query; UUID; optional); `EmployeeId` (query; UUID; optional); `From` (query; date YYYY-MM-DD; optional); `To` (query; date YYYY-MM-DD; optional); `Page` (query; integer/string; optional); `PageSize` (query; integer/string; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.ShiftDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ShiftService.HistoryAsync`](../CampionsSystem.API/Controllers/ShiftsController.cs#L16). Static test mapping: `ShiftApiTests.cs; ShiftReportingTests.cs`.

#### `POST /api/shifts`

- Purpose: Open the relevant Shifts record or workflow.
- Eligible: Authenticated; `Permission:Shifts.Manage`.
- Branch/record scope: Shift branch; own or in-scope override.
- URL inputs: none.
- Body: `OpenShiftRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `openingCash` **required** (number/string); `branchId` optional (UUID).
- Returns: `CampionsSystem.BLL.Shifts.ShiftDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ShiftService.OpenAsync`](../CampionsSystem.API/Controllers/ShiftsController.cs#L8). Static test mapping: `ShiftApiTests.cs; ShiftReportingTests.cs`.

#### `GET /api/shifts/current`

- Purpose: Retrieve current data in Shifts.
- Eligible: Authenticated; `Permission:Shifts.View`.
- Branch/record scope: Shift branch; own or in-scope override.
- URL inputs: none.
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.ShiftDetails?`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ShiftService.CurrentAsync`](../CampionsSystem.API/Controllers/ShiftsController.cs#L14). Static test mapping: `ShiftApiTests.cs; ShiftReportingTests.cs`.

#### `GET /api/shifts/{id}`

- Purpose: Retrieve get data in Shifts.
- Eligible: Authenticated; `Permission:Shifts.View`.
- Branch/record scope: Shift branch; own or in-scope override.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.ShiftDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ShiftService.GetAsync`](../CampionsSystem.API/Controllers/ShiftsController.cs#L18). Static test mapping: `ShiftApiTests.cs; ShiftReportingTests.cs`.

#### `GET /api/shifts/{id}/branch-transactions`

- Purpose: Review all operators’ branch financial activity during this Shift interval.
- Eligible: Authenticated; `Permission:Shifts.View` and `Permission:Vault.View`.
- Branch/record scope: Shift branch; own or in-scope override.
- URL inputs: `id` (path; UUID; required); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.ShiftBranchTransactionDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ShiftReportService.BranchTransactionsAsync`](../CampionsSystem.API/Controllers/ShiftsController.cs#L25). Static test mapping: `ShiftApiTests.cs; ShiftReportingTests.cs`.

#### `POST /api/shifts/{id}/close`

- Purpose: Close the relevant Shifts record or workflow.
- Eligible: Authenticated; `Permission:Shifts.Manage`.
- Branch/record scope: Shift branch; own or in-scope override.
- URL inputs: `id` (path; UUID; required).
- Body: `CloseShiftRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `expectedVersion` **required** (integer/string); `countedCash` **required** (number/string); `reason` optional (string).
- Returns: `CampionsSystem.BLL.Shifts.ShiftDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ShiftService.CloseAsync`](../CampionsSystem.API/Controllers/ShiftsController.cs#L10). Static test mapping: `ShiftApiTests.cs; ShiftReportingTests.cs`.

#### `POST /api/shifts/{id}/close-override`

- Purpose: Authorized Manager/Owner closure of another employee Shift.
- Eligible: Authenticated; `Permission:Shifts.CloseOthers`.
- Branch/record scope: Shift branch; own or in-scope override.
- URL inputs: `id` (path; UUID; required).
- Body: `OverrideCloseShiftRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `expectedVersion` **required** (integer/string); `reason` **required** (string); `countedCash` optional (number/string).
- Returns: `CampionsSystem.BLL.Shifts.ShiftDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ShiftService.CloseOverrideAsync`](../CampionsSystem.API/Controllers/ShiftsController.cs#L12). Static test mapping: `ShiftApiTests.cs; ShiftReportingTests.cs`.

#### `GET /api/shifts/{id}/summary`

- Purpose: Review employee Shift totals separately from shared branch account balances.
- Eligible: Authenticated; `Permission:Shifts.View`.
- Branch/record scope: Shift branch; own or in-scope override.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.ShiftFinancialSummary`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ShiftReportService.ShiftAsync`](../CampionsSystem.API/Controllers/ShiftsController.cs#L20). Static test mapping: `ShiftApiTests.cs; ShiftReportingTests.cs`.

#### `GET /api/shifts/{id}/transactions`

- Purpose: Review financial ledger activity attributed to this employee Shift.
- Eligible: Authenticated; `Permission:Shifts.View` and `Permission:Vault.View`.
- Branch/record scope: Shift branch; own or in-scope override.
- URL inputs: `id` (path; UUID; required); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.ShiftFinancialTransactionDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ShiftReportService.TransactionsAsync`](../CampionsSystem.API/Controllers/ShiftsController.cs#L22). Static test mapping: `ShiftApiTests.cs; ShiftReportingTests.cs`.

### Subscriptions

#### `POST /api/employees/{id}/membership-entitlement`

- Purpose: Employee the relevant Subscriptions record or workflow.
- Eligible: Authenticated; `Permission:Subscriptions.Manage` and `Permission:Employees.Manage`.
- Branch/record scope: subscription/member branch and BusinessDate.
- URL inputs: `id` (path; UUID; required).
- Body: `EmployeeEntitlementRequest` (`application/json`).
  - Top-level fields: `memberId` **required** (UUID); `planId` **required** (UUID); `operationId` **required** (UUID).
- Returns: `CampionsSystem.BLL.Subscriptions.SubscriptionGrantResult`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SubscriptionService.StartEmployeeEntitlementAsync`](../CampionsSystem.API/Controllers/SubscriptionsController.cs#L26). Static test mapping: `SubscriptionApiTests.cs`.

#### `GET /api/members/{memberId}/subscriptions`

- Purpose: Retrieve history data in Subscriptions.
- Eligible: Authenticated; `Permission:Subscriptions.View`.
- Branch/record scope: subscription/member branch and BusinessDate.
- URL inputs: `memberId` (path; UUID; required); `Page` (query; integer/string; optional); `PageSize` (query; integer/string; optional); `BranchId` (query; UUID; optional); `Status` (query; SubscriptionStatus; optional); `PlanType` (query; PlanCategory; optional); `From` (query; date YYYY-MM-DD; optional); `To` (query; date YYYY-MM-DD; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.Subscriptions.SubscriptionHistory`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SubscriptionService.GetHistoryAsync`](../CampionsSystem.API/Controllers/SubscriptionsController.cs#L9). Static test mapping: `SubscriptionApiTests.cs`.

#### `GET /api/members/{memberId}/subscriptions/current`

- Purpose: Retrieve current data in Subscriptions.
- Eligible: Authenticated; `Permission:Subscriptions.View`.
- Branch/record scope: subscription/member branch and BusinessDate.
- URL inputs: `memberId` (path; UUID; required); `branchId` (query; UUID; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.Subscriptions.MemberEntitlement`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SubscriptionService.GetCurrentEntitlementAsync`](../CampionsSystem.API/Controllers/SubscriptionsController.cs#L12). Static test mapping: `SubscriptionApiTests.cs`.

#### `POST /api/subscriptions/official-holidays`

- Purpose: Holiday the relevant Subscriptions record or workflow.
- Eligible: Authenticated; `Permission:Subscriptions.Extend` and `Permission:Branches.AccessAll`.
- Branch/record scope: subscription/member branch and BusinessDate.
- URL inputs: none.
- Body: `OfficialHolidayRequest` (`application/json`).
  - Top-level fields: `branchId` **required** (UUID); `startDate` **required** (date YYYY-MM-DD); `endDate` **required** (date YYYY-MM-DD); `extendSubscriptions` **required** (boolean); `reason` **required** (string).
- Returns: `CampionsSystem.BLL.Subscriptions.HolidayApplicationResult`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SubscriptionService.CreateOfficialHolidayAsync`](../CampionsSystem.API/Controllers/SubscriptionsController.cs#L29). Static test mapping: `SubscriptionApiTests.cs`.

#### `POST /api/subscriptions/official-holidays/{id}/apply`

- Purpose: Apply holiday the relevant Subscriptions record or workflow.
- Eligible: Authenticated; `Permission:Subscriptions.Extend` and `Permission:Branches.AccessAll`.
- Branch/record scope: subscription/member branch and BusinessDate.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.Subscriptions.HolidayApplicationResult`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SubscriptionService.ApplyOfficialHolidayAsync`](../CampionsSystem.API/Controllers/SubscriptionsController.cs#L32). Static test mapping: `SubscriptionApiTests.cs`.

#### `GET /api/subscriptions/{id}`

- Purpose: Retrieve details data in Subscriptions.
- Eligible: Authenticated; `Permission:Subscriptions.View`.
- Branch/record scope: subscription/member branch and BusinessDate.
- URL inputs: `id` (path; UUID; required).
- Body: none.
- Returns: `CampionsSystem.BLL.Subscriptions.SubscriptionDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SubscriptionService.GetDetailsAsync`](../CampionsSystem.API/Controllers/SubscriptionsController.cs#L15). Static test mapping: `SubscriptionApiTests.cs`.

#### `POST /api/subscriptions/{id}/custom-extension`

- Purpose: Extend the relevant Subscriptions record or workflow.
- Eligible: Authenticated; `Permission:Subscriptions.Extend`.
- Branch/record scope: subscription/member branch and BusinessDate.
- URL inputs: `id` (path; UUID; required).
- Body: `CustomExtensionRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `expectedVersion` **required** (integer/string); `days` **required** (integer/string); `reason` **required** (string).
- Returns: `CampionsSystem.BLL.Subscriptions.FreezeDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SubscriptionService.GrantCustomExtensionAsync`](../CampionsSystem.API/Controllers/SubscriptionsController.cs#L23). Static test mapping: `SubscriptionApiTests.cs`.

#### `POST /api/subscriptions/{id}/freeze`

- Purpose: Freeze the relevant Subscriptions record or workflow.
- Eligible: Authenticated; `Permission:Subscriptions.Freeze`.
- Branch/record scope: subscription/member branch and BusinessDate.
- URL inputs: `id` (path; UUID; required).
- Body: `FreezeRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `expectedVersion` **required** (integer/string); `startDate` **required** (date YYYY-MM-DD); `days` **required** (integer/string).
- Returns: `CampionsSystem.BLL.Subscriptions.FreezeDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SubscriptionService.StartFreezeAsync`](../CampionsSystem.API/Controllers/SubscriptionsController.cs#L17). Static test mapping: `SubscriptionApiTests.cs`.

#### `POST /api/subscriptions/{id}/freezes/{freezeId}/complete`

- Purpose: Complete the relevant Subscriptions record or workflow.
- Eligible: Authenticated; `Permission:Subscriptions.Freeze`.
- Branch/record scope: subscription/member branch and BusinessDate.
- URL inputs: `id` (path; UUID; required); `freezeId` (path; UUID; required).
- Body: `CompleteFreezeRequest` (`application/json`).
  - Top-level fields: `expectedVersion` **required** (integer/string).
- Returns: `CampionsSystem.BLL.Subscriptions.FreezeDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`SubscriptionService.CompleteFreezeAsync`](../CampionsSystem.API/Controllers/SubscriptionsController.cs#L20). Static test mapping: `SubscriptionApiTests.cs`.

### Vault

#### `GET /api/vault/audit`

- Purpose: Retrieve audit data in Vault.
- Eligible: Authenticated; `Permission:Vault.View`.
- Branch/record scope: shared drawer/account branch.
- URL inputs: `branchId` (query; UUID; optional); `entityId` (query; UUID; optional); `page` (query; integer/string; optional; default=1); `pageSize` (query; integer/string; optional; default=25).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.FinancialAuditDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ShiftReportService.AuditAsync`](../CampionsSystem.API/Controllers/VaultController.cs#L22). Static test mapping: `SharedVaultTests.cs`.

#### `GET /api/vault/counts`

- Purpose: Retrieve counts data in Vault.
- Eligible: Authenticated; `Permission:Vault.View`.
- Branch/record scope: shared drawer/account branch.
- URL inputs: `BranchId` (query; UUID; optional); `AccountId` (query; UUID; optional); `From` (query; date YYYY-MM-DD; optional); `To` (query; date YYYY-MM-DD; optional); `Page` (query; integer/string; optional); `PageSize` (query; integer/string; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.CashCountDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`VaultService.CountsAsync`](../CampionsSystem.API/Controllers/VaultController.cs#L14). Static test mapping: `SharedVaultTests.cs`.

#### `POST /api/vault/counts`

- Purpose: Count the relevant Vault record or workflow.
- Eligible: Authenticated; `Permission:Vault.Manage`.
- Branch/record scope: shared drawer/account branch.
- URL inputs: none.
- Body: `CountCashRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `countedCash` **required** (number/string); `reason` **required** (string); `branchId` optional (UUID).
- Returns: `CampionsSystem.BLL.Shifts.CashCountDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`VaultService.CountAsync`](../CampionsSystem.API/Controllers/VaultController.cs#L12). Static test mapping: `SharedVaultTests.cs`.

#### `GET /api/vault/movements`

- Purpose: Retrieve movements data in Vault.
- Eligible: Authenticated; `Permission:Vault.View`.
- Branch/record scope: shared drawer/account branch.
- URL inputs: `BranchId` (query; UUID; optional); `AccountId` (query; UUID; optional); `From` (query; date YYYY-MM-DD; optional); `To` (query; date YYYY-MM-DD; optional); `Page` (query; integer/string; optional); `PageSize` (query; integer/string; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.FinancialMovementDetails>`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`VaultService.MovementsAsync`](../CampionsSystem.API/Controllers/VaultController.cs#L16). Static test mapping: `SharedVaultTests.cs`.

#### `GET /api/vault/position`

- Purpose: Retrieve position data in Vault.
- Eligible: Authenticated; `Permission:Vault.View`.
- Branch/record scope: shared drawer/account branch.
- URL inputs: `branchId` (query; UUID; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.DrawerPosition`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`VaultService.PositionAsync`](../CampionsSystem.API/Controllers/VaultController.cs#L10). Static test mapping: `SharedVaultTests.cs`.

#### `POST /api/vault/setup`

- Purpose: Setup the relevant Vault record or workflow.
- Eligible: Authenticated; `Permission:Vault.Manage`.
- Branch/record scope: shared drawer/account branch.
- URL inputs: none.
- Body: `SetupDrawerRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `financialAccountId` **required** (UUID); `countedCash` **required** (number/string); `reason` **required** (string); `branchId` optional (UUID).
- Returns: `CampionsSystem.BLL.Shifts.DrawerPosition`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`VaultService.SetupAsync`](../CampionsSystem.API/Controllers/VaultController.cs#L8). Static test mapping: `SharedVaultTests.cs`.

#### `GET /api/vault/summary`

- Purpose: Retrieve summary data in Vault.
- Eligible: Authenticated; `Permission:Vault.View`.
- Branch/record scope: shared drawer/account branch.
- URL inputs: `branchId` (query; UUID; optional); `from` (query; date YYYY-MM-DD; optional); `to` (query; date YYYY-MM-DD; optional).
- Body: none.
- Returns: `CampionsSystem.BLL.Shifts.BranchFinancialSummary`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`ShiftReportService.BranchAsync`](../CampionsSystem.API/Controllers/VaultController.cs#L20). Static test mapping: `SharedVaultTests.cs`.

#### `POST /api/vault/withdrawals`

- Purpose: Withdraw the relevant Vault record or workflow.
- Eligible: Authenticated; `Permission:Vault.Withdraw`.
- Branch/record scope: shared drawer/account branch.
- URL inputs: none.
- Body: `WithdrawalRequest` (`application/json`).
  - Top-level fields: `operationId` **required** (UUID); `amount` **required** (number/string); `reason` **required** (string); `branchId` optional (UUID).
- Returns: `CampionsSystem.BLL.Shifts.CashMovementDetails`.
- Errors: documented statuses 200; standard service validation and authorization errors may also occur.
- Source: [`VaultService.WithdrawAsync`](../CampionsSystem.API/Controllers/VaultController.cs#L18). Static test mapping: `SharedVaultTests.cs`.

## Development-only diagnostic

`GET /health/database` is a Development diagnostic, not an application workflow. In particular, a POST method does not prove a database write: `POST /api/check-ins/evaluate` evaluates eligibility without recording a check-in, while Excel preview records an import session but creates no Lead or Member.

## Evidence limits

This guide covers each current route and declared top-level request input. It uses controller/source-mapped authorization and branch metadata, but service checks may impose additional requirements. The test file mapping is static evidence only; no tests were run to produce this guide. See [complete API catalog](backend-audit/COMPLETE_API_CATALOG.md), [OpenAPI](frontend-handoff/openapi.json), [DTO contracts](frontend-handoff/api/) and [Postman examples](frontend-handoff/postman/).
