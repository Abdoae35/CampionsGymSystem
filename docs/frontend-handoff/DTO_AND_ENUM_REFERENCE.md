# DTO and enum reference

Latest branch update: frozen 2026-10-12T10:44:04.604445+00:00 at `bf733e2502a85603b80f8ad8a01747264a0f518b` plus uncommitted operational files adds **35 Coaching routes** (190 current actions total). The later isolated snapshot builds; initial CS8999 was superseded by concurrent domain publication. Stable contracts below remain at 0bc4476; new draft contracts/confirmed policies and three legacy Class 410 changes are in [branch-only status](BRANCH_ONLY_PRIVATE_CLASSES_STATUS.md). **Use that update before interpreting any earlier pending/no-route statement below.**


**VERIFIED FROM CODE**, committed baseline. 311 reachable compiler type shapes, including framework primitives/constructed/nullable containers; this is not 311 distinct public DTO classes. Every endpoint links its exact request/output type here. JSON camelCase, null values emitted by default, computed getter properties included. No universal envelope. **Read all inherited fields** in each table.

C# primitive mapping: Guid → UUID string; DateOnly → YYYY-MM-DD string; DateTimeOffset → offset timestamp string; decimal → JSON number on output (exact backend decimal); int/long → JSON integer; bool → boolean; IReadOnlyList/List/array → JSON array. Nullable T?/reference annotations permit null; nonnullable does not alone guarantee omission validation. Web number handling accepts numeric strings on input; exported OpenAPI records number|string shapes, but outputs are numbers. `long`/ClientCode/large decimal can exceed JS precision. Status fields typed `string` are strings even when DAL persists an enum.

Requiredness has three distinct layers: OpenAPI `required`, MVC [Required]/nonnullable reference validation, and business checks. Primary-constructor nonoptional parameters are documented in OpenAPI but C# alone does not prove binding rejection for omitted value types. Do not infer a Guid.Empty is allowed. Use schema required and guard evidence together. Class initializers and record optional defaults appear below; exact declarations are in [evidence](api/DTO_DECLARATION_EVIDENCE.md).

Enum JSON: default numeric unless JsonStringEnumConverter on type/property. Property converter overrides the default for that field; query model binding accepts names/numeric enum values independently from JSON conversion. JsonStringEnumConverter generally also accepts defined integer values; BLL Enum.IsDefined/subset checks still apply. Read each field's attributes and enum table. No global string converter exists. Nullable enums preserve null. Fields declared as strings with ToString() projections use enum names, not numeric values.

Zod **RECOMMENDATION grounded in backend**: .uuid() plus reject all-zero operation identities; .max(n), .min/.max Range, email and regex from attributes; strict object for JsonUnmappedMemberHandling.Disallow; preserve nullability/defaults; validate finite positive/nonnegative decimal with <=2 fractional digits according to guard evidence, without relying on floating-point authority. Add conditional refinements for modes/sessions, member creation, tender/allocation sums, distinct benefits, date bounds and confirmations as documented. Do not invent stricter rules such as globally unique plan name or mandatory Lead name. Backend remains authoritative. [Guard evidence](api/RULES_INDEX.md) gives actual limits/subsets/ordering for validators not represented in OpenAPI.

TypeScript generation strategy (**RECOMMENDATION**): pin exported openapi.json/hash and generator in frontend repo, override captured server URL, generate only confirmed input/basic response contracts, then fill missing IActionResult responses from this compiler-derived sidecar/catalog. Preserve decimals/date strings, enum mixed encoding, nullable 204, total/totalCount/employeeCount. Compare runtime contracts after each backend publication; never assume the generated OpenAPI covers authorization or all success bodies.

<a id="anonymous-type-system-guid-id"></a>

## <anonymous type: Guid Id>

C# type: `<anonymous type: System.Guid Id>`; source: `CampionsSystem.BLL/Commissions/CommissionContracts.cs:3`.
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | computed/read-only output |
<a id="anonymous-type-bool-eligible"></a>
## <anonymous type: bool Eligible>
C# type: `<anonymous type: bool Eligible>`; source: `CampionsSystem.BLL/Commissions/CommissionContracts.cs:3`.
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `eligible` | `bool` | No | No attribute; see BLL guard evidence | computed/read-only output |
<a id="anonymous-type-bool-finalized"></a>
## <anonymous type: bool Finalized>
C# type: `<anonymous type: bool Finalized>`; source: `CampionsSystem.BLL/Commissions/CommissionContracts.cs:3`.
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `finalized` | `bool` | No | No attribute; see BLL guard evidence | computed/read-only output |
<a id="anonymous-type-bool-updated"></a>
## <anonymous type: bool Updated>
C# type: `<anonymous type: bool Updated>`; source: `CampionsSystem.BLL/Commissions/CommissionContracts.cs:3`.
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `updated` | `bool` | No | No attribute; see BLL guard evidence | computed/read-only output |
<a id="campionssystem-api-controllers-operationscontroller-assignmentrequest"></a>
## OperationsController.AssignmentRequest
C# type: `CampionsSystem.API.Controllers.OperationsController.AssignmentRequest`; source: `CampionsSystem.API/Controllers/OperationsController.cs:14`.
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `activityCoachId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-api-controllers-operationscontroller-percentagerequest"></a>
## OperationsController.PercentageRequest
C# type: `CampionsSystem.API.Controllers.OperationsController.PercentageRequest`; source: `CampionsSystem.API/Controllers/OperationsController.cs:13`.
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `percentage` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-authentication-branchcontext"></a>
## Authentication.BranchContext
C# type: `CampionsSystem.BLL.Authentication.BranchContext`; source: `CampionsSystem.BLL/Authentication/AuthContracts.cs:13`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `name` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-authentication-branchcontext"></a>
## Authentication.BranchContext?
C# type: `CampionsSystem.BLL.Authentication.BranchContext?`; source: `CampionsSystem.BLL/Authentication/AuthContracts.cs:13`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `name` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-authentication-currentusercontext"></a>
## Authentication.CurrentUserContext
C# type: `CampionsSystem.BLL.Authentication.CurrentUserContext`; source: `CampionsSystem.BLL/Authentication/AuthContracts.cs:14`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `userId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `userName` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employeeId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employeeName` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `roles` | `IReadOnlyList<string>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `permissions` | `IReadOnlyList<string>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `currentBranch` | [Authentication.BranchContext?](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-authentication-branchcontext) | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `currentBranchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | computed/read-only output |
<a id="campionssystem-bll-authentication-loginrequest"></a>
## Authentication.LoginRequest
C# type: `CampionsSystem.BLL.Authentication.LoginRequest`; source: `CampionsSystem.BLL/Authentication/AuthContracts.cs:5`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `userNameOrEmail` | `string` | No | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)`<br>`System.ComponentModel.DataAnnotations.RequiredAttribute` | string.Empty |
| `password` | `string` | No | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)`<br>`System.ComponentModel.DataAnnotations.RequiredAttribute` | string.Empty |
<a id="campionssystem-bll-authentication-loginresponse"></a>
## Authentication.LoginResponse
C# type: `CampionsSystem.BLL.Authentication.LoginResponse`; source: `CampionsSystem.BLL/Authentication/AuthContracts.cs:22`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `accessToken` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `expiresAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `user` | [Authentication.LoginUser](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-authentication-loginuser) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-authentication-loginuser"></a>
## Authentication.LoginUser
C# type: `CampionsSystem.BLL.Authentication.LoginUser`; source: `CampionsSystem.BLL/Authentication/AuthContracts.cs:20`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `userName` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employeeId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `fullName` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `roles` | `IReadOnlyList<string>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-branches-assignbranchrequest"></a>
## Branches.AssignBranchRequest
C# type: `CampionsSystem.BLL.Branches.AssignBranchRequest`; source: `CampionsSystem.BLL/Branches/EmployeeBranchAssignmentService.cs:12`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `startDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-branches-branchassignmentdetails"></a>
## Branches.BranchAssignmentDetails
C# type: `CampionsSystem.BLL.Branches.BranchAssignmentDetails`; source: `CampionsSystem.BLL/Branches/EmployeeBranchAssignmentService.cs:10`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employeeId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchName` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `startDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `endDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `isPrimary` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `assignedByUserId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `createdAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-branches-branchassignmentdetails"></a>
## Branches.BranchAssignmentDetails?
C# type: `CampionsSystem.BLL.Branches.BranchAssignmentDetails?`; source: `CampionsSystem.BLL/Branches/EmployeeBranchAssignmentService.cs:10`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employeeId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchName` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `startDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `endDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `isPrimary` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `assignedByUserId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `createdAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-branches-branchdetails"></a>
## Branches.BranchDetails
C# type: `CampionsSystem.BLL.Branches.BranchDetails`; source: `CampionsSystem.BLL/Branches/BranchContracts.cs:15`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `name` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `cityId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `cityName` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `address` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `phone` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `isActive` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `maxBorrowedGymSessions` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `quickRefundWindowHours` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `countryCallingCode` | `string?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-branches-savebranchrequest"></a>
## Branches.SaveBranchRequest
C# type: `CampionsSystem.BLL.Branches.SaveBranchRequest`; source: `CampionsSystem.BLL/Branches/BranchContracts.cs:5`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `name` | `string` | No | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)`<br>`System.ComponentModel.DataAnnotations.RequiredAttribute` | No declared optional default; CLR initialization and BLL checks apply |
| `cityId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `address` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` | null |
| `phone` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)` | null |
| `isActive` | `bool` | No | No attribute; see BLL guard evidence | true |
| `maxBorrowedGymSessions` | `int` | No | `System.ComponentModel.DataAnnotations.RangeAttribute(0, 2147483647)` | 2 |
| `quickRefundWindowHours` | `int` | No | `System.ComponentModel.DataAnnotations.RangeAttribute(0, 2147483647)` | 24 |
| `countryCallingCode` | `string?` | Yes | `System.ComponentModel.DataAnnotations.RegularExpressionAttribute("^[+][1-9][0-9]{0,2}$")` | null |
<a id="campionssystem-bll-branches-transferbranchrequest"></a>
## Branches.TransferBranchRequest
C# type: `CampionsSystem.BLL.Branches.TransferBranchRequest`; source: `CampionsSystem.BLL/Branches/EmployeeBranchAssignmentService.cs:13`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `targetBranchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `effectiveDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-checkins-accessdecision"></a>
## CheckIns.AccessDecision
C# type: `CampionsSystem.BLL.CheckIns.AccessDecision`; source: `CampionsSystem.BLL/CheckIns/CheckInContracts.cs:40`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `allowed` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reasonCode` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberCode` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberName` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `businessDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `businessDayId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `subscriptionId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `subscriptionMode` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `remainingSessions` | `int?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `borrowAvailable` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `borrowedUsed` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `borrowedRemaining` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `canActivateOnCheckIn` | `bool` | No | No attribute; see BLL guard evidence | false |
<a id="campionssystem-bll-checkins-accessrequest"></a>
## CheckIns.AccessRequest
C# type: `CampionsSystem.BLL.CheckIns.AccessRequest`; source: `CampionsSystem.BLL/CheckIns/CheckInContracts.cs:36`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `memberId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberCode` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-checkins-borroweddebtdetails"></a>
## CheckIns.BorrowedDebtDetails
C# type: `CampionsSystem.BLL.CheckIns.BorrowedDebtDetails`; source: `CampionsSystem.BLL/CheckIns/CheckInContracts.cs:61`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `expiredSubscriptionId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `checkInId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `borrowedAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `status` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `settledBySubscriptionId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `settledByTransactionId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `settledAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `settlementHistory` | `IReadOnlyList<CheckIns.BorrowedSettlementDetails>?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-checkins-borroweddebthistory"></a>
## CheckIns.BorrowedDebtHistory
C# type: `CampionsSystem.BLL.CheckIns.BorrowedDebtHistory`; source: `CampionsSystem.BLL/CheckIns/CheckInContracts.cs:81`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<CheckIns.BorrowedDebtDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `totalCount` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-checkins-borroweddebthistoryrequest"></a>
## CheckIns.BorrowedDebtHistoryRequest
C# type: `CampionsSystem.BLL.CheckIns.BorrowedDebtHistoryRequest`; source: `CampionsSystem.BLL/CheckIns/CheckInContracts.cs:74`.
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `expiredSubscriptionId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | 1 |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | 25 |
<a id="campionssystem-bll-checkins-borrowedsettlementdetails"></a>
## CheckIns.BorrowedSettlementDetails
C# type: `CampionsSystem.BLL.CheckIns.BorrowedSettlementDetails`; source: `CampionsSystem.BLL/CheckIns/CheckInContracts.cs:64`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `sequence` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `action` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `grantId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `refundId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `createdAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-checkins-checkindetails"></a>
## CheckIns.CheckInDetails
C# type: `CampionsSystem.BLL.CheckIns.CheckInDetails`; source: `CampionsSystem.BLL/CheckIns/CheckInContracts.cs:44`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `operationId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberCode` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberName` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `businessDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `businessDayId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `occurredAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `type` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `method` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `subscriptionId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `sessionConsumed` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `sessionsBefore` | `int?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `sessionsAfter` | `int?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `actorId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-checkins-checkindetails"></a>
## CheckIns.CheckInDetails?
C# type: `CampionsSystem.BLL.CheckIns.CheckInDetails?`; source: `CampionsSystem.BLL/CheckIns/CheckInContracts.cs:44`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `operationId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberCode` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberName` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `businessDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `businessDayId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `occurredAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `type` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `method` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `subscriptionId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `sessionConsumed` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `sessionsBefore` | `int?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `sessionsAfter` | `int?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `actorId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-checkins-checkinhistory"></a>
## CheckIns.CheckInHistory
C# type: `CampionsSystem.BLL.CheckIns.CheckInHistory`; source: `CampionsSystem.BLL/CheckIns/CheckInContracts.cs:60`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<CheckIns.CheckInDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `totalCount` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-checkins-checkinhistoryrequest"></a>
## CheckIns.CheckInHistoryRequest
C# type: `CampionsSystem.BLL.CheckIns.CheckInHistoryRequest`; source: `CampionsSystem.BLL/CheckIns/CheckInContracts.cs:49`.
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `from` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `to` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `type` | `CheckInType?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `method` | `CheckInMethod?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | 1 |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | 25 |
<a id="campionssystem-bll-checkins-checkinmembermatch"></a>
## CheckIns.CheckInMemberMatch
C# type: `CampionsSystem.BLL.CheckIns.CheckInMemberMatch`; source: `CampionsSystem.BLL/CheckIns/CheckInContracts.cs:72`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberCode` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `name` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `isActive` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `registrationBranchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `clientCode` | `long?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-checkins-checkinmembermatches"></a>
## CheckIns.CheckInMemberMatches
C# type: `CampionsSystem.BLL.CheckIns.CheckInMemberMatches`; source: `CampionsSystem.BLL/CheckIns/CheckInContracts.cs:73`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<CheckIns.CheckInMemberMatch>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `totalCount` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-checkins-checkinmembersearchrequest"></a>
## CheckIns.CheckInMemberSearchRequest
C# type: `CampionsSystem.BLL.CheckIns.CheckInMemberSearchRequest`; source: `CampionsSystem.BLL/CheckIns/CheckInContracts.cs:65`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `search` | `string` | No | No attribute; see BLL guard evidence | "" |
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | 1 |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | 25 |
<a id="campionssystem-bll-checkins-checkinresult"></a>
## CheckIns.CheckInResult
C# type: `CampionsSystem.BLL.CheckIns.CheckInResult`; source: `CampionsSystem.BLL/CheckIns/CheckInContracts.cs:48`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `decision` | [CheckIns.AccessDecision](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-checkins-accessdecision) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `checkIn` | [CheckIns.CheckInDetails?](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-checkins-checkindetails) | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-checkins-createcheckinrequest"></a>
## CheckIns.CreateCheckInRequest
C# type: `CampionsSystem.BLL.CheckIns.CreateCheckInRequest`; source: `CampionsSystem.BLL/CheckIns/CheckInContracts.cs:38`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `operationId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `method` | [CheckInMethod](DTO_AND_ENUM_REFERENCE.md#campionssystem-dal-enums-checkinmethod) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
| `memberCode` | `string?` | Yes | No attribute; see BLL guard evidence | null |
| `confirmBorrow` | `bool` | No | No attribute; see BLL guard evidence | false |
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-checkins-redeemtrialrequest"></a>
## CheckIns.RedeemTrialRequest
C# type: `CampionsSystem.BLL.CheckIns.RedeemTrialRequest`; source: `CampionsSystem.BLL/CheckIns/CheckInContracts.cs:84`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `operationId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `invitationId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `phone` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-checkins-trialcheckinresult"></a>
## CheckIns.TrialCheckInResult
C# type: `CampionsSystem.BLL.CheckIns.TrialCheckInResult`; source: `CampionsSystem.BLL/CheckIns/CheckInContracts.cs:85`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `allowed` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reasonCode` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `invitationId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `checkIn` | [CheckIns.CheckInDetails?](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-checkins-checkindetails) | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-commissions-categoryruleinput"></a>
## Commissions.CategoryRuleInput
C# type: `CampionsSystem.BLL.Commissions.CategoryRuleInput`; source: `CampionsSystem.BLL/Commissions/CommissionContracts.cs:8`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `target` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `belowRate` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reachedRate` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-commissions-commissioncategoryprogress"></a>
## Commissions.CommissionCategoryProgress
C# type: `CampionsSystem.BLL.Commissions.CommissionCategoryProgress`; source: `CampionsSystem.BLL/Commissions/CommissionContracts.cs:20`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `category` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `grossCollections` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `refunds` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `eligibleCollections` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `configured` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `target` | `decimal?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `targetReached` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `belowRate` | `decimal?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reachedRate` | `decimal?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `appliedRate` | `decimal?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `commission` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `ruleId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `retainedCollections` | `decimal` | No | No attribute; see BLL guard evidence | computed/read-only output |
<a id="campionssystem-bll-commissions-commissionpage-campionssystem-bll-commissions-commissionperioddetails"></a>
## Commissions.CommissionPage<Commissions.CommissionPeriodDetails>
C# type: `CampionsSystem.BLL.Commissions.CommissionPage<CampionsSystem.BLL.Commissions.CommissionPeriodDetails>`; source: `CampionsSystem.BLL/Commissions/CommissionContracts.cs:34`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Commissions.CommissionPeriodDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `total` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-commissions-commissionpage-campionssystem-bll-commissions-commissionrefundsource"></a>
## Commissions.CommissionPage<Commissions.CommissionRefundSource>
C# type: `CampionsSystem.BLL.Commissions.CommissionPage<CampionsSystem.BLL.Commissions.CommissionRefundSource>`; source: `CampionsSystem.BLL/Commissions/CommissionContracts.cs:34`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Commissions.CommissionRefundSource>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `total` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-commissions-commissionpage-campionssystem-bll-commissions-commissionruledetails"></a>
## Commissions.CommissionPage<Commissions.CommissionRuleDetails>
C# type: `CampionsSystem.BLL.Commissions.CommissionPage<CampionsSystem.BLL.Commissions.CommissionRuleDetails>`; source: `CampionsSystem.BLL/Commissions/CommissionContracts.cs:34`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Commissions.CommissionRuleDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `total` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-commissions-commissionpage-campionssystem-bll-commissions-commissionsourceline"></a>
## Commissions.CommissionPage<Commissions.CommissionSourceLine>
C# type: `CampionsSystem.BLL.Commissions.CommissionPage<CampionsSystem.BLL.Commissions.CommissionSourceLine>`; source: `CampionsSystem.BLL/Commissions/CommissionContracts.cs:34`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Commissions.CommissionSourceLine>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `total` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-commissions-commissionperioddetails"></a>
## Commissions.CommissionPeriodDetails
C# type: `CampionsSystem.BLL.Commissions.CommissionPeriodDetails`; source: `CampionsSystem.BLL/Commissions/CommissionContracts.cs:16`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `name` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `startDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `endDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `status` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `version` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `finalizedAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-commissions-commissionprogress"></a>
## Commissions.CommissionProgress
C# type: `CampionsSystem.BLL.Commissions.CommissionProgress`; source: `CampionsSystem.BLL/Commissions/CommissionContracts.cs:27`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `period` | [Commissions.CommissionPeriodDetails](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-commissions-commissionperioddetails) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `finalized` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `grossCollections` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `refunds` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `retainedCollections` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employeeEligibleCollections` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `houseCollections` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `houseNewClientCollections` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `houseRenewalCollections` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `excludedCollections` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `newClientEligibleCollections` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `renewalEligibleCollections` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `totalCommission` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employeeCount` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employees` | `IReadOnlyList<Commissions.EmployeeCommissionStatement>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-commissions-commissionrefundsource"></a>
## Commissions.CommissionRefundSource
C# type: `CampionsSystem.BLL.Commissions.CommissionRefundSource`; source: `CampionsSystem.BLL/Commissions/CommissionContracts.cs:35`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `refundId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `paymentId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `amount` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `collectionBusinessDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `refundBusinessDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `refundedAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-commissions-commissionruledetails"></a>
## Commissions.CommissionRuleDetails
C# type: `CampionsSystem.BLL.Commissions.CommissionRuleDetails`; source: `CampionsSystem.BLL/Commissions/CommissionContracts.cs:18`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employeeId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `category` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `effectiveFrom` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `target` | `decimal?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `belowRate` | `decimal?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reachedRate` | `decimal?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `revision` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-commissions-commissionsourceline"></a>
## Commissions.CommissionSourceLine
C# type: `CampionsSystem.BLL.Commissions.CommissionSourceLine`; source: `CampionsSystem.BLL/Commissions/CommissionContracts.cs:31`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `paymentId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `saleId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `originalSalesEmployeeId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employeeId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `category` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `eligibilityReason` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `businessDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `paidAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `grossCollection` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `refunds` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `retainedCollection` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `eligibleCollection` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-commissions-configurecommissionrequest"></a>
## Commissions.ConfigureCommissionRequest
C# type: `CampionsSystem.BLL.Commissions.ConfigureCommissionRequest`; source: `CampionsSystem.BLL/Commissions/CommissionContracts.cs:10`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `operationId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employeeId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `effectiveFrom` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `newClients` | [Commissions.CategoryRuleInput](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-commissions-categoryruleinput) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `renewals` | [Commissions.CategoryRuleInput](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-commissions-categoryruleinput) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-commissions-employeecommissionstatement"></a>
## Commissions.EmployeeCommissionStatement
C# type: `CampionsSystem.BLL.Commissions.EmployeeCommissionStatement`; source: `CampionsSystem.BLL/Commissions/CommissionContracts.cs:25`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `employeeId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employeeName` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `newClients` | [Commissions.CommissionCategoryProgress](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-commissions-commissioncategoryprogress) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `renewals` | [Commissions.CommissionCategoryProgress](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-commissions-commissioncategoryprogress) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `totalCommission` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `periodId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `finalized` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `houseCollections` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `excludedCollections` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-commissions-finalizecommissionperiodrequest"></a>
## Commissions.FinalizeCommissionPeriodRequest
C# type: `CampionsSystem.BLL.Commissions.FinalizeCommissionPeriodRequest`; source: `CampionsSystem.BLL/Commissions/CommissionContracts.cs:15`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `operationId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `expectedVersion` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-commissions-opencommissionperiodrequest"></a>
## Commissions.OpenCommissionPeriodRequest
C# type: `CampionsSystem.BLL.Commissions.OpenCommissionPeriodRequest`; source: `CampionsSystem.BLL/Commissions/CommissionContracts.cs:13`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `operationId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `name` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `startDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `endDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-crm-convertleadrequest"></a>
## Crm.ConvertLeadRequest
C# type: `CampionsSystem.BLL.Crm.ConvertLeadRequest`; source: `CampionsSystem.BLL/Crm/CrmContracts.cs:71`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `targetRegistrationBranchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `freelancePlanId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `fullName` | `string` | No | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)`<br>`System.ComponentModel.DataAnnotations.RequiredAttribute` | No declared optional default; CLR initialization and BLL checks apply |
| `primaryPhone` | `string` | No | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)`<br>`System.ComponentModel.DataAnnotations.RequiredAttribute` | No declared optional default; CLR initialization and BLL checks apply |
| `secondaryPhone` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)` | No declared optional default; CLR initialization and BLL checks apply |
| `dateOfBirth` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `gender` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(32)` | No declared optional default; CLR initialization and BLL checks apply |
| `notes` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(4096)` | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-crm-createleadrequest"></a>
## Crm.CreateLeadRequest
C# type: `CampionsSystem.BLL.Crm.CreateLeadRequest`; source: `CampionsSystem.BLL/Crm/CrmContracts.cs:21`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `fullName` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` | No declared optional default; CLR initialization and BLL checks apply |
| `primaryPhone` | `string` | No | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)`<br>`System.ComponentModel.DataAnnotations.RequiredAttribute` | "" |
| `secondaryPhone` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)` | No declared optional default; CLR initialization and BLL checks apply |
| `email` | `string?` | Yes | `System.ComponentModel.DataAnnotations.EmailAddressAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(320)` | No declared optional default; CLR initialization and BLL checks apply |
| `source` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` | No declared optional default; CLR initialization and BLL checks apply |
| `notes` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(4096)` | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-crm-followupitem"></a>
## Crm.FollowUpItem
C# type: `CampionsSystem.BLL.Crm.FollowUpItem`; source: `CampionsSystem.BLL/Crm/CrmContracts.cs:84`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `subjectType` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `leadId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `name` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `phone` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchName` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `dueAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `crossBranch` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-crm-followuplistrequest"></a>
## Crm.FollowUpListRequest
C# type: `CampionsSystem.BLL.Crm.FollowUpListRequest`; source: `CampionsSystem.BLL/Crm/CrmContracts.cs:77`.
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `window` | `Crm.FollowUpWindow?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | `System.ComponentModel.DataAnnotations.RangeAttribute(1, 2147483647)` | 1 |
| `pageSize` | `int` | No | `System.ComponentModel.DataAnnotations.RangeAttribute(1, 100)` | 25 |
<a id="campionssystem-bll-crm-followuplistresult"></a>
## Crm.FollowUpListResult
C# type: `CampionsSystem.BLL.Crm.FollowUpListResult`; source: `CampionsSystem.BLL/Crm/CrmContracts.cs:87`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Crm.FollowUpItem>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `totalCount` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-crm-followupwindow"></a>
## Crm.FollowUpWindow
C# type: `CampionsSystem.BLL.Crm.FollowUpWindow`; source: `CampionsSystem.BLL/Crm/CrmContracts.cs:8`.
Type attributes: `System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter<CampionsSystem.BLL.Crm.FollowUpWindow>))`
Type default JSON: **string names**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| Overdue | 0 |
| Today | 1 |
| Upcoming | 2 |
<a id="campionssystem-bll-crm-leadassignmentdetails"></a>
## Crm.LeadAssignmentDetails
C# type: `CampionsSystem.BLL.Crm.LeadAssignmentDetails`; source: `CampionsSystem.BLL/Crm/CrmContracts.cs:31`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `salesEmployeeId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `salesEmployeeName` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `assignedAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `endedAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `assignedByUserId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-crm-leadassignmentdetails"></a>
## Crm.LeadAssignmentDetails?
C# type: `CampionsSystem.BLL.Crm.LeadAssignmentDetails?`; source: `CampionsSystem.BLL/Crm/CrmContracts.cs:31`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `salesEmployeeId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `salesEmployeeName` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `assignedAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `endedAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `assignedByUserId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-crm-leadassignmentrequest"></a>
## Crm.LeadAssignmentRequest
C# type: `CampionsSystem.BLL.Crm.LeadAssignmentRequest`; source: `CampionsSystem.BLL/Crm/CrmContracts.cs:42`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `salesEmployeeId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(2048)` | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-crm-leadcreationresult"></a>
## Crm.LeadCreationResult
C# type: `CampionsSystem.BLL.Crm.LeadCreationResult`; source: `CampionsSystem.BLL/Crm/CrmContracts.cs:40`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `result` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `lead` | [Crm.LeadDetails?](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-leaddetails) | Yes | No attribute; see BLL guard evidence | null |
| `existingMemberId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
| `error` | `string?` | Yes | No attribute; see BLL guard evidence | null |
| `existingLeadId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-crm-leaddetails"></a>
## Crm.LeadDetails
C# type: `CampionsSystem.BLL.Crm.LeadDetails`; source: `CampionsSystem.BLL/Crm/CrmContracts.cs:33`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchName` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `fullName` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `primaryPhone` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `primaryPhoneNormalized` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `secondaryPhone` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `email` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `source` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `notes` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `status` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `createdAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `nextFollowUpAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `currentAssignment` | [Crm.LeadAssignmentDetails?](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-leadassignmentdetails) | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `convertedMemberId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `convertedAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `lostAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `lostReason` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-crm-leaddetails"></a>
## Crm.LeadDetails?
C# type: `CampionsSystem.BLL.Crm.LeadDetails?`; source: `CampionsSystem.BLL/Crm/CrmContracts.cs:33`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchName` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `fullName` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `primaryPhone` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `primaryPhoneNormalized` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `secondaryPhone` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `email` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `source` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `notes` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `status` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `createdAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `nextFollowUpAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `currentAssignment` | [Crm.LeadAssignmentDetails?](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-crm-leadassignmentdetails) | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `convertedMemberId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `convertedAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `lostAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `lostReason` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-crm-leadinteractiondetails"></a>
## Crm.LeadInteractionDetails
C# type: `CampionsSystem.BLL.Crm.LeadInteractionDetails`; source: `CampionsSystem.BLL/Crm/CrmContracts.cs:59`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employeeId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `createdByUserId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `interactionType` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `notes` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `outcome` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `contactedAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `nextFollowUpAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `createdAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-crm-leadinteractionrequest"></a>
## Crm.LeadInteractionRequest
C# type: `CampionsSystem.BLL.Crm.LeadInteractionRequest`; source: `CampionsSystem.BLL/Crm/CrmContracts.cs:48`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `interactionType` | [InteractionType](DTO_AND_ENUM_REFERENCE.md#campionssystem-dal-enums-interactiontype) | No | `System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter<CampionsSystem.DAL.Enums.InteractionType>))` | No declared optional default; CLR initialization and BLL checks apply |
| `notes` | `string` | No | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(4096)`<br>`System.ComponentModel.DataAnnotations.RequiredAttribute` | "" |
| `outcome` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` | No declared optional default; CLR initialization and BLL checks apply |
| `contactedAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `nextFollowUpAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `status` | [LeadStatus](DTO_AND_ENUM_REFERENCE.md#campionssystem-dal-enums-leadstatus) | No | `System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter<CampionsSystem.DAL.Enums.LeadStatus>))` | LeadStatus.Contacted |
<a id="campionssystem-bll-crm-leadlistrequest"></a>
## Crm.LeadListRequest
C# type: `CampionsSystem.BLL.Crm.LeadListRequest`; source: `CampionsSystem.BLL/Crm/CrmContracts.cs:9`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `search` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `salesEmployeeId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `unassignedOnly` | `bool?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `status` | `LeadStatus?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `followUp` | `Crm.FollowUpWindow?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | `System.ComponentModel.DataAnnotations.RangeAttribute(1, 2147483647)` | 1 |
| `pageSize` | `int` | No | `System.ComponentModel.DataAnnotations.RangeAttribute(1, 100)` | 25 |
<a id="campionssystem-bll-crm-leadlistresult"></a>
## Crm.LeadListResult
C# type: `CampionsSystem.BLL.Crm.LeadListResult`; source: `CampionsSystem.BLL/Crm/CrmContracts.cs:38`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Crm.LeadDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `totalCount` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-crm-leadstatusrequest"></a>
## Crm.LeadStatusRequest
C# type: `CampionsSystem.BLL.Crm.LeadStatusRequest`; source: `CampionsSystem.BLL/Crm/CrmContracts.cs:63`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `status` | `LeadStatus?` | Yes | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter<CampionsSystem.DAL.Enums.LeadStatus>))` | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(2048)` | No declared optional default; CLR initialization and BLL checks apply |
| `nextFollowUpAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-crm-memberfollowuprequest"></a>
## Crm.MemberFollowUpRequest
C# type: `CampionsSystem.BLL.Crm.MemberFollowUpRequest`; source: `CampionsSystem.BLL/Crm/CrmContracts.cs:89`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `reason` | `string` | No | `System.ComponentModel.DataAnnotations.RequiredAttribute` | "Reactivation" |
| `dueAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `notes` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(4096)` | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-crm-updateleadrequest"></a>
## Crm.UpdateLeadRequest
C# type: `CampionsSystem.BLL.Crm.UpdateLeadRequest`; source: `CampionsSystem.BLL/Crm/CrmContracts.cs:97`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `fullName` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` | No declared optional default; CLR initialization and BLL checks apply |
| `primaryPhone` | `string` | No | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)`<br>`System.ComponentModel.DataAnnotations.RequiredAttribute` | "" |
| `secondaryPhone` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)` | No declared optional default; CLR initialization and BLL checks apply |
| `email` | `string?` | Yes | `System.ComponentModel.DataAnnotations.EmailAddressAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(320)` | No declared optional default; CLR initialization and BLL checks apply |
| `source` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` | No declared optional default; CLR initialization and BLL checks apply |
| `notes` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(4096)` | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-employees-changeemployeestatusrequest"></a>
## Employees.ChangeEmployeeStatusRequest
C# type: `CampionsSystem.BLL.Employees.ChangeEmployeeStatusRequest`; source: `CampionsSystem.BLL/Employees/EmployeeContracts.cs:41`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `status` | `EmploymentStatus?` | Yes | `System.ComponentModel.DataAnnotations.EnumDataTypeAttribute(typeof(CampionsSystem.DAL.Enums.EmploymentStatus))`<br>`System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter<CampionsSystem.DAL.Enums.EmploymentStatus>))` | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(2048)` | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-employees-createemployeerequest"></a>
## Employees.CreateEmployeeRequest
C# type: `CampionsSystem.BLL.Employees.CreateEmployeeRequest`; source: `CampionsSystem.BLL/Employees/EmployeeContracts.cs:35`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `initialBranchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `fullName` | `string` | No | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)`<br>`System.ComponentModel.DataAnnotations.RequiredAttribute` | No declared optional default; CLR initialization and BLL checks apply |
| `phone` | `string` | No | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)`<br>`System.ComponentModel.DataAnnotations.RequiredAttribute` | No declared optional default; CLR initialization and BLL checks apply |
| `jobTitle` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` | No declared optional default; CLR initialization and BLL checks apply |
| `employmentType` | [EmploymentType](DTO_AND_ENUM_REFERENCE.md#campionssystem-dal-enums-employmenttype) | No | `System.ComponentModel.DataAnnotations.EnumDataTypeAttribute(typeof(CampionsSystem.DAL.Enums.EmploymentType))`<br>`System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter<CampionsSystem.DAL.Enums.EmploymentType>))` | No declared optional default; CLR initialization and BLL checks apply |
| `hireDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-employees-employeedetails"></a>
## Employees.EmployeeDetails
C# type: `CampionsSystem.BLL.Employees.EmployeeDetails`; source: `CampionsSystem.BLL/Employees/EmployeeContracts.cs:16`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `fullName` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `phone` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `jobTitle` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employmentType` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `status` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `hireDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `terminationDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `currentBranch` | [Authentication.BranchContext?](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-authentication-branchcontext) | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `hasSystemAccount` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-employees-employeelistrequest"></a>
## Employees.EmployeeListRequest
C# type: `CampionsSystem.BLL.Employees.EmployeeListRequest`; source: `CampionsSystem.BLL/Employees/EmployeeContracts.cs:6`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `search` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `status` | `EmploymentStatus?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employmentType` | `EmploymentType?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | `System.ComponentModel.DataAnnotations.RangeAttribute(1, 2147483647)` | 1 |
| `pageSize` | `int` | No | `System.ComponentModel.DataAnnotations.RangeAttribute(1, 100)` | 25 |
<a id="campionssystem-bll-employees-employeelistresult"></a>
## Employees.EmployeeListResult
C# type: `CampionsSystem.BLL.Employees.EmployeeListResult`; source: `CampionsSystem.BLL/Employees/EmployeeContracts.cs:19`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Employees.EmployeeDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `totalCount` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-employees-employeeprofilerequest"></a>
## Employees.EmployeeProfileRequest
C# type: `CampionsSystem.BLL.Employees.EmployeeProfileRequest`; source: `CampionsSystem.BLL/Employees/EmployeeContracts.cs:23`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `fullName` | `string` | No | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)`<br>`System.ComponentModel.DataAnnotations.RequiredAttribute` | string.Empty |
| `phone` | `string` | No | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)`<br>`System.ComponentModel.DataAnnotations.RequiredAttribute` | string.Empty |
| `jobTitle` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` | No declared optional default; CLR initialization and BLL checks apply |
| `employmentType` | [EmploymentType](DTO_AND_ENUM_REFERENCE.md#campionssystem-dal-enums-employmenttype) | No | `System.ComponentModel.DataAnnotations.EnumDataTypeAttribute(typeof(CampionsSystem.DAL.Enums.EmploymentType))`<br>`System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter<CampionsSystem.DAL.Enums.EmploymentType>))` | No declared optional default; CLR initialization and BLL checks apply |
| `hireDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-members-changememberstatusrequest"></a>
## Members.ChangeMemberStatusRequest
C# type: `CampionsSystem.BLL.Members.ChangeMemberStatusRequest`; source: `CampionsSystem.BLL/Members/MemberContracts.cs:63`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `isActive` | `bool?` | Yes | `System.ComponentModel.DataAnnotations.RequiredAttribute` | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(2048)` | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-members-creatememberrequest"></a>
## Members.CreateMemberRequest
C# type: `CampionsSystem.BLL.Members.CreateMemberRequest`; source: `CampionsSystem.BLL/Members/MemberContracts.cs:53`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `targetRegistrationBranchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `creationType` | `Members.MemberCreationType?` | Yes | `System.ComponentModel.DataAnnotations.RequiredAttribute` | No declared optional default; CLR initialization and BLL checks apply |
| `salesEmployeeId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `freelancePlanId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `fullName` | `string` | No | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)`<br>`System.ComponentModel.DataAnnotations.RequiredAttribute` | No declared optional default; CLR initialization and BLL checks apply |
| `primaryPhone` | `string` | No | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)`<br>`System.ComponentModel.DataAnnotations.RequiredAttribute` | No declared optional default; CLR initialization and BLL checks apply |
| `secondaryPhone` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)` | No declared optional default; CLR initialization and BLL checks apply |
| `dateOfBirth` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `gender` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(32)` | No declared optional default; CLR initialization and BLL checks apply |
| `notes` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(4096)` | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-members-eligiblefreelanceplan"></a>
## Members.EligibleFreelancePlan
C# type: `CampionsSystem.BLL.Members.EligibleFreelancePlan`; source: `CampionsSystem.BLL/Members/MemberContracts.cs:27`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `planId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `planName` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `coachName` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-members-membercreationtype"></a>
## Members.MemberCreationType
C# type: `CampionsSystem.BLL.Members.MemberCreationType`; source: `CampionsSystem.BLL/Members/MemberContracts.cs:50`.
Type attributes: `System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter<CampionsSystem.BLL.Members.MemberCreationType>))`
Type default JSON: **string names**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| NormalSales | 0 |
| FreelanceHouse | 1 |
| ManagementCreated | 2 |
<a id="campionssystem-bll-members-memberdetails"></a>
## Members.MemberDetails
C# type: `CampionsSystem.BLL.Members.MemberDetails`; source: `CampionsSystem.BLL/Members/MemberContracts.cs:29`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `clientCode` | `long?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberCode` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `fullName` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `primaryPhone` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `primaryPhoneNormalized` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `secondaryPhone` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `isPrimaryPhoneVerified` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `dateOfBirth` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `gender` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `notes` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `isActive` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `registrationBranch` | [Authentication.BranchContext](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-authentication-branchcontext) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `createdAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `currentSalesOwner` | [Members.SalesAssignmentDetails?](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-members-salesassignmentdetails) | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employee` | [Members.MemberEmployeeLink?](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-members-memberemployeelink) | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `eligibleFreelancePlans` | `IReadOnlyList<Members.EligibleFreelancePlan>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `isHouse` | `bool` | No | No attribute; see BLL guard evidence | computed/read-only output |
<a id="campionssystem-bll-members-memberemployeelink"></a>
## Members.MemberEmployeeLink?
C# type: `CampionsSystem.BLL.Members.MemberEmployeeLink?`; source: `CampionsSystem.BLL/Members/MemberContracts.cs:28`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `fullName` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-members-memberlistrequest"></a>
## Members.MemberListRequest
C# type: `CampionsSystem.BLL.Members.MemberListRequest`; source: `CampionsSystem.BLL/Members/MemberContracts.cs:7`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `search` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` | No declared optional default; CLR initialization and BLL checks apply |
| `registrationBranchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `clientCode` | `long?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `isActive` | `bool?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `salesEmployeeId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `houseOnly` | `bool?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | `System.ComponentModel.DataAnnotations.RangeAttribute(1, 2147483647)` | 1 |
| `pageSize` | `int` | No | `System.ComponentModel.DataAnnotations.RangeAttribute(1, 100)` | 25 |
<a id="campionssystem-bll-members-memberlistresult"></a>
## Members.MemberListResult
C# type: `CampionsSystem.BLL.Members.MemberListResult`; source: `CampionsSystem.BLL/Members/MemberContracts.cs:37`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Members.MemberSummary>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `totalCount` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-members-membersummary"></a>
## Members.MemberSummary
C# type: `CampionsSystem.BLL.Members.MemberSummary`; source: `CampionsSystem.BLL/Members/MemberContracts.cs:22`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `clientCode` | `long?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberCode` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `fullName` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `primaryPhone` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `isActive` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `registrationBranch` | [Authentication.BranchContext](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-authentication-branchcontext) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `currentSalesOwner` | [Members.SalesAssignmentDetails?](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-members-salesassignmentdetails) | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `isHouse` | `bool` | No | No attribute; see BLL guard evidence | computed/read-only output |
<a id="campionssystem-bll-members-returntohouserequest"></a>
## Members.ReturnToHouseRequest
C# type: `CampionsSystem.BLL.Members.ReturnToHouseRequest`; source: `CampionsSystem.BLL/Members/MemberContracts.cs:75`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `reason` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(2048)` | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-members-salesassignmentdetails"></a>
## Members.SalesAssignmentDetails?
C# type: `CampionsSystem.BLL.Members.SalesAssignmentDetails?`; source: `CampionsSystem.BLL/Members/MemberContracts.cs:18`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `salesEmployeeId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `salesEmployeeName` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `assignedAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `endedAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-members-salesassignmenthistoryitem"></a>
## Members.SalesAssignmentHistoryItem
C# type: `CampionsSystem.BLL.Members.SalesAssignmentHistoryItem`; source: `CampionsSystem.BLL/Members/MemberContracts.cs:20`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `salesEmployeeId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `salesEmployeeName` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `assignmentType` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `assignedAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `endedAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-members-salesassignmentrequest"></a>
## Members.SalesAssignmentRequest
C# type: `CampionsSystem.BLL.Members.SalesAssignmentRequest`; source: `CampionsSystem.BLL/Members/MemberContracts.cs:69`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `salesEmployeeId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(2048)` | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-members-updatememberrequest"></a>
## Members.UpdateMemberRequest
C# type: `CampionsSystem.BLL.Members.UpdateMemberRequest`; source: `CampionsSystem.BLL/Members/MemberContracts.cs:40`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `fullName` | `string` | No | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)`<br>`System.ComponentModel.DataAnnotations.RequiredAttribute` | string.Empty |
| `primaryPhone` | `string` | No | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)`<br>`System.ComponentModel.DataAnnotations.RequiredAttribute` | string.Empty |
| `secondaryPhone` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)` | No declared optional default; CLR initialization and BLL checks apply |
| `dateOfBirth` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `gender` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(32)` | No declared optional default; CLR initialization and BLL checks apply |
| `notes` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(4096)` | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-operations-activitycoachreport"></a>
## Operations.ActivityCoachReport
C# type: `CampionsSystem.BLL.Operations.ActivityCoachReport`; source: `CampionsSystem.BLL/Operations/CoachReportService.cs:8`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `coachId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `coach` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `activityId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `activity` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `coachType` | [ActivityCoachType](DTO_AND_ENUM_REFERENCE.md#campionssystem-dal-enums-activitycoachtype) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employeeId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `subscribersCount` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `eligibleRevenue` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `percentage` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `entitlement` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-operations-activitypurchaserequest"></a>
## Operations.ActivityPurchaseRequest
C# type: `CampionsSystem.BLL.Operations.ActivityPurchaseRequest`; source: `CampionsSystem.BLL/Operations/SubscriptionPurchaseService.cs:9`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `memberId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `activityPackageId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `activityCoachId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `existingSubscriptionId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-operations-classcoachreport"></a>
## Operations.ClassCoachReport
C# type: `CampionsSystem.BLL.Operations.ClassCoachReport`; source: `CampionsSystem.BLL/Operations/CoachReportService.cs:11`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `coachEmployeeId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `coach` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `completedClassesCount` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `totalAttendances` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `averageAttendance` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-operations-createactivitycoachrequest"></a>
## Operations.CreateActivityCoachRequest
C# type: `CampionsSystem.BLL.Operations.CreateActivityCoachRequest`; source: `CampionsSystem.BLL/Operations/ActivityManagementService.cs:8`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `coachType` | [ActivityCoachType](DTO_AND_ENUM_REFERENCE.md#campionssystem-dal-enums-activitycoachtype) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employeeId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `externalName` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `externalPhone` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `percentage` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-operations-freelanceplanreport"></a>
## Operations.FreelancePlanReport
C# type: `CampionsSystem.BLL.Operations.FreelancePlanReport`; source: `CampionsSystem.BLL/Operations/CoachReportService.cs:13`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `membershipPlanId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `plan` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `coachEmployeeId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `coach` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `eligibleMemberCount` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `purchasedMemberCount` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `actualGymRevenue` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `salesAttribution` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-operations-freelancepurchaserequest"></a>
## Operations.FreelancePurchaseRequest
C# type: `CampionsSystem.BLL.Operations.FreelancePurchaseRequest`; source: `CampionsSystem.BLL/Operations/SubscriptionPurchaseService.cs:11`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `memberId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `membershipPlanId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `existingSubscriptionId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-operations-purchaseresult"></a>
## Operations.PurchaseResult
C# type: `CampionsSystem.BLL.Operations.PurchaseResult`; source: `CampionsSystem.BLL/Operations/SubscriptionPurchaseService.cs:13`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `subscriptionId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `transactionId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `saleTransactionId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `gymAmountDue` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-operations-saveclasssessionrequest"></a>
## Operations.SaveClassSessionRequest
C# type: `CampionsSystem.BLL.Operations.SaveClassSessionRequest`; source: `CampionsSystem.BLL/Operations/ClassSessionService.cs:8`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `groupClassId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `classScheduleId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `coachEmployeeId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `startedAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `endedAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `status` | [ClassSessionStatus](DTO_AND_ENUM_REFERENCE.md#campionssystem-dal-enums-classsessionstatus) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-operations-savefreelanceplanrequest"></a>
## Operations.SaveFreelancePlanRequest
C# type: `CampionsSystem.BLL.Operations.SaveFreelancePlanRequest`; source: `CampionsSystem.BLL/Operations/FreelancePlanService.cs:6`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `cityId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `coachEmployeeId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `name` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `price` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `durationDays` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `accessMode` | [AccessMode](DTO_AND_ENUM_REFERENCE.md#campionssystem-dal-enums-accessmode) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `sessionCount` | `int?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `isActive` | `bool` | No | No attribute; see BLL guard evidence | true |
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-plans-catalogdetails"></a>
## Plans.CatalogDetails
C# type: `CampionsSystem.BLL.Plans.CatalogDetails`; source: `CampionsSystem.BLL/Plans/PlanContracts.cs:25`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `code` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `name` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `description` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `isActive` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `createdAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-plans-cataloglistrequest"></a>
## Plans.CatalogListRequest
C# type: `CampionsSystem.BLL.Plans.CatalogListRequest`; source: `CampionsSystem.BLL/Plans/PlanContracts.cs:6`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `search` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `isActive` | `bool?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | 1 |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | 25 |
<a id="campionssystem-bll-plans-cataloglistresult"></a>
## Plans.CatalogListResult
C# type: `CampionsSystem.BLL.Plans.CatalogListResult`; source: `CampionsSystem.BLL/Plans/PlanContracts.cs:26`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Plans.CatalogDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `totalCount` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-plans-changeplanstatusrequest"></a>
## Plans.ChangePlanStatusRequest
C# type: `CampionsSystem.BLL.Plans.ChangePlanStatusRequest`; source: `CampionsSystem.BLL/Plans/PlanContracts.cs:41`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `isActive` | `bool?` | Yes | `System.ComponentModel.DataAnnotations.RequiredAttribute` | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-plans-createmembershipplanrequest"></a>
## Plans.CreateMembershipPlanRequest
C# type: `CampionsSystem.BLL.Plans.CreateMembershipPlanRequest`; source: `CampionsSystem.BLL/Plans/PlanContracts.cs:62`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `cityId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `name` | `string` | No | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)`<br>`System.ComponentModel.DataAnnotations.RequiredAttribute` | No declared optional default; CLR initialization and BLL checks apply |
| `planType` | `PlanCategory?` | Yes | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))` | No declared optional default; CLR initialization and BLL checks apply |
| `audienceCategoryId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `mode` | `AccessMode?` | Yes | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))` | No declared optional default; CLR initialization and BLL checks apply |
| `durationDays` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `sessions` | `int?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `price` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `allowFreeze` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `freezeDays` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `freelanceCoachEmployeeId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-plans-eligiblemembercandidaterequest"></a>
## Plans.EligibleMemberCandidateRequest
C# type: `CampionsSystem.BLL.Plans.EligibleMemberCandidateRequest`; source: `CampionsSystem.BLL/Plans/PlanContracts.cs:93`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `memberId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `phone` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)` | No declared optional default; CLR initialization and BLL checks apply |
| `memberCode` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-plans-eligiblememberdetails"></a>
## Plans.EligibleMemberDetails
C# type: `CampionsSystem.BLL.Plans.EligibleMemberDetails`; source: `CampionsSystem.BLL/Plans/PlanContracts.cs:90`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `memberId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `fullName` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `primaryPhone` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `registrationBranchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `clientCode` | `long?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberCode` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `addedAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-plans-eligiblememberlistresult"></a>
## Plans.EligibleMemberListResult
C# type: `CampionsSystem.BLL.Plans.EligibleMemberListResult`; source: `CampionsSystem.BLL/Plans/PlanContracts.cs:92`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Plans.EligibleMemberDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `totalCount` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-plans-eligiblememberrequest"></a>
## Plans.EligibleMemberRequest
C# type: `CampionsSystem.BLL.Plans.EligibleMemberRequest`; source: `CampionsSystem.BLL/Plans/PlanContracts.cs:86`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `memberId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-plans-membershipplandetails"></a>
## Plans.MembershipPlanDetails
C# type: `CampionsSystem.BLL.Plans.MembershipPlanDetails`; source: `CampionsSystem.BLL/Plans/PlanContracts.cs:70`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `plan` | [Plans.MembershipPlanSummary](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-membershipplansummary) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `createdAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `benefits` | `IReadOnlyList<Plans.PlanBenefitDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `eligibleMemberCount` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-plans-membershipplanlistrequest"></a>
## Plans.MembershipPlanListRequest
C# type: `CampionsSystem.BLL.Plans.MembershipPlanListRequest`; source: `CampionsSystem.BLL/Plans/PlanContracts.cs:13`.
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `cityId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `planType` | `PlanCategory?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `audienceCategoryId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `mode` | `AccessMode?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `freelanceCoachEmployeeId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `includeInternal` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `search` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `isActive` | `bool?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-plans-membershipplanlistresult"></a>
## Plans.MembershipPlanListResult
C# type: `CampionsSystem.BLL.Plans.MembershipPlanListResult`; source: `CampionsSystem.BLL/Plans/PlanContracts.cs:72`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Plans.MembershipPlanSummary>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `totalCount` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-plans-membershipplansummary"></a>
## Plans.MembershipPlanSummary
C# type: `CampionsSystem.BLL.Plans.MembershipPlanSummary`; source: `CampionsSystem.BLL/Plans/PlanContracts.cs:63`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `name` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `planType` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branch` | [Plans.PlanReference?](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-planreference) | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `city` | [Plans.PlanReference](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-planreference) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `audienceCategory` | [Plans.PlanReference?](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-planreference) | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `mode` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `price` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `durationDays` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `sessions` | `int?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `allowFreeze` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `freezeDays` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `isActive` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `freelanceCoach` | [Plans.PlanReference?](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-plans-planreference) | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `customerPurchasable` | `bool` | No | No attribute; see BLL guard evidence | computed/read-only output |
<a id="campionssystem-bll-plans-planbenefitdetails"></a>
## Plans.PlanBenefitDetails
C# type: `CampionsSystem.BLL.Plans.PlanBenefitDetails`; source: `CampionsSystem.BLL/Plans/PlanContracts.cs:69`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `benefitId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `code` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `name` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `isActive` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `grantType` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `quantity` | `int?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-plans-planbenefitrequest"></a>
## Plans.PlanBenefitRequest
C# type: `CampionsSystem.BLL.Plans.PlanBenefitRequest`; source: `CampionsSystem.BLL/Plans/PlanContracts.cs:74`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `benefitId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `grantType` | `MembershipBenefitGrantType?` | Yes | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))` | No declared optional default; CLR initialization and BLL checks apply |
| `quantity` | `int?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-plans-planreference"></a>
## Plans.PlanReference
C# type: `CampionsSystem.BLL.Plans.PlanReference`; source: `CampionsSystem.BLL/Plans/PlanContracts.cs:24`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `name` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-plans-planreference"></a>
## Plans.PlanReference?
C# type: `CampionsSystem.BLL.Plans.PlanReference?`; source: `CampionsSystem.BLL/Plans/PlanContracts.cs:24`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `name` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-plans-savebenefitrequest"></a>
## Plans.SaveBenefitRequest
C# type: `CampionsSystem.BLL.Plans.SaveBenefitRequest`; source: `CampionsSystem.BLL/Plans/PlanContracts.cs:34`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `code` | `string` | No | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(128)`<br>`System.ComponentModel.DataAnnotations.RequiredAttribute` | "" |
| `name` | `string` | No | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)`<br>`System.ComponentModel.DataAnnotations.RequiredAttribute` | "" |
| `description` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(4096)` | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-plans-savecategoryrequest"></a>
## Plans.SaveCategoryRequest
C# type: `CampionsSystem.BLL.Plans.SaveCategoryRequest`; source: `CampionsSystem.BLL/Plans/PlanContracts.cs:28`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `name` | `string` | No | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)`<br>`System.ComponentModel.DataAnnotations.RequiredAttribute` | "" |
| `description` | `string?` | Yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(4096)` | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-plans-setplanbenefitsrequest"></a>
## Plans.SetPlanBenefitsRequest
C# type: `CampionsSystem.BLL.Plans.SetPlanBenefitsRequest`; source: `CampionsSystem.BLL/Plans/PlanContracts.cs:81`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `benefits` | `List<Plans.PlanBenefitRequest>` | No | No attribute; see BLL guard evidence | [] |
<a id="campionssystem-bll-plans-updatemembershipplanrequest"></a>
## Plans.UpdateMembershipPlanRequest
C# type: `CampionsSystem.BLL.Plans.UpdateMembershipPlanRequest`; source: `CampionsSystem.BLL/Plans/PlanContracts.cs:46`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `cityId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `name` | `string` | No | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)`<br>`System.ComponentModel.DataAnnotations.RequiredAttribute` | "" |
| `planType` | `PlanCategory?` | Yes | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))` | No declared optional default; CLR initialization and BLL checks apply |
| `audienceCategoryId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `mode` | `AccessMode?` | Yes | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))` | No declared optional default; CLR initialization and BLL checks apply |
| `durationDays` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `sessions` | `int?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `price` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `allowFreeze` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `freezeDays` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `freelanceCoachEmployeeId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-sales-branchfinancialtotals"></a>
## Sales.BranchFinancialTotals
C# type: `CampionsSystem.BLL.Sales.BranchFinancialTotals`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:73`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `cashCollected` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `instaPayCollected` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `cashRefunded` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `instaPayRefunded` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `grossCollected` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `refunds` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `netCollected` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-sales-createfinancialaccountrequest"></a>
## Sales.CreateFinancialAccountRequest
C# type: `CampionsSystem.BLL.Sales.CreateFinancialAccountRequest`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:88`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `name` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `method` | [FinancialAccountType](DTO_AND_ENUM_REFERENCE.md#campionssystem-dal-enums-financialaccounttype) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-sales-createsalerequest"></a>
## Sales.CreateSaleRequest
C# type: `CampionsSystem.BLL.Sales.CreateSaleRequest`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:21`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `operationId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `membershipPlanId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `purchaseMode` | [PurchaseMode](DTO_AND_ENUM_REFERENCE.md#campionssystem-dal-enums-purchasemode) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `activationMode` | [SubscriptionActivationMode](DTO_AND_ENUM_REFERENCE.md#campionssystem-dal-enums-subscriptionactivationmode) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `paymentParts` | `IReadOnlyList<Sales.PaymentPartInput>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `offerId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
| `startDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | null |
| `reservationDueDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | null |
| `installments` | `IReadOnlyList<Sales.InstallmentLineInput>?` | Yes | No attribute; see BLL guard evidence | null |
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
| `initialPaymentRequired` | `decimal?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-sales-financialaccountdetails"></a>
## Sales.FinancialAccountDetails
C# type: `CampionsSystem.BLL.Sales.FinancialAccountDetails`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:89`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `name` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `method` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `isActive` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-sales-financialpartdetails"></a>
## Sales.FinancialPartDetails
C# type: `CampionsSystem.BLL.Sales.FinancialPartDetails`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:43`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `accountId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `accountName` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `method` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `amount` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reference` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-sales-grantmemberofferrequest"></a>
## Sales.GrantMemberOfferRequest
C# type: `CampionsSystem.BLL.Sales.GrantMemberOfferRequest`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:77`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `memberId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `startDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | null |
| `endDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-sales-installmentallocationinput"></a>
## Sales.InstallmentAllocationInput
C# type: `CampionsSystem.BLL.Sales.InstallmentAllocationInput`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:19`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `installmentId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `amount` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-sales-installmentdetails"></a>
## Sales.InstallmentDetails
C# type: `CampionsSystem.BLL.Sales.InstallmentDetails`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:49`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `number` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `dueDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `amount` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `paid` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `status` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-sales-installmentlineinput"></a>
## Sales.InstallmentLineInput
C# type: `CampionsSystem.BLL.Sales.InstallmentLineInput`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:17`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `amount` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `dueDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-sales-memberaudiencedetails"></a>
## Sales.MemberAudienceDetails
C# type: `CampionsSystem.BLL.Sales.MemberAudienceDetails`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:81`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `categoryId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `categoryName` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `assignedAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `endedAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-sales-memberoffergrantdetails"></a>
## Sales.MemberOfferGrantDetails
C# type: `CampionsSystem.BLL.Sales.MemberOfferGrantDetails`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:78`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `offerId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `startDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `endDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `revokedAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-sales-offerdetails"></a>
## Sales.OfferDetails
C# type: `CampionsSystem.BLL.Sales.OfferDetails`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:38`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `name` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `type` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `value` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `startDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `endDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `isPermanent` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `isActive` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `membershipPlanIds` | `IReadOnlyList<Guid>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `eligibilityMode` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `audienceCategoryId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-sales-offerhistory"></a>
## Sales.OfferHistory
C# type: `CampionsSystem.BLL.Sales.OfferHistory`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:86`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Sales.OfferDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `totalCount` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-sales-offerrequest"></a>
## Sales.OfferRequest
C# type: `CampionsSystem.BLL.Sales.OfferRequest`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:34`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `name` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `type` | [OfferType](DTO_AND_ENUM_REFERENCE.md#campionssystem-dal-enums-offertype) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `value` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `membershipPlanIds` | `IReadOnlyList<Guid>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `eligibilityMode` | `OfferEligibilityMode?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `audienceCategoryId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
| `startDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | null |
| `endDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | null |
| `isPermanent` | `bool` | No | No attribute; see BLL guard evidence | false |
| `isActive` | `bool` | No | No attribute; see BLL guard evidence | true |
| `description` | `string?` | Yes | No attribute; see BLL guard evidence | null |
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-sales-paymentdetails"></a>
## Sales.PaymentDetails
C# type: `CampionsSystem.BLL.Sales.PaymentDetails`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:44`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `operationId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `businessDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `businessDayId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `paidAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `amount` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `parts` | `IReadOnlyList<Sales.FinancialPartDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `allocations` | `IReadOnlyList<Sales.InstallmentAllocationInput>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-sales-paymenthistory"></a>
## Sales.PaymentHistory
C# type: `CampionsSystem.BLL.Sales.PaymentHistory`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:84`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Sales.PaymentDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `totalCount` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-sales-paymentpartinput"></a>
## Sales.PaymentPartInput
C# type: `CampionsSystem.BLL.Sales.PaymentPartInput`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:15`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `financialAccountId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `amount` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `providerReference` | `string?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-sales-pricingresult"></a>
## Sales.PricingResult
C# type: `CampionsSystem.BLL.Sales.PricingResult`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:40`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `originalPrice` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `discount` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `finalPrice` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `extraDays` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `offerId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `offerName` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `offerType` | `OfferType?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `offerValue` | `decimal?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `eligibilityMode` | `OfferEligibilityMode?` | Yes | No attribute; see BLL guard evidence | null |
| `audienceCategoryId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-sales-quickcancelrequest"></a>
## Sales.QuickCancelRequest
C# type: `CampionsSystem.BLL.Sales.QuickCancelRequest`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:32`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `operationId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-sales-recordpaymentrequest"></a>
## Sales.RecordPaymentRequest
C# type: `CampionsSystem.BLL.Sales.RecordPaymentRequest`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:26`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `operationId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `amount` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `parts` | `IReadOnlyList<Sales.PaymentPartInput>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `allocations` | `IReadOnlyList<Sales.InstallmentAllocationInput>?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-sales-refunddetails"></a>
## Sales.RefundDetails
C# type: `CampionsSystem.BLL.Sales.RefundDetails`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:47`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `operationId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `businessDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `createdAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `amount` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `type` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `parts` | `IReadOnlyList<Sales.FinancialPartDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-sales-refundhistory"></a>
## Sales.RefundHistory
C# type: `CampionsSystem.BLL.Sales.RefundHistory`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:85`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Sales.RefundDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `totalCount` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-sales-refundrequest"></a>
## Sales.RefundRequest
C# type: `CampionsSystem.BLL.Sales.RefundRequest`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:29`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `operationId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `amount` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `parts` | `IReadOnlyList<Sales.PaymentPartInput>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-sales-saledetails"></a>
## Sales.SaleDetails
C# type: `CampionsSystem.BLL.Sales.SaleDetails`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:53`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `sale` | [Sales.SaleSummary](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-sales-salesummary) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberName` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `planName` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `planType` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `planId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `accessCityId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `salesEmployeeId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `salesAttribution` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `commissionEligible` | `bool?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `targetEligible` | `bool?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `subscriptionId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `grantId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `activationStatus` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `requestedStartDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reservationDueDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `offerName` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `extraDays` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `retainedCancellationRevenue` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `payments` | `IReadOnlyList<Sales.PaymentDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `refunds` | `IReadOnlyList<Sales.RefundDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `installments` | `IReadOnlyList<Sales.InstallmentDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `offerId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
| `offerType` | `string?` | Yes | No attribute; see BLL guard evidence | null |
| `offerValue` | `decimal?` | Yes | No attribute; see BLL guard evidence | null |
| `offerEligibilityMode` | `string?` | Yes | No attribute; see BLL guard evidence | null |
| `offerAudienceCategoryId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
| `retainedRevenueClassification` | `string` | No | No attribute; see BLL guard evidence | "CancellationFee" |
| `retainedCommissionEligible` | `bool` | No | No attribute; see BLL guard evidence | false |
| `retainedTargetEligible` | `bool` | No | No attribute; see BLL guard evidence | false |
<a id="campionssystem-bll-sales-salehistory"></a>
## Sales.SaleHistory
C# type: `CampionsSystem.BLL.Sales.SaleHistory`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:72`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Sales.SaleSummary>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `totalCount` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-sales-salehistoryrequest"></a>
## Sales.SaleHistoryRequest
C# type: `CampionsSystem.BLL.Sales.SaleHistoryRequest`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:62`.
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `purchaseMode` | `PurchaseMode?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `from` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `to` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | 1 |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | 25 |
<a id="campionssystem-bll-sales-salereceipt"></a>
## Sales.SaleReceipt
C# type: `CampionsSystem.BLL.Sales.SaleReceipt`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:83`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `saleId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `paymentIds` | `IReadOnlyList<Guid>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-sales-salesummary"></a>
## Sales.SaleSummary
C# type: `CampionsSystem.BLL.Sales.SaleSummary`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:50`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `businessDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `createdAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `originalPrice` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `discount` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `finalPrice` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `collected` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `refunded` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `outstanding` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `status` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `purchaseMode` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `activationMode` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `cancelled` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `version` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-sales-setmemberaudiencerequest"></a>
## Sales.SetMemberAudienceRequest
C# type: `CampionsSystem.BLL.Sales.SetMemberAudienceRequest`; source: `CampionsSystem.BLL/Sales/SalesContracts.cs:80`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `audienceCategoryIds` | `IReadOnlyList<Guid>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-advancedetails"></a>
## Shifts.AdvanceDetails
C# type: `CampionsSystem.BLL.Shifts.AdvanceDetails`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:51`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `shiftId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `receivingEmployeeId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `receivingEmployeeName` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `recordingEmployeeId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `accountId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `method` | [FinancialAccountType](DTO_AND_ENUM_REFERENCE.md#campionssystem-dal-enums-financialaccounttype) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `amount` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reference` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `occurredAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `businessDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-advancerequest"></a>
## Shifts.AdvanceRequest
C# type: `CampionsSystem.BLL.Shifts.AdvanceRequest`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:34`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `operationId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `receivingEmployeeId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `financialAccountId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `amount` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reference` | `string?` | Yes | No attribute; see BLL guard evidence | null |
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-shifts-branchfinancialsummary"></a>
## Shifts.BranchFinancialSummary
C# type: `CampionsSystem.BLL.Shifts.BranchFinancialSummary`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:61`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `from` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `to` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `activity` | `IReadOnlyList<Shifts.MethodFinancialTotals>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `currentSharedDrawer` | [Shifts.DrawerPosition](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-drawerposition) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-businessdaydetails"></a>
## Shifts.BusinessDayDetails
C# type: `CampionsSystem.BLL.Shifts.BusinessDayDetails`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:54`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `businessDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `status` | [BusinessDayStatus](DTO_AND_ENUM_REFERENCE.md#campionssystem-dal-enums-businessdaystatus) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `openedAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `closedAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `openedByUserId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `closedByUserId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-businessdaydetails"></a>
## Shifts.BusinessDayDetails?
C# type: `CampionsSystem.BLL.Shifts.BusinessDayDetails?`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:54`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `businessDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `status` | [BusinessDayStatus](DTO_AND_ENUM_REFERENCE.md#campionssystem-dal-enums-businessdaystatus) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `openedAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `closedAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `openedByUserId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `closedByUserId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-cashcountdetails"></a>
## Shifts.CashCountDetails
C# type: `CampionsSystem.BLL.Shifts.CashCountDetails`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:37`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `shiftId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `kind` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `expectedCash` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `countedCash` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `bookCash` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `difference` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `occurredAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `businessDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `actorId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `observationOrder` | `long` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-cashmovementdetails"></a>
## Shifts.CashMovementDetails
C# type: `CampionsSystem.BLL.Shifts.CashMovementDetails`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:39`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `shiftId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `kind` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `amount` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `occurredAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `businessDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-closeshiftrequest"></a>
## Shifts.CloseShiftRequest
C# type: `CampionsSystem.BLL.Shifts.CloseShiftRequest`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:15`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `operationId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `expectedVersion` | `long` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `countedCash` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-shifts-countcashrequest"></a>
## Shifts.CountCashRequest
C# type: `CampionsSystem.BLL.Shifts.CountCashRequest`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:24`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `operationId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `countedCash` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-shifts-drawerposition"></a>
## Shifts.DrawerPosition
C# type: `CampionsSystem.BLL.Shifts.DrawerPosition`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:19`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `accountId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `bookCash` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `expectedPhysicalCash` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `lastCountedCash` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `movementSequence` | `long` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `lastCountId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `observedDiscrepancies` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-expensecategorydetails"></a>
## Shifts.ExpenseCategoryDetails
C# type: `CampionsSystem.BLL.Shifts.ExpenseCategoryDetails`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:50`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `name` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `isActive` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-expensecategoryrequest"></a>
## Shifts.ExpenseCategoryRequest
C# type: `CampionsSystem.BLL.Shifts.ExpenseCategoryRequest`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:49`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `name` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `isActive` | `bool` | No | No attribute; see BLL guard evidence | true |
<a id="campionssystem-bll-shifts-expensedetails"></a>
## Shifts.ExpenseDetails
C# type: `CampionsSystem.BLL.Shifts.ExpenseDetails`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:43`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `shiftId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `recordingEmployeeId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `categoryId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `categoryName` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `accountId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `method` | [FinancialAccountType](DTO_AND_ENUM_REFERENCE.md#campionssystem-dal-enums-financialaccounttype) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `amount` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `returnedAmount` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `description` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `receiptReference` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `occurredAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `businessDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `paidAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `originalStatus` | [ExpenseStatus](DTO_AND_ENUM_REFERENCE.md#campionssystem-dal-enums-expensestatus) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-expensereversaldetails"></a>
## Shifts.ExpenseReversalDetails
C# type: `CampionsSystem.BLL.Shifts.ExpenseReversalDetails`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:46`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `expenseId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `shiftId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `amount` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `returnReference` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `occurredAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `businessDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-financialauditdetails"></a>
## Shifts.FinancialAuditDetails
C# type: `CampionsSystem.BLL.Shifts.FinancialAuditDetails`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:63`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `event` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `entityType` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `entityId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `actorId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `occurredAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `before` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `after` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-financialmovementdetails"></a>
## Shifts.FinancialMovementDetails
C# type: `CampionsSystem.BLL.Shifts.FinancialMovementDetails`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:41`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `accountId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `shiftId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `sourceType` | [FinancialSourceType](DTO_AND_ENUM_REFERENCE.md#campionssystem-dal-enums-financialsourcetype) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `direction` | [FinancialTransactionType](DTO_AND_ENUM_REFERENCE.md#campionssystem-dal-enums-financialtransactiontype) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `amount` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `occurredAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `businessDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `actorId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-methodfinancialtotals"></a>
## Shifts.MethodFinancialTotals
C# type: `CampionsSystem.BLL.Shifts.MethodFinancialTotals`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:56`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `method` | [FinancialAccountType](DTO_AND_ENUM_REFERENCE.md#campionssystem-dal-enums-financialaccounttype) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `collected` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `refunded` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `expensesPaid` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `expenseReturns` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `advancesPaid` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `withdrawals` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `initialPosition` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `otherCredits` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `otherDebits` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `netMovement` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `legacyUnshiftedNetMovement` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-openbusinessdayrequest"></a>
## Shifts.OpenBusinessDayRequest
C# type: `CampionsSystem.BLL.Shifts.OpenBusinessDayRequest`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:28`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `operationId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `businessDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-shifts-openshiftrequest"></a>
## Shifts.OpenShiftRequest
C# type: `CampionsSystem.BLL.Shifts.OpenShiftRequest`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:13`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `operationId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `openingCash` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-shifts-paidexpenserequest"></a>
## Shifts.PaidExpenseRequest
C# type: `CampionsSystem.BLL.Shifts.PaidExpenseRequest`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:30`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `operationId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `expenseCategoryId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `financialAccountId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `amount` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `description` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `receiptReference` | `string?` | Yes | No attribute; see BLL guard evidence | null |
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-shifts-reverseexpenserequest"></a>
## Shifts.ReverseExpenseRequest
C# type: `CampionsSystem.BLL.Shifts.ReverseExpenseRequest`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:32`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `operationId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `returnedAmount` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `fundsActuallyReturned` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `returnReference` | `string?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-shifts-setupdrawerrequest"></a>
## Shifts.SetupDrawerRequest
C# type: `CampionsSystem.BLL.Shifts.SetupDrawerRequest`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:22`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `operationId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `financialAccountId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `countedCash` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-shifts-shiftdetails"></a>
## Shifts.ShiftDetails
C# type: `CampionsSystem.BLL.Shifts.ShiftDetails`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:16`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employeeId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employeeName` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `status` | [ShiftStatus](DTO_AND_ENUM_REFERENCE.md#campionssystem-dal-enums-shiftstatus) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `version` | `long` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `openedAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `closedAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `openingBusinessDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `closingBusinessDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `openingCash` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `expectedClosingCash` | `decimal?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `actualClosingCash` | `decimal?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `difference` | `decimal?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-shiftdetails"></a>
## Shifts.ShiftDetails?
C# type: `CampionsSystem.BLL.Shifts.ShiftDetails?`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:16`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employeeId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employeeName` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `status` | [ShiftStatus](DTO_AND_ENUM_REFERENCE.md#campionssystem-dal-enums-shiftstatus) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `version` | `long` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `openedAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `closedAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `openingBusinessDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `closingBusinessDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `openingCash` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `expectedClosingCash` | `decimal?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `actualClosingCash` | `decimal?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `difference` | `decimal?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-shiftfinancialsummary"></a>
## Shifts.ShiftFinancialSummary
C# type: `CampionsSystem.BLL.Shifts.ShiftFinancialSummary`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:59`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `shift` | [Shifts.ShiftDetails](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-shiftdetails) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `activity` | `IReadOnlyList<Shifts.MethodFinancialTotals>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `currentSharedDrawer` | [Shifts.DrawerPosition](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-shifts-drawerposition) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-shifthistoryrequest"></a>
## Shifts.ShiftHistoryRequest
C# type: `CampionsSystem.BLL.Shifts.ShiftHistoryRequest`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:35`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
| `employeeId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
| `from` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | null |
| `to` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | null |
| `page` | `int` | No | No attribute; see BLL guard evidence | 1 |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | 25 |
<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-advancedetails"></a>
## Shifts.ShiftPage<Shifts.AdvanceDetails>
C# type: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.AdvanceDetails>`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:36`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Shifts.AdvanceDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `total` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-cashcountdetails"></a>
## Shifts.ShiftPage<Shifts.CashCountDetails>
C# type: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.CashCountDetails>`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:36`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Shifts.CashCountDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `total` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-expensecategorydetails"></a>
## Shifts.ShiftPage<Shifts.ExpenseCategoryDetails>
C# type: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.ExpenseCategoryDetails>`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:36`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Shifts.ExpenseCategoryDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `total` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-expensedetails"></a>
## Shifts.ShiftPage<Shifts.ExpenseDetails>
C# type: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.ExpenseDetails>`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:36`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Shifts.ExpenseDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `total` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-expensereversaldetails"></a>
## Shifts.ShiftPage<Shifts.ExpenseReversalDetails>
C# type: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.ExpenseReversalDetails>`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:36`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Shifts.ExpenseReversalDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `total` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-financialauditdetails"></a>
## Shifts.ShiftPage<Shifts.FinancialAuditDetails>
C# type: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.FinancialAuditDetails>`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:36`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Shifts.FinancialAuditDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `total` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-financialmovementdetails"></a>
## Shifts.ShiftPage<Shifts.FinancialMovementDetails>
C# type: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.FinancialMovementDetails>`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:36`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Shifts.FinancialMovementDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `total` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-shiftdetails"></a>
## Shifts.ShiftPage<Shifts.ShiftDetails>
C# type: `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.ShiftDetails>`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:36`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Shifts.ShiftDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `total` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-shifts-vaulthistoryrequest"></a>
## Shifts.VaultHistoryRequest
C# type: `CampionsSystem.BLL.Shifts.VaultHistoryRequest`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:40`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
| `accountId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
| `from` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | null |
| `to` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | null |
| `page` | `int` | No | No attribute; see BLL guard evidence | 1 |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | 25 |
<a id="campionssystem-bll-shifts-withdrawalrequest"></a>
## Shifts.WithdrawalRequest
C# type: `CampionsSystem.BLL.Shifts.WithdrawalRequest`; source: `CampionsSystem.BLL/Shifts/ShiftContracts.cs:26`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `operationId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `amount` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-subscriptions-benefitsnapshot"></a>
## Subscriptions.BenefitSnapshot
C# type: `CampionsSystem.BLL.Subscriptions.BenefitSnapshot`; source: `CampionsSystem.BLL/Subscriptions/SubscriptionContracts.cs:28`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `grantId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `code` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `name` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `grantType` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `grantedQuantity` | `int?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `cancelled` | `bool` | No | No attribute; see BLL guard evidence | false |
<a id="campionssystem-bll-subscriptions-completefreezerequest"></a>
## Subscriptions.CompleteFreezeRequest
C# type: `CampionsSystem.BLL.Subscriptions.CompleteFreezeRequest`; source: `CampionsSystem.BLL/Subscriptions/SubscriptionContracts.cs:51`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `expectedVersion` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-subscriptions-customextensionrequest"></a>
## Subscriptions.CustomExtensionRequest
C# type: `CampionsSystem.BLL.Subscriptions.CustomExtensionRequest`; source: `CampionsSystem.BLL/Subscriptions/SubscriptionContracts.cs:53`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `operationId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `expectedVersion` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `days` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-subscriptions-employeeentitlementrequest"></a>
## Subscriptions.EmployeeEntitlementRequest
C# type: `CampionsSystem.BLL.Subscriptions.EmployeeEntitlementRequest`; source: `CampionsSystem.BLL/Subscriptions/SubscriptionContracts.cs:47`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `memberId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `planId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `operationId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-subscriptions-freezedetails"></a>
## Subscriptions.FreezeDetails
C# type: `CampionsSystem.BLL.Subscriptions.FreezeDetails`; source: `CampionsSystem.BLL/Subscriptions/SubscriptionContracts.cs:34`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `type` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `startDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `endDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `daysApplied` | `int?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `approvedBy` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `createdAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `completedAt` | `DateTimeOffset?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `daysAlreadyCompensated` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `previousSubscriptionEndDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `newSubscriptionEndDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-subscriptions-freezerequest"></a>
## Subscriptions.FreezeRequest
C# type: `CampionsSystem.BLL.Subscriptions.FreezeRequest`; source: `CampionsSystem.BLL/Subscriptions/SubscriptionContracts.cs:49`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `operationId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `expectedVersion` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `startDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `days` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-subscriptions-holidayapplicationresult"></a>
## Subscriptions.HolidayApplicationResult
C# type: `CampionsSystem.BLL.Subscriptions.HolidayApplicationResult`; source: `CampionsSystem.BLL/Subscriptions/SubscriptionContracts.cs:56`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `closureId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `protectedDays` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `extendedDays` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-subscriptions-holidayextensiondetails"></a>
## Subscriptions.HolidayExtensionDetails
C# type: `CampionsSystem.BLL.Subscriptions.HolidayExtensionDetails`; source: `CampionsSystem.BLL/Subscriptions/SubscriptionContracts.cs:37`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `closureId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `protectedDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `daysApplied` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-subscriptions-memberentitlement"></a>
## Subscriptions.MemberEntitlement
C# type: `CampionsSystem.BLL.Subscriptions.MemberEntitlement`; source: `CampionsSystem.BLL/Subscriptions/SubscriptionContracts.cs:57`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `hasAccess` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `subscription` | [Subscriptions.SubscriptionSummary?](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-subscriptions-subscriptionsummary) | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-subscriptions-officialholidayrequest"></a>
## Subscriptions.OfficialHolidayRequest
C# type: `CampionsSystem.BLL.Subscriptions.OfficialHolidayRequest`; source: `CampionsSystem.BLL/Subscriptions/SubscriptionContracts.cs:55`.
Type attributes: `System.Text.Json.Serialization.JsonUnmappedMemberHandlingAttribute(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)`
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `branchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `startDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `endDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `extendSubscriptions` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `reason` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-subscriptions-subscriptiondetails"></a>
## Subscriptions.SubscriptionDetails
C# type: `CampionsSystem.BLL.Subscriptions.SubscriptionDetails`; source: `CampionsSystem.BLL/Subscriptions/SubscriptionContracts.cs:38`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `subscription` | [Subscriptions.SubscriptionSummary](DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-subscriptions-subscriptionsummary) | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberName` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `freezeAllowance` | `int?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `consumedFreezeDays` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `remainingFreezeDays` | `int?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `benefits` | `IReadOnlyList<Subscriptions.BenefitSnapshot>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `grants` | `IReadOnlyList<Subscriptions.SubscriptionGrantDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `freezes` | `IReadOnlyList<Subscriptions.FreezeDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `holidays` | `IReadOnlyList<Subscriptions.HolidayExtensionDetails>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-subscriptions-subscriptiongrantdetails"></a>
## Subscriptions.SubscriptionGrantDetails
C# type: `CampionsSystem.BLL.Subscriptions.SubscriptionGrantDetails`; source: `CampionsSystem.BLL/Subscriptions/SubscriptionContracts.cs:29`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `planId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `planName` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `planType` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `planBranchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `accessCityId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `mode` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `durationDays` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `sessions` | `int?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `freezeDays` | `int?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `planPrice` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `netPrice` | `decimal` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `freelanceCoachId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `transactionType` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `startDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `previousEndDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `newEndDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `previousRemainingSessions` | `int?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `renewedFromSubscriptionId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `businessDayId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `actorId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `createdAt` | `DateTimeOffset` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `cancellationRefundId` | `Guid?` | Yes | No attribute; see BLL guard evidence | null |
<a id="campionssystem-bll-subscriptions-subscriptiongrantresult"></a>
## Subscriptions.SubscriptionGrantResult
C# type: `CampionsSystem.BLL.Subscriptions.SubscriptionGrantResult`; source: `CampionsSystem.BLL/Subscriptions/SubscriptionContracts.cs:45`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `subscriptionId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `grantId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `scheduled` | `bool` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-subscriptions-subscriptionhistory"></a>
## Subscriptions.SubscriptionHistory
C# type: `CampionsSystem.BLL.Subscriptions.SubscriptionHistory`; source: `CampionsSystem.BLL/Subscriptions/SubscriptionContracts.cs:27`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `items` | `IReadOnlyList<Subscriptions.SubscriptionSummary>` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `totalCount` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `page` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-subscriptions-subscriptionhistoryrequest"></a>
## Subscriptions.SubscriptionHistoryRequest
C# type: `CampionsSystem.BLL.Subscriptions.SubscriptionHistoryRequest`; source: `CampionsSystem.BLL/Subscriptions/SubscriptionContracts.cs:13`.
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `page` | `int` | No | No attribute; see BLL guard evidence | 1 |
| `pageSize` | `int` | No | No attribute; see BLL guard evidence | 25 |
| `branchId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `status` | `SubscriptionStatus?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `planType` | `PlanCategory?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `from` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `to` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
<a id="campionssystem-bll-subscriptions-subscriptionsummary"></a>
## Subscriptions.SubscriptionSummary
C# type: `CampionsSystem.BLL.Subscriptions.SubscriptionSummary`; source: `CampionsSystem.BLL/Subscriptions/SubscriptionContracts.cs:23`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `sourcePlanId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `planName` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `planType` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `originBranchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `accessCityId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `mode` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `startDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `endDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `status` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `totalSessions` | `int?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `remainingSessions` | `int?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `version` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employeeId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `followsSubscriptionId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `isStandaloneVisit` | `bool` | No | No attribute; see BLL guard evidence | false |
<a id="campionssystem-bll-subscriptions-subscriptionsummary"></a>
## Subscriptions.SubscriptionSummary?
C# type: `CampionsSystem.BLL.Subscriptions.SubscriptionSummary?`; source: `CampionsSystem.BLL/Subscriptions/SubscriptionContracts.cs:23`.
Type attributes: 
| Exact JSON field | C# type/nesting | Null allowed | Validation/converter | Default (input) / getter |
|---|---|---|---|---|
| `id` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `memberId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `sourcePlanId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `planName` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `planType` | `string?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `originBranchId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `accessCityId` | `Guid` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `mode` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `startDate` | `DateOnly` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `endDate` | `DateOnly?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `status` | `string` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `totalSessions` | `int?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `remainingSessions` | `int?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `version` | `int` | No | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `employeeId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `followsSubscriptionId` | `Guid?` | Yes | No attribute; see BLL guard evidence | No declared optional default; CLR initialization and BLL checks apply |
| `isStandaloneVisit` | `bool` | No | No attribute; see BLL guard evidence | false |
<a id="campionssystem-dal-enums-accessmode"></a>
## AccessMode
C# type: `CampionsSystem.DAL.Enums.AccessMode`; source: `CampionsSystem.DAL/Enums/AccessMode.cs`.
Type default JSON: **numeric values**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| Unlimited | 0 |
| SessionBased | 1 |
<a id="campionssystem-dal-enums-activitycoachtype"></a>
## ActivityCoachType
C# type: `CampionsSystem.DAL.Enums.ActivityCoachType`; source: `CampionsSystem.DAL/Enums/ActivityCoachType.cs`.
Type default JSON: **numeric values**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| Internal | 0 |
| External | 1 |
<a id="campionssystem-dal-enums-businessdaystatus"></a>
## BusinessDayStatus
C# type: `CampionsSystem.DAL.Enums.BusinessDayStatus`; source: `CampionsSystem.DAL/Enums/BusinessDayStatus.cs`.
Type default JSON: **numeric values**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| Open | 0 |
| Closed | 1 |
<a id="campionssystem-dal-enums-checkinmethod"></a>
## CheckInMethod
C# type: `CampionsSystem.DAL.Enums.CheckInMethod`; source: `CampionsSystem.DAL/Enums/CheckInMethod.cs`.
Type default JSON: **numeric values**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| Scanner | 0 |
| Manual | 1 |
| Coach | 2 |
<a id="campionssystem-dal-enums-checkintype"></a>
## CheckInType
C# type: `CampionsSystem.DAL.Enums.CheckInType`; source: `CampionsSystem.DAL/Enums/CheckInType.cs`.
Type default JSON: **numeric values**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| Gym | 0 |
| BorrowedGym | 1 |
| Activity | 2 |
| Class | 3 |
| Trial | 4 |
<a id="campionssystem-dal-enums-classsessionstatus"></a>
## ClassSessionStatus
C# type: `CampionsSystem.DAL.Enums.ClassSessionStatus`; source: `CampionsSystem.DAL/Enums/ClassSessionStatus.cs`.
Type default JSON: **numeric values**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| Scheduled | 0 |
| Completed | 1 |
| Cancelled | 2 |
<a id="campionssystem-dal-enums-employmentstatus"></a>
## EmploymentStatus
C# type: `CampionsSystem.DAL.Enums.EmploymentStatus`; source: `CampionsSystem.DAL/Enums/EmploymentStatus.cs`.
Type default JSON: **numeric values**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| Active | 0 |
| Resigned | 1 |
| Suspended | 2 |
<a id="campionssystem-dal-enums-employmenttype"></a>
## EmploymentType
C# type: `CampionsSystem.DAL.Enums.EmploymentType`; source: `CampionsSystem.DAL/Enums/EmploymentType.cs`.
Type default JSON: **numeric values**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| Staff | 0 |
| Freelance | 1 |
<a id="campionssystem-dal-enums-expensestatus"></a>
## ExpenseStatus
C# type: `CampionsSystem.DAL.Enums.ExpenseStatus`; source: `CampionsSystem.DAL/Enums/ExpenseStatus.cs`.
Type default JSON: **numeric values**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| Recorded | 0 |
| Cancelled | 1 |
<a id="campionssystem-dal-enums-financialaccounttype"></a>
## FinancialAccountType
C# type: `CampionsSystem.DAL.Enums.FinancialAccountType`; source: `CampionsSystem.DAL/Enums/FinancialAccountType.cs`.
Type default JSON: **numeric values**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| Cash | 0 |
| InstaPay | 1 |
| Bank | 2 |
| Card | 3 |
| Wallet | 4 |
<a id="campionssystem-dal-enums-financialsourcetype"></a>
## FinancialSourceType
C# type: `CampionsSystem.DAL.Enums.FinancialSourceType`; source: `CampionsSystem.DAL/Enums/FinancialSourceType.cs`.
Type default JSON: **numeric values**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| Payment | 0 |
| Refund | 1 |
| Expense | 2 |
| Payroll | 3 |
| Adjustment | 4 |
| EmployeeAdvance | 5 |
| CashMovement | 6 |
<a id="campionssystem-dal-enums-financialtransactiontype"></a>
## FinancialTransactionType
C# type: `CampionsSystem.DAL.Enums.FinancialTransactionType`; source: `CampionsSystem.DAL/Enums/FinancialTransactionType.cs`.
Type default JSON: **numeric values**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| Credit | 0 |
| Debit | 1 |
<a id="campionssystem-dal-enums-interactiontype"></a>
## InteractionType
C# type: `CampionsSystem.DAL.Enums.InteractionType`; source: `CampionsSystem.DAL/Enums/InteractionType.cs`.
Type default JSON: **numeric values**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| Call | 0 |
| WhatsApp | 1 |
| Visit | 2 |
| Other | 3 |
<a id="campionssystem-dal-enums-leadstatus"></a>
## LeadStatus
C# type: `CampionsSystem.DAL.Enums.LeadStatus`; source: `CampionsSystem.DAL/Enums/LeadStatus.cs`.
Type default JSON: **numeric values**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| New | 0 |
| Contacted | 1 |
| Interested | 2 |
| FollowUp | 3 |
| Converted | 4 |
| Lost | 5 |
| Inactive | 6 |
<a id="campionssystem-dal-enums-membershipbenefitgranttype"></a>
## MembershipBenefitGrantType
C# type: `CampionsSystem.DAL.Enums.MembershipBenefitGrantType`; source: `CampionsSystem.DAL/Enums/MembershipBenefitGrantType.cs`.
Type default JSON: **numeric values**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| Quantity | 0 |
| Access | 1 |
<a id="campionssystem-dal-enums-offereligibilitymode"></a>
## OfferEligibilityMode
C# type: `CampionsSystem.DAL.Enums.OfferEligibilityMode`; source: `CampionsSystem.DAL/Enums/OfferEligibilityMode.cs`.
Type default JSON: **numeric values**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| General | 0 |
| MemberOnly | 1 |
<a id="campionssystem-dal-enums-offertype"></a>
## OfferType
C# type: `CampionsSystem.DAL.Enums.OfferType`; source: `CampionsSystem.DAL/Enums/OfferType.cs`.
Type default JSON: **numeric values**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| PercentageDiscount | 0 |
| FixedDiscount | 1 |
| ExtraDays | 2 |
<a id="campionssystem-dal-enums-plancategory"></a>
## PlanCategory
C# type: `CampionsSystem.DAL.Enums.PlanCategory`; source: `CampionsSystem.DAL/Enums/PlanCategory.cs`.
Type default JSON: **numeric values**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| Normal | 0 |
| Freelance | 1 |
| Employee | 2 |
<a id="campionssystem-dal-enums-purchasemode"></a>
## PurchaseMode
C# type: `CampionsSystem.DAL.Enums.PurchaseMode`; source: `CampionsSystem.DAL/Enums/PurchaseMode.cs`.
Type default JSON: **numeric values**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| FullPayment | 0 |
| Reservation | 1 |
| Installment | 2 |
| StandaloneSingleSession | 3 |
<a id="campionssystem-dal-enums-shiftstatus"></a>
## ShiftStatus
C# type: `CampionsSystem.DAL.Enums.ShiftStatus`; source: `CampionsSystem.DAL/Enums/ShiftStatus.cs`.
Type default JSON: **numeric values**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| Open | 0 |
| Closed | 1 |
<a id="campionssystem-dal-enums-subscriptionactivationmode"></a>
## SubscriptionActivationMode
C# type: `CampionsSystem.DAL.Enums.SubscriptionActivationMode`; source: `CampionsSystem.DAL/Enums/SubscriptionActivationMode.cs`.
Type default JSON: **numeric values**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| Now | 0 |
| FirstCheckIn | 1 |
| SpecificDate | 2 |
<a id="campionssystem-dal-enums-subscriptionstatus"></a>
## SubscriptionStatus
C# type: `CampionsSystem.DAL.Enums.SubscriptionStatus`; source: `CampionsSystem.DAL/Enums/SubscriptionStatus.cs`.
Type default JSON: **numeric values**; a property converter can change this.
| Name | Numeric value |
|---|---:|
| Scheduled | 0 |
| Active | 1 |
| Frozen | 2 |
| Expired | 3 |
| Cancelled | 4 |
| SuspendedForInstallment | 5 |



## Primitive, nullable and array/constructed shapes

<a id="campionssystem-bll-crm-followupwindow"></a>

### `CampionsSystem.BLL.Crm.FollowUpWindow?`

Framework/constructed type. Nullable wrapper; preserve null rather than substituting zero/empty.

<a id="campionssystem-bll-members-membercreationtype"></a>

### `CampionsSystem.BLL.Members.MemberCreationType?`

Framework/constructed type. Nullable wrapper; preserve null rather than substituting zero/empty.

<a id="campionssystem-dal-enums-accessmode"></a>

### `CampionsSystem.DAL.Enums.AccessMode?`

Framework/constructed type. Nullable wrapper; preserve null rather than substituting zero/empty.

<a id="campionssystem-dal-enums-checkinmethod"></a>

### `CampionsSystem.DAL.Enums.CheckInMethod?`

Framework/constructed type. Nullable wrapper; preserve null rather than substituting zero/empty.

<a id="campionssystem-dal-enums-checkintype"></a>

### `CampionsSystem.DAL.Enums.CheckInType?`

Framework/constructed type. Nullable wrapper; preserve null rather than substituting zero/empty.

<a id="campionssystem-dal-enums-employmentstatus"></a>

### `CampionsSystem.DAL.Enums.EmploymentStatus?`

Framework/constructed type. Nullable wrapper; preserve null rather than substituting zero/empty.

<a id="campionssystem-dal-enums-employmenttype"></a>

### `CampionsSystem.DAL.Enums.EmploymentType?`

Framework/constructed type. Nullable wrapper; preserve null rather than substituting zero/empty.

<a id="campionssystem-dal-enums-leadstatus"></a>

### `CampionsSystem.DAL.Enums.LeadStatus?`

Framework/constructed type. Nullable wrapper; preserve null rather than substituting zero/empty.

<a id="campionssystem-dal-enums-membershipbenefitgranttype"></a>

### `CampionsSystem.DAL.Enums.MembershipBenefitGrantType?`

Framework/constructed type. Nullable wrapper; preserve null rather than substituting zero/empty.

<a id="campionssystem-dal-enums-offereligibilitymode"></a>

### `CampionsSystem.DAL.Enums.OfferEligibilityMode?`

Framework/constructed type. Nullable wrapper; preserve null rather than substituting zero/empty.

<a id="campionssystem-dal-enums-offertype"></a>

### `CampionsSystem.DAL.Enums.OfferType?`

Framework/constructed type. Nullable wrapper; preserve null rather than substituting zero/empty.

<a id="campionssystem-dal-enums-plancategory"></a>

### `CampionsSystem.DAL.Enums.PlanCategory?`

Framework/constructed type. Nullable wrapper; preserve null rather than substituting zero/empty.

<a id="campionssystem-dal-enums-purchasemode"></a>

### `CampionsSystem.DAL.Enums.PurchaseMode?`

Framework/constructed type. Nullable wrapper; preserve null rather than substituting zero/empty.

<a id="campionssystem-dal-enums-subscriptionstatus"></a>

### `CampionsSystem.DAL.Enums.SubscriptionStatus?`

Framework/constructed type. Nullable wrapper; preserve null rather than substituting zero/empty.

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-branches-branchassignmentdetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Branches.BranchAssignmentDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Branches.BranchAssignmentDetails](#campionssystem-bll-branches-branchassignmentdetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-branches-branchdetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Branches.BranchDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Branches.BranchDetails](#campionssystem-bll-branches-branchdetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-checkins-borroweddebtdetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.CheckIns.BorrowedDebtDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.CheckIns.BorrowedDebtDetails](#campionssystem-bll-checkins-borroweddebtdetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-checkins-borrowedsettlementdetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.CheckIns.BorrowedSettlementDetails>?`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.CheckIns.BorrowedSettlementDetails>](#campionssystem-bll-checkins-borrowedsettlementdetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-checkins-checkindetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.CheckIns.CheckInDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.CheckIns.CheckInDetails](#campionssystem-bll-checkins-checkindetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-checkins-checkinmembermatch"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.CheckIns.CheckInMemberMatch>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.CheckIns.CheckInMemberMatch](#campionssystem-bll-checkins-checkinmembermatch).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-commissions-commissionperioddetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Commissions.CommissionPeriodDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Commissions.CommissionPeriodDetails](#campionssystem-bll-commissions-commissionperioddetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-commissions-commissionrefundsource"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Commissions.CommissionRefundSource>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Commissions.CommissionRefundSource](#campionssystem-bll-commissions-commissionrefundsource).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-commissions-commissionruledetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Commissions.CommissionRuleDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Commissions.CommissionRuleDetails](#campionssystem-bll-commissions-commissionruledetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-commissions-commissionsourceline"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Commissions.CommissionSourceLine>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Commissions.CommissionSourceLine](#campionssystem-bll-commissions-commissionsourceline).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-commissions-employeecommissionstatement"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Commissions.EmployeeCommissionStatement>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Commissions.EmployeeCommissionStatement](#campionssystem-bll-commissions-employeecommissionstatement).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-crm-followupitem"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Crm.FollowUpItem>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Crm.FollowUpItem](#campionssystem-bll-crm-followupitem).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-crm-leadassignmentdetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Crm.LeadAssignmentDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Crm.LeadAssignmentDetails](#campionssystem-bll-crm-leadassignmentdetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-crm-leadcreationresult"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Crm.LeadCreationResult>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Crm.LeadCreationResult](#campionssystem-bll-crm-leadcreationresult).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-crm-leaddetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Crm.LeadDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Crm.LeadDetails](#campionssystem-bll-crm-leaddetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-crm-leadinteractiondetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Crm.LeadInteractionDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Crm.LeadInteractionDetails](#campionssystem-bll-crm-leadinteractiondetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-employees-employeedetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Employees.EmployeeDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Employees.EmployeeDetails](#campionssystem-bll-employees-employeedetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-members-eligiblefreelanceplan"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Members.EligibleFreelancePlan>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Members.EligibleFreelancePlan](#campionssystem-bll-members-eligiblefreelanceplan).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-members-membersummary"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Members.MemberSummary>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Members.MemberSummary](#campionssystem-bll-members-membersummary).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-members-salesassignmenthistoryitem"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Members.SalesAssignmentHistoryItem>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Members.SalesAssignmentHistoryItem](#campionssystem-bll-members-salesassignmenthistoryitem).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-operations-activitycoachreport"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Operations.ActivityCoachReport>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Operations.ActivityCoachReport](#campionssystem-bll-operations-activitycoachreport).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-operations-classcoachreport"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Operations.ClassCoachReport>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Operations.ClassCoachReport](#campionssystem-bll-operations-classcoachreport).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-operations-freelanceplanreport"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Operations.FreelancePlanReport>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Operations.FreelancePlanReport](#campionssystem-bll-operations-freelanceplanreport).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-plans-catalogdetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Plans.CatalogDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Plans.CatalogDetails](#campionssystem-bll-plans-catalogdetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-plans-eligiblememberdetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Plans.EligibleMemberDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Plans.EligibleMemberDetails](#campionssystem-bll-plans-eligiblememberdetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-plans-membershipplansummary"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Plans.MembershipPlanSummary>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Plans.MembershipPlanSummary](#campionssystem-bll-plans-membershipplansummary).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-plans-planbenefitdetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Plans.PlanBenefitDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Plans.PlanBenefitDetails](#campionssystem-bll-plans-planbenefitdetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-sales-financialaccountdetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Sales.FinancialAccountDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Sales.FinancialAccountDetails](#campionssystem-bll-sales-financialaccountdetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-sales-financialpartdetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Sales.FinancialPartDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Sales.FinancialPartDetails](#campionssystem-bll-sales-financialpartdetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-sales-installmentallocationinput"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Sales.InstallmentAllocationInput>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Sales.InstallmentAllocationInput](#campionssystem-bll-sales-installmentallocationinput).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-sales-installmentallocationinput"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Sales.InstallmentAllocationInput>?`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Sales.InstallmentAllocationInput>](#campionssystem-bll-sales-installmentallocationinput).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-sales-installmentdetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Sales.InstallmentDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Sales.InstallmentDetails](#campionssystem-bll-sales-installmentdetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-sales-installmentlineinput"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Sales.InstallmentLineInput>?`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Sales.InstallmentLineInput>](#campionssystem-bll-sales-installmentlineinput).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-sales-memberaudiencedetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Sales.MemberAudienceDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Sales.MemberAudienceDetails](#campionssystem-bll-sales-memberaudiencedetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-sales-offerdetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Sales.OfferDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Sales.OfferDetails](#campionssystem-bll-sales-offerdetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-sales-paymentdetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Sales.PaymentDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Sales.PaymentDetails](#campionssystem-bll-sales-paymentdetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-sales-paymentpartinput"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Sales.PaymentPartInput>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Sales.PaymentPartInput](#campionssystem-bll-sales-paymentpartinput).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-sales-refunddetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Sales.RefundDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Sales.RefundDetails](#campionssystem-bll-sales-refunddetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-sales-salesummary"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Sales.SaleSummary>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Sales.SaleSummary](#campionssystem-bll-sales-salesummary).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-shifts-advancedetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Shifts.AdvanceDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Shifts.AdvanceDetails](#campionssystem-bll-shifts-advancedetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-shifts-cashcountdetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Shifts.CashCountDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Shifts.CashCountDetails](#campionssystem-bll-shifts-cashcountdetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-shifts-expensecategorydetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Shifts.ExpenseCategoryDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Shifts.ExpenseCategoryDetails](#campionssystem-bll-shifts-expensecategorydetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-shifts-expensedetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Shifts.ExpenseDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Shifts.ExpenseDetails](#campionssystem-bll-shifts-expensedetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-shifts-expensereversaldetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Shifts.ExpenseReversalDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Shifts.ExpenseReversalDetails](#campionssystem-bll-shifts-expensereversaldetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-shifts-financialauditdetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Shifts.FinancialAuditDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Shifts.FinancialAuditDetails](#campionssystem-bll-shifts-financialauditdetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-shifts-financialmovementdetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Shifts.FinancialMovementDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Shifts.FinancialMovementDetails](#campionssystem-bll-shifts-financialmovementdetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-shifts-methodfinancialtotals"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Shifts.MethodFinancialTotals>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Shifts.MethodFinancialTotals](#campionssystem-bll-shifts-methodfinancialtotals).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-shifts-shiftdetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Shifts.ShiftDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Shifts.ShiftDetails](#campionssystem-bll-shifts-shiftdetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-subscriptions-benefitsnapshot"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Subscriptions.BenefitSnapshot>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Subscriptions.BenefitSnapshot](#campionssystem-bll-subscriptions-benefitsnapshot).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-subscriptions-freezedetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Subscriptions.FreezeDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Subscriptions.FreezeDetails](#campionssystem-bll-subscriptions-freezedetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-subscriptions-holidayextensiondetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Subscriptions.HolidayExtensionDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Subscriptions.HolidayExtensionDetails](#campionssystem-bll-subscriptions-holidayextensiondetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-subscriptions-subscriptiongrantdetails"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Subscriptions.SubscriptionGrantDetails>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Subscriptions.SubscriptionGrantDetails](#campionssystem-bll-subscriptions-subscriptiongrantdetails).

<a id="system-collections-generic-ireadonlylist-campionssystem-bll-subscriptions-subscriptionsummary"></a>

### `System.Collections.Generic.IReadOnlyList<CampionsSystem.BLL.Subscriptions.SubscriptionSummary>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Subscriptions.SubscriptionSummary](#campionssystem-bll-subscriptions-subscriptionsummary).

<a id="system-collections-generic-ireadonlylist-system-guid"></a>

### `System.Collections.Generic.IReadOnlyList<System.Guid>`

JSON array for list/collection interfaces; element schema: [System.Guid](#system-guid).

<a id="system-collections-generic-ireadonlylist-string"></a>

### `System.Collections.Generic.IReadOnlyList<string>`

JSON array for list/collection interfaces; element schema: [string](#string).

<a id="system-collections-generic-list-campionssystem-bll-plans-planbenefitrequest"></a>

### `System.Collections.Generic.List<CampionsSystem.BLL.Plans.PlanBenefitRequest>`

JSON array for list/collection interfaces; element schema: [CampionsSystem.BLL.Plans.PlanBenefitRequest](#campionssystem-bll-plans-planbenefitrequest).

<a id="system-dateonly"></a>

### `System.DateOnly`

Framework/constructed type. 

<a id="system-dateonly"></a>

### `System.DateOnly?`

Framework/constructed type. Nullable wrapper; preserve null rather than substituting zero/empty.

<a id="system-datetimeoffset"></a>

### `System.DateTimeOffset`

Framework/constructed type. 

<a id="system-datetimeoffset"></a>

### `System.DateTimeOffset?`

Framework/constructed type. Nullable wrapper; preserve null rather than substituting zero/empty.

<a id="system-guid"></a>

### `System.Guid`

Framework/constructed type. 

<a id="system-guid"></a>

### `System.Guid?`

Framework/constructed type. Nullable wrapper; preserve null rather than substituting zero/empty.

<a id="system-text-json-jsonelement"></a>

### `System.Text.Json.JsonElement`

Framework/constructed type. 

<a id="system-text-json-jsonelement"></a>

### `System.Text.Json.JsonElement[]`

JSON array; element System.Text.Json.JsonElement

<a id="bool"></a>

### `bool`

Framework/constructed type. 

<a id="bool"></a>

### `bool?`

Framework/constructed type. Nullable wrapper; preserve null rather than substituting zero/empty.

<a id="decimal"></a>

### `decimal`

Framework/constructed type. 

<a id="decimal"></a>

### `decimal?`

Framework/constructed type. Nullable wrapper; preserve null rather than substituting zero/empty.

<a id="int"></a>

### `int`

Framework/constructed type. 

<a id="int"></a>

### `int?`

Framework/constructed type. Nullable wrapper; preserve null rather than substituting zero/empty.

<a id="long"></a>

### `long`

Framework/constructed type. 

<a id="long"></a>

### `long?`

Framework/constructed type. Nullable wrapper; preserve null rather than substituting zero/empty.

<a id="string"></a>

### `string`

Framework/constructed type. 

<a id="string"></a>

### `string?`

Framework/constructed type. Nullable wrapper; preserve null rather than substituting zero/empty.

## String-valued enum projections

Several response fields are explicitly C# string and project `Enum.ToString()`, even though mutation/query enum fields may use numeric JSON. The exact name vocabularies below apply only to their corresponding projections, not every free-text status/name field. Service guards may further narrow valid transitions; Category in rule history uses SalesNew/SalesRenewal, while progress/source purchase category uses NewMembership/Renewal. Names are case-sensitive output values.

| Enum projection | Exact names | Source |
|---|---|---|
| SaleStatus | `Open`, `Completed`, `Cancelled`, `PartiallyRefunded`, `Refunded` | `DAL/Enums/SaleStatus.cs` |
| SubscriptionStatus | `Scheduled`, `Active`, `Frozen`, `Expired`, `Cancelled`, `SuspendedForInstallment` | `DAL/Enums/SubscriptionStatus.cs` |
| EmploymentStatus | `Active`, `Resigned`, `Suspended` | `DAL/Enums/EmploymentStatus.cs` |
| EmploymentType | `Staff`, `Freelance` | `DAL/Enums/EmploymentType.cs` |
| PlanCategory | `Normal`, `Freelance`, `Employee` | `DAL/Enums/PlanCategory.cs` |
| AccessMode | `Unlimited`, `SessionBased` | `DAL/Enums/AccessMode.cs` |
| LeadStatus | `New`, `Contacted`, `Interested`, `FollowUp`, `Converted`, `Lost`, `Inactive` | `DAL/Enums/LeadStatus.cs` |
| InteractionType | `Call`, `WhatsApp`, `Visit`, `Other` | `DAL/Enums/InteractionType.cs` |
| AssignmentType | `Sales`, `House` | `DAL/Enums/AssignmentType.cs` |
| RefundType | `QuickRefund`, `ManagerRefund` | `DAL/Enums/RefundType.cs` |
| OfferType | `PercentageDiscount`, `FixedDiscount`, `ExtraDays` | `DAL/Enums/OfferType.cs` |
| OfferEligibilityMode | `General`, `MemberOnly` | `DAL/Enums/OfferEligibilityMode.cs` |
| PurchaseMode | `FullPayment`, `Reservation`, `Installment`, `StandaloneSingleSession` | `DAL/Enums/PurchaseMode.cs` |
| SubscriptionActivationMode | `Now`, `FirstCheckIn`, `SpecificDate` | `DAL/Enums/SubscriptionActivationMode.cs` |
| SubscriptionTransactionType | `Initial`, `Renewal` | `DAL/Enums/SubscriptionTransactionType.cs` |
| FreezeType | `Normal`, `Medical` | `DAL/Enums/FreezeType.cs` |
| MembershipBenefitGrantType | `Quantity`, `Access` | `DAL/Enums/MembershipBenefitGrantType.cs` |
| CheckInType | `Gym`, `BorrowedGym`, `Activity`, `Class`, `Trial` | `DAL/Enums/CheckInType.cs` |
| CheckInMethod | `Scanner`, `Manual`, `Coach` | `DAL/Enums/CheckInMethod.cs` |
| InstallmentStatus | `Pending`, `PartiallyPaid`, `Paid`, `Overdue` | `DAL/Enums/InstallmentStatus.cs` |
| CommissionPeriodStatus | `Open`, `Closed` | `DAL/Enums/CommissionPeriodStatus.cs` |
| SaleCategory | `NewMembership`, `Renewal` | `DAL/Enums/SaleCategory.cs` |
| CommissionType | `SalesNew`, `SalesRenewal`, `Private`, `Activity`, `Class`, `Freelance` | `DAL/Enums/CommissionType.cs` |
| ClassSessionStatus | `Scheduled`, `Completed`, `Cancelled` | `DAL/Enums/ClassSessionStatus.cs` |
