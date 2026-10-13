# Campeones database guide

Start with one module at a time. This is a source-derived guide to the current
EF Core/PostgreSQL schema at **`c3f77b5`**, on **`Permissions&Administration`**.
It documents **112 tables in a fully migrated database**: **104 application tables,
7 ASP.NET Identity tables and `__EFMigrationsHistory`**. The EF model itself maps 111.
The actual deployed database count is **not verified**: no database was accessed.

## Reading order

1. [Overview ERD and relationship legend](OVERVIEW.md): the main connections.
2. [Complete inventory](TABLE_INVENTORY.md): every table, category, purpose and PK.
3. Choose a module below: small ERDs, table cards, all FKs and source links.
4. [Business data flow](DATA_FLOW.md): registration through financial closing.
5. [Review flags](REVIEW_FLAGS.md): questions for later QA, with evidence.
6. [Analysis scope and validation](ANALYSIS_NOTES.md): provenance and limitations.

| Module | Tables | Begin here |
|---|---:|---|
| Authentication / Employees / Branches | 15 | [Accounts, permissions and assignments](01-auth-employees-branches.md) |
| Members / CRM | 7 | [Contacts and commercial ownership](02-members-crm.md) |
| Subscriptions / Check-In | 17 | [Plans, access and attendance](03-subscriptions-check-in.md) |
| Sales / Payments | 14 | [Sales, collections and refunds](04-sales-payments.md) |
| Shifts / Vault / Expenses | 13 | [Drawer and movement ledger](05-shifts-vault-expenses.md) |
| Sales Commissions | 6 | [Collection attribution and finalized statements](06-commissions.md) |
| Private / Classes / Activities | 24 | [Coaching purchases, attendance and earnings](07-private-classes.md) |
| Monthly Closing / Payroll | 9 | [Financial periods and employee obligations](08-monthly-closing-payroll.md) |
| Supporting infrastructure | 7 | [Audit, contracts, messages and EF metadata](09-supporting-infrastructure.md) |
| **Total** | **112** | |

Each table has exactly one primary module in the inventory; cross-module diagrams
repeat references without adding to the count. The Private/Classes page also covers
the existing separate Activities and Freelance tables so none are left out.
Infrastructure includes the EF migration-history table; Identity tables are grouped
with authentication. FinancialOccurrenceEvidence is grouped with monthly reporting;
EmployeeAdvances stays with actual money operations, while salary settings are in payroll.

## Table categories

- **Core:** a customer, entitlement, operation or payable business record.
- **Configuration:** plans, capability grants, rules or settings.
- **History:** dated assignments, revisions or past operational changes.
- **Evidence:** the facts needed to explain a collection, recognition or frozen result.
- **Junction:** connects records in a many-to-many or allocation relationship.
- **Audit:** records actors, reasons, timestamps and changes.
- **Framework/infrastructure:** Identity or application support records.

Categories can overlap. A historical price snapshot is deliberate evidence, not
automatically a duplicate to remove. Immutability depends on actual constraints
and service/context guards, not the label used in this guide.

For exact stored columns, types, nullability, PKs, all **355 FKs**, unique indexes,
CHECK expressions and source references, use [schema.json](schema.json). It is a
documentation artifact, not a migration or a database dump. SQL nullable columns
may still be mandatory for current API operations because of business validation.

Step 1 only: no refactoring, implementation, migration execution, data access,
commit or push was performed. Existing frontend-handoff documents are preserved.
