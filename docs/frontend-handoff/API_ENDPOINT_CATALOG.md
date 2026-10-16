# V1 API operation catalog
Current `master` source `e3b8cc3`: **227 controller operations across 28 controllers**. The runtime OpenAPI has **228 operations** because it also exposes Development-only `GET /health/database`. Four legacy Operations writes return HTTP 410. The Development OpenAPI document-serving registration is not itself listed in the spec.
For a route, use its [controller contract](api/), exact [DTO inventory](DTO_AND_ENUM_REFERENCE.md), and [runtime OpenAPI](openapi.json). Controller policy attributes are listed below. Service-level Owner checks, branch/actor restrictions, employment and financial state still apply; UI permissions are advisory.
| Controller | Operations | Contract |
|---|---:|---|
| AccessAdministration | 9 | [AccessAdministration](api/AccessAdministration.md) |
| Auth | 2 | [Auth](api/Auth.md) |
| Branches | 4 | [Branches](api/Branches.md) |
| BusinessDays | 3 | [BusinessDays](api/BusinessDays.md) |
| CheckIns | 7 | [CheckIns](api/CheckIns.md) |
| Coaching | 36 | [Coaching](api/Coaching.md) |
| Commissions | 9 | [Commissions](api/Commissions.md) |
| Crm | 5 | [Crm](api/Crm.md) |
| EmployeeAdvances | 3 | [EmployeeAdvances](api/EmployeeAdvances.md) |
| EmployeeBranches | 4 | [EmployeeBranches](api/EmployeeBranches.md) |
| EmployeeSalaries | 2 | [EmployeeSalaries](api/EmployeeSalaries.md) |
| Employees | 6 | [Employees](api/Employees.md) |
| Expenses | 8 | [Expenses](api/Expenses.md) |
| FinancialAccounts | 2 | [FinancialAccounts](api/FinancialAccounts.md) |
| Leads | 14 | [Leads](api/Leads.md) |
| MaintenancePlans | 5 | [MaintenancePlans](api/MaintenancePlans.md) |
| Members | 9 | [Members](api/Members.md) |
| MembershipBenefits | 5 | [MembershipBenefits](api/MembershipBenefits.md) |
| MembershipPlanCategories | 4 | [MembershipPlanCategories](api/MembershipPlanCategories.md) |
| MembershipPlans | 11 | [MembershipPlans](api/MembershipPlans.md) |
| MonthlyFinance | 13 | [MonthlyFinance](api/MonthlyFinance.md) |
| Offers | 9 | [Offers](api/Offers.md) |
| Operations | 17 | [Operations](api/Operations.md) |
| Payroll | 4 | [Payroll](api/Payroll.md) |
| Sales | 10 | [Sales](api/Sales.md) |
| Shifts | 9 | [Shifts](api/Shifts.md) |
| Subscriptions | 9 | [Subscriptions](api/Subscriptions.md) |
| Vault | 8 | [Vault](api/Vault.md) |

| Method | Route | Controller action | Controller policies (AND) | Request body | Success shape |
|---|---|---|---|---|---|
| GET | `/api/access-administration/history` | [AccessAdministration.History](api/AccessAdministration.md#get-api-access-administration-history) | `OwnerAccessAdministration`, `Permission:Users.View`, `Permission:Roles.View` | `—` | `AccessPage<AccessAuditDetails>` |
| GET | `/api/access-administration/permissions` | [AccessAdministration.Catalog](api/AccessAdministration.md#get-api-access-administration-permissions) | `OwnerAccessAdministration`, `Permission:Roles.View` | `—` | `PermissionModule[]` |
| GET | `/api/access-administration/roles` | [AccessAdministration.Roles](api/AccessAdministration.md#get-api-access-administration-roles) | `OwnerAccessAdministration`, `Permission:Roles.View` | `—` | `AccessRole[]` |
| GET | `/api/access-administration/roles/{roleId:guid}` | [AccessAdministration.Role](api/AccessAdministration.md#get-api-access-administration-roles-roleid-guid) | `OwnerAccessAdministration`, `Permission:Roles.View` | `—` | `RoleAccessDetails` |
| PUT | `/api/access-administration/roles/{roleId:guid}/permissions/{permissionId:guid}` | [AccessAdministration.Grant](api/AccessAdministration.md#put-api-access-administration-roles-roleid-guid-permissions-permissionid-guid) | `OwnerAccessAdministration`, `Permission:Roles.Manage` | `Authentication.RoleGrantRequest` | `AccessChangeResult` |
| GET | `/api/access-administration/users` | [AccessAdministration.Users](api/AccessAdministration.md#get-api-access-administration-users) | `OwnerAccessAdministration`, `Permission:Users.View` | `—` | `AccessPage<AccessUserItem>` |
| GET | `/api/access-administration/users/{userId:guid}` | [AccessAdministration.UserAccess](api/AccessAdministration.md#get-api-access-administration-users-userid-guid) | `OwnerAccessAdministration`, `Permission:Users.View` | `—` | `UserAccessDetails` |
| PUT | `/api/access-administration/users/{userId:guid}/permissions/{permissionId:guid}` | [AccessAdministration.Override](api/AccessAdministration.md#put-api-access-administration-users-userid-guid-permissions-permissionid-guid) | `OwnerAccessAdministration`, `Permission:Users.Manage` | `Authentication.PermissionOverrideRequest` | `AccessChangeResult` |
| PUT | `/api/access-administration/users/{userId:guid}/roles/{roleId:guid}` | [AccessAdministration.Membership](api/AccessAdministration.md#put-api-access-administration-users-userid-guid-roles-roleid-guid) | `OwnerAccessAdministration`, `Permission:Users.Manage` | `Authentication.RoleMembershipRequest` | `AccessChangeResult` |
| DELETE | `/api/activities/coach-assignments/{id:guid}` | [Operations.Unassign](api/Operations.md#delete-api-activities-coach-assignments-id-guid) | `Permission:Activities.ManageCoaches` | `—` | `<anonymous type: bool Updated>` |
| GET | `/api/activities/coach-revenue/{periodId:guid}` | [Operations.ActivityRevenue](api/Operations.md#get-api-activities-coach-revenue-periodid-guid) | `Permission:Activities.ViewCoachRevenue` | `—` | `IReadOnlyList<Operations.ActivityCoachReport>` |
| POST | `/api/activities/coach-revenue/{periodId:guid}/finalize` | [Operations.FinalizeActivity](api/Operations.md#post-api-activities-coach-revenue-periodid-guid-finalize) | `Permission:Commission.ClosePeriod` | `—` | `<anonymous type: bool Finalized>` |
| POST | `/api/activities/coaches` | [Operations.CreateCoach](api/Operations.md#post-api-activities-coaches) | `Permission:Activities.ManageCoaches` | `Operations.CreateActivityCoachRequest` | `<anonymous type: Guid Id>` |
| PUT | `/api/activities/coaches/{id:guid}/percentage` | [Operations.Percentage](api/Operations.md#put-api-activities-coaches-id-guid-percentage) | `Permission:Activities.ManageCoaches` | `CampionsAPI.Controllers.OperationsController.PercentageRequest` | `<anonymous type: bool Updated>` |
| POST | `/api/activities/purchases` | [Operations.PurchaseActivity](api/Operations.md#post-api-activities-purchases) | `Permission:Activities.Manage` | `Operations.ActivityPurchaseRequest` | `Operations.PurchaseResult` |
| POST | `/api/activities/{id:guid}/coach-assignments` | [Operations.Assign](api/Operations.md#post-api-activities-id-guid-coach-assignments) | `Permission:Activities.ManageCoaches` | `CampionsAPI.Controllers.OperationsController.AssignmentRequest` | `<anonymous type: Guid Id>` |
| POST | `/api/auth/login` | [Auth.Login](api/Auth.md#post-api-auth-login) | Anonymous | `Authentication.LoginRequest` | `Authentication.LoginResponse` |
| GET | `/api/auth/me` | [Auth.Me](api/Auth.md#get-api-auth-me) | Authenticated | `—` | `Authentication.CurrentUserContext` |
| GET | `/api/branches` | [Branches.List](api/Branches.md#get-api-branches) | `Permission:Branches.View` | `—` | `IReadOnlyList<Branches.BranchDetails>` |
| POST | `/api/branches` | [Branches.Create](api/Branches.md#post-api-branches) | `Permission:Branches.Manage` | `Branches.SaveBranchRequest` | `Branches.BranchDetails` |
| GET | `/api/branches/{branchId:guid}/employees` | [Employees.BranchEmployees](api/Employees.md#get-api-branches-branchid-guid-employees) | `Permission:Employees.View` | `—` | `Employees.EmployeeListResult` |
| GET | `/api/branches/{id:guid}` | [Branches.Get](api/Branches.md#get-api-branches-id-guid) | `Permission:Branches.View` | `—` | `Branches.BranchDetails` |
| PUT | `/api/branches/{id:guid}` | [Branches.Update](api/Branches.md#put-api-branches-id-guid) | `Permission:Branches.Manage` | `Branches.SaveBranchRequest` | `Branches.BranchDetails` |
| POST | `/api/business-days` | [BusinessDays.Open](api/BusinessDays.md#post-api-business-days) | `Permission:Shifts.Manage` | `Shifts.OpenBusinessDayRequest` | `Shifts.BusinessDayDetails` |
| GET | `/api/business-days/current` | [BusinessDays.Current](api/BusinessDays.md#get-api-business-days-current) | `Permission:Shifts.View` | `—` | `Shifts.BusinessDayDetails?` |
| POST | `/api/business-days/{id:guid}/close` | [BusinessDays.Close](api/BusinessDays.md#post-api-business-days-id-guid-close) | `Permission:Shifts.Manage` | `—` | `Shifts.BusinessDayDetails` |
| GET | `/api/check-ins` | [CheckIns.History](api/CheckIns.md#get-api-check-ins) | `Permission:CheckIns.View` | `—` | `CheckIns.CheckInHistory` |
| POST | `/api/check-ins` | [CheckIns.Create](api/CheckIns.md#post-api-check-ins) | `Permission:CheckIns.Create` | `CheckIns.CreateCheckInRequest` | `CheckIns.CheckInResult` |
| POST | `/api/check-ins/evaluate` | [CheckIns.Evaluate](api/CheckIns.md#post-api-check-ins-evaluate) | `Permission:CheckIns.Create` | `CheckIns.AccessRequest` | `CheckIns.AccessDecision` |
| GET | `/api/check-ins/members` | [CheckIns.Lookup](api/CheckIns.md#get-api-check-ins-members) | `Permission:CheckIns.Create` | `—` | `CheckIns.CheckInMemberMatches` |
| POST | `/api/check-ins/trials` | [CheckIns.Trial](api/CheckIns.md#post-api-check-ins-trials) | `Permission:CheckIns.Create` | `CheckIns.RedeemTrialRequest` | `CheckIns.TrialCheckInResult` |
| GET | `/api/classes/attendance-report` | [Operations.ClassReport](api/Operations.md#get-api-classes-attendance-report) | `Permission:Classes.ViewAttendanceReport` | `—` | `IReadOnlyList<Operations.ClassCoachReport>` |
| POST | `/api/classes/sessions` | [Operations.CreateSession](api/Operations.md#post-api-classes-sessions) | `Permission:Classes.Manage` | `Operations.SaveClassSessionRequest` | `410 Gone` |
| PUT | `/api/classes/sessions/{id:guid}` | [Operations.UpdateSession](api/Operations.md#put-api-classes-sessions-id-guid) | `Permission:Classes.Manage` | `Operations.SaveClassSessionRequest` | `410 Gone` |
| POST | `/api/classes/sessions/{id:guid}/attendance/{memberId:guid}` | [Operations.Attendance](api/Operations.md#post-api-classes-sessions-id-guid-attendance-memberid-guid) | `Permission:Classes.Manage` | `—` | `410 Gone` |
| GET | `/api/coaching/classes/coaches/{coach:guid}/statistics` | [Coaching.ClassStatistics](api/Coaching.md#get-api-coaching-classes-coaches-coach-guid-statistics) | `Permission:Classes.ViewAttendanceReport` | `—` | `Coaching.ClassCoachStatistics` |
| GET | `/api/coaching/classes/members` | [Coaching.ClassMembers](api/Coaching.md#get-api-coaching-classes-members) | `Permission:Classes.View` | `—` | `Coaching.CoachingPage<Coaching.CoachingMemberResult>` |
| GET | `/api/coaching/classes/members/{member:guid}/attendance` | [Coaching.ClassMemberAttendance](api/Coaching.md#get-api-coaching-classes-members-member-guid-attendance) | `Permission:Classes.View` | `—` | `Coaching.CoachingPage<Coaching.ClassAttendanceDetails>` |
| GET | `/api/coaching/classes/occurrences` | [Coaching.Occurrences](api/Coaching.md#get-api-coaching-classes-occurrences) | `Permission:Classes.View` | `—` | `Coaching.CoachingPage<Coaching.ClassOccurrenceDetails>` |
| POST | `/api/coaching/classes/occurrences` | [Coaching.CreateOccurrence](api/Coaching.md#post-api-coaching-classes-occurrences) | `Permission:Classes.Manage` | `Coaching.ClassOccurrenceRequest` | `Coaching.ClassOccurrenceDetails` |
| GET | `/api/coaching/classes/occurrences/{id:guid}/attendance` | [Coaching.ClassAttendance](api/Coaching.md#get-api-coaching-classes-occurrences-id-guid-attendance) | `Permission:Classes.View` | `—` | `Coaching.CoachingPage<Coaching.ClassAttendanceDetails>` |
| POST | `/api/coaching/classes/occurrences/{id:guid}/attendance` | [Coaching.RegisterClass](api/Coaching.md#post-api-coaching-classes-occurrences-id-guid-attendance) | `Permission:Classes.RegisterAttendance` | `Coaching.ClassAttendanceRequest` | `Coaching.ClassAttendanceDetails` |
| POST | `/api/coaching/classes/occurrences/{id:guid}/complete` | [Coaching.CompleteOccurrence](api/Coaching.md#post-api-coaching-classes-occurrences-id-guid-complete) | `Permission:Classes.Manage` | `Coaching.CompleteClassRequest` | `Coaching.ClassOccurrenceDetails` |
| GET | `/api/coaching/classes/packages` | [Coaching.ClassesPackages](api/Coaching.md#get-api-coaching-classes-packages) | `Permission:Classes.View` | `—` | `Coaching.CoachingPage<Coaching.CoachingPackageDetails>` |
| POST | `/api/coaching/classes/packages` | [Coaching.CreateClassesPackage](api/Coaching.md#post-api-coaching-classes-packages) | `Permission:Classes.Manage` | `Coaching.CoachingPackageRequest` | `Coaching.CoachingPackageDetails` |
| PUT | `/api/coaching/classes/packages/{id:guid}` | [Coaching.UpdateClassesPackage](api/Coaching.md#put-api-coaching-classes-packages-id-guid) | `Permission:Classes.Manage` | `Coaching.CoachingPackageRequest` | `Coaching.CoachingPackageDetails` |
| POST | `/api/coaching/classes/purchases` | [Coaching.PurchaseClasses](api/Coaching.md#post-api-coaching-classes-purchases) | `Permission:Classes.Manage`, `Permission:Payments.Create` | `Coaching.CoachingPurchaseRequest` | `Coaching.CoachingPurchaseReceipt` |
| GET | `/api/coaching/classes/subscriptions` | [Coaching.ClassSubscriptions](api/Coaching.md#get-api-coaching-classes-subscriptions) | `Permission:Classes.View` | `—` | `Coaching.CoachingPage<Coaching.ClassesSubscriptionDetails>` |
| GET | `/api/coaching/classes/types` | [Coaching.ClassTypes](api/Coaching.md#get-api-coaching-classes-types) | `Permission:Classes.View` | `—` | `Coaching.CoachingPage<Coaching.ClassTypeDetails>` |
| POST | `/api/coaching/classes/types` | [Coaching.CreateClassType](api/Coaching.md#post-api-coaching-classes-types) | `Permission:Classes.Manage` | `Coaching.ClassTypeRequest` | `Coaching.ClassTypeDetails` |
| PUT | `/api/coaching/classes/types/{id:guid}` | [Coaching.UpdateClassType](api/Coaching.md#put-api-coaching-classes-types-id-guid) | `Permission:Classes.Manage` | `Coaching.ClassTypeRequest` | `Coaching.ClassTypeDetails` |
| GET | `/api/coaching/earnings/coaches/{coach:guid}/balance` | [Coaching.Balance](api/Coaching.md#get-api-coaching-earnings-coaches-coach-guid-balance) | `Permission:CoachEarnings.View` | `—` | `Coaching.CoachBalance` |
| GET | `/api/coaching/earnings/coaches/{coach:guid}/history` | [Coaching.EarningsHistory](api/Coaching.md#get-api-coaching-earnings-coaches-coach-guid-history) | `Permission:CoachEarnings.View` | `—` | `Coaching.CoachingPage<Coaching.CoachEarningDetails>` |
| GET | `/api/coaching/earnings/coaches/{coach:guid}/payouts` | [Coaching.PayoutHistory](api/Coaching.md#get-api-coaching-earnings-coaches-coach-guid-payouts) | `Permission:CoachEarnings.View` | `—` | `Coaching.CoachingPage<Coaching.CoachPayoutDetails>` |
| POST | `/api/coaching/earnings/payouts` | [Coaching.Payout](api/Coaching.md#post-api-coaching-earnings-payouts) | `Permission:CoachEarnings.Pay` | `Coaching.CoachPayoutRequest` | `Coaching.CoachPayoutDetails` |
| GET | `/api/coaching/earnings/periods/{period:guid}/progress` | [Coaching.Progress](api/Coaching.md#get-api-coaching-earnings-periods-period-guid-progress) | `Permission:CoachEarnings.View` | `—` | `Coaching.CoachEarningsProgress` |
| GET | `/api/coaching/earnings/periods/{period:guid}/targets` | [Coaching.TargetHistory](api/Coaching.md#get-api-coaching-earnings-periods-period-guid-targets) | `Permission:CoachEarnings.View` | `—` | `IReadOnlyList<Coaching.CoachTargetDetails>` |
| POST | `/api/coaching/earnings/recognitions` | [Coaching.Recognize](api/Coaching.md#post-api-coaching-earnings-recognitions) | `Permission:CoachEarnings.Recognize` | `Coaching.RecognizeCoachEarningsRequest` | `Coaching.CoachEarningsProgress` |
| POST | `/api/coaching/earnings/targets` | [Coaching.Targets](api/Coaching.md#post-api-coaching-earnings-targets) | `Permission:CoachEarnings.ManageTargets` | `Coaching.CoachTargetRequest` | `Coaching.CoachTargetDetails` |
| GET | `/api/coaching/private/members` | [Coaching.PrivateMembers](api/Coaching.md#get-api-coaching-private-members) | `Permission:Private.View` | `—` | `Coaching.CoachingPage<Coaching.CoachingMemberResult>` |
| GET | `/api/coaching/private/packages` | [Coaching.PrivatePackages](api/Coaching.md#get-api-coaching-private-packages) | `Permission:Private.View` | `—` | `Coaching.CoachingPage<Coaching.CoachingPackageDetails>` |
| POST | `/api/coaching/private/packages` | [Coaching.CreatePrivatePackage](api/Coaching.md#post-api-coaching-private-packages) | `Permission:Private.Manage` | `Coaching.CoachingPackageRequest` | `Coaching.CoachingPackageDetails` |
| PUT | `/api/coaching/private/packages/{id:guid}` | [Coaching.UpdatePrivatePackage](api/Coaching.md#put-api-coaching-private-packages-id-guid) | `Permission:Private.Manage` | `Coaching.CoachingPackageRequest` | `Coaching.CoachingPackageDetails` |
| POST | `/api/coaching/private/purchases` | [Coaching.PurchasePrivate](api/Coaching.md#post-api-coaching-private-purchases) | `Permission:Private.Manage`, `Permission:Payments.Create` | `Coaching.CoachingPurchaseRequest` | `Coaching.CoachingPurchaseReceipt` |
| GET | `/api/coaching/private/subscriptions` | [Coaching.PrivateSubscriptions](api/Coaching.md#get-api-coaching-private-subscriptions) | `Permission:Private.View` | `—` | `Coaching.CoachingPage<Coaching.PrivateDetails>` |
| GET | `/api/coaching/private/subscriptions/{id:guid}` | [Coaching.PrivateSubscription](api/Coaching.md#get-api-coaching-private-subscriptions-id-guid) | `Permission:Private.View` | `—` | `Coaching.PrivateDetails` |
| GET | `/api/coaching/private/subscriptions/{id:guid}/expiry-settlements` | [Coaching.ExpiryHistory](api/Coaching.md#get-api-coaching-private-subscriptions-id-guid-expiry-settlements) | `Permission:Private.View` | `—` | `Coaching.CoachingPage<Coaching.PrivateExpiryDetails>` |
| POST | `/api/coaching/private/subscriptions/{id:guid}/expiry-settlements` | [Coaching.SettleExpiry](api/Coaching.md#post-api-coaching-private-subscriptions-id-guid-expiry-settlements) | `Permission:Private.SettleExpiry` | `Coaching.PrivateExpiryRequest` | `Coaching.PrivateExpiryDetails` |
| GET | `/api/coaching/private/subscriptions/{id:guid}/sessions` | [Coaching.PrivateSessions](api/Coaching.md#get-api-coaching-private-subscriptions-id-guid-sessions) | `Permission:Private.View` | `—` | `Coaching.CoachingPage<Coaching.PrivateAttendanceDetails>` |
| POST | `/api/coaching/private/subscriptions/{id:guid}/sessions` | [Coaching.RegisterPrivate](api/Coaching.md#post-api-coaching-private-subscriptions-id-guid-sessions) | `Permission:Private.RegisterSession` | `Coaching.PrivateAttendanceRequest` | `Coaching.PrivateAttendanceDetails` |
| POST | `/api/coaching/private/subscriptions/{id:guid}/transfers` | [Coaching.Transfer](api/Coaching.md#post-api-coaching-private-subscriptions-id-guid-transfers) | `Permission:Private.Transfer` | `Coaching.PrivateTransferRequest` | `Coaching.PrivateAssignmentDetails` |
| GET | `/api/commissions/periods` | [Commissions.History](api/Commissions.md#get-api-commissions-periods) | `Permission:Commission.View` | `—` | `Commissions.CommissionPage<Commissions.CommissionPeriodDetails>` |
| POST | `/api/commissions/periods` | [Commissions.Open](api/Commissions.md#post-api-commissions-periods) | `Permission:Commission.ClosePeriod` | `Commissions.OpenCommissionPeriodRequest` | `Commissions.CommissionPeriodDetails` |
| GET | `/api/commissions/periods/{id:guid}` | [Commissions.Progress](api/Commissions.md#get-api-commissions-periods-id-guid) | `Permission:Commission.View` | `—` | `Commissions.CommissionProgress` |
| GET | `/api/commissions/periods/{id:guid}/employees/{employeeId:guid}` | [Commissions.Employee](api/Commissions.md#get-api-commissions-periods-id-guid-employees-employeeid-guid) | `Permission:Commission.View` | `—` | `Commissions.EmployeeCommissionStatement` |
| POST | `/api/commissions/periods/{id:guid}/finalize` | [Commissions.Finalize](api/Commissions.md#post-api-commissions-periods-id-guid-finalize) | `Permission:Commission.ClosePeriod` | `Commissions.FinalizeCommissionPeriodRequest` | `Commissions.CommissionPeriodDetails` |
| GET | `/api/commissions/periods/{id:guid}/refund-allocations` | [Commissions.RefundAllocations](api/Commissions.md#get-api-commissions-periods-id-guid-refund-allocations) | `Permission:Commission.View` | `—` | `Commissions.CommissionPage<Commissions.CommissionRefundSource>` |
| GET | `/api/commissions/periods/{id:guid}/sources` | [Commissions.Sources](api/Commissions.md#get-api-commissions-periods-id-guid-sources) | `Permission:Commission.View` | `—` | `Commissions.CommissionPage<Commissions.CommissionSourceLine>` |
| GET | `/api/commissions/rules` | [Commissions.Rules](api/Commissions.md#get-api-commissions-rules) | `Permission:Commission.View` | `—` | `Commissions.CommissionPage<Commissions.CommissionRuleDetails>` |
| POST | `/api/commissions/rules` | [Commissions.Configure](api/Commissions.md#post-api-commissions-rules) | `Permission:Commission.ManageRules` | `Commissions.ConfigureCommissionRequest` | `IReadOnlyList<Commissions.CommissionRuleDetails>` |
| GET | `/api/crm/house-follow-ups` | [Crm.House](api/Crm.md#get-api-crm-house-follow-ups) | `Permission:CRM.ManageHouseFollowUps` | `—` | `Crm.FollowUpListResult` |
| POST | `/api/crm/house-follow-ups/{id:guid}/assign` | [Crm.Assign](api/Crm.md#post-api-crm-house-follow-ups-id-guid-assign) | `Permission:CRM.ManageHouseFollowUps` | `Members.SalesAssignmentRequest` | `Members.MemberDetails` |
| POST | `/api/crm/member-follow-ups/{id:guid}/complete` | [Crm.Complete](api/Crm.md#post-api-crm-member-follow-ups-id-guid-complete) | `Permission:CRM.ManageMemberFollowUps` | `—` | `Guid` |
| POST | `/api/crm/members/{id:guid}/follow-ups` | [Crm.Open](api/Crm.md#post-api-crm-members-id-guid-follow-ups) | `Permission:CRM.ManageMemberFollowUps` | `Crm.MemberFollowUpRequest` | `Guid` |
| GET | `/api/crm/my-follow-ups` | [Crm.Mine](api/Crm.md#get-api-crm-my-follow-ups) | `Permission:CRM.ViewMemberFollowUps` | `—` | `Crm.FollowUpListResult` |
| GET | `/api/employee-advances` | [EmployeeAdvances.History](api/EmployeeAdvances.md#get-api-employee-advances) | `Permission:Advances.View` | `—` | `Shifts.ShiftPage<Shifts.AdvanceDetails>` |
| POST | `/api/employee-advances` | [EmployeeAdvances.Paid](api/EmployeeAdvances.md#post-api-employee-advances) | `Permission:Advances.Create` | `Shifts.AdvanceRequest` | `Shifts.AdvanceDetails` |
| GET | `/api/employee-advances/{id:guid}` | [EmployeeAdvances.Get](api/EmployeeAdvances.md#get-api-employee-advances-id-guid) | `Permission:Advances.View` | `—` | `Shifts.AdvanceDetails` |
| GET | `/api/employee-salaries/{employeeId:guid}/settings` | [EmployeeSalaries.History](api/EmployeeSalaries.md#get-api-employee-salaries-employeeid-guid-settings) | `Permission:Salaries.View` | `—` | `IReadOnlyList<MonthlyFinance.SalarySettingDetails>` |
| POST | `/api/employee-salaries/{employeeId:guid}/settings` | [EmployeeSalaries.Configure](api/EmployeeSalaries.md#post-api-employee-salaries-employeeid-guid-settings) | `Permission:Salaries.Manage` | `MonthlyFinance.SetSalaryRequest` | `MonthlyFinance.SalarySettingDetails` |
| GET | `/api/employees` | [Employees.List](api/Employees.md#get-api-employees) | `Permission:Employees.View` | `—` | `Employees.EmployeeListResult` |
| POST | `/api/employees` | [Employees.Create](api/Employees.md#post-api-employees) | `Permission:Employees.Manage` | `Employees.CreateEmployeeRequest` | `Employees.EmployeeDetails` |
| POST | `/api/employees/{employeeId:guid}/assign-branch` | [EmployeeBranches.Assign](api/EmployeeBranches.md#post-api-employees-employeeid-guid-assign-branch) | `Permission:Employees.Manage` | `Branches.AssignBranchRequest` | `Branches.BranchAssignmentDetails` |
| GET | `/api/employees/{employeeId:guid}/branch-assignment` | [EmployeeBranches.Current](api/EmployeeBranches.md#get-api-employees-employeeid-guid-branch-assignment) | `Permission:Employees.View` | `—` | `Branches.BranchAssignmentDetails?` |
| GET | `/api/employees/{employeeId:guid}/branch-history` | [EmployeeBranches.History](api/EmployeeBranches.md#get-api-employees-employeeid-guid-branch-history) | `Permission:Employees.View` | `—` | `IReadOnlyList<Branches.BranchAssignmentDetails>` |
| POST | `/api/employees/{employeeId:guid}/transfer-branch` | [EmployeeBranches.Transfer](api/EmployeeBranches.md#post-api-employees-employeeid-guid-transfer-branch) | `Permission:Employees.Transfer` | `Branches.TransferBranchRequest` | `Branches.BranchAssignmentDetails` |
| GET | `/api/employees/{id:guid}` | [Employees.Get](api/Employees.md#get-api-employees-id-guid) | `Permission:Employees.View` | `—` | `Employees.EmployeeDetails` |
| PUT | `/api/employees/{id:guid}` | [Employees.Update](api/Employees.md#put-api-employees-id-guid) | `Permission:Employees.Manage` | `Employees.EmployeeProfileRequest` | `Employees.EmployeeDetails` |
| POST | `/api/employees/{id:guid}/membership-entitlement` | [Subscriptions.Employee](api/Subscriptions.md#post-api-employees-id-guid-membership-entitlement) | `Permission:Subscriptions.Manage`, `Permission:Employees.Manage` | `Subscriptions.EmployeeEntitlementRequest` | `Subscriptions.SubscriptionGrantResult` |
| PATCH | `/api/employees/{id:guid}/status` | [Employees.Status](api/Employees.md#patch-api-employees-id-guid-status) | `Permission:Employees.Manage` | `Employees.ChangeEmployeeStatusRequest` | `Employees.EmployeeDetails` |
| GET | `/api/expenses` | [Expenses.History](api/Expenses.md#get-api-expenses) | `Permission:Expenses.View` | `—` | `Shifts.ShiftPage<Shifts.ExpenseDetails>` |
| POST | `/api/expenses` | [Expenses.Paid](api/Expenses.md#post-api-expenses) | `Permission:Expenses.Create` | `Shifts.PaidExpenseRequest` | `Shifts.ExpenseDetails` |
| GET | `/api/expenses/categories` | [Expenses.Categories](api/Expenses.md#get-api-expenses-categories) | `Permission:Expenses.View` | `—` | `Shifts.ShiftPage<Shifts.ExpenseCategoryDetails>` |
| POST | `/api/expenses/categories` | [Expenses.Category](api/Expenses.md#post-api-expenses-categories) | `Permission:Expenses.Manage` | `Shifts.ExpenseCategoryRequest` | `Shifts.ExpenseCategoryDetails` |
| PUT | `/api/expenses/categories/{id:guid}` | [Expenses.Category](api/Expenses.md#put-api-expenses-categories-id-guid) | `Permission:Expenses.Manage` | `Shifts.ExpenseCategoryRequest` | `Shifts.ExpenseCategoryDetails` |
| GET | `/api/expenses/{id:guid}` | [Expenses.Get](api/Expenses.md#get-api-expenses-id-guid) | `Permission:Expenses.View` | `—` | `Shifts.ExpenseDetails` |
| GET | `/api/expenses/{id:guid}/returns` | [Expenses.Returns](api/Expenses.md#get-api-expenses-id-guid-returns) | `Permission:Expenses.View` | `—` | `Shifts.ShiftPage<Shifts.ExpenseReversalDetails>` |
| POST | `/api/expenses/{id:guid}/returns` | [Expenses.Return](api/Expenses.md#post-api-expenses-id-guid-returns) | `Permission:Expenses.Reverse` | `Shifts.ReverseExpenseRequest` | `Shifts.ExpenseReversalDetails` |
| GET | `/api/financial-accounts` | [FinancialAccounts.Accounts](api/FinancialAccounts.md#get-api-financial-accounts) | `Permission:Payments.View` | `—` | `IReadOnlyList<Sales.FinancialAccountDetails>` |
| POST | `/api/financial-accounts` | [FinancialAccounts.Create](api/FinancialAccounts.md#post-api-financial-accounts) | `Permission:Branches.Manage`, `Permission:Branches.AccessAll` | `Sales.CreateFinancialAccountRequest` | `Sales.FinancialAccountDetails` |
| GET | `/api/financial-months` | [MonthlyFinance.History](api/MonthlyFinance.md#get-api-financial-months) | `Permission:FinancialMonths.View` | `—` | `Shifts.ShiftPage<MonthlyFinance.FinancialMonthDetails>` |
| POST | `/api/financial-months` | [MonthlyFinance.Open](api/MonthlyFinance.md#post-api-financial-months) | `Permission:FinancialMonths.Manage` | `MonthlyFinance.OpenFinancialMonthRequest` | `MonthlyFinance.FinancialMonthDetails` |
| GET | `/api/financial-months/consolidated` | [MonthlyFinance.Consolidated](api/MonthlyFinance.md#get-api-financial-months-consolidated) | `Permission:FinancialMonths.ViewConsolidated` | `—` | `MonthlyFinance.ConsolidatedMonthSummary` |
| POST | `/api/financial-months/transactions/{ledgerId:guid}/original-occurrence` | [MonthlyFinance.Occurrence](api/MonthlyFinance.md#post-api-financial-months-transactions-ledgerid-guid-original-occurrence) | `Permission:FinancialMonths.Manage` | `MonthlyFinance.RecordOccurrenceRequest` | `Guid` |
| GET | `/api/financial-months/{id:guid}` | [MonthlyFinance.Get](api/MonthlyFinance.md#get-api-financial-months-id-guid) | `Permission:FinancialMonths.View` | `—` | `MonthlyFinance.FinancialMonthDetails` |
| POST | `/api/financial-months/{id:guid}/close` | [MonthlyFinance.Close](api/MonthlyFinance.md#post-api-financial-months-id-guid-close) | `Permission:FinancialMonths.Close` | `MonthlyFinance.CloseFinancialMonthRequest` | `MonthlyFinance.FinancialMonthDetails` |
| GET | `/api/financial-months/{id:guid}/employees/{employeeId:guid}/salary` | [MonthlyFinance.Salary](api/MonthlyFinance.md#get-api-financial-months-id-guid-employees-employeeid-guid-salary) | `Permission:Salaries.View` | `—` | `MonthlyFinance.SalaryEntitlement` |
| GET | `/api/financial-months/{id:guid}/events` | [MonthlyFinance.Events](api/MonthlyFinance.md#get-api-financial-months-id-guid-events) | `Permission:FinancialMonths.View` | `—` | `Shifts.ShiftPage<MonthlyFinance.MonthEventDetails>` |
| GET | `/api/financial-months/{id:guid}/export` | [MonthlyFinance.Export](api/MonthlyFinance.md#get-api-financial-months-id-guid-export) | `Permission:FinancialMonths.Export` | `—` | `XLSX file` |
| POST | `/api/financial-months/{id:guid}/reopen` | [MonthlyFinance.Reopen](api/MonthlyFinance.md#post-api-financial-months-id-guid-reopen) | `Permission:FinancialMonths.Reopen` | `MonthlyFinance.ReopenFinancialMonthRequest` | `MonthlyFinance.FinancialMonthDetails` |
| GET | `/api/financial-months/{id:guid}/report` | [MonthlyFinance.Report](api/MonthlyFinance.md#get-api-financial-months-id-guid-report) | `Permission:FinancialMonths.View` | `—` | `MonthlyFinance.MonthlyReport` |
| GET | `/api/financial-months/{id:guid}/revisions` | [MonthlyFinance.Revisions](api/MonthlyFinance.md#get-api-financial-months-id-guid-revisions) | `Permission:FinancialMonths.View` | `—` | `IReadOnlyList<MonthlyFinance.RevisionDetails>` |
| GET | `/api/financial-months/{id:guid}/transactions` | [MonthlyFinance.Transactions](api/MonthlyFinance.md#get-api-financial-months-id-guid-transactions) | `Permission:FinancialMonths.View` | `—` | `Shifts.ShiftPage<MonthlyFinance.FinancialTransactionReportRow>` |
| POST | `/api/freelance-plans` | [Operations.CreatePlan](api/Operations.md#post-api-freelance-plans) | `Permission:FreelancePlans.Manage` | `Operations.SaveFreelancePlanRequest` | `<anonymous type: Guid Id>` |
| POST | `/api/freelance-plans/purchases` | [Operations.PurchaseFreelance](api/Operations.md#post-api-freelance-plans-purchases) | `Permission:FreelancePlans.Manage` | `Operations.FreelancePurchaseRequest` | `410 Gone` |
| PUT | `/api/freelance-plans/{id:guid}` | [Operations.UpdatePlan](api/Operations.md#put-api-freelance-plans-id-guid) | `Permission:FreelancePlans.Manage` | `Operations.SaveFreelancePlanRequest` | `<anonymous type: Guid Id>` |
| DELETE | `/api/freelance-plans/{id:guid}/eligible-members/{memberId:guid}` | [Operations.RemoveEligible](api/Operations.md#delete-api-freelance-plans-id-guid-eligible-members-memberid-guid) | `Permission:FreelancePlans.ManageEligibleMembers` | `—` | `<anonymous type: bool Eligible>` |
| PUT | `/api/freelance-plans/{id:guid}/eligible-members/{memberId:guid}` | [Operations.AddEligible](api/Operations.md#put-api-freelance-plans-id-guid-eligible-members-memberid-guid) | `Permission:FreelancePlans.ManageEligibleMembers` | `—` | `<anonymous type: bool Eligible>` |
| GET | `/api/freelance-plans/{id:guid}/report` | [Operations.FreelanceReport](api/Operations.md#get-api-freelance-plans-id-guid-report) | `Permission:FreelancePlans.View` | `—` | `IReadOnlyList<Operations.FreelancePlanReport>` |
| GET | `/api/leads` | [Leads.List](api/Leads.md#get-api-leads) | `Permission:Leads.View` | `—` | `Crm.LeadListResult` |
| POST | `/api/leads` | [Leads.Create](api/Leads.md#post-api-leads) | `Permission:Leads.Manage` | `Crm.CreateLeadRequest` | `Crm.LeadCreationResult` |
| POST | `/api/leads/bulk` | [Leads.Bulk](api/Leads.md#post-api-leads-bulk) | `Permission:Leads.Assign` | `Text.Json.JsonElement[]` | `IReadOnlyList<Crm.LeadCreationResult>` |
| GET | `/api/leads/lookup` | [Leads.Lookup](api/Leads.md#get-api-leads-lookup) | `Permission:Leads.View` | `—` | `Crm.LeadCreationResult` |
| GET | `/api/leads/my` | [Leads.Mine](api/Leads.md#get-api-leads-my) | `Permission:Leads.View` | `—` | `Crm.LeadListResult` |
| GET | `/api/leads/{id:guid}` | [Leads.Get](api/Leads.md#get-api-leads-id-guid) | `Permission:Leads.View` | `—` | `Crm.LeadDetails` |
| PUT | `/api/leads/{id:guid}` | [Leads.Update](api/Leads.md#put-api-leads-id-guid) | `Permission:Leads.Manage` | `Crm.UpdateLeadRequest` | `Crm.LeadDetails` |
| POST | `/api/leads/{id:guid}/assign` | [Leads.Assign](api/Leads.md#post-api-leads-id-guid-assign) | `Permission:Leads.Assign` | `Crm.LeadAssignmentRequest` | `Crm.LeadDetails` |
| GET | `/api/leads/{id:guid}/assignment` | [Leads.Assignment](api/Leads.md#get-api-leads-id-guid-assignment) | `Permission:Leads.View` | `—` | `Crm.LeadAssignmentDetails?` |
| GET | `/api/leads/{id:guid}/assignment-history` | [Leads.AssignmentHistory](api/Leads.md#get-api-leads-id-guid-assignment-history) | `Permission:Leads.View` | `—` | `IReadOnlyList<Crm.LeadAssignmentDetails>` |
| POST | `/api/leads/{id:guid}/convert` | [Leads.Convert](api/Leads.md#post-api-leads-id-guid-convert) | `Permission:Leads.Convert` | `Crm.ConvertLeadRequest` | `Members.MemberDetails` |
| GET | `/api/leads/{id:guid}/interactions` | [Leads.Contacts](api/Leads.md#get-api-leads-id-guid-interactions) | `Permission:Leads.View` | `—` | `IReadOnlyList<Crm.LeadInteractionDetails>` |
| POST | `/api/leads/{id:guid}/interactions` | [Leads.Contact](api/Leads.md#post-api-leads-id-guid-interactions) | `Permission:Leads.Manage` | `Crm.LeadInteractionRequest` | `Crm.LeadDetails` |
| PATCH | `/api/leads/{id:guid}/status` | [Leads.Status](api/Leads.md#patch-api-leads-id-guid-status) | `Permission:Leads.Manage` | `Crm.LeadStatusRequest` | `Crm.LeadDetails` |
| GET | `/api/maintenance-plans` | [MaintenancePlans.Plans](api/MaintenancePlans.md#get-api-maintenance-plans) | `Permission:Maintenance.View` | `—` | `Shifts.ShiftPage<MonthlyFinance.MaintenanceForecast>` |
| POST | `/api/maintenance-plans` | [MaintenancePlans.Configure](api/MaintenancePlans.md#post-api-maintenance-plans) | `Permission:Maintenance.Manage` | `MonthlyFinance.MaintenancePlanRequest` | `MonthlyFinance.MaintenanceForecast` |
| GET | `/api/maintenance-plans/reminders` | [MaintenancePlans.Reminders](api/MaintenancePlans.md#get-api-maintenance-plans-reminders) | `Permission:Maintenance.View` | `—` | `Shifts.ShiftPage<MonthlyFinance.MaintenanceForecast>` |
| GET | `/api/maintenance-plans/summary` | [MaintenancePlans.Summary](api/MaintenancePlans.md#get-api-maintenance-plans-summary) | `Permission:Maintenance.View` | `—` | `MonthlyFinance.MaintenanceBranchSummary` |
| GET | `/api/maintenance-plans/{id:guid}/history` | [MaintenancePlans.History](api/MaintenancePlans.md#get-api-maintenance-plans-id-guid-history) | `Permission:Maintenance.View` | `—` | `Shifts.ShiftPage<MonthlyFinance.MaintenanceForecast>` |
| GET | `/api/members` | [Members.List](api/Members.md#get-api-members) | `Permission:Members.View` | `—` | `Members.MemberListResult` |
| POST | `/api/members` | [Members.Create](api/Members.md#post-api-members) | `Permission:Members.Create` | `Members.CreateMemberRequest` | `Members.MemberDetails` |
| GET | `/api/members/{id:guid}` | [Members.Get](api/Members.md#get-api-members-id-guid) | `Permission:Members.View` | `—` | `Members.MemberDetails` |
| PUT | `/api/members/{id:guid}` | [Members.Update](api/Members.md#put-api-members-id-guid) | `Permission:Members.Edit` | `Members.UpdateMemberRequest` | `Members.MemberDetails` |
| POST | `/api/members/{id:guid}/assign-sales` | [Members.AssignSales](api/Members.md#post-api-members-id-guid-assign-sales) | `Permission:Members.AssignSales` | `Members.SalesAssignmentRequest` | `Members.MemberDetails` |
| POST | `/api/members/{id:guid}/return-to-house` | [Members.ReturnToHouse](api/Members.md#post-api-members-id-guid-return-to-house) | `Permission:Members.AssignSales` | `Members.ReturnToHouseRequest` | `Members.MemberDetails` |
| GET | `/api/members/{id:guid}/sales-assignment` | [Members.CurrentSales](api/Members.md#get-api-members-id-guid-sales-assignment) | `Permission:Members.View` | `—` | `Members.SalesAssignmentDetails?` |
| GET | `/api/members/{id:guid}/sales-history` | [Members.SalesHistory](api/Members.md#get-api-members-id-guid-sales-history) | `Permission:Members.ViewSalesHistory` | `—` | `IReadOnlyList<Members.SalesAssignmentHistoryItem>` |
| PATCH | `/api/members/{id:guid}/status` | [Members.Status](api/Members.md#patch-api-members-id-guid-status) | `Permission:Members.ChangeStatus` | `Members.ChangeMemberStatusRequest` | `Members.MemberDetails` |
| PUT | `/api/members/{memberId:guid}/audience-categories` | [Offers.Audiences](api/Offers.md#put-api-members-memberid-guid-audience-categories) | `Permission:Members.Manage` | `Sales.SetMemberAudienceRequest` | `IReadOnlyList<Sales.MemberAudienceDetails>` |
| GET | `/api/members/{memberId:guid}/borrowed-gym-sessions` | [CheckIns.Debts](api/CheckIns.md#get-api-members-memberid-guid-borrowed-gym-sessions) | `Permission:CheckIns.View` | `—` | `CheckIns.BorrowedDebtHistory` |
| GET | `/api/members/{memberId:guid}/check-ins/current-business-day` | [CheckIns.Today](api/CheckIns.md#get-api-members-memberid-guid-check-ins-current-business-day) | `Permission:CheckIns.View` | `—` | `CheckIns.CheckInHistory` |
| GET | `/api/members/{memberId:guid}/subscriptions` | [Subscriptions.History](api/Subscriptions.md#get-api-members-memberid-guid-subscriptions) | `Permission:Subscriptions.View` | `—` | `Subscriptions.SubscriptionHistory` |
| GET | `/api/members/{memberId:guid}/subscriptions/current` | [Subscriptions.Current](api/Subscriptions.md#get-api-members-memberid-guid-subscriptions-current) | `Permission:Subscriptions.View` | `—` | `Subscriptions.MemberEntitlement` |
| GET | `/api/membership-benefits` | [MembershipBenefits.List](api/MembershipBenefits.md#get-api-membership-benefits) | `Permission:MembershipPlans.View` | `—` | `Plans.CatalogListResult` |
| POST | `/api/membership-benefits` | [MembershipBenefits.Create](api/MembershipBenefits.md#post-api-membership-benefits) | `Permission:MembershipPlans.Manage` | `Plans.SaveBenefitRequest` | `Plans.CatalogDetails` |
| GET | `/api/membership-benefits/{id:guid}` | [MembershipBenefits.Get](api/MembershipBenefits.md#get-api-membership-benefits-id-guid) | `Permission:MembershipPlans.View` | `—` | `Plans.CatalogDetails` |
| PUT | `/api/membership-benefits/{id:guid}` | [MembershipBenefits.Update](api/MembershipBenefits.md#put-api-membership-benefits-id-guid) | `Permission:MembershipPlans.Manage` | `Plans.SaveBenefitRequest` | `Plans.CatalogDetails` |
| PATCH | `/api/membership-benefits/{id:guid}/status` | [MembershipBenefits.Status](api/MembershipBenefits.md#patch-api-membership-benefits-id-guid-status) | `Permission:MembershipPlans.Manage` | `Plans.ChangePlanStatusRequest` | `Plans.CatalogDetails` |
| GET | `/api/membership-plan-categories` | [MembershipPlanCategories.List](api/MembershipPlanCategories.md#get-api-membership-plan-categories) | `Permission:MembershipPlans.View` | `—` | `Plans.CatalogListResult` |
| POST | `/api/membership-plan-categories` | [MembershipPlanCategories.Create](api/MembershipPlanCategories.md#post-api-membership-plan-categories) | `Permission:MembershipPlans.Manage` | `Plans.SaveCategoryRequest` | `Plans.CatalogDetails` |
| PUT | `/api/membership-plan-categories/{id:guid}` | [MembershipPlanCategories.Update](api/MembershipPlanCategories.md#put-api-membership-plan-categories-id-guid) | `Permission:MembershipPlans.Manage` | `Plans.SaveCategoryRequest` | `Plans.CatalogDetails` |
| PATCH | `/api/membership-plan-categories/{id:guid}/status` | [MembershipPlanCategories.Status](api/MembershipPlanCategories.md#patch-api-membership-plan-categories-id-guid-status) | `Permission:MembershipPlans.Manage` | `Plans.ChangePlanStatusRequest` | `Plans.CatalogDetails` |
| GET | `/api/membership-plans` | [MembershipPlans.List](api/MembershipPlans.md#get-api-membership-plans) | `Permission:MembershipPlans.View` | `—` | `Plans.MembershipPlanListResult` |
| POST | `/api/membership-plans` | [MembershipPlans.Create](api/MembershipPlans.md#post-api-membership-plans) | `Permission:MembershipPlans.Manage` | `Plans.CreateMembershipPlanRequest` | `Plans.MembershipPlanDetails` |
| GET | `/api/membership-plans/{id:guid}` | [MembershipPlans.Get](api/MembershipPlans.md#get-api-membership-plans-id-guid) | `Permission:MembershipPlans.View` | `—` | `Plans.MembershipPlanDetails` |
| PUT | `/api/membership-plans/{id:guid}` | [MembershipPlans.Update](api/MembershipPlans.md#put-api-membership-plans-id-guid) | `Permission:MembershipPlans.Manage` | `Plans.UpdateMembershipPlanRequest` | `Plans.MembershipPlanDetails` |
| GET | `/api/membership-plans/{id:guid}/benefits` | [MembershipPlans.Benefits](api/MembershipPlans.md#get-api-membership-plans-id-guid-benefits) | `Permission:MembershipPlans.View` | `—` | `IReadOnlyList<Plans.PlanBenefitDetails>` |
| PUT | `/api/membership-plans/{id:guid}/benefits` | [MembershipPlans.SetBenefits](api/MembershipPlans.md#put-api-membership-plans-id-guid-benefits) | `Permission:MembershipPlans.Manage` | `Plans.SetPlanBenefitsRequest` | `IReadOnlyList<Plans.PlanBenefitDetails>` |
| GET | `/api/membership-plans/{id:guid}/eligible-members` | [MembershipPlans.Eligible](api/MembershipPlans.md#get-api-membership-plans-id-guid-eligible-members) | `Permission:FreelancePlans.ManageEligibleMembers` | `—` | `Plans.EligibleMemberListResult` |
| POST | `/api/membership-plans/{id:guid}/eligible-members` | [MembershipPlans.AddEligible](api/MembershipPlans.md#post-api-membership-plans-id-guid-eligible-members) | `Permission:FreelancePlans.ManageEligibleMembers` | `Plans.EligibleMemberRequest` | `bool` |
| GET | `/api/membership-plans/{id:guid}/eligible-members/candidates` | [MembershipPlans.Candidates](api/MembershipPlans.md#get-api-membership-plans-id-guid-eligible-members-candidates) | `Permission:FreelancePlans.ManageEligibleMembers` | `—` | `IReadOnlyList<Plans.EligibleMemberDetails>` |
| DELETE | `/api/membership-plans/{id:guid}/eligible-members/{memberId:guid}` | [MembershipPlans.RemoveEligible](api/MembershipPlans.md#delete-api-membership-plans-id-guid-eligible-members-memberid-guid) | `Permission:FreelancePlans.ManageEligibleMembers` | `—` | `bool` |
| PATCH | `/api/membership-plans/{id:guid}/status` | [MembershipPlans.Status](api/MembershipPlans.md#patch-api-membership-plans-id-guid-status) | `Permission:MembershipPlans.Manage` | `Plans.ChangePlanStatusRequest` | `Plans.MembershipPlanDetails` |
| GET | `/api/offers` | [Offers.Eligible](api/Offers.md#get-api-offers) | `Permission:Payments.Create` | `—` | `IReadOnlyList<Sales.OfferDetails>` |
| POST | `/api/offers` | [Offers.Create](api/Offers.md#post-api-offers) | `Permission:Offers.Manage` | `Sales.OfferRequest` | `Sales.OfferDetails` |
| GET | `/api/offers/management` | [Offers.Management](api/Offers.md#get-api-offers-management) | `Permission:Offers.Manage` | `—` | `Sales.OfferHistory` |
| GET | `/api/offers/quote` | [Offers.Quote](api/Offers.md#get-api-offers-quote) | `Permission:Payments.Create` | `—` | `Sales.PricingResult` |
| GET | `/api/offers/{id:guid}` | [Offers.Details](api/Offers.md#get-api-offers-id-guid) | `Permission:Offers.Manage` | `—` | `Sales.OfferDetails` |
| PUT | `/api/offers/{id:guid}` | [Offers.Update](api/Offers.md#put-api-offers-id-guid) | `Permission:Offers.Manage` | `Sales.OfferRequest` | `Sales.OfferDetails` |
| POST | `/api/offers/{id:guid}/member-entitlements` | [Offers.Grant](api/Offers.md#post-api-offers-id-guid-member-entitlements) | `Permission:Offers.Manage` | `Sales.GrantMemberOfferRequest` | `Sales.MemberOfferGrantDetails` |
| DELETE | `/api/offers/{id:guid}/member-entitlements/{grantId:guid}` | [Offers.Revoke](api/Offers.md#delete-api-offers-id-guid-member-entitlements-grantid-guid) | `Permission:Offers.Manage` | `—` | `Sales.MemberOfferGrantDetails` |
| GET | `/api/payroll` | [Payroll.History](api/Payroll.md#get-api-payroll) | `Permission:Payroll.View` | `—` | `Shifts.ShiftPage<MonthlyFinance.PayrollDetails>` |
| POST | `/api/payroll/commission-recognitions` | [Payroll.Recognize](api/Payroll.md#post-api-payroll-commission-recognitions) | `Permission:Payroll.Manage` | `MonthlyFinance.RecognizePayrollCommissionsRequest` | `IReadOnlyList<MonthlyFinance.PayrollDetails>` |
| GET | `/api/payroll/{id:guid}/payments` | [Payroll.Payments](api/Payroll.md#get-api-payroll-id-guid-payments) | `Permission:Payroll.View` | `—` | `Shifts.ShiftPage<MonthlyFinance.PayrollPaymentDetails>` |
| POST | `/api/payroll/{id:guid}/payments` | [Payroll.Pay](api/Payroll.md#post-api-payroll-id-guid-payments) | `Permission:Payroll.RecordPayment` | `MonthlyFinance.RecordPayrollPaymentRequest` | `MonthlyFinance.PayrollPaymentDetails` |
| GET | `/api/sales` | [Sales.History](api/Sales.md#get-api-sales) | `Permission:Payments.View` | `—` | `Sales.SaleHistory` |
| POST | `/api/sales` | [Sales.Create](api/Sales.md#post-api-sales) | `Permission:Payments.Create` | `Sales.CreateSaleRequest` | `Sales.SaleReceipt` |
| GET | `/api/sales/branch-totals` | [Sales.Totals](api/Sales.md#get-api-sales-branch-totals) | `Permission:Payments.View` | `—` | `Sales.BranchFinancialTotals` |
| GET | `/api/sales/{id:guid}` | [Sales.Details](api/Sales.md#get-api-sales-id-guid) | `Permission:Payments.View` | `—` | `Sales.SaleDetails` |
| GET | `/api/sales/{id:guid}/installments` | [Sales.Installments](api/Sales.md#get-api-sales-id-guid-installments) | `Permission:Payments.View` | `—` | `IReadOnlyList<Sales.InstallmentDetails>` |
| GET | `/api/sales/{id:guid}/payments` | [Sales.Payments](api/Sales.md#get-api-sales-id-guid-payments) | `Permission:Payments.View` | `—` | `Sales.PaymentHistory` |
| POST | `/api/sales/{id:guid}/payments` | [Sales.Pay](api/Sales.md#post-api-sales-id-guid-payments) | `Permission:Payments.Create` | `Sales.RecordPaymentRequest` | `Sales.PaymentDetails` |
| POST | `/api/sales/{id:guid}/quick-cancel` | [Sales.QuickCancel](api/Sales.md#post-api-sales-id-guid-quick-cancel) | `Permission:Sales.QuickCancel` | `Sales.QuickCancelRequest` | `Sales.RefundDetails` |
| GET | `/api/sales/{id:guid}/refunds` | [Sales.Refunds](api/Sales.md#get-api-sales-id-guid-refunds) | `Permission:Payments.View` | `—` | `Sales.RefundHistory` |
| POST | `/api/sales/{id:guid}/refunds` | [Sales.Refund](api/Sales.md#post-api-sales-id-guid-refunds) | `Permission:Payments.Refund` | `Sales.RefundRequest` | `Sales.RefundDetails` |
| GET | `/api/shifts` | [Shifts.History](api/Shifts.md#get-api-shifts) | `Permission:Shifts.View` | `—` | `Shifts.ShiftPage<Shifts.ShiftDetails>` |
| POST | `/api/shifts` | [Shifts.Open](api/Shifts.md#post-api-shifts) | `Permission:Shifts.Manage` | `Shifts.OpenShiftRequest` | `Shifts.ShiftDetails` |
| GET | `/api/shifts/current` | [Shifts.Current](api/Shifts.md#get-api-shifts-current) | `Permission:Shifts.View` | `—` | `Shifts.ShiftDetails?` |
| GET | `/api/shifts/{id:guid}` | [Shifts.Get](api/Shifts.md#get-api-shifts-id-guid) | `Permission:Shifts.View` | `—` | `Shifts.ShiftDetails` |
| GET | `/api/shifts/{id:guid}/branch-transactions` | [Shifts.BranchTransactions](api/Shifts.md#get-api-shifts-id-guid-branch-transactions) | `Permission:Shifts.View`, `Permission:Vault.View` | `—` | `Shifts.ShiftPage<Shifts.ShiftBranchTransactionDetails>` |
| POST | `/api/shifts/{id:guid}/close` | [Shifts.Close](api/Shifts.md#post-api-shifts-id-guid-close) | `Permission:Shifts.Manage` | `Shifts.CloseShiftRequest` | `Shifts.ShiftDetails` |
| POST | `/api/shifts/{id:guid}/close-override` | [Shifts.CloseOverride](api/Shifts.md#post-api-shifts-id-guid-close-override) | `Permission:Shifts.CloseOthers` | `Shifts.OverrideCloseShiftRequest` | `Shifts.ShiftDetails` |
| GET | `/api/shifts/{id:guid}/summary` | [Shifts.Summary](api/Shifts.md#get-api-shifts-id-guid-summary) | `Permission:Shifts.View` | `—` | `Shifts.ShiftFinancialSummary` |
| GET | `/api/shifts/{id:guid}/transactions` | [Shifts.Transactions](api/Shifts.md#get-api-shifts-id-guid-transactions) | `Permission:Shifts.View`, `Permission:Vault.View` | `—` | `Shifts.ShiftPage<Shifts.ShiftFinancialTransactionDetails>` |
| POST | `/api/subscriptions/official-holidays` | [Subscriptions.Holiday](api/Subscriptions.md#post-api-subscriptions-official-holidays) | `Permission:Subscriptions.Extend`, `Permission:Branches.AccessAll` | `Subscriptions.OfficialHolidayRequest` | `Subscriptions.HolidayApplicationResult` |
| POST | `/api/subscriptions/official-holidays/{id:guid}/apply` | [Subscriptions.ApplyHoliday](api/Subscriptions.md#post-api-subscriptions-official-holidays-id-guid-apply) | `Permission:Subscriptions.Extend`, `Permission:Branches.AccessAll` | `—` | `Subscriptions.HolidayApplicationResult` |
| GET | `/api/subscriptions/{id:guid}` | [Subscriptions.Details](api/Subscriptions.md#get-api-subscriptions-id-guid) | `Permission:Subscriptions.View` | `—` | `Subscriptions.SubscriptionDetails` |
| POST | `/api/subscriptions/{id:guid}/custom-extension` | [Subscriptions.Extend](api/Subscriptions.md#post-api-subscriptions-id-guid-custom-extension) | `Permission:Subscriptions.Extend` | `Subscriptions.CustomExtensionRequest` | `Subscriptions.FreezeDetails` |
| POST | `/api/subscriptions/{id:guid}/freeze` | [Subscriptions.Freeze](api/Subscriptions.md#post-api-subscriptions-id-guid-freeze) | `Permission:Subscriptions.Freeze` | `Subscriptions.FreezeRequest` | `Subscriptions.FreezeDetails` |
| POST | `/api/subscriptions/{id:guid}/freezes/{freezeId:guid}/complete` | [Subscriptions.Complete](api/Subscriptions.md#post-api-subscriptions-id-guid-freezes-freezeid-guid-complete) | `Permission:Subscriptions.Freeze` | `Subscriptions.CompleteFreezeRequest` | `Subscriptions.FreezeDetails` |
| GET | `/api/vault/audit` | [Vault.Audit](api/Vault.md#get-api-vault-audit) | `Permission:Vault.View` | `—` | `Shifts.ShiftPage<Shifts.FinancialAuditDetails>` |
| GET | `/api/vault/counts` | [Vault.Counts](api/Vault.md#get-api-vault-counts) | `Permission:Vault.View` | `—` | `Shifts.ShiftPage<Shifts.CashCountDetails>` |
| POST | `/api/vault/counts` | [Vault.Count](api/Vault.md#post-api-vault-counts) | `Permission:Vault.Manage` | `Shifts.CountCashRequest` | `Shifts.CashCountDetails` |
| GET | `/api/vault/movements` | [Vault.Movements](api/Vault.md#get-api-vault-movements) | `Permission:Vault.View` | `—` | `Shifts.ShiftPage<Shifts.FinancialMovementDetails>` |
| GET | `/api/vault/position` | [Vault.Position](api/Vault.md#get-api-vault-position) | `Permission:Vault.View` | `—` | `Shifts.DrawerPosition` |
| POST | `/api/vault/setup` | [Vault.Setup](api/Vault.md#post-api-vault-setup) | `Permission:Vault.Manage` | `Shifts.SetupDrawerRequest` | `Shifts.DrawerPosition` |
| GET | `/api/vault/summary` | [Vault.Summary](api/Vault.md#get-api-vault-summary) | `Permission:Vault.View` | `—` | `Shifts.BranchFinancialSummary` |
| POST | `/api/vault/withdrawals` | [Vault.Withdraw](api/Vault.md#post-api-vault-withdrawals) | `Permission:Vault.Withdraw` | `Shifts.WithdrawalRequest` | `Shifts.CashMovementDetails` |
