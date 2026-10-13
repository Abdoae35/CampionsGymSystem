# Read-only analysis checkpoint

Date: 2026-10-13. Requested scope: Backend Stabilization & QA, Step 1 only,
database documentation and ERDs. Baseline branch `Permissions&Administration`,
HEAD `c3f77b5`; the initial working tree was clean.

## Evidence and counting method

1. Read AGENTS.md and PROJECT_HISTORY.md, verified Git state, and inspected
   ApplicationDbContext, current entities/configurations and migrations.
2. Extracted the current model snapshot's distinct physical table mappings,
   properties, PKs, relationships, nullability, indexes and CHECK expressions.
   Cross-checked all application mappings against their entity/configuration files.
   Framework Identity mappings come from the inherited IdentityDbContext model.
3. Reconciled table creation/removal across all **27 ordered migration Up methods**
   with the snapshot's table set. Both have **111 distinct mapped tables**.
4. Included **`__EFMigrationsHistory`** separately, as shown in existing generated
   EF SQL; it is framework metadata outside ApplicationDbContext's mapped entities.
5. Distinguished configured EF one-to-one links from one-to-one enforced by a PK
   or unique FK index. A conditional unique index limited to managed operations
   is not represented as unconditional one-to-one for all legacy records.

The documented full schema is **112 tables: 104 application, 7 Identity, 1 EF history**,
with **355 declared FKs**. No additional history, period, ledger or attendance table
was invented. Source fingerprints and exact extracted facts are in schema.json.
All 185 entity/configuration source files were read for this extraction; selected
BLL workflows were checked to explain data flow and review candidates.

Latest migration inspected:
`20261013125623_ImplementOwnerAccessAdministration`, which adds an audit-operation
index without adding a table. Snapshot and latest migration target have the same
table/property/key relationship mappings. No EF command, build, test, migration
execution or application startup was needed for this documentation task.

## Scope limitations

- No database connection or data query was made, including Development or Production.
  Installed migration state, live counts and data quality remain unknown.
- Exact meanings of opaque historical JSON, polymorphic AuditLogs.EntityId and
  unconstrained revision/operation identifiers require their writer, not a guessed FK.
- Classification by business module is a reading aid, not a database schema split.
- A table or nullable field does not establish an implemented API/workflow.
- Mermaid uses ordinary ER syntax and is checked against actual FK/key/cardinality
  metadata. No locally available Mermaid renderer was found, so graphical rendering
  and layout review in a specific viewer remain unverified.

## Completed validation

- All 112 tables appear once in the inventory and have an individual module card
  with a short purpose, classification, PK, all FKs and source references.
- All 355 FK definitions reference existing mapped tables and valid child columns.
- All 24 ERDs were structurally/syntax checked against the actual PK/FK metadata;
  every table appears in at least one diagram. The high-level ERD has 14 representative
  tables and 16 selected links; each module diagram has at most 10 tables/18 links.
- All local documentation/source links and inventory anchors resolve.
- Latest migration target and current snapshot model blocks match exactly after
  whitespace normalization; table sets also reconcile across all migration Up methods.
- Source hashes confirm inspected entities/configurations and the snapshot stayed
  unchanged; Git reports only the new documentation directory, at the original HEAD.

During extraction, repeated provider-annotation references to integer identity
properties initially obscured their SQL types; the extraction was corrected and
all physical column types were rechecked. No source/model change was needed.

## Preservation and handoff

Only documentation artifacts under docs/database-guide/ were created. Existing
frontend-handoff documents, code, configurations, migrations and Git HEAD were
preserved. No branch switch, reset, staging, commit or push is part of this task.

The normal project-history-update skill requires changes to PROJECT_HISTORY.md and
AGENTS.md. The user's explicit folder-only restriction takes precedence, so those
files are intentionally unchanged; this local checkpoint records the work instead.
Proceed no further than documentation. Review flags are for a later approved QA
step, not instructions to implement/refactor now.
