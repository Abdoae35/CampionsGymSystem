# Source-derived DTO and enum reference
V1 baseline (`e3b8cc3`) compiler inventory plus the Excel Bulk Import addendum below. The raw OpenAPI defines public request schemas; the integration supplement adds omitted success response schemas. Nullable annotation alone does not settle MVC or business requiredness.
Dates use ISO `YYYY-MM-DD`; timestamps carry offsets; Guid is UUID; decimals are JSON numbers. Check per-property converters: most enums are numeric, but annotated properties emit string enum names. Do not use JavaScript floating point as a money authority.

## Excel Bulk Import addendum

`ExcelImportUploadRequest` is multipart form-data with required binary field `File`. It is not a JSON request. `ExcelImportResult` is JSON: `importId` (UUID), `kind` (string), `expiresAt` (timestamp), `totalRows`, `validRows`, `invalidRows`, `duplicateRows`, `createdRows`, `failedRows` (integers), `rows` (`ExcelImportRowResult[]`) and `warnings` (`string[]`). `ExcelImportRowResult` is `rowNumber` (Excel row integer), `status` (string), `message` (nullable string), `createdRecordId` (nullable UUID) and `values` (map of submitted column names to strings). See [Excel Bulk Import](EXCEL_BULK_IMPORT.md) for allowed values and row-level rules. No new financial DTO is introduced.

<a id="anonymous-type-system-guid-id"></a>
## `<anonymous type: System.Guid Id>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |

<a id="anonymous-type-bool-eligible"></a>
## `<anonymous type: bool Eligible>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `eligible` | `bool` | no | — |

<a id="anonymous-type-bool-finalized"></a>
## `<anonymous type: bool Finalized>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `finalized` | `bool` | no | — |

<a id="anonymous-type-bool-updated"></a>
## `<anonymous type: bool Updated>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `updated` | `bool` | no | — |

<a id="campionssystem-api-controllers-operationscontroller-assignmentrequest"></a>
## `CampionsSystem.API.Controllers.OperationsController.AssignmentRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `activityCoachId` | `Guid` | no | — |

<a id="campionssystem-api-controllers-operationscontroller-percentagerequest"></a>
## `CampionsSystem.API.Controllers.OperationsController.PercentageRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `percentage` | `decimal` | no | — |

<a id="campionssystem-bll-authentication-accessauditquery"></a>
## `CampionsSystem.BLL.Authentication.AccessAuditQuery`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `page` | `int` | no | `System.ComponentModel.DataAnnotations.RangeAttribute(1, 2147483647)` |
| `pageSize` | `int` | no | `System.ComponentModel.DataAnnotations.RangeAttribute(1, 100)` |
| `targetUserId` | `Guid?` | yes | — |
| `roleId` | `Guid?` | yes | — |
| `actorUserId` | `Guid?` | yes | — |
| `operationId` | `Guid?` | yes | — |

<a id="campionssystem-bll-authentication-accessinventoryquery"></a>
## `CampionsSystem.BLL.Authentication.AccessInventoryQuery`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `page` | `int` | no | `System.ComponentModel.DataAnnotations.RangeAttribute(1, 2147483647)` |
| `pageSize` | `int` | no | `System.ComponentModel.DataAnnotations.RangeAttribute(1, 100)` |
| `search` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |
| `roleId` | `Guid?` | yes | — |
| `branchId` | `Guid?` | yes | — |
| `isActive` | `bool?` | yes | — |

<a id="campionssystem-bll-authentication-branchcontext"></a>
## `CampionsSystem.BLL.Authentication.BranchContext`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `name` | `string` | no | — |

<a id="campionssystem-bll-authentication-branchcontext"></a>
## `CampionsSystem.BLL.Authentication.BranchContext?`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `name` | `string` | no | — |

<a id="campionssystem-bll-authentication-currentusercontext"></a>
## `CampionsSystem.BLL.Authentication.CurrentUserContext`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `userId` | `Guid` | no | — |
| `userName` | `string?` | yes | — |
| `employeeId` | `Guid?` | yes | — |
| `employeeName` | `string?` | yes | — |
| `roles` | `IReadOnlyList<string>` | no | `System.Runtime.CompilerServices.NullableAttribute(1)` |
| `permissions` | `IReadOnlyList<string>` | no | `System.Runtime.CompilerServices.NullableAttribute(1)` |
| `currentBranch` | `Authentication.BranchContext?` | yes | — |
| `currentBranchId` | `Guid?` | yes | — |

<a id="campionssystem-bll-authentication-loginrequest"></a>
## `CampionsSystem.BLL.Authentication.LoginRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `userNameOrEmail` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |
| `password` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |

<a id="campionssystem-bll-authentication-loginresponse"></a>
## `CampionsSystem.BLL.Authentication.LoginResponse`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `accessToken` | `string` | no | — |
| `expiresAt` | `DateTimeOffset` | no | — |
| `user` | `Authentication.LoginUser` | no | — |

<a id="campionssystem-bll-authentication-loginuser"></a>
## `CampionsSystem.BLL.Authentication.LoginUser`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `userName` | `string?` | yes | — |
| `employeeId` | `Guid?` | yes | — |
| `fullName` | `string?` | yes | — |
| `roles` | `IReadOnlyList<string>` | no | `System.Runtime.CompilerServices.NullableAttribute(1)` |

<a id="campionssystem-bll-authentication-permissionoverriderequest"></a>
## `CampionsSystem.BLL.Authentication.PermissionOverrideRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `effect` | `string?` | yes | `System.Text.Json.Serialization.JsonRequiredAttribute`<br>`System.ComponentModel.DataAnnotations.RegularExpressionAttribute("^(Allow\|Deny)$")` |
| `operationId` | `Guid` | no | — |
| `expectedVersion` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.StringLengthAttribute(64, MinimumLength = 64)` |
| `reason` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(2000)` |

<a id="campionssystem-bll-authentication-rolegrantrequest"></a>
## `CampionsSystem.BLL.Authentication.RoleGrantRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `granted` | `bool?` | yes | `System.ComponentModel.DataAnnotations.RequiredAttribute` |
| `operationId` | `Guid` | no | — |
| `expectedVersion` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.StringLengthAttribute(64, MinimumLength = 64)` |
| `reason` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(2000)` |

<a id="campionssystem-bll-authentication-rolemembershiprequest"></a>
## `CampionsSystem.BLL.Authentication.RoleMembershipRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `assigned` | `bool?` | yes | `System.ComponentModel.DataAnnotations.RequiredAttribute` |
| `operationId` | `Guid` | no | — |
| `expectedVersion` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.StringLengthAttribute(64, MinimumLength = 64)` |
| `reason` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(2000)` |

<a id="campionssystem-bll-branches-assignbranchrequest"></a>
## `CampionsSystem.BLL.Branches.AssignBranchRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `branchId` | `Guid` | no | — |
| `startDate` | `DateOnly` | no | — |

<a id="campionssystem-bll-branches-branchassignmentdetails"></a>
## `CampionsSystem.BLL.Branches.BranchAssignmentDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `employeeId` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `branchName` | `string` | no | — |
| `startDate` | `DateOnly` | no | — |
| `endDate` | `DateOnly?` | yes | — |
| `isPrimary` | `bool` | no | — |
| `assignedByUserId` | `Guid` | no | — |
| `createdAt` | `DateTimeOffset` | no | — |

<a id="campionssystem-bll-branches-branchassignmentdetails"></a>
## `CampionsSystem.BLL.Branches.BranchAssignmentDetails?`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `employeeId` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `branchName` | `string` | no | — |
| `startDate` | `DateOnly` | no | — |
| `endDate` | `DateOnly?` | yes | — |
| `isPrimary` | `bool` | no | — |
| `assignedByUserId` | `Guid` | no | — |
| `createdAt` | `DateTimeOffset` | no | — |

<a id="campionssystem-bll-branches-branchdetails"></a>
## `CampionsSystem.BLL.Branches.BranchDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `name` | `string` | no | `System.Runtime.CompilerServices.NullableAttribute(1)` |
| `cityId` | `Guid` | no | — |
| `cityName` | `string` | no | `System.Runtime.CompilerServices.NullableAttribute(1)` |
| `address` | `string?` | yes | — |
| `phone` | `string?` | yes | — |
| `isActive` | `bool` | no | — |
| `maxBorrowedGymSessions` | `int` | no | — |
| `quickRefundWindowHours` | `int` | no | — |
| `countryCallingCode` | `string?` | yes | — |

<a id="campionssystem-bll-branches-savebranchrequest"></a>
## `CampionsSystem.BLL.Branches.SaveBranchRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `name` | `string` | no | `System.Runtime.CompilerServices.NullableAttribute(1)`<br>`System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |
| `cityId` | `Guid` | no | — |
| `address` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |
| `phone` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)` |
| `isActive` | `bool` | no | — |
| `maxBorrowedGymSessions` | `int` | no | `System.ComponentModel.DataAnnotations.RangeAttribute(0, 2147483647)` |
| `quickRefundWindowHours` | `int` | no | `System.ComponentModel.DataAnnotations.RangeAttribute(0, 2147483647)` |
| `countryCallingCode` | `string?` | yes | `System.ComponentModel.DataAnnotations.RegularExpressionAttribute("^[+][1-9][0-9]{0,2}$")` |

<a id="campionssystem-bll-branches-transferbranchrequest"></a>
## `CampionsSystem.BLL.Branches.TransferBranchRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `targetBranchId` | `Guid` | no | — |
| `effectiveDate` | `DateOnly` | no | — |

<a id="campionssystem-bll-checkins-accessdecision"></a>
## `CampionsSystem.BLL.CheckIns.AccessDecision`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `allowed` | `bool` | no | — |
| `reasonCode` | `string` | no | `System.Runtime.CompilerServices.NullableAttribute(1)` |
| `memberId` | `Guid?` | yes | — |
| `memberCode` | `string?` | yes | — |
| `memberName` | `string?` | yes | — |
| `branchId` | `Guid` | no | — |
| `businessDate` | `DateOnly` | no | — |
| `businessDayId` | `Guid?` | yes | — |
| `subscriptionId` | `Guid?` | yes | — |
| `subscriptionMode` | `string?` | yes | — |
| `remainingSessions` | `int?` | yes | — |
| `borrowAvailable` | `bool` | no | — |
| `borrowedUsed` | `int` | no | — |
| `borrowedRemaining` | `int` | no | — |
| `canActivateOnCheckIn` | `bool` | no | — |

<a id="campionssystem-bll-checkins-accessrequest"></a>
## `CampionsSystem.BLL.CheckIns.AccessRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `memberId` | `Guid?` | yes | — |
| `memberCode` | `string?` | yes | — |
| `branchId` | `Guid?` | yes | — |

<a id="campionssystem-bll-checkins-borroweddebtdetails"></a>
## `CampionsSystem.BLL.CheckIns.BorrowedDebtDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `expiredSubscriptionId` | `Guid` | no | — |
| `checkInId` | `Guid?` | yes | — |
| `branchId` | `Guid` | no | — |
| `borrowedAt` | `DateTimeOffset` | no | — |
| `status` | `string` | no | — |
| `settledBySubscriptionId` | `Guid?` | yes | — |
| `settledByTransactionId` | `Guid?` | yes | — |
| `settledAt` | `DateTimeOffset?` | yes | — |
| `settlementHistory` | `IReadOnlyList<CheckIns.BorrowedSettlementDetails>?` | yes | `System.Runtime.CompilerServices.NullableAttribute({2, 1})` |

<a id="campionssystem-bll-checkins-borroweddebthistory"></a>
## `CampionsSystem.BLL.CheckIns.BorrowedDebtHistory`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<CheckIns.BorrowedDebtDetails>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-checkins-borroweddebthistoryrequest"></a>
## `CampionsSystem.BLL.CheckIns.BorrowedDebtHistoryRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `branchId` | `Guid?` | yes | — |
| `expiredSubscriptionId` | `Guid?` | yes | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-checkins-borrowedsettlementdetails"></a>
## `CampionsSystem.BLL.CheckIns.BorrowedSettlementDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `sequence` | `int` | no | — |
| `action` | `string` | no | — |
| `grantId` | `Guid` | no | — |
| `refundId` | `Guid?` | yes | — |
| `createdAt` | `DateTimeOffset` | no | — |

<a id="campionssystem-bll-checkins-checkindetails"></a>
## `CampionsSystem.BLL.CheckIns.CheckInDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `operationId` | `Guid?` | yes | — |
| `memberId` | `Guid?` | yes | — |
| `memberCode` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `memberName` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `branchId` | `Guid` | no | — |
| `businessDate` | `DateOnly?` | yes | — |
| `businessDayId` | `Guid?` | yes | — |
| `occurredAt` | `DateTimeOffset` | no | — |
| `type` | `string` | no | — |
| `method` | `string` | no | — |
| `subscriptionId` | `Guid?` | yes | — |
| `sessionConsumed` | `bool` | no | — |
| `sessionsBefore` | `int?` | yes | — |
| `sessionsAfter` | `int?` | yes | — |
| `actorId` | `Guid` | no | — |

<a id="campionssystem-bll-checkins-checkindetails"></a>
## `CampionsSystem.BLL.CheckIns.CheckInDetails?`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `operationId` | `Guid?` | yes | — |
| `memberId` | `Guid?` | yes | — |
| `memberCode` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `memberName` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `branchId` | `Guid` | no | — |
| `businessDate` | `DateOnly?` | yes | — |
| `businessDayId` | `Guid?` | yes | — |
| `occurredAt` | `DateTimeOffset` | no | — |
| `type` | `string` | no | — |
| `method` | `string` | no | — |
| `subscriptionId` | `Guid?` | yes | — |
| `sessionConsumed` | `bool` | no | — |
| `sessionsBefore` | `int?` | yes | — |
| `sessionsAfter` | `int?` | yes | — |
| `actorId` | `Guid` | no | — |

<a id="campionssystem-bll-checkins-checkinhistory"></a>
## `CampionsSystem.BLL.CheckIns.CheckInHistory`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<CheckIns.CheckInDetails>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-checkins-checkinhistoryrequest"></a>
## `CampionsSystem.BLL.CheckIns.CheckInHistoryRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `branchId` | `Guid?` | yes | — |
| `memberId` | `Guid?` | yes | — |
| `from` | `DateOnly?` | yes | — |
| `to` | `DateOnly?` | yes | — |
| `type` | `Enums.CheckInType?` | yes | — |
| `method` | `Enums.CheckInMethod?` | yes | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-checkins-checkinmembermatch"></a>
## `CampionsSystem.BLL.CheckIns.CheckInMemberMatch`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `memberCode` | `string` | no | — |
| `name` | `string` | no | — |
| `isActive` | `bool` | no | — |
| `registrationBranchId` | `Guid` | no | — |
| `clientCode` | `long?` | yes | — |

<a id="campionssystem-bll-checkins-checkinmembermatches"></a>
## `CampionsSystem.BLL.CheckIns.CheckInMemberMatches`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<CheckIns.CheckInMemberMatch>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-checkins-checkinmembersearchrequest"></a>
## `CampionsSystem.BLL.CheckIns.CheckInMemberSearchRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `search` | `string` | no | — |
| `branchId` | `Guid?` | yes | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-checkins-checkinresult"></a>
## `CampionsSystem.BLL.CheckIns.CheckInResult`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `decision` | `CheckIns.AccessDecision` | no | — |
| `checkIn` | `CheckIns.CheckInDetails?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |

<a id="campionssystem-bll-checkins-createcheckinrequest"></a>
## `CampionsSystem.BLL.CheckIns.CreateCheckInRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `method` | `Enums.CheckInMethod` | no | — |
| `memberId` | `Guid?` | yes | — |
| `memberCode` | `string?` | yes | — |
| `confirmBorrow` | `bool` | no | — |
| `branchId` | `Guid?` | yes | — |

<a id="campionssystem-bll-checkins-redeemtrialrequest"></a>
## `CampionsSystem.BLL.CheckIns.RedeemTrialRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `invitationId` | `Guid` | no | — |
| `phone` | `string` | no | — |
| `branchId` | `Guid?` | yes | — |

<a id="campionssystem-bll-checkins-trialcheckinresult"></a>
## `CampionsSystem.BLL.CheckIns.TrialCheckInResult`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `allowed` | `bool` | no | — |
| `reasonCode` | `string` | no | — |
| `invitationId` | `Guid` | no | — |
| `checkIn` | `CheckIns.CheckInDetails?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |

<a id="campionssystem-bll-coaching-classattendancedetails"></a>
## `CampionsSystem.BLL.Coaching.ClassAttendanceDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `memberId` | `Guid` | no | — |
| `subscriptionId` | `Guid` | no | — |
| `classSessionId` | `Guid` | no | — |
| `coachEmployeeId` | `Guid` | no | — |
| `businessDate` | `DateOnly` | no | — |
| `remainingSessions` | `int` | no | — |
| `registeredAt` | `DateTimeOffset` | no | — |

<a id="campionssystem-bll-coaching-classattendancerequest"></a>
## `CampionsSystem.BLL.Coaching.ClassAttendanceRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `memberId` | `Guid` | no | — |
| `classesSubscriptionId` | `Guid?` | yes | — |

<a id="campionssystem-bll-coaching-classcoachstatistics"></a>
## `CampionsSystem.BLL.Coaching.ClassCoachStatistics`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `coachEmployeeId` | `Guid` | no | — |
| `completedClasses` | `int` | no | — |
| `totalAttendance` | `int` | no | — |
| `averageAttendance` | `decimal` | no | — |
| `target` | `decimal?` | yes | — |
| `targetReached` | `bool?` | yes | — |
| `occurrences` | `IReadOnlyList<Coaching.ClassOccurrenceDetails>` | no | — |
| `compensationStatus` | `string` | no | — |

<a id="campionssystem-bll-coaching-classoccurrencedetails"></a>
## `CampionsSystem.BLL.Coaching.ClassOccurrenceDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `classTypeId` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `coachEmployeeId` | `Guid` | no | — |
| `businessDate` | `DateOnly?` | yes | — |
| `startsAt` | `DateTimeOffset` | no | — |
| `endedAt` | `DateTimeOffset?` | yes | — |
| `status` | `string` | no | — |
| `attendance` | `int` | no | — |
| `version` | `int` | no | — |

<a id="campionssystem-bll-coaching-classoccurrencerequest"></a>
## `CampionsSystem.BLL.Coaching.ClassOccurrenceRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `classTypeId` | `Guid` | no | — |
| `coachEmployeeId` | `Guid` | no | — |
| `startsAt` | `DateTimeOffset` | no | — |

<a id="campionssystem-bll-coaching-classtypedetails"></a>
## `CampionsSystem.BLL.Coaching.ClassTypeDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `name` | `string` | no | — |
| `coachEmployeeId` | `Guid` | no | — |
| `isActive` | `bool` | no | — |
| `description` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |

<a id="campionssystem-bll-coaching-classtyperequest"></a>
## `CampionsSystem.BLL.Coaching.ClassTypeRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `branchId` | `Guid` | no | — |
| `name` | `string` | no | — |
| `coachEmployeeId` | `Guid` | no | — |
| `isActive` | `bool` | no | — |
| `description` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |

<a id="campionssystem-bll-coaching-classessubscriptiondetails"></a>
## `CampionsSystem.BLL.Coaching.ClassesSubscriptionDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `memberId` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `saleId` | `Guid` | no | — |
| `packageName` | `string` | no | — |
| `startDate` | `DateOnly` | no | — |
| `endDate` | `DateOnly` | no | — |
| `purchasedSessions` | `int` | no | — |
| `remainingSessions` | `int` | no | — |
| `price` | `decimal` | no | — |
| `cancelled` | `bool` | no | — |
| `version` | `int` | no | — |

<a id="campionssystem-bll-coaching-coachbalance"></a>
## `CampionsSystem.BLL.Coaching.CoachBalance`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `branchId` | `Guid` | no | — |
| `coachEmployeeId` | `Guid` | no | — |
| `earned` | `decimal` | no | — |
| `paid` | `decimal` | no | — |
| `outstanding` | `decimal` | no | — |

<a id="campionssystem-bll-coaching-coachearningdetails"></a>
## `CampionsSystem.BLL.Coaching.CoachEarningDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `coachEmployeeId` | `Guid` | no | — |
| `periodId` | `Guid` | no | — |
| `saleId` | `Guid` | no | — |
| `sessionId` | `Guid?` | yes | — |
| `expirySettlementId` | `Guid?` | yes | — |
| `eligibleValue` | `decimal` | no | — |
| `rate` | `decimal` | no | — |
| `earned` | `decimal` | no | — |
| `transferredPackage` | `bool` | no | — |
| `businessDate` | `DateOnly` | no | — |

<a id="campionssystem-bll-coaching-coachearningsprogress"></a>
## `CampionsSystem.BLL.Coaching.CoachEarningsProgress`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `periodId` | `Guid` | no | — |
| `finalized` | `bool` | no | — |
| `coaches` | `IReadOnlyList<Coaching.CoachProgress>` | no | — |

<a id="campionssystem-bll-coaching-coachpayoutdetails"></a>
## `CampionsSystem.BLL.Coaching.CoachPayoutDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `coachEmployeeId` | `Guid` | no | — |
| `expenseId` | `Guid` | no | — |
| `accountId` | `Guid` | no | — |
| `method` | `string` | no | — |
| `amount` | `decimal` | no | — |
| `balanceBefore` | `decimal` | no | — |
| `balanceAfter` | `decimal` | no | — |
| `businessDate` | `DateOnly` | no | — |
| `paidAt` | `DateTimeOffset` | no | — |
| `authorizedByUserId` | `Guid` | no | — |

<a id="campionssystem-bll-coaching-coachpayoutrequest"></a>
## `CampionsSystem.BLL.Coaching.CoachPayoutRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `coachEmployeeId` | `Guid` | no | — |
| `amount` | `decimal` | no | — |
| `financialAccountId` | `Guid` | no | — |
| `expenseCategoryId` | `Guid` | no | — |
| `reason` | `string` | no | — |
| `receiptReference` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |

<a id="campionssystem-bll-coaching-coachprogress"></a>
## `CampionsSystem.BLL.Coaching.CoachProgress`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `coachEmployeeId` | `Guid` | no | — |
| `eligibleValue` | `decimal` | no | — |
| `target` | `decimal?` | yes | — |
| `targetReached` | `bool?` | yes | — |
| `normalRate` | `decimal?` | yes | — |
| `projectedEarnings` | `decimal?` | yes | — |
| `deferredValue` | `decimal` | no | — |

<a id="campionssystem-bll-coaching-coachtargetdetails"></a>
## `CampionsSystem.BLL.Coaching.CoachTargetDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `periodId` | `Guid` | no | — |
| `coachEmployeeId` | `Guid` | no | — |
| `privateTarget` | `decimal?` | yes | — |
| `classesAverageTarget` | `decimal?` | yes | — |

<a id="campionssystem-bll-coaching-coachtargetrequest"></a>
## `CampionsSystem.BLL.Coaching.CoachTargetRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `commissionPeriodId` | `Guid` | no | — |
| `coachEmployeeId` | `Guid` | no | — |
| `privateTarget` | `decimal?` | yes | — |
| `classesAverageTarget` | `decimal?` | yes | — |

<a id="campionssystem-bll-coaching-coachingmemberresult"></a>
## `CampionsSystem.BLL.Coaching.CoachingMemberResult`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `memberCode` | `string` | no | — |
| `clientCode` | `long?` | yes | — |
| `name` | `string` | no | — |
| `phone` | `string` | no | — |

<a id="campionssystem-bll-coaching-coachingpackagedetails"></a>
## `CampionsSystem.BLL.Coaching.CoachingPackageDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `name` | `string` | no | — |
| `sessionCount` | `int` | no | — |
| `durationDays` | `int` | no | — |
| `price` | `decimal` | no | — |
| `isActive` | `bool` | no | — |

<a id="campionssystem-bll-coaching-coachingpackagerequest"></a>
## `CampionsSystem.BLL.Coaching.CoachingPackageRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `branchId` | `Guid` | no | — |
| `name` | `string` | no | — |
| `sessionCount` | `int` | no | — |
| `durationDays` | `int` | no | — |
| `price` | `decimal` | no | — |
| `isActive` | `bool` | no | — |

<a id="campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-classattendancedetails"></a>
## `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.ClassAttendanceDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Coaching.ClassAttendanceDetails>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-classoccurrencedetails"></a>
## `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.ClassOccurrenceDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Coaching.ClassOccurrenceDetails>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-classtypedetails"></a>
## `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.ClassTypeDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Coaching.ClassTypeDetails>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-classessubscriptiondetails"></a>
## `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.ClassesSubscriptionDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Coaching.ClassesSubscriptionDetails>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-coachearningdetails"></a>
## `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.CoachEarningDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Coaching.CoachEarningDetails>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-coachpayoutdetails"></a>
## `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.CoachPayoutDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Coaching.CoachPayoutDetails>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-coachingmemberresult"></a>
## `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.CoachingMemberResult>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Coaching.CoachingMemberResult>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-coachingpackagedetails"></a>
## `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.CoachingPackageDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Coaching.CoachingPackageDetails>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-privateattendancedetails"></a>
## `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.PrivateAttendanceDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Coaching.PrivateAttendanceDetails>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-privatedetails"></a>
## `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.PrivateDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Coaching.PrivateDetails>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-privateexpirydetails"></a>
## `CampionsSystem.BLL.Coaching.CoachingPage<CampionsSystem.BLL.Coaching.PrivateExpiryDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Coaching.PrivateExpiryDetails>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-coaching-coachingpurchasereceipt"></a>
## `CampionsSystem.BLL.Coaching.CoachingPurchaseReceipt`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `subscriptionId` | `Guid` | no | — |
| `saleId` | `Guid` | no | — |
| `paymentIds` | `IReadOnlyList<Guid>` | no | — |

<a id="campionssystem-bll-coaching-coachingpurchaserequest"></a>
## `CampionsSystem.BLL.Coaching.CoachingPurchaseRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `memberId` | `Guid` | no | — |
| `packageId` | `Guid` | no | — |
| `paymentParts` | `IReadOnlyList<Sales.PaymentPartInput>` | no | — |
| `coachEmployeeId` | `Guid?` | yes | — |
| `purchaseMode` | `Enums.PurchaseMode` | no | — |
| `installments` | `IReadOnlyList<Sales.InstallmentLineInput>?` | yes | `System.Runtime.CompilerServices.NullableAttribute({2, 1})` |

<a id="campionssystem-bll-coaching-completeclassrequest"></a>
## `CampionsSystem.BLL.Coaching.CompleteClassRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `expectedVersion` | `int` | no | — |
| `cancelled` | `bool` | no | — |

<a id="campionssystem-bll-coaching-privateassignmentdetails"></a>
## `CampionsSystem.BLL.Coaching.PrivateAssignmentDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `coachEmployeeId` | `Guid` | no | — |
| `assignedAt` | `DateTimeOffset` | no | — |
| `endedAt` | `DateTimeOffset?` | yes | — |
| `reason` | `string?` | yes | — |

<a id="campionssystem-bll-coaching-privateattendancedetails"></a>
## `CampionsSystem.BLL.Coaching.PrivateAttendanceDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `subscriptionId` | `Guid` | no | — |
| `coachEmployeeId` | `Guid` | no | — |
| `gymCheckInId` | `Guid` | no | — |
| `businessDate` | `DateOnly?` | yes | — |
| `sessionAt` | `DateTimeOffset` | no | — |
| `sessionNumber` | `int?` | yes | — |
| `sessionValue` | `decimal?` | yes | — |
| `transferredPackage` | `bool?` | yes | — |

<a id="campionssystem-bll-coaching-privateattendancerequest"></a>
## `CampionsSystem.BLL.Coaching.PrivateAttendanceRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `gymCheckInId` | `Guid` | no | — |
| `notes` | `string?` | yes | — |

<a id="campionssystem-bll-coaching-privatedetails"></a>
## `CampionsSystem.BLL.Coaching.PrivateDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `memberId` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `startDate` | `DateOnly` | no | — |
| `endDate` | `DateOnly` | no | — |
| `purchasedSessions` | `int` | no | — |
| `usedSessions` | `int` | no | — |
| `remainingSessions` | `int` | no | — |
| `version` | `int` | no | — |
| `cancelled` | `bool` | no | — |
| `packageName` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `price` | `decimal` | no | — |
| `unitSessionValue` | `decimal` | no | — |
| `remainderValue` | `decimal` | no | — |
| `originalCoachEmployeeId` | `Guid?` | yes | — |
| `assignments` | `IReadOnlyList<Coaching.PrivateAssignmentDetails>` | no | — |

<a id="campionssystem-bll-coaching-privateexpirydetails"></a>
## `CampionsSystem.BLL.Coaching.PrivateExpiryDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `subscriptionId` | `Guid` | no | — |
| `coachEmployeeId` | `Guid` | no | — |
| `unusedSessions` | `int` | no | — |
| `eligibleValue` | `decimal` | no | — |
| `transferredPackage` | `bool?` | yes | — |
| `businessDate` | `DateOnly?` | yes | — |
| `settledAt` | `DateTimeOffset` | no | — |
| `reason` | `string?` | yes | — |

<a id="campionssystem-bll-coaching-privateexpiryrequest"></a>
## `CampionsSystem.BLL.Coaching.PrivateExpiryRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `commissionPeriodId` | `Guid` | no | — |
| `reason` | `string` | no | — |

<a id="campionssystem-bll-coaching-privatetransferrequest"></a>
## `CampionsSystem.BLL.Coaching.PrivateTransferRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `coachEmployeeId` | `Guid` | no | — |
| `expectedVersion` | `int` | no | — |
| `reason` | `string` | no | — |

<a id="campionssystem-bll-coaching-recognizecoachearningsrequest"></a>
## `CampionsSystem.BLL.Coaching.RecognizeCoachEarningsRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `commissionPeriodId` | `Guid` | no | — |

<a id="campionssystem-bll-commissions-categoryruleinput"></a>
## `CampionsSystem.BLL.Commissions.CategoryRuleInput`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `target` | `decimal` | no | — |
| `belowRate` | `decimal` | no | — |
| `reachedRate` | `decimal` | no | — |

<a id="campionssystem-bll-commissions-commissioncategoryprogress"></a>
## `CampionsSystem.BLL.Commissions.CommissionCategoryProgress`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `category` | `string` | no | — |
| `grossCollections` | `decimal` | no | — |
| `refunds` | `decimal` | no | — |
| `eligibleCollections` | `decimal` | no | — |
| `configured` | `bool` | no | — |
| `target` | `decimal?` | yes | — |
| `targetReached` | `bool` | no | — |
| `belowRate` | `decimal?` | yes | — |
| `reachedRate` | `decimal?` | yes | — |
| `appliedRate` | `decimal?` | yes | — |
| `commission` | `decimal` | no | — |
| `ruleId` | `Guid?` | yes | — |
| `retainedCollections` | `decimal` | no | — |

<a id="campionssystem-bll-commissions-commissionpage-campionssystem-bll-commissions-commissionperioddetails"></a>
## `CampionsSystem.BLL.Commissions.CommissionPage<CampionsSystem.BLL.Commissions.CommissionPeriodDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Commissions.CommissionPeriodDetails>` | no | — |
| `total` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-commissions-commissionpage-campionssystem-bll-commissions-commissionrefundsource"></a>
## `CampionsSystem.BLL.Commissions.CommissionPage<CampionsSystem.BLL.Commissions.CommissionRefundSource>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Commissions.CommissionRefundSource>` | no | — |
| `total` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-commissions-commissionpage-campionssystem-bll-commissions-commissionruledetails"></a>
## `CampionsSystem.BLL.Commissions.CommissionPage<CampionsSystem.BLL.Commissions.CommissionRuleDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Commissions.CommissionRuleDetails>` | no | — |
| `total` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-commissions-commissionpage-campionssystem-bll-commissions-commissionsourceline"></a>
## `CampionsSystem.BLL.Commissions.CommissionPage<CampionsSystem.BLL.Commissions.CommissionSourceLine>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Commissions.CommissionSourceLine>` | no | — |
| `total` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-commissions-commissionperioddetails"></a>
## `CampionsSystem.BLL.Commissions.CommissionPeriodDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `name` | `string` | no | — |
| `startDate` | `DateOnly?` | yes | — |
| `endDate` | `DateOnly?` | yes | — |
| `status` | `string` | no | — |
| `version` | `int` | no | — |
| `finalizedAt` | `DateTimeOffset?` | yes | — |

<a id="campionssystem-bll-commissions-commissionprogress"></a>
## `CampionsSystem.BLL.Commissions.CommissionProgress`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `period` | `Commissions.CommissionPeriodDetails` | no | — |
| `finalized` | `bool` | no | — |
| `grossCollections` | `decimal` | no | — |
| `refunds` | `decimal` | no | — |
| `retainedCollections` | `decimal` | no | — |
| `employeeEligibleCollections` | `decimal` | no | — |
| `houseCollections` | `decimal` | no | — |
| `houseNewClientCollections` | `decimal` | no | — |
| `houseRenewalCollections` | `decimal` | no | — |
| `excludedCollections` | `decimal` | no | — |
| `newClientEligibleCollections` | `decimal` | no | — |
| `renewalEligibleCollections` | `decimal` | no | — |
| `totalCommission` | `decimal` | no | — |
| `employeeCount` | `int` | no | — |
| `employees` | `IReadOnlyList<Commissions.EmployeeCommissionStatement>` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-commissions-commissionrefundsource"></a>
## `CampionsSystem.BLL.Commissions.CommissionRefundSource`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `refundId` | `Guid` | no | — |
| `paymentId` | `Guid` | no | — |
| `amount` | `decimal` | no | — |
| `collectionBusinessDate` | `DateOnly?` | yes | — |
| `refundBusinessDate` | `DateOnly?` | yes | — |
| `refundedAt` | `DateTimeOffset` | no | — |

<a id="campionssystem-bll-commissions-commissionruledetails"></a>
## `CampionsSystem.BLL.Commissions.CommissionRuleDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `employeeId` | `Guid?` | yes | — |
| `branchId` | `Guid` | no | — |
| `category` | `string` | no | — |
| `effectiveFrom` | `DateOnly` | no | — |
| `target` | `decimal?` | yes | — |
| `belowRate` | `decimal?` | yes | — |
| `reachedRate` | `decimal?` | yes | — |
| `revision` | `int` | no | — |

<a id="campionssystem-bll-commissions-commissionsourceline"></a>
## `CampionsSystem.BLL.Commissions.CommissionSourceLine`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `paymentId` | `Guid` | no | — |
| `saleId` | `Guid` | no | — |
| `memberId` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `originalSalesEmployeeId` | `Guid?` | yes | — |
| `employeeId` | `Guid?` | yes | — |
| `category` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `eligibilityReason` | `string` | no | — |
| `businessDate` | `DateOnly?` | yes | — |
| `paidAt` | `DateTimeOffset` | no | — |
| `grossCollection` | `decimal` | no | — |
| `refunds` | `decimal` | no | — |
| `retainedCollection` | `decimal` | no | — |
| `eligibleCollection` | `decimal` | no | — |

<a id="campionssystem-bll-commissions-configurecommissionrequest"></a>
## `CampionsSystem.BLL.Commissions.ConfigureCommissionRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `employeeId` | `Guid?` | yes | — |
| `effectiveFrom` | `DateOnly` | no | — |
| `newClients` | `Commissions.CategoryRuleInput` | no | — |
| `renewals` | `Commissions.CategoryRuleInput` | no | — |
| `branchId` | `Guid?` | yes | — |

<a id="campionssystem-bll-commissions-employeecommissionstatement"></a>
## `CampionsSystem.BLL.Commissions.EmployeeCommissionStatement`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `employeeId` | `Guid` | no | — |
| `employeeName` | `string` | no | — |
| `newClients` | `Commissions.CommissionCategoryProgress` | no | — |
| `renewals` | `Commissions.CommissionCategoryProgress` | no | — |
| `totalCommission` | `decimal` | no | — |
| `periodId` | `Guid` | no | — |
| `finalized` | `bool` | no | — |
| `houseCollections` | `decimal` | no | — |
| `excludedCollections` | `decimal` | no | — |

<a id="campionssystem-bll-commissions-finalizecommissionperiodrequest"></a>
## `CampionsSystem.BLL.Commissions.FinalizeCommissionPeriodRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `expectedVersion` | `int` | no | — |

<a id="campionssystem-bll-commissions-opencommissionperiodrequest"></a>
## `CampionsSystem.BLL.Commissions.OpenCommissionPeriodRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `name` | `string` | no | — |
| `startDate` | `DateOnly` | no | — |
| `endDate` | `DateOnly` | no | — |
| `branchId` | `Guid?` | yes | — |

<a id="campionssystem-bll-crm-convertleadrequest"></a>
## `CampionsSystem.BLL.Crm.ConvertLeadRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `targetRegistrationBranchId` | `Guid` | no | — |
| `freelancePlanId` | `Guid?` | yes | — |
| `fullName` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |
| `primaryPhone` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)` |
| `secondaryPhone` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)` |
| `dateOfBirth` | `DateOnly?` | yes | — |
| `gender` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(32)` |
| `notes` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(4096)` |

<a id="campionssystem-bll-crm-createleadrequest"></a>
## `CampionsSystem.BLL.Crm.CreateLeadRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `branchId` | `Guid` | no | — |
| `fullName` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |
| `primaryPhone` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)` |
| `secondaryPhone` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)` |
| `email` | `string?` | yes | `System.ComponentModel.DataAnnotations.EmailAddressAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(320)` |
| `source` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |
| `notes` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(4096)` |

<a id="campionssystem-bll-crm-followupitem"></a>
## `CampionsSystem.BLL.Crm.FollowUpItem`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `subjectType` | `string` | no | — |
| `leadId` | `Guid?` | yes | — |
| `memberId` | `Guid?` | yes | — |
| `name` | `string` | no | — |
| `phone` | `string` | no | — |
| `branchId` | `Guid` | no | — |
| `branchName` | `string` | no | — |
| `reason` | `string` | no | — |
| `dueAt` | `DateTimeOffset` | no | — |
| `crossBranch` | `bool` | no | — |

<a id="campionssystem-bll-crm-followuplistrequest"></a>
## `CampionsSystem.BLL.Crm.FollowUpListRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `window` | `Crm.FollowUpWindow?` | yes | — |
| `branchId` | `Guid?` | yes | — |
| `page` | `int` | no | `System.ComponentModel.DataAnnotations.RangeAttribute(1, 2147483647)` |
| `pageSize` | `int` | no | `System.ComponentModel.DataAnnotations.RangeAttribute(1, 100)` |

<a id="campionssystem-bll-crm-followuplistresult"></a>
## `CampionsSystem.BLL.Crm.FollowUpListResult`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Crm.FollowUpItem>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-crm-followupwindow"></a>
## `CampionsSystem.BLL.Crm.FollowUpWindow`
| Name | Numeric value |
|---|---:|
| `Overdue` | 0 |
| `Today` | 1 |
| `Upcoming` | 2 |

<a id="campionssystem-bll-crm-leadassignmentdetails"></a>
## `CampionsSystem.BLL.Crm.LeadAssignmentDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `salesEmployeeId` | `Guid` | no | — |
| `salesEmployeeName` | `string` | no | — |
| `assignedAt` | `DateTimeOffset` | no | — |
| `endedAt` | `DateTimeOffset?` | yes | — |
| `assignedByUserId` | `Guid` | no | — |
| `reason` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |

<a id="campionssystem-bll-crm-leadassignmentdetails"></a>
## `CampionsSystem.BLL.Crm.LeadAssignmentDetails?`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `salesEmployeeId` | `Guid` | no | — |
| `salesEmployeeName` | `string` | no | — |
| `assignedAt` | `DateTimeOffset` | no | — |
| `endedAt` | `DateTimeOffset?` | yes | — |
| `assignedByUserId` | `Guid` | no | — |
| `reason` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |

<a id="campionssystem-bll-crm-leadassignmentrequest"></a>
## `CampionsSystem.BLL.Crm.LeadAssignmentRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `salesEmployeeId` | `Guid` | no | — |
| `reason` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(2048)` |

<a id="campionssystem-bll-crm-leadcreationresult"></a>
## `CampionsSystem.BLL.Crm.LeadCreationResult`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `result` | `string` | no | `System.Runtime.CompilerServices.NullableAttribute(1)` |
| `lead` | `Crm.LeadDetails?` | yes | — |
| `existingMemberId` | `Guid?` | yes | — |
| `error` | `string?` | yes | — |
| `existingLeadId` | `Guid?` | yes | — |

<a id="campionssystem-bll-crm-leaddetails"></a>
## `CampionsSystem.BLL.Crm.LeadDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `branchName` | `string` | no | `System.Runtime.CompilerServices.NullableAttribute(1)` |
| `fullName` | `string?` | yes | — |
| `primaryPhone` | `string` | no | `System.Runtime.CompilerServices.NullableAttribute(1)` |
| `primaryPhoneNormalized` | `string?` | yes | — |
| `secondaryPhone` | `string?` | yes | — |
| `email` | `string?` | yes | — |
| `source` | `string?` | yes | — |
| `notes` | `string?` | yes | — |
| `status` | `string` | no | `System.Runtime.CompilerServices.NullableAttribute(1)` |
| `createdAt` | `DateTimeOffset` | no | — |
| `nextFollowUpAt` | `DateTimeOffset?` | yes | — |
| `currentAssignment` | `Crm.LeadAssignmentDetails?` | yes | — |
| `convertedMemberId` | `Guid?` | yes | — |
| `convertedAt` | `DateTimeOffset?` | yes | — |
| `lostAt` | `DateTimeOffset?` | yes | — |
| `lostReason` | `string?` | yes | — |

<a id="campionssystem-bll-crm-leaddetails"></a>
## `CampionsSystem.BLL.Crm.LeadDetails?`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `branchName` | `string` | no | `System.Runtime.CompilerServices.NullableAttribute(1)` |
| `fullName` | `string?` | yes | — |
| `primaryPhone` | `string` | no | `System.Runtime.CompilerServices.NullableAttribute(1)` |
| `primaryPhoneNormalized` | `string?` | yes | — |
| `secondaryPhone` | `string?` | yes | — |
| `email` | `string?` | yes | — |
| `source` | `string?` | yes | — |
| `notes` | `string?` | yes | — |
| `status` | `string` | no | `System.Runtime.CompilerServices.NullableAttribute(1)` |
| `createdAt` | `DateTimeOffset` | no | — |
| `nextFollowUpAt` | `DateTimeOffset?` | yes | — |
| `currentAssignment` | `Crm.LeadAssignmentDetails?` | yes | — |
| `convertedMemberId` | `Guid?` | yes | — |
| `convertedAt` | `DateTimeOffset?` | yes | — |
| `lostAt` | `DateTimeOffset?` | yes | — |
| `lostReason` | `string?` | yes | — |

<a id="campionssystem-bll-crm-leadinteractiondetails"></a>
## `CampionsSystem.BLL.Crm.LeadInteractionDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `employeeId` | `Guid` | no | — |
| `createdByUserId` | `Guid?` | yes | — |
| `interactionType` | `string` | no | — |
| `notes` | `string` | no | — |
| `outcome` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `contactedAt` | `DateTimeOffset` | no | — |
| `nextFollowUpAt` | `DateTimeOffset?` | yes | — |
| `createdAt` | `DateTimeOffset` | no | — |

<a id="campionssystem-bll-crm-leadinteractionrequest"></a>
## `CampionsSystem.BLL.Crm.LeadInteractionRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `interactionType` | `Enums.InteractionType` | no | `System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter<CampionsSystem.DAL.Enums.InteractionType>))` |
| `notes` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(4096)` |
| `outcome` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |
| `contactedAt` | `DateTimeOffset?` | yes | — |
| `nextFollowUpAt` | `DateTimeOffset?` | yes | — |
| `status` | `Enums.LeadStatus` | no | `System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter<CampionsSystem.DAL.Enums.LeadStatus>))` |

<a id="campionssystem-bll-crm-leadlistrequest"></a>
## `CampionsSystem.BLL.Crm.LeadListRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `search` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |
| `branchId` | `Guid?` | yes | — |
| `salesEmployeeId` | `Guid?` | yes | — |
| `unassignedOnly` | `bool?` | yes | — |
| `status` | `Enums.LeadStatus?` | yes | — |
| `followUp` | `Crm.FollowUpWindow?` | yes | — |
| `page` | `int` | no | `System.ComponentModel.DataAnnotations.RangeAttribute(1, 2147483647)` |
| `pageSize` | `int` | no | `System.ComponentModel.DataAnnotations.RangeAttribute(1, 100)` |

<a id="campionssystem-bll-crm-leadlistresult"></a>
## `CampionsSystem.BLL.Crm.LeadListResult`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Crm.LeadDetails>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-crm-leadstatusrequest"></a>
## `CampionsSystem.BLL.Crm.LeadStatusRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `status` | `Enums.LeadStatus?` | yes | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter<CampionsSystem.DAL.Enums.LeadStatus>))` |
| `reason` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(2048)` |
| `nextFollowUpAt` | `DateTimeOffset?` | yes | — |

<a id="campionssystem-bll-crm-memberfollowuprequest"></a>
## `CampionsSystem.BLL.Crm.MemberFollowUpRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `reason` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute` |
| `dueAt` | `DateTimeOffset` | no | — |
| `notes` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(4096)` |

<a id="campionssystem-bll-crm-updateleadrequest"></a>
## `CampionsSystem.BLL.Crm.UpdateLeadRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `fullName` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |
| `primaryPhone` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)` |
| `secondaryPhone` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)` |
| `email` | `string?` | yes | `System.ComponentModel.DataAnnotations.EmailAddressAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(320)` |
| `source` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |
| `notes` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(4096)` |

<a id="campionssystem-bll-employees-changeemployeestatusrequest"></a>
## `CampionsSystem.BLL.Employees.ChangeEmployeeStatusRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `status` | `Enums.EmploymentStatus?` | yes | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.EnumDataTypeAttribute(typeof(CampionsSystem.DAL.Enums.EmploymentStatus))`<br>`System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter<CampionsSystem.DAL.Enums.EmploymentStatus>))` |
| `reason` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(2048)` |

<a id="campionssystem-bll-employees-createemployeerequest"></a>
## `CampionsSystem.BLL.Employees.CreateEmployeeRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `initialBranchId` | `Guid?` | yes | — |
| `fullName` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |
| `phone` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)` |
| `jobTitle` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |
| `employmentType` | `Enums.EmploymentType` | no | `System.ComponentModel.DataAnnotations.EnumDataTypeAttribute(typeof(CampionsSystem.DAL.Enums.EmploymentType))`<br>`System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter<CampionsSystem.DAL.Enums.EmploymentType>))` |
| `hireDate` | `DateOnly` | no | — |

<a id="campionssystem-bll-employees-employeedetails"></a>
## `CampionsSystem.BLL.Employees.EmployeeDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `fullName` | `string` | no | — |
| `phone` | `string` | no | — |
| `jobTitle` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `employmentType` | `string` | no | — |
| `status` | `string` | no | — |
| `hireDate` | `DateOnly` | no | — |
| `terminationDate` | `DateOnly?` | yes | — |
| `currentBranch` | `Authentication.BranchContext?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `hasSystemAccount` | `bool` | no | — |

<a id="campionssystem-bll-employees-employeelistrequest"></a>
## `CampionsSystem.BLL.Employees.EmployeeListRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `search` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |
| `branchId` | `Guid?` | yes | — |
| `status` | `Enums.EmploymentStatus?` | yes | — |
| `employmentType` | `Enums.EmploymentType?` | yes | — |
| `page` | `int` | no | `System.ComponentModel.DataAnnotations.RangeAttribute(1, 2147483647)` |
| `pageSize` | `int` | no | `System.ComponentModel.DataAnnotations.RangeAttribute(1, 100)` |

<a id="campionssystem-bll-employees-employeelistresult"></a>
## `CampionsSystem.BLL.Employees.EmployeeListResult`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Employees.EmployeeDetails>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-employees-employeeprofilerequest"></a>
## `CampionsSystem.BLL.Employees.EmployeeProfileRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `fullName` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |
| `phone` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)` |
| `jobTitle` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |
| `employmentType` | `Enums.EmploymentType` | no | `System.ComponentModel.DataAnnotations.EnumDataTypeAttribute(typeof(CampionsSystem.DAL.Enums.EmploymentType))`<br>`System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter<CampionsSystem.DAL.Enums.EmploymentType>))` |
| `hireDate` | `DateOnly` | no | — |

<a id="campionssystem-bll-members-changememberstatusrequest"></a>
## `CampionsSystem.BLL.Members.ChangeMemberStatusRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `isActive` | `bool?` | yes | `System.ComponentModel.DataAnnotations.RequiredAttribute` |
| `reason` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(2048)` |

<a id="campionssystem-bll-members-creatememberrequest"></a>
## `CampionsSystem.BLL.Members.CreateMemberRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `targetRegistrationBranchId` | `Guid` | no | — |
| `creationType` | `Members.MemberCreationType?` | yes | `System.ComponentModel.DataAnnotations.RequiredAttribute` |
| `salesEmployeeId` | `Guid?` | yes | — |
| `freelancePlanId` | `Guid?` | yes | — |
| `fullName` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |
| `primaryPhone` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)` |
| `secondaryPhone` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)` |
| `dateOfBirth` | `DateOnly?` | yes | — |
| `gender` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(32)` |
| `notes` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(4096)` |

<a id="campionssystem-bll-members-eligiblefreelanceplan"></a>
## `CampionsSystem.BLL.Members.EligibleFreelancePlan`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `planId` | `Guid` | no | — |
| `planName` | `string` | no | — |
| `coachName` | `string` | no | — |

<a id="campionssystem-bll-members-membercreationtype"></a>
## `CampionsSystem.BLL.Members.MemberCreationType`
| Name | Numeric value |
|---|---:|
| `NormalSales` | 0 |
| `FreelanceHouse` | 1 |
| `ManagementCreated` | 2 |

<a id="campionssystem-bll-members-memberdetails"></a>
## `CampionsSystem.BLL.Members.MemberDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `clientCode` | `long?` | yes | — |
| `memberCode` | `string` | no | `System.Runtime.CompilerServices.NullableAttribute(1)` |
| `fullName` | `string` | no | `System.Runtime.CompilerServices.NullableAttribute(1)` |
| `primaryPhone` | `string` | no | `System.Runtime.CompilerServices.NullableAttribute(1)` |
| `primaryPhoneNormalized` | `string?` | yes | — |
| `secondaryPhone` | `string?` | yes | — |
| `isPrimaryPhoneVerified` | `bool` | no | — |
| `dateOfBirth` | `DateOnly?` | yes | — |
| `gender` | `string?` | yes | — |
| `notes` | `string?` | yes | — |
| `isActive` | `bool` | no | — |
| `registrationBranch` | `Authentication.BranchContext` | no | `System.Runtime.CompilerServices.NullableAttribute(1)` |
| `createdAt` | `DateTimeOffset` | no | — |
| `currentSalesOwner` | `Members.SalesAssignmentDetails?` | yes | — |
| `employee` | `Members.MemberEmployeeLink?` | yes | — |
| `eligibleFreelancePlans` | `IReadOnlyList<Members.EligibleFreelancePlan>` | no | `System.Runtime.CompilerServices.NullableAttribute(1)` |
| `isHouse` | `bool` | no | — |

<a id="campionssystem-bll-members-memberemployeelink"></a>
## `CampionsSystem.BLL.Members.MemberEmployeeLink?`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `fullName` | `string` | no | — |

<a id="campionssystem-bll-members-memberlistrequest"></a>
## `CampionsSystem.BLL.Members.MemberListRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `search` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |
| `registrationBranchId` | `Guid?` | yes | — |
| `clientCode` | `long?` | yes | — |
| `isActive` | `bool?` | yes | — |
| `salesEmployeeId` | `Guid?` | yes | — |
| `houseOnly` | `bool?` | yes | — |
| `page` | `int` | no | `System.ComponentModel.DataAnnotations.RangeAttribute(1, 2147483647)` |
| `pageSize` | `int` | no | `System.ComponentModel.DataAnnotations.RangeAttribute(1, 100)` |

<a id="campionssystem-bll-members-memberlistresult"></a>
## `CampionsSystem.BLL.Members.MemberListResult`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Members.MemberSummary>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-members-membersummary"></a>
## `CampionsSystem.BLL.Members.MemberSummary`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `clientCode` | `long?` | yes | — |
| `memberCode` | `string` | no | — |
| `fullName` | `string` | no | — |
| `primaryPhone` | `string` | no | — |
| `isActive` | `bool` | no | — |
| `registrationBranch` | `Authentication.BranchContext` | no | — |
| `currentSalesOwner` | `Members.SalesAssignmentDetails?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `isHouse` | `bool` | no | — |

<a id="campionssystem-bll-members-returntohouserequest"></a>
## `CampionsSystem.BLL.Members.ReturnToHouseRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `reason` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(2048)` |

<a id="campionssystem-bll-members-salesassignmentdetails"></a>
## `CampionsSystem.BLL.Members.SalesAssignmentDetails?`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `salesEmployeeId` | `Guid` | no | — |
| `salesEmployeeName` | `string` | no | — |
| `assignedAt` | `DateTimeOffset` | no | — |
| `endedAt` | `DateTimeOffset?` | yes | — |
| `reason` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |

<a id="campionssystem-bll-members-salesassignmenthistoryitem"></a>
## `CampionsSystem.BLL.Members.SalesAssignmentHistoryItem`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `salesEmployeeId` | `Guid?` | yes | — |
| `salesEmployeeName` | `string?` | yes | — |
| `assignmentType` | `string` | no | `System.Runtime.CompilerServices.NullableAttribute(1)` |
| `assignedAt` | `DateTimeOffset` | no | — |
| `endedAt` | `DateTimeOffset?` | yes | — |
| `reason` | `string?` | yes | — |

<a id="campionssystem-bll-members-salesassignmentrequest"></a>
## `CampionsSystem.BLL.Members.SalesAssignmentRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `salesEmployeeId` | `Guid` | no | — |
| `reason` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(2048)` |

<a id="campionssystem-bll-members-updatememberrequest"></a>
## `CampionsSystem.BLL.Members.UpdateMemberRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `fullName` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |
| `primaryPhone` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)` |
| `secondaryPhone` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)` |
| `dateOfBirth` | `DateOnly?` | yes | — |
| `gender` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(32)` |
| `notes` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(4096)` |

<a id="campionssystem-bll-monthlyfinance-classoccurrencereportrow"></a>
## `CampionsSystem.BLL.MonthlyFinance.ClassOccurrenceReportRow`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `coachEmployeeId` | `Guid` | no | — |
| `classType` | `string` | no | — |
| `businessDate` | `DateOnly?` | yes | — |
| `startedAt` | `DateTimeOffset` | no | — |
| `completedAt` | `DateTimeOffset?` | yes | — |
| `status` | `string` | no | — |
| `attendance` | `int` | no | — |

<a id="campionssystem-bll-monthlyfinance-closefinancialmonthrequest"></a>
## `CampionsSystem.BLL.MonthlyFinance.CloseFinancialMonthRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `expectedVersion` | `int` | no | — |
| `reason` | `string` | no | — |
| `countedCash` | `decimal?` | yes | — |

<a id="campionssystem-bll-monthlyfinance-coachearningreportrow"></a>
## `CampionsSystem.BLL.MonthlyFinance.CoachEarningReportRow`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `coachEmployeeId` | `Guid` | no | — |
| `commissionPeriodId` | `Guid` | no | — |
| `financialMonthId` | `Guid?` | yes | — |
| `saleId` | `Guid` | no | — |
| `sessionId` | `Guid?` | yes | — |
| `expirySettlementId` | `Guid?` | yes | — |
| `eligibleValue` | `decimal` | no | — |
| `rate` | `decimal` | no | — |
| `amount` | `decimal` | no | — |
| `transferredPackage` | `bool` | no | — |
| `businessDate` | `DateOnly` | no | — |
| `recognizedAt` | `DateTimeOffset` | no | — |
| `attributionEvidence` | `string` | no | — |

<a id="campionssystem-bll-monthlyfinance-coachmonthrow"></a>
## `CampionsSystem.BLL.MonthlyFinance.CoachMonthRow`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `coachEmployeeId` | `Guid` | no | — |
| `coachName` | `string` | no | — |
| `openingOutstanding` | `decimal` | no | — |
| `actualSessionEarnings` | `decimal` | no | — |
| `expirationEarnings` | `decimal` | no | — |
| `earned` | `decimal` | no | — |
| `paid` | `decimal` | no | — |
| `closingOutstanding` | `decimal` | no | — |
| `completedClasses` | `int` | no | — |
| `classAttendances` | `int` | no | — |
| `averageAttendees` | `decimal?` | yes | — |
| `classesCompensationStatus` | `string` | no | — |

<a id="campionssystem-bll-monthlyfinance-coachpaymentreportrow"></a>
## `CampionsSystem.BLL.MonthlyFinance.CoachPaymentReportRow`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `coachEmployeeId` | `Guid` | no | — |
| `expenseId` | `Guid` | no | — |
| `amount` | `decimal` | no | — |
| `businessDate` | `DateOnly` | no | — |
| `paidAt` | `DateTimeOffset` | no | — |

<a id="campionssystem-bll-monthlyfinance-consolidatedmonthsummary"></a>
## `CampionsSystem.BLL.MonthlyFinance.ConsolidatedMonthSummary`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `months` | `IReadOnlyList<Guid>` | no | — |
| `netCollections` | `decimal` | no | — |
| `ordinaryExpenses` | `decimal` | no | — |
| `earnedEmployeeCosts` | `decimal` | no | — |
| `earnedCoachCosts` | `decimal` | no | — |
| `netOperatingResult` | `decimal` | no | — |
| `employeeOutstanding` | `decimal` | no | — |
| `coachOutstanding` | `decimal` | no | — |

<a id="campionssystem-bll-monthlyfinance-expensecategorytotal"></a>
## `CampionsSystem.BLL.MonthlyFinance.ExpenseCategoryTotal`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `categoryId` | `Guid` | no | — |
| `category` | `string` | no | — |
| `paid` | `decimal` | no | — |
| `returned` | `decimal` | no | — |
| `net` | `decimal` | no | — |

<a id="campionssystem-bll-monthlyfinance-financialmonthdetails"></a>
## `CampionsSystem.BLL.MonthlyFinance.FinancialMonthDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `name` | `string` | no | — |
| `startDate` | `DateOnly` | no | — |
| `endDate` | `DateOnly` | no | — |
| `status` | `string` | no | — |
| `version` | `int` | no | — |
| `lastRevision` | `int` | no | — |
| `firstClosedAt` | `DateTimeOffset?` | yes | — |

<a id="campionssystem-bll-monthlyfinance-financialtransactionreportrow"></a>
## `CampionsSystem.BLL.MonthlyFinance.FinancialTransactionReportRow`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `ledgerId` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `branchName` | `string` | no | — |
| `financialMonthId` | `Guid?` | yes | — |
| `periodName` | `string` | no | — |
| `timestamp` | `DateTimeOffset` | no | — |
| `postedAt` | `DateTimeOffset?` | yes | — |
| `originalOccurredAt` | `DateTimeOffset?` | yes | — |
| `businessDate` | `DateOnly?` | yes | — |
| `accountId` | `Guid` | no | — |
| `accountName` | `string` | no | — |
| `paymentMethod` | `string` | no | — |
| `sourceType` | `string` | no | — |
| `direction` | `string` | no | — |
| `amount` | `decimal` | no | — |
| `actorUserId` | `Guid` | no | — |
| `performingEmployeeId` | `Guid?` | yes | — |
| `performingEmployeeName` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `shiftId` | `Guid?` | yes | — |
| `customerId` | `Guid?` | yes | — |
| `customerName` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `commercialSalesOwnerId` | `Guid?` | yes | — |
| `commercialSalesOwnerName` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `saleId` | `Guid?` | yes | — |
| `paymentId` | `Guid?` | yes | — |
| `refundId` | `Guid?` | yes | — |
| `expenseId` | `Guid?` | yes | — |
| `advanceId` | `Guid?` | yes | — |
| `withdrawalId` | `Guid?` | yes | — |
| `coachPayoutId` | `Guid?` | yes | — |
| `payrollId` | `Guid?` | yes | — |
| `operationId` | `Guid?` | yes | — |
| `expenseCategoryId` | `Guid?` | yes | — |
| `category` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `notes` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `receipt` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `reversalId` | `Guid?` | yes | — |
| `originalPaymentId` | `Guid?` | yes | — |
| `sourceEvidence` | `string` | no | — |
| `auditEvidence` | `string` | no | — |
| `providerReference` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |

<a id="campionssystem-bll-monthlyfinance-maintenancebranchsummary"></a>
## `CampionsSystem.BLL.MonthlyFinance.MaintenanceBranchSummary`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `branchId` | `Guid` | no | — |
| `asOf` | `DateOnly` | no | — |
| `horizonDays` | `int` | no | — |
| `totalSuggestedMonthlyAmount` | `decimal` | no | — |
| `activePlanCount` | `int` | no | — |
| `upcomingCount` | `int` | no | — |
| `dueTodayCount` | `int` | no | — |
| `overdueCount` | `int` | no | — |

<a id="campionssystem-bll-monthlyfinance-maintenanceforecast"></a>
## `CampionsSystem.BLL.MonthlyFinance.MaintenanceForecast`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `planId` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `revision` | `int` | no | — |
| `name` | `string` | no | — |
| `expectedCost` | `decimal` | no | — |
| `frequencyMonths` | `int` | no | — |
| `firstDueDate` | `DateOnly` | no | — |
| `nextExpectedDate` | `DateOnly` | no | — |
| `suggestedMonthlyAmount` | `decimal` | no | — |
| `isActive` | `bool` | no | — |
| `status` | `string` | no | — |
| `description` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `dueStatus` | `string` | no | — |
| `daysUntilDue` | `int` | no | — |

<a id="campionssystem-bll-monthlyfinance-maintenanceplanrequest"></a>
## `CampionsSystem.BLL.MonthlyFinance.MaintenancePlanRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `name` | `string` | no | — |
| `expectedCost` | `decimal` | no | — |
| `frequencyMonths` | `int` | no | — |
| `firstDueDate` | `DateOnly` | no | — |
| `reason` | `string` | no | — |
| `planId` | `Guid?` | yes | — |
| `expectedRevision` | `int` | no | — |
| `isActive` | `bool` | no | — |
| `description` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `nextExpectedDueDate` | `DateOnly?` | yes | — |

<a id="campionssystem-bll-monthlyfinance-montheventdetails"></a>
## `CampionsSystem.BLL.MonthlyFinance.MonthEventDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `kind` | `string` | no | — |
| `operationId` | `Guid` | no | — |
| `actorUserId` | `Guid` | no | — |
| `reason` | `string` | no | — |
| `recordedAt` | `DateTimeOffset` | no | — |
| `resultVersion` | `int` | no | — |
| `revision` | `int?` | yes | — |

<a id="campionssystem-bll-monthlyfinance-monthlyreport"></a>
## `CampionsSystem.BLL.MonthlyFinance.MonthlyReport`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `month` | `MonthlyFinance.FinancialMonthDetails` | no | — |
| `branchName` | `string` | no | — |
| `revision` | `int` | no | — |
| `snapshotAt` | `DateTimeOffset` | no | — |
| `summary` | `MonthlyFinance.MonthlySummary` | no | — |
| `transactions` | `IReadOnlyList<MonthlyFinance.FinancialTransactionReportRow>` | no | — |
| `expenseCategories` | `IReadOnlyList<MonthlyFinance.ExpenseCategoryTotal>` | no | — |
| `salesPerformance` | `IReadOnlyList<Commissions.CommissionProgress>` | no | — |
| `salesSources` | `IReadOnlyList<Commissions.CommissionSourceLine>` | no | — |
| `salesRefundAdjustments` | `IReadOnlyList<Commissions.CommissionRefundSource>` | no | — |
| `salaries` | `IReadOnlyList<MonthlyFinance.SalaryEntitlement>` | no | — |
| `payroll` | `IReadOnlyList<MonthlyFinance.PayrollDetails>` | no | — |
| `employeePayments` | `IReadOnlyList<MonthlyFinance.PayrollPaymentDetails>` | no | — |
| `advances` | `IReadOnlyList<Shifts.AdvanceDetails>` | no | — |
| `coaches` | `IReadOnlyList<MonthlyFinance.CoachMonthRow>` | no | — |
| `coachEarnings` | `IReadOnlyList<MonthlyFinance.CoachEarningReportRow>` | no | — |
| `coachPayments` | `IReadOnlyList<MonthlyFinance.CoachPaymentReportRow>` | no | — |
| `classes` | `IReadOnlyList<MonthlyFinance.ClassOccurrenceReportRow>` | no | — |
| `drawer` | `Shifts.DrawerPosition` | no | — |
| `cashCounts` | `IReadOnlyList<Shifts.CashCountDetails>` | no | — |
| `shifts` | `IReadOnlyList<Shifts.ShiftDetails>` | no | — |
| `maintenance` | `IReadOnlyList<MonthlyFinance.MaintenanceForecast>` | no | — |
| `limitations` | `IReadOnlyList<string>` | no | — |

<a id="campionssystem-bll-monthlyfinance-monthlysummary"></a>
## `CampionsSystem.BLL.MonthlyFinance.MonthlySummary`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `grossCollections` | `decimal` | no | — |
| `refunds` | `decimal` | no | — |
| `netCollections` | `decimal` | no | — |
| `ordinaryExpenses` | `decimal` | no | — |
| `basicSalaryEarned` | `decimal` | no | — |
| `salesCommissionRecognized` | `decimal` | no | — |
| `privateCoachEarningsRecognized` | `decimal` | no | — |
| `employeePaymentRecords` | `decimal` | no | — |
| `coachCashPayouts` | `decimal` | no | — |
| `advanceCashPayouts` | `decimal` | no | — |
| `ledgerCredits` | `decimal` | no | — |
| `ledgerDebits` | `decimal` | no | — |
| `ledgerNetCashflow` | `decimal` | no | — |
| `netOperatingResult` | `decimal` | no | — |
| `employeeOpeningOutstanding` | `decimal` | no | — |
| `employeeClosingOutstanding` | `decimal` | no | — |
| `coachOpeningOutstanding` | `decimal` | no | — |
| `coachClosingOutstanding` | `decimal` | no | — |
| `bookCash` | `decimal` | no | — |
| `expectedPhysicalCash` | `decimal` | no | — |
| `lastCountedCash` | `decimal` | no | — |
| `physicalVsBook` | `decimal` | no | — |
| `periodObservedDiscrepancies` | `decimal` | no | — |
| `operatingResultBasis` | `string` | no | — |

<a id="campionssystem-bll-monthlyfinance-openfinancialmonthrequest"></a>
## `CampionsSystem.BLL.MonthlyFinance.OpenFinancialMonthRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `name` | `string` | no | — |
| `startDate` | `DateOnly` | no | — |
| `endDate` | `DateOnly` | no | — |
| `reason` | `string` | no | — |
| `branchId` | `Guid?` | yes | — |

<a id="campionssystem-bll-monthlyfinance-payrolldetails"></a>
## `CampionsSystem.BLL.MonthlyFinance.PayrollDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `employeeId` | `Guid` | no | — |
| `employeeName` | `string` | no | — |
| `branchId` | `Guid` | no | — |
| `financialMonthId` | `Guid?` | yes | — |
| `commissionPeriodId` | `Guid?` | yes | — |
| `commissionSummaryId` | `Guid?` | yes | — |
| `basicSalary` | `decimal` | no | — |
| `commission` | `decimal` | no | — |
| `advancesDeducted` | `decimal` | no | — |
| `payable` | `decimal` | no | — |
| `paid` | `decimal` | no | — |
| `outstanding` | `decimal` | no | — |
| `status` | `string` | no | — |

<a id="campionssystem-bll-monthlyfinance-payrollpaymentdetails"></a>
## `CampionsSystem.BLL.MonthlyFinance.PayrollPaymentDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `payrollId` | `Guid` | no | — |
| `employeeId` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `financialMonthId` | `Guid` | no | — |
| `amount` | `decimal` | no | — |
| `balanceBefore` | `decimal` | no | — |
| `balanceAfter` | `decimal` | no | — |
| `paidAt` | `DateTimeOffset` | no | — |
| `recordedAt` | `DateTimeOffset` | no | — |
| `businessDate` | `DateOnly` | no | — |
| `actorUserId` | `Guid` | no | — |
| `reason` | `string` | no | — |
| `reference` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |

<a id="campionssystem-bll-monthlyfinance-recognizepayrollcommissionsrequest"></a>
## `CampionsSystem.BLL.MonthlyFinance.RecognizePayrollCommissionsRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `commissionPeriodId` | `Guid` | no | — |

<a id="campionssystem-bll-monthlyfinance-recordoccurrencerequest"></a>
## `CampionsSystem.BLL.MonthlyFinance.RecordOccurrenceRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `originalOccurredAt` | `DateTimeOffset` | no | — |
| `reason` | `string` | no | — |

<a id="campionssystem-bll-monthlyfinance-recordpayrollpaymentrequest"></a>
## `CampionsSystem.BLL.MonthlyFinance.RecordPayrollPaymentRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `amount` | `decimal` | no | — |
| `paidAt` | `DateTimeOffset` | no | — |
| `reason` | `string` | no | — |
| `reference` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |

<a id="campionssystem-bll-monthlyfinance-reopenfinancialmonthrequest"></a>
## `CampionsSystem.BLL.MonthlyFinance.ReopenFinancialMonthRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `expectedVersion` | `int` | no | — |
| `reason` | `string` | no | — |

<a id="campionssystem-bll-monthlyfinance-revisiondetails"></a>
## `CampionsSystem.BLL.MonthlyFinance.RevisionDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `revision` | `int` | no | — |
| `snapshotAt` | `DateTimeOffset` | no | — |
| `actorUserId` | `Guid` | no | — |
| `reason` | `string` | no | — |
| `workbookSha256` | `string` | no | — |
| `schemaVersion` | `int` | no | — |

<a id="campionssystem-bll-monthlyfinance-salarydayevidence"></a>
## `CampionsSystem.BLL.MonthlyFinance.SalaryDayEvidence`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `date` | `DateOnly` | no | — |
| `assignmentId` | `Guid` | no | — |
| `salarySettingId` | `Guid` | no | — |
| `monthlyRate` | `decimal` | no | — |
| `unroundedDailyAmount` | `decimal` | no | — |

<a id="campionssystem-bll-monthlyfinance-salaryentitlement"></a>
## `CampionsSystem.BLL.MonthlyFinance.SalaryEntitlement`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `employeeId` | `Guid` | no | — |
| `employeeName` | `string` | no | — |
| `branchId` | `Guid` | no | — |
| `financialMonthId` | `Guid` | no | — |
| `periodDays` | `int` | no | — |
| `eligibleDays` | `int` | no | — |
| `basicSalary` | `decimal` | no | — |
| `advances` | `decimal` | no | — |
| `payableBasicSalary` | `decimal` | no | — |
| `days` | `IReadOnlyList<MonthlyFinance.SalaryDayEvidence>` | no | — |

<a id="campionssystem-bll-monthlyfinance-salarysettingdetails"></a>
## `CampionsSystem.BLL.MonthlyFinance.SalarySettingDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `employeeId` | `Guid` | no | — |
| `basicSalary` | `decimal` | no | — |
| `effectiveFrom` | `DateOnly` | no | — |
| `effectiveTo` | `DateOnly?` | yes | — |
| `actorUserId` | `Guid` | no | — |
| `createdAt` | `DateTimeOffset` | no | — |

<a id="campionssystem-bll-monthlyfinance-setsalaryrequest"></a>
## `CampionsSystem.BLL.MonthlyFinance.SetSalaryRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `basicSalary` | `decimal` | no | — |
| `effectiveFrom` | `DateOnly` | no | — |
| `reason` | `string` | no | — |

<a id="campionssystem-bll-monthlyfinance-workbookdownload"></a>
## `CampionsSystem.BLL.MonthlyFinance.WorkbookDownload`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `bytes` | `byte[]` | no | — |
| `fileName` | `string` | no | — |
| `sha256` | `string` | no | — |

<a id="campionssystem-bll-operations-activitycoachreport"></a>
## `CampionsSystem.BLL.Operations.ActivityCoachReport`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `coachId` | `Guid` | no | — |
| `coach` | `string` | no | — |
| `activityId` | `Guid` | no | — |
| `activity` | `string` | no | — |
| `coachType` | `Enums.ActivityCoachType` | no | — |
| `employeeId` | `Guid?` | yes | — |
| `subscribersCount` | `int` | no | — |
| `eligibleRevenue` | `decimal` | no | — |
| `percentage` | `decimal` | no | — |
| `entitlement` | `decimal` | no | — |

<a id="campionssystem-bll-operations-activitypurchaserequest"></a>
## `CampionsSystem.BLL.Operations.ActivityPurchaseRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `memberId` | `Guid` | no | — |
| `activityPackageId` | `Guid` | no | — |
| `activityCoachId` | `Guid` | no | — |
| `existingSubscriptionId` | `Guid?` | yes | — |

<a id="campionssystem-bll-operations-classcoachreport"></a>
## `CampionsSystem.BLL.Operations.ClassCoachReport`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `coachEmployeeId` | `Guid` | no | — |
| `coach` | `string` | no | — |
| `completedClassesCount` | `int` | no | — |
| `totalAttendances` | `int` | no | — |
| `averageAttendance` | `decimal` | no | — |

<a id="campionssystem-bll-operations-createactivitycoachrequest"></a>
## `CampionsSystem.BLL.Operations.CreateActivityCoachRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `coachType` | `Enums.ActivityCoachType` | no | — |
| `employeeId` | `Guid?` | yes | — |
| `externalName` | `string?` | yes | — |
| `externalPhone` | `string?` | yes | — |
| `percentage` | `decimal` | no | — |

<a id="campionssystem-bll-operations-freelanceplanreport"></a>
## `CampionsSystem.BLL.Operations.FreelancePlanReport`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `membershipPlanId` | `Guid` | no | — |
| `plan` | `string` | no | — |
| `coachEmployeeId` | `Guid` | no | — |
| `coach` | `string` | no | — |
| `eligibleMemberCount` | `int` | no | — |
| `purchasedMemberCount` | `int` | no | — |
| `actualGymRevenue` | `decimal` | no | — |
| `salesAttribution` | `string` | no | — |

<a id="campionssystem-bll-operations-freelancepurchaserequest"></a>
## `CampionsSystem.BLL.Operations.FreelancePurchaseRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `memberId` | `Guid` | no | — |
| `membershipPlanId` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `existingSubscriptionId` | `Guid?` | yes | — |

<a id="campionssystem-bll-operations-purchaseresult"></a>
## `CampionsSystem.BLL.Operations.PurchaseResult`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `subscriptionId` | `Guid` | no | — |
| `transactionId` | `Guid` | no | — |
| `saleTransactionId` | `Guid` | no | — |
| `gymAmountDue` | `decimal` | no | — |

<a id="campionssystem-bll-operations-saveclasssessionrequest"></a>
## `CampionsSystem.BLL.Operations.SaveClassSessionRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `groupClassId` | `Guid` | no | — |
| `classScheduleId` | `Guid?` | yes | — |
| `coachEmployeeId` | `Guid` | no | — |
| `startedAt` | `DateTimeOffset` | no | — |
| `endedAt` | `DateTimeOffset?` | yes | — |
| `status` | `Enums.ClassSessionStatus` | no | — |

<a id="campionssystem-bll-operations-savefreelanceplanrequest"></a>
## `CampionsSystem.BLL.Operations.SaveFreelancePlanRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `cityId` | `Guid` | no | — |
| `coachEmployeeId` | `Guid` | no | — |
| `name` | `string` | no | — |
| `price` | `decimal` | no | — |
| `durationDays` | `int` | no | — |
| `accessMode` | `Enums.AccessMode` | no | — |
| `sessionCount` | `int?` | yes | — |
| `isActive` | `bool` | no | — |
| `branchId` | `Guid?` | yes | — |

<a id="campionssystem-bll-plans-catalogdetails"></a>
## `CampionsSystem.BLL.Plans.CatalogDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `code` | `string?` | yes | — |
| `name` | `string` | no | `System.Runtime.CompilerServices.NullableAttribute(1)` |
| `description` | `string?` | yes | — |
| `isActive` | `bool` | no | — |
| `createdAt` | `DateTimeOffset` | no | — |

<a id="campionssystem-bll-plans-cataloglistrequest"></a>
## `CampionsSystem.BLL.Plans.CatalogListRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `search` | `string?` | yes | — |
| `isActive` | `bool?` | yes | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-plans-cataloglistresult"></a>
## `CampionsSystem.BLL.Plans.CatalogListResult`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Plans.CatalogDetails>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-plans-changeplanstatusrequest"></a>
## `CampionsSystem.BLL.Plans.ChangePlanStatusRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `isActive` | `bool?` | yes | `System.ComponentModel.DataAnnotations.RequiredAttribute` |

<a id="campionssystem-bll-plans-createmembershipplanrequest"></a>
## `CampionsSystem.BLL.Plans.CreateMembershipPlanRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `branchId` | `Guid` | no | — |
| `cityId` | `Guid` | no | — |
| `name` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |
| `planType` | `Enums.PlanCategory?` | yes | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))` |
| `audienceCategoryId` | `Guid?` | yes | — |
| `mode` | `Enums.AccessMode?` | yes | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))` |
| `durationDays` | `int` | no | — |
| `sessions` | `int?` | yes | — |
| `price` | `decimal` | no | — |
| `allowFreeze` | `bool` | no | — |
| `freezeDays` | `int` | no | — |
| `freelanceCoachEmployeeId` | `Guid?` | yes | — |

<a id="campionssystem-bll-plans-eligiblemembercandidaterequest"></a>
## `CampionsSystem.BLL.Plans.EligibleMemberCandidateRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `memberId` | `Guid?` | yes | — |
| `phone` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(64)` |
| `memberCode` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |

<a id="campionssystem-bll-plans-eligiblememberdetails"></a>
## `CampionsSystem.BLL.Plans.EligibleMemberDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `memberId` | `Guid` | no | — |
| `fullName` | `string` | no | — |
| `primaryPhone` | `string` | no | — |
| `registrationBranchId` | `Guid` | no | — |
| `clientCode` | `long?` | yes | — |
| `memberCode` | `string` | no | — |
| `addedAt` | `DateTimeOffset?` | yes | — |

<a id="campionssystem-bll-plans-eligiblememberlistresult"></a>
## `CampionsSystem.BLL.Plans.EligibleMemberListResult`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Plans.EligibleMemberDetails>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-plans-eligiblememberrequest"></a>
## `CampionsSystem.BLL.Plans.EligibleMemberRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `memberId` | `Guid` | no | — |

<a id="campionssystem-bll-plans-membershipplandetails"></a>
## `CampionsSystem.BLL.Plans.MembershipPlanDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `plan` | `Plans.MembershipPlanSummary` | no | — |
| `createdAt` | `DateTimeOffset` | no | — |
| `benefits` | `IReadOnlyList<Plans.PlanBenefitDetails>` | no | — |
| `eligibleMemberCount` | `int` | no | — |

<a id="campionssystem-bll-plans-membershipplanlistrequest"></a>
## `CampionsSystem.BLL.Plans.MembershipPlanListRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `branchId` | `Guid?` | yes | — |
| `cityId` | `Guid?` | yes | — |
| `planType` | `Enums.PlanCategory?` | yes | — |
| `audienceCategoryId` | `Guid?` | yes | — |
| `mode` | `Enums.AccessMode?` | yes | — |
| `freelanceCoachEmployeeId` | `Guid?` | yes | — |
| `includeInternal` | `bool` | no | — |
| `search` | `string?` | yes | — |
| `isActive` | `bool?` | yes | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-plans-membershipplanlistresult"></a>
## `CampionsSystem.BLL.Plans.MembershipPlanListResult`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Plans.MembershipPlanSummary>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-plans-membershipplansummary"></a>
## `CampionsSystem.BLL.Plans.MembershipPlanSummary`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `name` | `string` | no | — |
| `planType` | `string` | no | — |
| `branch` | `Plans.PlanReference?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `city` | `Plans.PlanReference` | no | — |
| `audienceCategory` | `Plans.PlanReference?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `mode` | `string` | no | — |
| `price` | `decimal` | no | — |
| `durationDays` | `int` | no | — |
| `sessions` | `int?` | yes | — |
| `allowFreeze` | `bool` | no | — |
| `freezeDays` | `int` | no | — |
| `isActive` | `bool` | no | — |
| `freelanceCoach` | `Plans.PlanReference?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `customerPurchasable` | `bool` | no | — |

<a id="campionssystem-bll-plans-planbenefitdetails"></a>
## `CampionsSystem.BLL.Plans.PlanBenefitDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `benefitId` | `Guid` | no | — |
| `code` | `string` | no | — |
| `name` | `string` | no | — |
| `isActive` | `bool` | no | — |
| `grantType` | `string` | no | — |
| `quantity` | `int?` | yes | — |

<a id="campionssystem-bll-plans-planbenefitrequest"></a>
## `CampionsSystem.BLL.Plans.PlanBenefitRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `benefitId` | `Guid` | no | — |
| `grantType` | `Enums.MembershipBenefitGrantType?` | yes | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))` |
| `quantity` | `int?` | yes | — |

<a id="campionssystem-bll-plans-planreference"></a>
## `CampionsSystem.BLL.Plans.PlanReference`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `name` | `string` | no | — |

<a id="campionssystem-bll-plans-planreference"></a>
## `CampionsSystem.BLL.Plans.PlanReference?`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `name` | `string` | no | — |

<a id="campionssystem-bll-plans-savebenefitrequest"></a>
## `CampionsSystem.BLL.Plans.SaveBenefitRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `code` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(128)` |
| `name` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |
| `description` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(4096)` |

<a id="campionssystem-bll-plans-savecategoryrequest"></a>
## `CampionsSystem.BLL.Plans.SaveCategoryRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `name` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |
| `description` | `string?` | yes | `System.ComponentModel.DataAnnotations.MaxLengthAttribute(4096)` |

<a id="campionssystem-bll-plans-setplanbenefitsrequest"></a>
## `CampionsSystem.BLL.Plans.SetPlanBenefitsRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `benefits` | `List<Plans.PlanBenefitRequest>` | no | — |

<a id="campionssystem-bll-plans-updatemembershipplanrequest"></a>
## `CampionsSystem.BLL.Plans.UpdateMembershipPlanRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `branchId` | `Guid` | no | — |
| `cityId` | `Guid` | no | — |
| `name` | `string` | no | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.ComponentModel.DataAnnotations.MaxLengthAttribute(256)` |
| `planType` | `Enums.PlanCategory?` | yes | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))` |
| `audienceCategoryId` | `Guid?` | yes | — |
| `mode` | `Enums.AccessMode?` | yes | `System.ComponentModel.DataAnnotations.RequiredAttribute`<br>`System.Text.Json.Serialization.JsonConverterAttribute(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))` |
| `durationDays` | `int` | no | — |
| `sessions` | `int?` | yes | — |
| `price` | `decimal` | no | — |
| `allowFreeze` | `bool` | no | — |
| `freezeDays` | `int` | no | — |
| `freelanceCoachEmployeeId` | `Guid?` | yes | — |

<a id="campionssystem-bll-sales-branchfinancialtotals"></a>
## `CampionsSystem.BLL.Sales.BranchFinancialTotals`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `branchId` | `Guid` | no | — |
| `cashCollected` | `decimal` | no | — |
| `instaPayCollected` | `decimal` | no | — |
| `cashRefunded` | `decimal` | no | — |
| `instaPayRefunded` | `decimal` | no | — |
| `grossCollected` | `decimal` | no | — |
| `refunds` | `decimal` | no | — |
| `netCollected` | `decimal` | no | — |

<a id="campionssystem-bll-sales-createfinancialaccountrequest"></a>
## `CampionsSystem.BLL.Sales.CreateFinancialAccountRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `name` | `string` | no | — |
| `method` | `Enums.FinancialAccountType` | no | — |
| `branchId` | `Guid?` | yes | — |

<a id="campionssystem-bll-sales-createsalerequest"></a>
## `CampionsSystem.BLL.Sales.CreateSaleRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `memberId` | `Guid` | no | — |
| `membershipPlanId` | `Guid` | no | — |
| `purchaseMode` | `Enums.PurchaseMode` | no | — |
| `activationMode` | `Enums.SubscriptionActivationMode` | no | — |
| `paymentParts` | `IReadOnlyList<Sales.PaymentPartInput>` | no | — |
| `offerId` | `Guid?` | yes | — |
| `startDate` | `DateOnly?` | yes | — |
| `reservationDueDate` | `DateOnly?` | yes | — |
| `installments` | `IReadOnlyList<Sales.InstallmentLineInput>?` | yes | `System.Runtime.CompilerServices.NullableAttribute({2, 1})` |
| `branchId` | `Guid?` | yes | — |
| `initialPaymentRequired` | `decimal?` | yes | — |

<a id="campionssystem-bll-sales-financialaccountdetails"></a>
## `CampionsSystem.BLL.Sales.FinancialAccountDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `name` | `string` | no | — |
| `method` | `string` | no | — |
| `isActive` | `bool` | no | — |

<a id="campionssystem-bll-sales-financialpartdetails"></a>
## `CampionsSystem.BLL.Sales.FinancialPartDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `accountId` | `Guid` | no | — |
| `accountName` | `string` | no | — |
| `method` | `string` | no | — |
| `amount` | `decimal` | no | — |
| `reference` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |

<a id="campionssystem-bll-sales-grantmemberofferrequest"></a>
## `CampionsSystem.BLL.Sales.GrantMemberOfferRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `memberId` | `Guid` | no | — |
| `startDate` | `DateOnly?` | yes | — |
| `endDate` | `DateOnly?` | yes | — |

<a id="campionssystem-bll-sales-installmentallocationinput"></a>
## `CampionsSystem.BLL.Sales.InstallmentAllocationInput`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `installmentId` | `Guid` | no | — |
| `amount` | `decimal` | no | — |

<a id="campionssystem-bll-sales-installmentdetails"></a>
## `CampionsSystem.BLL.Sales.InstallmentDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `number` | `int` | no | — |
| `dueDate` | `DateOnly` | no | — |
| `amount` | `decimal` | no | — |
| `paid` | `decimal` | no | — |
| `status` | `string` | no | — |

<a id="campionssystem-bll-sales-installmentlineinput"></a>
## `CampionsSystem.BLL.Sales.InstallmentLineInput`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `amount` | `decimal` | no | — |
| `dueDate` | `DateOnly` | no | — |

<a id="campionssystem-bll-sales-memberaudiencedetails"></a>
## `CampionsSystem.BLL.Sales.MemberAudienceDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `categoryId` | `Guid` | no | — |
| `categoryName` | `string` | no | — |
| `assignedAt` | `DateTimeOffset` | no | — |
| `endedAt` | `DateTimeOffset?` | yes | — |

<a id="campionssystem-bll-sales-memberoffergrantdetails"></a>
## `CampionsSystem.BLL.Sales.MemberOfferGrantDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `memberId` | `Guid` | no | — |
| `offerId` | `Guid` | no | — |
| `startDate` | `DateOnly?` | yes | — |
| `endDate` | `DateOnly?` | yes | — |
| `revokedAt` | `DateTimeOffset?` | yes | — |

<a id="campionssystem-bll-sales-offerdetails"></a>
## `CampionsSystem.BLL.Sales.OfferDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `branchId` | `Guid?` | yes | — |
| `name` | `string` | no | — |
| `type` | `string` | no | — |
| `value` | `decimal` | no | — |
| `startDate` | `DateOnly?` | yes | — |
| `endDate` | `DateOnly?` | yes | — |
| `isPermanent` | `bool` | no | — |
| `isActive` | `bool` | no | — |
| `membershipPlanIds` | `IReadOnlyList<Guid>` | no | — |
| `eligibilityMode` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `audienceCategoryId` | `Guid?` | yes | — |

<a id="campionssystem-bll-sales-offerhistory"></a>
## `CampionsSystem.BLL.Sales.OfferHistory`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Sales.OfferDetails>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-sales-offerrequest"></a>
## `CampionsSystem.BLL.Sales.OfferRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `name` | `string` | no | — |
| `type` | `Enums.OfferType` | no | — |
| `value` | `decimal` | no | — |
| `membershipPlanIds` | `IReadOnlyList<Guid>` | no | — |
| `eligibilityMode` | `Enums.OfferEligibilityMode?` | yes | — |
| `audienceCategoryId` | `Guid?` | yes | — |
| `startDate` | `DateOnly?` | yes | — |
| `endDate` | `DateOnly?` | yes | — |
| `isPermanent` | `bool` | no | — |
| `isActive` | `bool` | no | — |
| `description` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `branchId` | `Guid?` | yes | — |

<a id="campionssystem-bll-sales-paymentdetails"></a>
## `CampionsSystem.BLL.Sales.PaymentDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `operationId` | `Guid?` | yes | — |
| `branchId` | `Guid` | no | — |
| `businessDate` | `DateOnly?` | yes | — |
| `businessDayId` | `Guid?` | yes | — |
| `paidAt` | `DateTimeOffset` | no | — |
| `amount` | `decimal` | no | — |
| `parts` | `IReadOnlyList<Sales.FinancialPartDetails>` | no | — |
| `allocations` | `IReadOnlyList<Sales.InstallmentAllocationInput>` | no | — |

<a id="campionssystem-bll-sales-paymenthistory"></a>
## `CampionsSystem.BLL.Sales.PaymentHistory`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Sales.PaymentDetails>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-sales-paymentpartinput"></a>
## `CampionsSystem.BLL.Sales.PaymentPartInput`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `financialAccountId` | `Guid` | no | — |
| `amount` | `decimal` | no | — |
| `providerReference` | `string?` | yes | — |

<a id="campionssystem-bll-sales-pricingresult"></a>
## `CampionsSystem.BLL.Sales.PricingResult`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `originalPrice` | `decimal` | no | — |
| `discount` | `decimal` | no | — |
| `finalPrice` | `decimal` | no | — |
| `extraDays` | `int` | no | — |
| `offerId` | `Guid?` | yes | — |
| `offerName` | `string?` | yes | — |
| `offerType` | `Enums.OfferType?` | yes | — |
| `offerValue` | `decimal?` | yes | — |
| `eligibilityMode` | `Enums.OfferEligibilityMode?` | yes | — |
| `audienceCategoryId` | `Guid?` | yes | — |

<a id="campionssystem-bll-sales-quickcancelrequest"></a>
## `CampionsSystem.BLL.Sales.QuickCancelRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `reason` | `string` | no | — |
| `amount` | `decimal?` | yes | `System.Text.Json.Serialization.JsonRequiredAttribute` |
| `parts` | `IReadOnlyList<Sales.PaymentPartInput>?` | yes | `System.Runtime.CompilerServices.NullableAttribute({2, 1})` |

<a id="campionssystem-bll-sales-recordpaymentrequest"></a>
## `CampionsSystem.BLL.Sales.RecordPaymentRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `amount` | `decimal` | no | — |
| `parts` | `IReadOnlyList<Sales.PaymentPartInput>` | no | — |
| `allocations` | `IReadOnlyList<Sales.InstallmentAllocationInput>?` | yes | `System.Runtime.CompilerServices.NullableAttribute({2, 1})` |

<a id="campionssystem-bll-sales-refunddetails"></a>
## `CampionsSystem.BLL.Sales.RefundDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `operationId` | `Guid?` | yes | — |
| `branchId` | `Guid` | no | — |
| `businessDate` | `DateOnly?` | yes | — |
| `createdAt` | `DateTimeOffset` | no | — |
| `amount` | `decimal` | no | — |
| `type` | `string` | no | — |
| `reason` | `string` | no | — |
| `parts` | `IReadOnlyList<Sales.FinancialPartDetails>` | no | — |

<a id="campionssystem-bll-sales-refundhistory"></a>
## `CampionsSystem.BLL.Sales.RefundHistory`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Sales.RefundDetails>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-sales-refundrequest"></a>
## `CampionsSystem.BLL.Sales.RefundRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `amount` | `decimal` | no | — |
| `parts` | `IReadOnlyList<Sales.PaymentPartInput>` | no | — |
| `reason` | `string` | no | — |

<a id="campionssystem-bll-sales-saledetails"></a>
## `CampionsSystem.BLL.Sales.SaleDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `sale` | `Sales.SaleSummary` | no | — |
| `memberName` | `string` | no | — |
| `planName` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `planType` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `planId` | `Guid?` | yes | — |
| `accessCityId` | `Guid?` | yes | — |
| `salesEmployeeId` | `Guid?` | yes | — |
| `salesAttribution` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `commissionEligible` | `bool?` | yes | — |
| `targetEligible` | `bool?` | yes | — |
| `subscriptionId` | `Guid?` | yes | — |
| `grantId` | `Guid?` | yes | — |
| `activationStatus` | `string` | no | — |
| `requestedStartDate` | `DateOnly?` | yes | — |
| `reservationDueDate` | `DateOnly?` | yes | — |
| `offerName` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `extraDays` | `int` | no | — |
| `retainedCancellationRevenue` | `decimal` | no | — |
| `payments` | `IReadOnlyList<Sales.PaymentDetails>` | no | — |
| `refunds` | `IReadOnlyList<Sales.RefundDetails>` | no | — |
| `installments` | `IReadOnlyList<Sales.InstallmentDetails>` | no | — |
| `offerId` | `Guid?` | yes | — |
| `offerType` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `offerValue` | `decimal?` | yes | — |
| `offerEligibilityMode` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `offerAudienceCategoryId` | `Guid?` | yes | — |
| `retainedRevenueClassification` | `string` | no | — |
| `retainedCommissionEligible` | `bool` | no | — |
| `retainedTargetEligible` | `bool` | no | — |

<a id="campionssystem-bll-sales-salehistory"></a>
## `CampionsSystem.BLL.Sales.SaleHistory`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Sales.SaleSummary>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-sales-salehistoryrequest"></a>
## `CampionsSystem.BLL.Sales.SaleHistoryRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `branchId` | `Guid?` | yes | — |
| `memberId` | `Guid?` | yes | — |
| `purchaseMode` | `Enums.PurchaseMode?` | yes | — |
| `from` | `DateOnly?` | yes | — |
| `to` | `DateOnly?` | yes | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-sales-salereceipt"></a>
## `CampionsSystem.BLL.Sales.SaleReceipt`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `saleId` | `Guid` | no | — |
| `paymentIds` | `IReadOnlyList<Guid>` | no | — |

<a id="campionssystem-bll-sales-salesummary"></a>
## `CampionsSystem.BLL.Sales.SaleSummary`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `memberId` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `businessDate` | `DateOnly?` | yes | — |
| `createdAt` | `DateTimeOffset` | no | — |
| `originalPrice` | `decimal` | no | — |
| `discount` | `decimal` | no | — |
| `finalPrice` | `decimal` | no | — |
| `collected` | `decimal` | no | — |
| `refunded` | `decimal` | no | — |
| `outstanding` | `decimal` | no | — |
| `status` | `string` | no | `System.Runtime.CompilerServices.NullableAttribute(1)` |
| `purchaseMode` | `string?` | yes | — |
| `activationMode` | `string?` | yes | — |
| `cancelled` | `bool` | no | — |
| `version` | `int` | no | — |

<a id="campionssystem-bll-sales-setmemberaudiencerequest"></a>
## `CampionsSystem.BLL.Sales.SetMemberAudienceRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `audienceCategoryIds` | `IReadOnlyList<Guid>` | no | — |

<a id="campionssystem-bll-shifts-advancedetails"></a>
## `CampionsSystem.BLL.Shifts.AdvanceDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `shiftId` | `Guid` | no | — |
| `receivingEmployeeId` | `Guid` | no | — |
| `receivingEmployeeName` | `string` | no | — |
| `recordingEmployeeId` | `Guid` | no | — |
| `accountId` | `Guid` | no | — |
| `method` | `Enums.FinancialAccountType` | no | — |
| `amount` | `decimal` | no | — |
| `reason` | `string` | no | — |
| `reference` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `occurredAt` | `DateTimeOffset` | no | — |
| `businessDate` | `DateOnly` | no | — |

<a id="campionssystem-bll-shifts-advancerequest"></a>
## `CampionsSystem.BLL.Shifts.AdvanceRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `receivingEmployeeId` | `Guid` | no | — |
| `financialAccountId` | `Guid` | no | — |
| `amount` | `decimal` | no | — |
| `reason` | `string` | no | — |
| `reference` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `branchId` | `Guid?` | yes | — |

<a id="campionssystem-bll-shifts-branchfinancialsummary"></a>
## `CampionsSystem.BLL.Shifts.BranchFinancialSummary`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `branchId` | `Guid` | no | — |
| `from` | `DateOnly?` | yes | — |
| `to` | `DateOnly?` | yes | — |
| `activity` | `IReadOnlyList<Shifts.MethodFinancialTotals>` | no | — |
| `currentSharedDrawer` | `Shifts.DrawerPosition` | no | — |

<a id="campionssystem-bll-shifts-businessdaydetails"></a>
## `CampionsSystem.BLL.Shifts.BusinessDayDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `businessDate` | `DateOnly` | no | — |
| `status` | `Enums.BusinessDayStatus` | no | — |
| `openedAt` | `DateTimeOffset` | no | — |
| `closedAt` | `DateTimeOffset?` | yes | — |
| `openedByUserId` | `Guid?` | yes | — |
| `closedByUserId` | `Guid?` | yes | — |

<a id="campionssystem-bll-shifts-businessdaydetails"></a>
## `CampionsSystem.BLL.Shifts.BusinessDayDetails?`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `businessDate` | `DateOnly` | no | — |
| `status` | `Enums.BusinessDayStatus` | no | — |
| `openedAt` | `DateTimeOffset` | no | — |
| `closedAt` | `DateTimeOffset?` | yes | — |
| `openedByUserId` | `Guid?` | yes | — |
| `closedByUserId` | `Guid?` | yes | — |

<a id="campionssystem-bll-shifts-cashcountdetails"></a>
## `CampionsSystem.BLL.Shifts.CashCountDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `shiftId` | `Guid?` | yes | — |
| `kind` | `string` | no | — |
| `expectedCash` | `decimal` | no | — |
| `countedCash` | `decimal` | no | — |
| `bookCash` | `decimal` | no | — |
| `difference` | `decimal` | no | — |
| `occurredAt` | `DateTimeOffset` | no | — |
| `businessDate` | `DateOnly` | no | — |
| `actorId` | `Guid` | no | — |
| `reason` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `observationOrder` | `long` | no | — |

<a id="campionssystem-bll-shifts-cashmovementdetails"></a>
## `CampionsSystem.BLL.Shifts.CashMovementDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `shiftId` | `Guid?` | yes | — |
| `kind` | `string` | no | — |
| `amount` | `decimal` | no | — |
| `occurredAt` | `DateTimeOffset` | no | — |
| `businessDate` | `DateOnly` | no | — |
| `reason` | `string` | no | — |

<a id="campionssystem-bll-shifts-closeshiftrequest"></a>
## `CampionsSystem.BLL.Shifts.CloseShiftRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `expectedVersion` | `long` | no | — |
| `countedCash` | `decimal` | no | `System.Text.Json.Serialization.JsonRequiredAttribute` |
| `reason` | `string?` | yes | — |

<a id="campionssystem-bll-shifts-countcashrequest"></a>
## `CampionsSystem.BLL.Shifts.CountCashRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `countedCash` | `decimal` | no | — |
| `reason` | `string` | no | — |
| `branchId` | `Guid?` | yes | — |

<a id="campionssystem-bll-shifts-drawerposition"></a>
## `CampionsSystem.BLL.Shifts.DrawerPosition`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `branchId` | `Guid` | no | — |
| `accountId` | `Guid` | no | — |
| `bookCash` | `decimal` | no | — |
| `expectedPhysicalCash` | `decimal` | no | — |
| `lastCountedCash` | `decimal` | no | — |
| `movementSequence` | `long` | no | — |
| `lastCountId` | `Guid?` | yes | — |
| `observedDiscrepancies` | `decimal` | no | — |

<a id="campionssystem-bll-shifts-expensecategorydetails"></a>
## `CampionsSystem.BLL.Shifts.ExpenseCategoryDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `name` | `string` | no | — |
| `isActive` | `bool` | no | — |

<a id="campionssystem-bll-shifts-expensecategoryrequest"></a>
## `CampionsSystem.BLL.Shifts.ExpenseCategoryRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `name` | `string` | no | — |
| `isActive` | `bool` | no | — |

<a id="campionssystem-bll-shifts-expensedetails"></a>
## `CampionsSystem.BLL.Shifts.ExpenseDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `shiftId` | `Guid?` | yes | — |
| `recordingEmployeeId` | `Guid?` | yes | — |
| `categoryId` | `Guid` | no | — |
| `categoryName` | `string` | no | — |
| `accountId` | `Guid` | no | — |
| `method` | `Enums.FinancialAccountType` | no | — |
| `amount` | `decimal` | no | — |
| `returnedAmount` | `decimal` | no | — |
| `description` | `string` | no | — |
| `receiptReference` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `occurredAt` | `DateTimeOffset` | no | — |
| `businessDate` | `DateOnly?` | yes | — |
| `paidAt` | `DateTimeOffset?` | yes | — |
| `originalStatus` | `Enums.ExpenseStatus` | no | — |
| `recordedByUserId` | `Guid?` | yes | — |

<a id="campionssystem-bll-shifts-expensereversaldetails"></a>
## `CampionsSystem.BLL.Shifts.ExpenseReversalDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `expenseId` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `shiftId` | `Guid` | no | — |
| `amount` | `decimal` | no | — |
| `reason` | `string` | no | — |
| `returnReference` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `occurredAt` | `DateTimeOffset` | no | — |
| `businessDate` | `DateOnly` | no | — |

<a id="campionssystem-bll-shifts-financialauditdetails"></a>
## `CampionsSystem.BLL.Shifts.FinancialAuditDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `event` | `string` | no | `System.Runtime.CompilerServices.NullableAttribute(1)` |
| `entityType` | `string?` | yes | — |
| `entityId` | `Guid?` | yes | — |
| `actorId` | `Guid?` | yes | — |
| `occurredAt` | `DateTimeOffset` | no | — |
| `before` | `string?` | yes | — |
| `after` | `string?` | yes | — |

<a id="campionssystem-bll-shifts-financialmovementdetails"></a>
## `CampionsSystem.BLL.Shifts.FinancialMovementDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `accountId` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `shiftId` | `Guid?` | yes | — |
| `sourceType` | `Enums.FinancialSourceType` | no | — |
| `direction` | `Enums.FinancialTransactionType` | no | — |
| `amount` | `decimal` | no | — |
| `occurredAt` | `DateTimeOffset` | no | — |
| `businessDate` | `DateOnly?` | yes | — |
| `actorId` | `Guid` | no | — |

<a id="campionssystem-bll-shifts-methodfinancialtotals"></a>
## `CampionsSystem.BLL.Shifts.MethodFinancialTotals`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `method` | `Enums.FinancialAccountType` | no | — |
| `collected` | `decimal` | no | — |
| `refunded` | `decimal` | no | — |
| `expensesPaid` | `decimal` | no | — |
| `expenseReturns` | `decimal` | no | — |
| `advancesPaid` | `decimal` | no | — |
| `withdrawals` | `decimal` | no | — |
| `initialPosition` | `decimal` | no | — |
| `otherCredits` | `decimal` | no | — |
| `otherDebits` | `decimal` | no | — |
| `netMovement` | `decimal` | no | — |
| `legacyUnshiftedNetMovement` | `decimal` | no | — |

<a id="campionssystem-bll-shifts-openbusinessdayrequest"></a>
## `CampionsSystem.BLL.Shifts.OpenBusinessDayRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `businessDate` | `DateOnly` | no | — |
| `branchId` | `Guid?` | yes | — |

<a id="campionssystem-bll-shifts-openshiftrequest"></a>
## `CampionsSystem.BLL.Shifts.OpenShiftRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `openingCash` | `decimal` | no | — |
| `branchId` | `Guid?` | yes | — |

<a id="campionssystem-bll-shifts-overridecloseshiftrequest"></a>
## `CampionsSystem.BLL.Shifts.OverrideCloseShiftRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `expectedVersion` | `long` | no | — |
| `reason` | `string` | no | — |
| `countedCash` | `decimal?` | yes | — |

<a id="campionssystem-bll-shifts-paidexpenserequest"></a>
## `CampionsSystem.BLL.Shifts.PaidExpenseRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `expenseCategoryId` | `Guid` | no | — |
| `financialAccountId` | `Guid` | no | — |
| `amount` | `decimal` | no | — |
| `description` | `string` | no | — |
| `receiptReference` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `branchId` | `Guid?` | yes | — |

<a id="campionssystem-bll-shifts-reverseexpenserequest"></a>
## `CampionsSystem.BLL.Shifts.ReverseExpenseRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `returnedAmount` | `decimal` | no | — |
| `fundsActuallyReturned` | `bool` | no | — |
| `reason` | `string` | no | — |
| `returnReference` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |

<a id="campionssystem-bll-shifts-setupdrawerrequest"></a>
## `CampionsSystem.BLL.Shifts.SetupDrawerRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `financialAccountId` | `Guid` | no | — |
| `countedCash` | `decimal` | no | — |
| `reason` | `string` | no | — |
| `branchId` | `Guid?` | yes | — |

<a id="campionssystem-bll-shifts-shiftaccountbalance"></a>
## `CampionsSystem.BLL.Shifts.ShiftAccountBalance`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `accountId` | `Guid` | no | — |
| `method` | `Enums.FinancialAccountType` | no | — |
| `bookBalance` | `decimal` | no | — |

<a id="campionssystem-bll-shifts-shiftbranchaccountreconciliation"></a>
## `CampionsSystem.BLL.Shifts.ShiftBranchAccountReconciliation`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `accountId` | `Guid` | no | — |
| `method` | `Enums.FinancialAccountType` | no | — |
| `openingBookBalance` | `decimal` | no | — |
| `inflows` | `decimal` | no | — |
| `outflows` | `decimal` | no | — |
| `calculatedClosingBookBalance` | `decimal` | no | — |
| `expectedClosingBookBalance` | `decimal?` | yes | — |
| `matchesExpectedClosingBalance` | `bool?` | yes | — |

<a id="campionssystem-bll-shifts-shiftbranchoperatortotals"></a>
## `CampionsSystem.BLL.Shifts.ShiftBranchOperatorTotals`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `actorId` | `Guid` | no | — |
| `actorName` | `string` | no | — |
| `method` | `Enums.FinancialAccountType` | no | — |
| `transactionCount` | `int` | no | — |
| `inflows` | `decimal` | no | — |
| `outflows` | `decimal` | no | — |
| `netMovement` | `decimal` | no | — |

<a id="campionssystem-bll-shifts-shiftbranchperiodsummary"></a>
## `CampionsSystem.BLL.Shifts.ShiftBranchPeriodSummary?`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `from` | `DateTimeOffset` | no | — |
| `cutoffAt` | `DateTimeOffset` | no | — |
| `isHistorical` | `bool` | no | — |
| `transactionCount` | `int` | no | — |
| `activity` | `IReadOnlyList<Shifts.MethodFinancialTotals>` | no | — |
| `operators` | `IReadOnlyList<Shifts.ShiftBranchOperatorTotals>` | no | — |
| `accounts` | `IReadOnlyList<Shifts.ShiftBranchAccountReconciliation>` | no | — |
| `expectedPhysicalCashAtCutoff` | `decimal?` | yes | — |
| `countedCashAtClosure` | `decimal?` | yes | — |
| `limitations` | `IReadOnlyList<string>` | no | — |

<a id="campionssystem-bll-shifts-shiftbranchtransactiondetails"></a>
## `CampionsSystem.BLL.Shifts.ShiftBranchTransactionDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `shiftId` | `Guid?` | yes | — |
| `accountId` | `Guid` | no | — |
| `method` | `Enums.FinancialAccountType` | no | — |
| `sourceType` | `Enums.FinancialSourceType` | no | — |
| `direction` | `Enums.FinancialTransactionType` | no | — |
| `amount` | `decimal` | no | — |
| `occurredAt` | `DateTimeOffset` | no | — |
| `postedAt` | `DateTimeOffset?` | yes | — |
| `businessDate` | `DateOnly?` | yes | — |
| `actorId` | `Guid` | no | — |
| `actorName` | `string` | no | — |
| `paymentId` | `Guid?` | yes | — |
| `refundId` | `Guid?` | yes | — |
| `expenseId` | `Guid?` | yes | — |
| `employeeAdvanceId` | `Guid?` | yes | — |
| `cashMovementId` | `Guid?` | yes | — |
| `payrollId` | `Guid?` | yes | — |

<a id="campionssystem-bll-shifts-shiftdetails"></a>
## `CampionsSystem.BLL.Shifts.ShiftDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `employeeId` | `Guid?` | yes | — |
| `employeeName` | `string?` | yes | — |
| `status` | `Enums.ShiftStatus` | no | — |
| `version` | `long` | no | — |
| `openedAt` | `DateTimeOffset` | no | — |
| `closedAt` | `DateTimeOffset?` | yes | — |
| `openingBusinessDate` | `DateOnly?` | yes | — |
| `closingBusinessDate` | `DateOnly?` | yes | — |
| `openingCash` | `decimal` | no | — |
| `expectedClosingCash` | `decimal?` | yes | — |
| `actualClosingCash` | `decimal?` | yes | — |
| `difference` | `decimal?` | yes | — |
| `closedByUserId` | `Guid?` | yes | — |
| `closingReason` | `string?` | yes | — |
| `closureType` | `string?` | yes | — |
| `verificationStatus` | `string?` | yes | — |
| `closingAccountBalances` | `IReadOnlyList<Shifts.ShiftAccountBalance>?` | yes | `System.Runtime.CompilerServices.NullableAttribute({2, 1})` |
| `closingCashBookBalance` | `decimal?` | yes | — |
| `closingInstaPayBookBalance` | `decimal?` | yes | — |

<a id="campionssystem-bll-shifts-shiftdetails"></a>
## `CampionsSystem.BLL.Shifts.ShiftDetails?`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `branchId` | `Guid` | no | — |
| `employeeId` | `Guid?` | yes | — |
| `employeeName` | `string?` | yes | — |
| `status` | `Enums.ShiftStatus` | no | — |
| `version` | `long` | no | — |
| `openedAt` | `DateTimeOffset` | no | — |
| `closedAt` | `DateTimeOffset?` | yes | — |
| `openingBusinessDate` | `DateOnly?` | yes | — |
| `closingBusinessDate` | `DateOnly?` | yes | — |
| `openingCash` | `decimal` | no | — |
| `expectedClosingCash` | `decimal?` | yes | — |
| `actualClosingCash` | `decimal?` | yes | — |
| `difference` | `decimal?` | yes | — |
| `closedByUserId` | `Guid?` | yes | — |
| `closingReason` | `string?` | yes | — |
| `closureType` | `string?` | yes | — |
| `verificationStatus` | `string?` | yes | — |
| `closingAccountBalances` | `IReadOnlyList<Shifts.ShiftAccountBalance>?` | yes | `System.Runtime.CompilerServices.NullableAttribute({2, 1})` |
| `closingCashBookBalance` | `decimal?` | yes | — |
| `closingInstaPayBookBalance` | `decimal?` | yes | — |

<a id="campionssystem-bll-shifts-shiftfinancialsummary"></a>
## `CampionsSystem.BLL.Shifts.ShiftFinancialSummary`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `shift` | `Shifts.ShiftDetails` | no | — |
| `activity` | `IReadOnlyList<Shifts.MethodFinancialTotals>` | no | — |
| `currentSharedDrawer` | `Shifts.DrawerPosition` | no | — |
| `currentSharedAccountBalances` | `IReadOnlyList<Shifts.ShiftAccountBalance>` | no | — |
| `branchPeriod` | `Shifts.ShiftBranchPeriodSummary?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |

<a id="campionssystem-bll-shifts-shiftfinancialtransactiondetails"></a>
## `CampionsSystem.BLL.Shifts.ShiftFinancialTransactionDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `shiftId` | `Guid` | no | — |
| `accountId` | `Guid` | no | — |
| `method` | `Enums.FinancialAccountType` | no | — |
| `sourceType` | `Enums.FinancialSourceType` | no | — |
| `direction` | `Enums.FinancialTransactionType` | no | — |
| `amount` | `decimal` | no | — |
| `occurredAt` | `DateTimeOffset` | no | — |
| `businessDate` | `DateOnly?` | yes | — |
| `actorId` | `Guid` | no | — |
| `paymentId` | `Guid?` | yes | — |
| `refundId` | `Guid?` | yes | — |
| `expenseId` | `Guid?` | yes | — |
| `employeeAdvanceId` | `Guid?` | yes | — |
| `cashMovementId` | `Guid?` | yes | — |
| `payrollId` | `Guid?` | yes | — |
| `actorName` | `string` | no | — |

<a id="campionssystem-bll-shifts-shifthistoryrequest"></a>
## `CampionsSystem.BLL.Shifts.ShiftHistoryRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `branchId` | `Guid?` | yes | — |
| `employeeId` | `Guid?` | yes | — |
| `from` | `DateOnly?` | yes | — |
| `to` | `DateOnly?` | yes | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-monthlyfinance-financialmonthdetails"></a>
## `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.MonthlyFinance.FinancialMonthDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<MonthlyFinance.FinancialMonthDetails>` | no | — |
| `total` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-monthlyfinance-financialtransactionreportrow"></a>
## `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.MonthlyFinance.FinancialTransactionReportRow>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<MonthlyFinance.FinancialTransactionReportRow>` | no | — |
| `total` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-monthlyfinance-maintenanceforecast"></a>
## `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.MonthlyFinance.MaintenanceForecast>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<MonthlyFinance.MaintenanceForecast>` | no | — |
| `total` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-monthlyfinance-montheventdetails"></a>
## `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.MonthlyFinance.MonthEventDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<MonthlyFinance.MonthEventDetails>` | no | — |
| `total` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-monthlyfinance-payrolldetails"></a>
## `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.MonthlyFinance.PayrollDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<MonthlyFinance.PayrollDetails>` | no | — |
| `total` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-monthlyfinance-payrollpaymentdetails"></a>
## `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.MonthlyFinance.PayrollPaymentDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<MonthlyFinance.PayrollPaymentDetails>` | no | — |
| `total` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-advancedetails"></a>
## `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.AdvanceDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Shifts.AdvanceDetails>` | no | — |
| `total` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-cashcountdetails"></a>
## `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.CashCountDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Shifts.CashCountDetails>` | no | — |
| `total` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-expensecategorydetails"></a>
## `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.ExpenseCategoryDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Shifts.ExpenseCategoryDetails>` | no | — |
| `total` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-expensedetails"></a>
## `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.ExpenseDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Shifts.ExpenseDetails>` | no | — |
| `total` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-expensereversaldetails"></a>
## `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.ExpenseReversalDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Shifts.ExpenseReversalDetails>` | no | — |
| `total` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-financialauditdetails"></a>
## `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.FinancialAuditDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Shifts.FinancialAuditDetails>` | no | — |
| `total` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-financialmovementdetails"></a>
## `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.FinancialMovementDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Shifts.FinancialMovementDetails>` | no | — |
| `total` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-shiftbranchtransactiondetails"></a>
## `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.ShiftBranchTransactionDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Shifts.ShiftBranchTransactionDetails>` | no | — |
| `total` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-shiftdetails"></a>
## `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.ShiftDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Shifts.ShiftDetails>` | no | — |
| `total` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-shifts-shiftpage-campionssystem-bll-shifts-shiftfinancialtransactiondetails"></a>
## `CampionsSystem.BLL.Shifts.ShiftPage<CampionsSystem.BLL.Shifts.ShiftFinancialTransactionDetails>`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Shifts.ShiftFinancialTransactionDetails>` | no | — |
| `total` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-shifts-vaulthistoryrequest"></a>
## `CampionsSystem.BLL.Shifts.VaultHistoryRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `branchId` | `Guid?` | yes | — |
| `accountId` | `Guid?` | yes | — |
| `from` | `DateOnly?` | yes | — |
| `to` | `DateOnly?` | yes | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-shifts-withdrawalrequest"></a>
## `CampionsSystem.BLL.Shifts.WithdrawalRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `amount` | `decimal` | no | — |
| `reason` | `string` | no | — |
| `branchId` | `Guid?` | yes | — |

<a id="campionssystem-bll-subscriptions-benefitsnapshot"></a>
## `CampionsSystem.BLL.Subscriptions.BenefitSnapshot`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `grantId` | `Guid` | no | — |
| `code` | `string` | no | — |
| `name` | `string` | no | — |
| `grantType` | `string` | no | — |
| `grantedQuantity` | `int?` | yes | — |
| `cancelled` | `bool` | no | — |

<a id="campionssystem-bll-subscriptions-completefreezerequest"></a>
## `CampionsSystem.BLL.Subscriptions.CompleteFreezeRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `expectedVersion` | `int` | no | — |

<a id="campionssystem-bll-subscriptions-customextensionrequest"></a>
## `CampionsSystem.BLL.Subscriptions.CustomExtensionRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `expectedVersion` | `int` | no | — |
| `days` | `int` | no | — |
| `reason` | `string` | no | — |

<a id="campionssystem-bll-subscriptions-employeeentitlementrequest"></a>
## `CampionsSystem.BLL.Subscriptions.EmployeeEntitlementRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `memberId` | `Guid` | no | — |
| `planId` | `Guid` | no | — |
| `operationId` | `Guid` | no | — |

<a id="campionssystem-bll-subscriptions-freezedetails"></a>
## `CampionsSystem.BLL.Subscriptions.FreezeDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `type` | `string` | no | — |
| `startDate` | `DateOnly` | no | — |
| `endDate` | `DateOnly?` | yes | — |
| `daysApplied` | `int?` | yes | — |
| `reason` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `approvedBy` | `Guid` | no | — |
| `createdAt` | `DateTimeOffset` | no | — |
| `completedAt` | `DateTimeOffset?` | yes | — |
| `daysAlreadyCompensated` | `int` | no | — |
| `previousSubscriptionEndDate` | `DateOnly?` | yes | — |
| `newSubscriptionEndDate` | `DateOnly?` | yes | — |

<a id="campionssystem-bll-subscriptions-freezerequest"></a>
## `CampionsSystem.BLL.Subscriptions.FreezeRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `operationId` | `Guid` | no | — |
| `expectedVersion` | `int` | no | — |
| `startDate` | `DateOnly` | no | — |
| `days` | `int` | no | — |

<a id="campionssystem-bll-subscriptions-holidayapplicationresult"></a>
## `CampionsSystem.BLL.Subscriptions.HolidayApplicationResult`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `closureId` | `Guid` | no | — |
| `protectedDays` | `int` | no | — |
| `extendedDays` | `int` | no | — |

<a id="campionssystem-bll-subscriptions-holidayextensiondetails"></a>
## `CampionsSystem.BLL.Subscriptions.HolidayExtensionDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `closureId` | `Guid` | no | — |
| `protectedDate` | `DateOnly` | no | — |
| `daysApplied` | `int` | no | — |

<a id="campionssystem-bll-subscriptions-memberentitlement"></a>
## `CampionsSystem.BLL.Subscriptions.MemberEntitlement`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `hasAccess` | `bool` | no | — |
| `subscription` | `Subscriptions.SubscriptionSummary?` | yes | — |

<a id="campionssystem-bll-subscriptions-officialholidayrequest"></a>
## `CampionsSystem.BLL.Subscriptions.OfficialHolidayRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `branchId` | `Guid` | no | — |
| `startDate` | `DateOnly` | no | — |
| `endDate` | `DateOnly` | no | — |
| `extendSubscriptions` | `bool` | no | — |
| `reason` | `string` | no | — |

<a id="campionssystem-bll-subscriptions-subscriptiondetails"></a>
## `CampionsSystem.BLL.Subscriptions.SubscriptionDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `subscription` | `Subscriptions.SubscriptionSummary` | no | — |
| `memberName` | `string` | no | — |
| `freezeAllowance` | `int?` | yes | — |
| `consumedFreezeDays` | `int` | no | — |
| `remainingFreezeDays` | `int?` | yes | — |
| `benefits` | `IReadOnlyList<Subscriptions.BenefitSnapshot>` | no | — |
| `grants` | `IReadOnlyList<Subscriptions.SubscriptionGrantDetails>` | no | — |
| `freezes` | `IReadOnlyList<Subscriptions.FreezeDetails>` | no | — |
| `holidays` | `IReadOnlyList<Subscriptions.HolidayExtensionDetails>` | no | — |

<a id="campionssystem-bll-subscriptions-subscriptiongrantdetails"></a>
## `CampionsSystem.BLL.Subscriptions.SubscriptionGrantDetails`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `planId` | `Guid` | no | — |
| `planName` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `planType` | `string` | no | — |
| `planBranchId` | `Guid?` | yes | — |
| `accessCityId` | `Guid?` | yes | — |
| `mode` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `durationDays` | `int` | no | — |
| `sessions` | `int?` | yes | — |
| `freezeDays` | `int?` | yes | — |
| `planPrice` | `decimal` | no | — |
| `netPrice` | `decimal` | no | — |
| `freelanceCoachId` | `Guid?` | yes | — |
| `transactionType` | `string` | no | — |
| `startDate` | `DateOnly?` | yes | — |
| `previousEndDate` | `DateOnly?` | yes | — |
| `newEndDate` | `DateOnly?` | yes | — |
| `previousRemainingSessions` | `int?` | yes | — |
| `renewedFromSubscriptionId` | `Guid?` | yes | — |
| `businessDayId` | `Guid?` | yes | — |
| `actorId` | `Guid?` | yes | — |
| `createdAt` | `DateTimeOffset` | no | — |
| `cancellationRefundId` | `Guid?` | yes | — |

<a id="campionssystem-bll-subscriptions-subscriptiongrantresult"></a>
## `CampionsSystem.BLL.Subscriptions.SubscriptionGrantResult`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `subscriptionId` | `Guid` | no | — |
| `grantId` | `Guid` | no | — |
| `scheduled` | `bool` | no | — |

<a id="campionssystem-bll-subscriptions-subscriptionhistory"></a>
## `CampionsSystem.BLL.Subscriptions.SubscriptionHistory`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `items` | `IReadOnlyList<Subscriptions.SubscriptionSummary>` | no | — |
| `totalCount` | `int` | no | — |
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |

<a id="campionssystem-bll-subscriptions-subscriptionhistoryrequest"></a>
## `CampionsSystem.BLL.Subscriptions.SubscriptionHistoryRequest`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `page` | `int` | no | — |
| `pageSize` | `int` | no | — |
| `branchId` | `Guid?` | yes | — |
| `status` | `Enums.SubscriptionStatus?` | yes | — |
| `planType` | `Enums.PlanCategory?` | yes | — |
| `from` | `DateOnly?` | yes | — |
| `to` | `DateOnly?` | yes | — |

<a id="campionssystem-bll-subscriptions-subscriptionsummary"></a>
## `CampionsSystem.BLL.Subscriptions.SubscriptionSummary`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `memberId` | `Guid` | no | — |
| `sourcePlanId` | `Guid` | no | — |
| `planName` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `planType` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `originBranchId` | `Guid` | no | — |
| `accessCityId` | `Guid` | no | — |
| `mode` | `string` | no | — |
| `startDate` | `DateOnly` | no | — |
| `endDate` | `DateOnly?` | yes | — |
| `status` | `string` | no | — |
| `totalSessions` | `int?` | yes | — |
| `remainingSessions` | `int?` | yes | — |
| `version` | `int` | no | — |
| `employeeId` | `Guid?` | yes | — |
| `followsSubscriptionId` | `Guid?` | yes | — |
| `isStandaloneVisit` | `bool` | no | — |

<a id="campionssystem-bll-subscriptions-subscriptionsummary"></a>
## `CampionsSystem.BLL.Subscriptions.SubscriptionSummary?`
| JSON property | CLR type | Nullable | Validation / serialization attributes |
|---|---|---|---|
| `id` | `Guid` | no | — |
| `memberId` | `Guid` | no | — |
| `sourcePlanId` | `Guid` | no | — |
| `planName` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `planType` | `string?` | yes | `System.Runtime.CompilerServices.NullableAttribute(2)` |
| `originBranchId` | `Guid` | no | — |
| `accessCityId` | `Guid` | no | — |
| `mode` | `string` | no | — |
| `startDate` | `DateOnly` | no | — |
| `endDate` | `DateOnly?` | yes | — |
| `status` | `string` | no | — |
| `totalSessions` | `int?` | yes | — |
| `remainingSessions` | `int?` | yes | — |
| `version` | `int` | no | — |
| `employeeId` | `Guid?` | yes | — |
| `followsSubscriptionId` | `Guid?` | yes | — |
| `isStandaloneVisit` | `bool` | no | — |

<a id="campionssystem-dal-enums-accessmode"></a>
## `CampionsSystem.DAL.Enums.AccessMode`
| Name | Numeric value |
|---|---:|
| `Unlimited` | 0 |
| `SessionBased` | 1 |

<a id="campionssystem-dal-enums-activitycoachtype"></a>
## `CampionsSystem.DAL.Enums.ActivityCoachType`
| Name | Numeric value |
|---|---:|
| `Internal` | 0 |
| `External` | 1 |

<a id="campionssystem-dal-enums-businessdaystatus"></a>
## `CampionsSystem.DAL.Enums.BusinessDayStatus`
| Name | Numeric value |
|---|---:|
| `Open` | 0 |
| `Closed` | 1 |

<a id="campionssystem-dal-enums-checkinmethod"></a>
## `CampionsSystem.DAL.Enums.CheckInMethod`
| Name | Numeric value |
|---|---:|
| `Scanner` | 0 |
| `Manual` | 1 |
| `Coach` | 2 |

<a id="campionssystem-dal-enums-checkintype"></a>
## `CampionsSystem.DAL.Enums.CheckInType`
| Name | Numeric value |
|---|---:|
| `Gym` | 0 |
| `BorrowedGym` | 1 |
| `Activity` | 2 |
| `Class` | 3 |
| `Trial` | 4 |

<a id="campionssystem-dal-enums-classsessionstatus"></a>
## `CampionsSystem.DAL.Enums.ClassSessionStatus`
| Name | Numeric value |
|---|---:|
| `Scheduled` | 0 |
| `Completed` | 1 |
| `Cancelled` | 2 |

<a id="campionssystem-dal-enums-employmentstatus"></a>
## `CampionsSystem.DAL.Enums.EmploymentStatus`
| Name | Numeric value |
|---|---:|
| `Active` | 0 |
| `Resigned` | 1 |
| `Suspended` | 2 |

<a id="campionssystem-dal-enums-employmenttype"></a>
## `CampionsSystem.DAL.Enums.EmploymentType`
| Name | Numeric value |
|---|---:|
| `Staff` | 0 |
| `Freelance` | 1 |

<a id="campionssystem-dal-enums-expensestatus"></a>
## `CampionsSystem.DAL.Enums.ExpenseStatus`
| Name | Numeric value |
|---|---:|
| `Recorded` | 0 |
| `Cancelled` | 1 |

<a id="campionssystem-dal-enums-financialaccounttype"></a>
## `CampionsSystem.DAL.Enums.FinancialAccountType`
| Name | Numeric value |
|---|---:|
| `Cash` | 0 |
| `InstaPay` | 1 |
| `Bank` | 2 |
| `Card` | 3 |
| `Wallet` | 4 |

<a id="campionssystem-dal-enums-financialsourcetype"></a>
## `CampionsSystem.DAL.Enums.FinancialSourceType`
| Name | Numeric value |
|---|---:|
| `Payment` | 0 |
| `Refund` | 1 |
| `Expense` | 2 |
| `Payroll` | 3 |
| `Adjustment` | 4 |
| `EmployeeAdvance` | 5 |
| `CashMovement` | 6 |

<a id="campionssystem-dal-enums-financialtransactiontype"></a>
## `CampionsSystem.DAL.Enums.FinancialTransactionType`
| Name | Numeric value |
|---|---:|
| `Credit` | 0 |
| `Debit` | 1 |

<a id="campionssystem-dal-enums-interactiontype"></a>
## `CampionsSystem.DAL.Enums.InteractionType`
| Name | Numeric value |
|---|---:|
| `Call` | 0 |
| `WhatsApp` | 1 |
| `Visit` | 2 |
| `Other` | 3 |

<a id="campionssystem-dal-enums-leadstatus"></a>
## `CampionsSystem.DAL.Enums.LeadStatus`
| Name | Numeric value |
|---|---:|
| `New` | 0 |
| `Contacted` | 1 |
| `Interested` | 2 |
| `FollowUp` | 3 |
| `Converted` | 4 |
| `Lost` | 5 |
| `Inactive` | 6 |

<a id="campionssystem-dal-enums-membershipbenefitgranttype"></a>
## `CampionsSystem.DAL.Enums.MembershipBenefitGrantType`
| Name | Numeric value |
|---|---:|
| `Quantity` | 0 |
| `Access` | 1 |

<a id="campionssystem-dal-enums-offereligibilitymode"></a>
## `CampionsSystem.DAL.Enums.OfferEligibilityMode`
| Name | Numeric value |
|---|---:|
| `General` | 0 |
| `MemberOnly` | 1 |

<a id="campionssystem-dal-enums-offertype"></a>
## `CampionsSystem.DAL.Enums.OfferType`
| Name | Numeric value |
|---|---:|
| `PercentageDiscount` | 0 |
| `FixedDiscount` | 1 |
| `ExtraDays` | 2 |

<a id="campionssystem-dal-enums-plancategory"></a>
## `CampionsSystem.DAL.Enums.PlanCategory`
| Name | Numeric value |
|---|---:|
| `Normal` | 0 |
| `Freelance` | 1 |
| `Employee` | 2 |

<a id="campionssystem-dal-enums-purchasemode"></a>
## `CampionsSystem.DAL.Enums.PurchaseMode`
| Name | Numeric value |
|---|---:|
| `FullPayment` | 0 |
| `Reservation` | 1 |
| `Installment` | 2 |
| `StandaloneSingleSession` | 3 |

<a id="campionssystem-dal-enums-shiftstatus"></a>
## `CampionsSystem.DAL.Enums.ShiftStatus`
| Name | Numeric value |
|---|---:|
| `Open` | 0 |
| `Closed` | 1 |

<a id="campionssystem-dal-enums-subscriptionactivationmode"></a>
## `CampionsSystem.DAL.Enums.SubscriptionActivationMode`
| Name | Numeric value |
|---|---:|
| `Now` | 0 |
| `FirstCheckIn` | 1 |
| `SpecificDate` | 2 |

<a id="campionssystem-dal-enums-subscriptionstatus"></a>
## `CampionsSystem.DAL.Enums.SubscriptionStatus`
| Name | Numeric value |
|---|---:|
| `Scheduled` | 0 |
| `Active` | 1 |
| `Frozen` | 2 |
| `Expired` | 3 |
| `Cancelled` | 4 |
| `SuspendedForInstallment` | 5 |

## Access Administration response schemas
The nine Owner administration actions declare success types using `ProducesResponseType`; their JSON schemas are present in the **runtime** OpenAPI, though the compiler wrapper inventory does not traverse these response-only types. Use the runtime schemas below rather than inferring their fields from names.

### `AccessAuditDetails`
| JSON property | Runtime OpenAPI schema | Required |
|---|---|---|
| `auditId` | `{"type":"string","format":"uuid"}` | yes |
| `operationId` | `{"type":"string","format":"uuid"}` | yes |
| `actorUserId` | `{"type":["null","string"],"format":"uuid"}` | yes |
| `timestamp` | `{"type":"string","format":"date-time"}` | yes |
| `kind` | `{"type":"string"}` | yes |
| `targetUserId` | `{"type":["null","string"],"format":"uuid"}` | yes |
| `roleId` | `{"type":["null","string"],"format":"uuid"}` | yes |
| `permissionId` | `{"type":["null","string"],"format":"uuid"}` | yes |
| `reason` | `{"type":"string"}` | yes |
| `oldValues` | `{"type":"string"}` | yes |
| `newValues` | `{"type":"string"}` | yes |
| `result` | `{"$ref":"#/components/schemas/AccessChangeResult"}` | yes |

### `AccessBranchAssignment`
| JSON property | Runtime OpenAPI schema | Required |
|---|---|---|
| `id` | `{"type":"string","format":"uuid"}` | yes |
| `branchId` | `{"type":"string","format":"uuid"}` | yes |
| `branchName` | `{"type":"string"}` | yes |
| `branchIsActive` | `{"type":"boolean"}` | yes |
| `startDate` | `{"type":"string","format":"date"}` | yes |
| `endDate` | `{"type":["null","string"],"format":"date"}` | yes |
| `isPrimary` | `{"type":"boolean"}` | yes |

### `AccessChangeResult`
| JSON property | Runtime OpenAPI schema | Required |
|---|---|---|
| `operationId` | `{"type":"string","format":"uuid"}` | yes |
| `changed` | `{"type":"boolean"}` | yes |
| `version` | `{"type":"string"}` | yes |

### `AccessEmployee`
| JSON property | Runtime OpenAPI schema | Required |
|---|---|---|
| `id` | `{"type":"string","format":"uuid"}` | yes |
| `fullName` | `{"type":"string"}` | yes |
| `status` | `{"type":"string"}` | yes |
| `hireDate` | `{"type":"string","format":"date"}` | yes |
| `terminationDate` | `{"type":["null","string"],"format":"date"}` | yes |

### `AccessMode`
| JSON property | Runtime OpenAPI schema | Required |
|---|---|---|

### `AccessPageOfAccessAuditDetails`
| JSON property | Runtime OpenAPI schema | Required |
|---|---|---|
| `items` | `{"type":"array","items":{"$ref":"#/components/schemas/AccessAuditDetails"}}` | yes |
| `totalCount` | `{"pattern":"^-?(?:0|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | yes |
| `page` | `{"pattern":"^-?(?:0|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | yes |
| `pageSize` | `{"pattern":"^-?(?:0|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | yes |

### `AccessPageOfAccessUserItem`
| JSON property | Runtime OpenAPI schema | Required |
|---|---|---|
| `items` | `{"type":"array","items":{"$ref":"#/components/schemas/AccessUserItem"}}` | yes |
| `totalCount` | `{"pattern":"^-?(?:0|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | yes |
| `page` | `{"pattern":"^-?(?:0|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | yes |
| `pageSize` | `{"pattern":"^-?(?:0|[1-9]\\d*)$","type":["integer","string"],"format":"int32"}` | yes |

### `AccessRequest`
| JSON property | Runtime OpenAPI schema | Required |
|---|---|---|
| `memberId` | `{"type":["null","string"],"format":"uuid"}` | yes |
| `memberCode` | `{"type":["null","string"]}` | yes |
| `branchId` | `{"type":["null","string"],"format":"uuid"}` | no |

### `AccessRole`
| JSON property | Runtime OpenAPI schema | Required |
|---|---|---|
| `id` | `{"type":"string","format":"uuid"}` | yes |
| `name` | `{"type":"string"}` | yes |
| `isOwner` | `{"type":"boolean"}` | yes |

### `AccessUserItem`
| JSON property | Runtime OpenAPI schema | Required |
|---|---|---|
| `userId` | `{"type":"string","format":"uuid"}` | yes |
| `userName` | `{"type":["null","string"]}` | yes |
| `isActive` | `{"type":"boolean"}` | yes |
| `employeeId` | `{"type":["null","string"],"format":"uuid"}` | yes |
| `employeeName` | `{"type":["null","string"]}` | yes |

### `PermissionModule`
| JSON property | Runtime OpenAPI schema | Required |
|---|---|---|
| `module` | `{"type":"string"}` | yes |
| `permissions` | `{"type":"array","items":{"$ref":"#/components/schemas/PermissionCatalogItem"}}` | yes |

### `RoleAccessDetails`
| JSON property | Runtime OpenAPI schema | Required |
|---|---|---|
| `role` | `{"$ref":"#/components/schemas/AccessRole"}` | yes |
| `version` | `{"type":"string"}` | yes |
| `grants` | `{"type":"array","items":{"$ref":"#/components/schemas/PermissionCatalogItem"}}` | yes |

### `UserAccessDetails`
| JSON property | Runtime OpenAPI schema | Required |
|---|---|---|
| `userId` | `{"type":"string","format":"uuid"}` | yes |
| `userName` | `{"type":["null","string"]}` | yes |
| `isActive` | `{"type":"boolean"}` | yes |
| `isLockedOut` | `{"type":"boolean"}` | yes |
| `runtimeAccountEligible` | `{"type":"boolean"}` | yes |
| `employee` | `{"oneOf":[{"type":"null"},{"$ref":"#/components/schemas/AccessEmployee"}]}` | yes |
| `currentBranch` | `{"oneOf":[{"type":"null"},{"$ref":"#/components/schemas/BranchContext"}]}` | yes |
| `hasGlobalBranchScope` | `{"type":"boolean"}` | yes |
| `branchAssignments` | `{"type":"array","items":{"$ref":"#/components/schemas/AccessBranchAssignment"}}` | yes |
| `roles` | `{"type":"array","items":{"$ref":"#/components/schemas/AccessRole"}}` | yes |
| `version` | `{"type":"string"}` | yes |
| `permissions` | `{"type":"array","items":{"$ref":"#/components/schemas/PermissionExplanation"}}` | yes |
