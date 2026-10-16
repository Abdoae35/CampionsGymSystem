# Optional Excel bulk registration (V1 addendum)

Excel import is an additional way to create **new** Leads or Members. Existing single-Lead, single-Member, FreelanceHouse and Lead-conversion endpoints remain available and unchanged. Import never creates a subscription, Sale, Payment or financial ledger row. It does not import historical subscriptions or payments.

## Routes and access

All routes require a bearer JWT. `{kind}` is exactly `leads`, `members`, or `freelance-customers`; any other value returns 400. The importing user owns a 30-minute, server-stored preview session. A different user cannot confirm or read it. Existing effective Allow/Deny permissions and branch scope are checked on every call; confirmation invokes the same creation service as manual registration.

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/excel-imports/{kind}/template` | Download `Campeones-{kind}-import-template.xlsx` |
| POST | `/api/excel-imports/{kind}/preview` | Multipart form field **`File`** (`.xlsx`, up to 1 MiB); validate and persist preview only |
| POST | `/api/excel-imports/{id}/confirm` | Explicitly process eligible `Ready` rows |
| GET | `/api/excel-imports/{id}` | Re-read current row results before session expiry |

For Leads, `Leads.Manage` is required; without `Leads.Assign`, the manual self-assignment rule also requires a linked Sales employee with `Members.OwnSalesCustomers`. For Members, `Members.Create` and an eligible open **own Shift** in the operational branch are required. `NormalSales` requires the actor's `Members.OwnSalesCustomers`; cross-branch registration additionally requires `Members.CreateCrossBranch`. `ManagementCreated` requires `Members.AssignSales` and an eligible `SalesEmployeeId`. Freelance customers require `Members.Create`, `FreelancePlans.ManageEligibleMembers`, an open own Shift, and an active authorized Freelance plan with an active Freelance coach in the registration city. No new import capability is granted or needed.

## Workbook columns

Each template contains a **Data** worksheet with the exact header names below and an **Instructions** worksheet with sample values. Enter records from Data row 2. Keep phone cells as text so a local `0` is preserved. Dates are literal `YYYY-MM-DD`; IDs are UUIDs obtained from the authorized branch, employee and plan APIs. The workbook contains examples only in Instructions, never real customer data.

| Kind | Required columns | Optional columns |
|---|---|---|
| `leads` | `BranchId`, `PrimaryPhone` | `FullName`, `SecondaryPhone`, `Email`, `Source`, `Notes` |
| `members` | `TargetRegistrationBranchId`, `CreationType`, `PrimaryPhone`, `FullName` | `SecondaryPhone`, `DateOfBirth`, `Gender`, `Notes`, `SalesEmployeeId` (**required for `ManagementCreated` only**) |
| `freelance-customers` | `TargetRegistrationBranchId`, `FreelancePlanId`, `PrimaryPhone`, `FullName` | `SecondaryPhone`, `DateOfBirth`, `Gender`, `Notes` |

`CreationType` is exactly `NormalSales` or `ManagementCreated`. `NormalSales` resolves the actor as Sales owner and rejects a supplied `SalesEmployeeId`; `ManagementCreated` requires an eligible owner. Freelance import uses existing `FreelanceHouse` semantics: the new Member is House and receives plan eligibility, not a paid subscription. Leads retain their `Source` such as Instagram, Facebook, WhatsApp or a campaign name.

The server accepts at most **100 nonempty rows** and a 1 MiB `.xlsx` file. Unknown/repeated columns, missing required headers, excessive rows/cells, invalid ZIP/XML components, formulas, macros and external links reject the workbook. Each row then checks required values, phone normalization using the branch calling code, dates, field lengths, authorization, Shift/plan eligibility and existing Lead/Member phones. A repeated normalized phone within the sheet or an existing phone is marked `Duplicate`. Validation errors are row-specific; malformed workbook structure returns HTTP 400 before a preview session is saved.

## Preview and confirmation contract

`ExcelImportResult` has `importId`, `kind`, `expiresAt`, `totalRows`, `validRows`, `invalidRows`, `duplicateRows`, `createdRows`, `failedRows`, `rows` and `warnings`. Each row contains its Excel `rowNumber`, `status`, `message`, nullable `createdRecordId`, and `values` (the submitted column/value map). Preview statuses are `Ready`, `Invalid`, `Duplicate`. After confirmation, a Ready row becomes `Created`, `Duplicate` or `Failed`. Counts reflect the **current** state; `validRows` decreases as Ready rows finish. Invalid and duplicate preview rows are skipped, so partial success is explicit.

Preview creates only the protected import session and its row evidence; it creates no Lead or Member. Confirmation reads the stored row request, not an editable client preview payload. It rechecks current identity, effective capabilities, branch, Shift, phone uniqueness and all manual-service rules. Each Ready row and its result commit in one database transaction under a row lock. Concurrent confirmations and identical retries return the stored outcome without repeating creation. A database/concurrency 409 can be retried with the **same import ID**; already completed rows are skipped. A row that fails a business rule is final for that session; download/correct/re-preview the file to try it again. Earlier successful rows remain committed. The result endpoint is available only to the original uploader while the session is unexpired.

Example (fabricated UUIDs and phone):

```http
GET /api/excel-imports/leads/template
Authorization: Bearer EXAMPLE_TOKEN
```

```http
POST /api/excel-imports/leads/preview
Authorization: Bearer EXAMPLE_TOKEN
Content-Type: multipart/form-data

File=@leads.xlsx
```

```json
{"importId":"11111111-1111-4111-8111-111111111111","kind":"leads","expiresAt":"2026-10-17T14:30:00Z","totalRows":2,"validRows":1,"invalidRows":0,"duplicateRows":1,"createdRows":0,"failedRows":0,"rows":[{"rowNumber":2,"status":"Ready","message":null,"createdRecordId":null,"values":{"BranchId":"22222222-2222-4222-8222-222222222222","PrimaryPhone":"01012345678","Source":"Instagram"}},{"rowNumber":3,"status":"Duplicate","message":"Phone repeats within this workbook.","createdRecordId":null,"values":{"BranchId":"22222222-2222-4222-8222-222222222222","PrimaryPhone":"+201012345678"}}],"warnings":["Preview is advisory. Confirmation rechecks current permissions, branch scope, Shift, eligibility and phone uniqueness."]}
```

```http
POST /api/excel-imports/11111111-1111-4111-8111-111111111111/confirm
Authorization: Bearer EXAMPLE_TOKEN
```

Use `GET /api/excel-imports/{id}` after a timeout before retrying confirmation. React should show the sheet row number, submitted values and message; require a deliberate Confirm action, display created/failed/skipped totals, and let the user correct failed rows in a new workbook. Send `FormData` with field `File`; do not set multipart `Content-Type` manually because the browser supplies the boundary.

Errors follow existing API conventions: 400 `ProblemDetails` for a bad file/type or invalid workbook, 401 for unauthenticated requests, 403 for an expired session, wrong uploader or missing capability/scope, 404 for an unknown import ID, and 409 for a concurrent database conflict. The server may reject an oversized HTTP body with 413 before application validation. Backend sources are `CampionsSystem.API/Controllers/ExcelImportsController.cs`, `CampionsSystem.BLL/Imports/ExcelImportService.cs`, `ExcelImportWorkbook.cs`, and the existing Lead/Member creation services.

**Activation:** migration `20261016201337_AddExcelImportSessions` adds only `ExcelImportBatches` and `ExcelImportRows`; it must be separately approved/applied to the target environment before these endpoints work. It was not applied to Development or Production by this feature implementation. Existing V1 grants still require Owner review as described in [operational gaps](OPEN_QUESTIONS_AND_GAPS.md). No historical financial import is provided.
