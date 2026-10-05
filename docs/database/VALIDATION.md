# InitialCreate verification

Verified on local PostgreSQL 14.20, database `campions_gym`, using the configured
development account. No production database was touched.

- `dotnet build CampionsGymSystem.slnx`: zero warnings and errors.
- DAL migration `20261005185159_InitialCreate` generated and compiled.
- `dotnet ef migrations script`: generated `InitialCreate.sql`.
- `dotnet ef migrations has-pending-model-changes`: no model/snapshot changes.
- `dotnet ef database update`: succeeded; migration recorded in local history.
- PostgreSQL catalog assertions: 61 application tables, 7 Identity tables,
  0 cascading business FKs, 4 partial unique indexes, 2 AuditLog jsonb columns,
  1 applied migration, and 0 leftover synthetic users.
- `validate-local.sql`: 17 expected rejections, including duplicate member code,
  phone, employee user, current sales/primary-branch/private-coach assignments,
  open branch periods, negative payment parts, invalid FKs, missing session
  counts, session bounds, percentage/date ranges, mismatched ledger source,
  missing gym subscription FK, and deletes of referenced history.
- Positive transactional fixtures: split payments/refunds, explicit-source
  ledger posting, unlimited gym attendance referencing a subscription, historical
  assignments alongside current ones, JSON audit values, cross-branch summary.
  All fixture rows rolled back.
- Isolated C# EF metadata verification: 68 entity types, no shadow FKs, restricted
  business deletes, decimal precision, nonempty new ApplicationUser Guid, and
  synchronous/async rejection of tracked audit changes/deletes. It did not open
  a database connection.

The first fixture run omitted standard Identity non-null flags; it failed and
rolled back. The corrected fixture populated those fields and passed. Initial
secret-key migration needed UTF-8 BOM support and was corrected without exposing
values. Expected history-table-not-found messages occurred before the first
migration created the history table; the completed update succeeded.

These are schema/persistence checks, not security certification or tests of
unimplemented business workflows. Audit append-only guards do not intercept
raw SQL/bulk APIs; production grants and BLL policies remain necessary.
