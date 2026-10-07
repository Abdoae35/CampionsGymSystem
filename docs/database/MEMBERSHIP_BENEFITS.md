# Membership benefits alignment

## Initial gap analysis

- Already supported: MembershipPlans, direct subscription purchase/renewal transaction
  history, price/category/coach snapshots, subscription and transaction dates, Guid IDs,
  restricted historical foreign keys, and the existing AuditLog.
- Partially supported: purchase transactions provide source attribution and date context
  for grants, but did not store any benefit configuration or snapshot.
- Missing: dynamic benefit catalog, per-plan Quantity/Access configuration,
  purchase-batch benefit snapshots and quantity usage history. No equivalent existed.

## Decisions

Four new tables only; no existing business table columns change. Benefits are dynamic
records, not enums or per-benefit columns. The only enum is MembershipBenefitGrantType:
Quantity or Access, configured independently for each plan and snapshotted per purchase.

Each grant has one MembershipSubscriptionTransactionId. Its subscription is directly
available through that transaction, so a second subscription FK would duplicate truth.
Guid PKs follow existing entity conventions; unique pair indexes enforce plan and
purchase-batch benefit uniqueness. Grants reference the catalog for identity and carry
independent code/name/type/quantity snapshots, without linking to mutable plan-benefit
configuration. Existing historical grants therefore survive configuration removal.

No ValidFrom/ValidUntil fields: existing subscription dates, transaction PreviousEndDate,
NewEndDate, PurchasedDurationDays and CreatedAt retain source context. Exact grant
validity, carryover/expiry, reset and FIFO behavior are not finalized. No new policy was
invented. Existing dates do not by themselves establish that unresolved policy.

Quantity usages belong to a specific grant and record actual branch, optional user,
quantity and time. Remaining quantity will be calculated from granted quantity minus
summed usage; future BLL must validate availability, type, authority, dates and concurrency.
Access grants store NULL quantity and represent entitlement without artificial counts.
No controllers, purchase/usage services, automatic grant creation, financial records,
authorization components, activity benefits or new audit system were added.

## Changed files

- DAL/Entities: MembershipBenefit, MembershipPlanBenefit, MemberSubscriptionBenefit,
  MemberBenefitUsage.
- DAL/Configurations: corresponding four IEntityTypeConfiguration classes.
- DAL/Enums/MembershipBenefitGrantType.cs; DAL/Data/ApplicationDbContext.cs (four DbSets).
- DAL/Migrations: new AddMembershipBenefits migration/designer and updated model snapshot.
- DAL.Tests/MembershipBenefitsTests.cs: PostgreSQL constraints/history and migration tests.
- docs/database: SCHEMA.md, DATA_FLOWS.md, BUSINESS_RULES_NOT_ENFORCED_BY_DB.md,
  this report and generated AddMembershipBenefits.sql.
- PROJECT_HISTORY.md and AGENTS.md: required checkpoint.

Paths above are relative to the CampionsSystem project folders where applicable.
Previous migration files must remain unchanged. No development or production migrations
are applied by this task. Integration tests use isolated PostgreSQL schemas inside
rolled-back transactions, not the gym application schema. Deployment is separate.

## Migration review

`20261007153001_AddMembershipBenefits` creates four tables, seven restricted foreign
keys, three check constraints, four unique secondary indexes and five nonunique
indexes. Its forward Up/SQL contains no existing-table alteration, drop, rename or
backfill. Standard Down removes only these four new tables and would discard their
new benefit data if rollback were explicitly requested; it has not been run.

All eight previous migration source/designer files were verified byte-for-byte with
SHA-256, including `20261007151235_FinalBusinessSchemaAlignment`. Only the current
EF model snapshot changes alongside the newly generated migration.

## Verification

Build before migration and final build: zero warnings/errors. Full suite: **156 passed,
zero failed/skipped**, including 28 new benefit test cases. An initial migration assertion
used culture-sensitive table-name ordering; it was corrected to ordinal ordering before
the final successful full run. EF reports no pending model changes. Forward SQL was
reviewed; only new benefit tables/indexes are created. No application database update.
Changes remain uncommitted/unpushed on DataBaseInit for user review.
