# ExcelImports API

See [Excel bulk import](../EXCEL_BULK_IMPORT.md) for exact template columns, React flow, permission/Shift rules, row statuses, example payloads and activation.

| Method | Route | Request | Response |
|---|---|---|---|
| GET | `/api/excel-imports/{kind}/template` | `kind`: `leads`, `members`, `freelance-customers` | `.xlsx` file |
| POST | `/api/excel-imports/{kind}/preview` | multipart `File` (`.xlsx`, ≤1 MiB, ≤100 rows) | `ExcelImportResult` |
| POST | `/api/excel-imports/{id}/confirm` | no body | `ExcelImportResult` |
| GET | `/api/excel-imports/{id}` | no body | `ExcelImportResult` |

All require authentication plus kind-specific existing manual-create capabilities. Results/confirmation belong to the original uploader, expire after 30 minutes, and are reauthorized. The backend creates rows only on confirmation. `ExcelImportResult` fields and statuses are described in the feature guide; the OpenAPI schema is in [integration OpenAPI](../openapi.integration.json).
