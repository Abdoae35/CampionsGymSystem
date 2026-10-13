# Questions for later stabilization and QA

[Navigation](README.md) · [Exact model facts](schema.json)

These are **review candidates**, not confirmed production defects. No live rows,
execution plans or operational configuration were examined, and no fixes are proposed
for automatic implementation. Evidence references are current code, not assumptions.

| Area | Confirmed model/source fact | What to verify later |
|---|---|---|
| Branch consistency | FinancialTransactions has separate BranchId, FinancialAccountId and source FKs, rather than a composite branch/account/source FK. | Ensure every supported writer and importer enforces matching branches; an ordinary FK proves existence, not that the linked records share a branch. |
| Private evidence consistency | PrivateSessions links subscription, purchased batch, assignment and gym CheckIn separately. | Validate that references belong to the same member, branch, purchase and BusinessDate; SQL FKs alone do not prove that whole combination. |
| Multi-purpose attendance | CheckIns supports Gym, BorrowedGym, Activity, Class and Trial through nullable alternative references and CHECK constraints. | Keep reports type-aware; do not treat every CheckIns row as reception entry or every null member/subscription as a corrupt row. |
| Duplicate-looking subscription balances | PrivateSubscriptions and PrivateSubscriptionTransactions both store remaining session balances; regular subscriptions also have grant history. | Verify atomic updates and grant/subscription reconciliation in all supported paths; retain legitimate purchase-level balances and historical snapshots. |
| Coach identity alternatives | ActivityCoachAssignments allows ActivityCoachId or CoachEmployeeId; ActivitySubscriptions retains both ActivityCoachId and ActivityCoachAssignmentId. | Review legacy versus current activity attribution and whether mixed references can disagree; do not merge these with Private or Classes without a confirmed business rule. |
| Several coaching domains | ActivityCoachSettlements, CoachEarnings and EmployeeCommissionSummaries are separate facts. | Ensure reports distinguish activity entitlements, recognized Private earnings and employee Sales commission; Classes compensation is still undefined. |
| Shift attribution | Shifts has optional legacy EmployeeId/FinancialAccountId, while ShiftEmployees and ShiftEmployeeChanges retain participation/history. | Keep current employee-shift reconciliation distinct from legacy participation records and one shared branch drawer; summing shift drawer counts would double count. |
| Employee payment representations | Payrolls retains legacy PaidAt/PaidByUserId/PaidFromFinancialAccountId, while PayrollPaymentRecords records current external partial payments. | Confirm reporting uses the correct evidence for each workflow; current declarations must not create another payroll ledger debit or silently infer funding. |
| Missing financial context on legacy rows | OperationId, BusinessDate, BusinessDayId, FinancialMonthId and historical coach/price links can be nullable in relevant entities. | Classify legacy/unconfigured rows explicitly before reporting/import; current managed CHECK/service requirements can be stricter than SQL nullability. |
| Meaningful optional references | Employees.UserId/MemberId are optional unique links; CommissionCollections.EmployeeId can be null for House/exclusion; CoachEarnings has alternative optional source IDs. | Preserve legitimate optionality; do not invent accounts, commercial owners or attendance merely to fill nulls. |
| Period boundaries | BusinessDays, CommissionPeriods and FinancialMonths are independent; commission source recording rejects collections assigned to a closed commission period. | Test late financial posting together with commission/refund guards, rather than assuming next-month posting waives finalized-source restrictions. |
| Legacy reopening fields | CommissionPeriods has ReopenedAt/ReopenedByUserId, but the current CommissionService exposes finalization and no reopening method. | Do not infer a supported commission-reopen workflow from columns or older capability catalog names; financial-month reopening is a different operation. |
| Temporal employee assignment | EmployeeBranchAssignments has date checks and a filtered unique open-primary index, not a full exclusion constraint against every dated overlap. | Review legacy overlaps and raw imports; EmployeeBranchResolver returns no current branch when assignments are ambiguous. |
| Generic identifiers without FKs | AuditLogs.EntityId is polymorphic; MaintenancePlanRevisions.PlanId groups revisions without a separate plan-master FK; OperationId fields identify retries, not accounts or branches. | Validate audit lookup, revision continuity and idempotency through services; do not draw these as invented relational links. |
| Frozen report storage | FinancialClosingRevisions stores JSON plus exact XLSX bytes, hash and SchemaVersion. | Plan retention, backup size and JSON schema evolution; this is intentional reproducibility, not a second financial ledger. |
| History immutability boundary | ApplicationDbContext guards tracked updates/deletes of selected evidence; many FKs restrict deletion. | Confirm supported tools use those guards; EF tracked immutability is not a guarantee against unrestricted raw SQL or database-administrator changes. |
| Report double counting | Payments/parts, ledger movements, commission slices, coach payouts/Expenses and payroll obligations are linked explanations of different facts. | Aggregate each intended dataset once, distinguishing cash flow, earned costs, advances and later payment. |

## Concrete source pointers

- [Ledger relationships and source exclusivity](../../CampionsSystem.DAL/Configurations/FinancialTransactionConfiguration.cs)
- [Private session relationships and managed evidence checks](../../CampionsSystem.DAL/Configurations/PrivateSessionConfiguration.cs)
- [Attendance type/source rules](../../CampionsSystem.DAL/Configurations/CheckInConfiguration.cs)
- [Private balances](../../CampionsSystem.DAL/Configurations/PrivateSubscriptionConfiguration.cs)
  and [purchase batches](../../CampionsSystem.DAL/Configurations/PrivateSubscriptionTransactionConfiguration.cs)
- [Activity assignment alternatives](../../CampionsSystem.DAL/Configurations/ActivityCoachAssignmentConfiguration.cs)
  and [activity subscription links](../../CampionsSystem.DAL/Configurations/ActivitySubscriptionConfiguration.cs)
- [Shift records](../../CampionsSystem.DAL/Entities/Shift.cs) and
  [shift configuration](../../CampionsSystem.DAL/Configurations/ShiftConfiguration.cs)
- [Payroll legacy/current fields](../../CampionsSystem.DAL/Entities/Payroll.cs) and
  [monthly evidence](../../CampionsSystem.DAL/Entities/MonthlyFinance.cs)
- [Commission collection and refund restrictions](../../CampionsSystem.BLL/Commissions/CommissionSources.cs)
  and [finalization](../../CampionsSystem.BLL/Commissions/CommissionService.Calculation.cs)
- [Dated employee assignment constraints](../../CampionsSystem.DAL/Configurations/EmployeeBranchAssignmentConfiguration.cs)
  and [branch resolution](../../CampionsSystem.BLL/Branches/EmployeeBranchResolver.cs)
- [Append-only guards](../../CampionsSystem.DAL/Data/ApplicationDbContext.cs)

The historical Project overview count of 77 application tables in PROJECT_HISTORY.md
is stale relative to the current model. This task intentionally leaves that file
unchanged under the folder-only restriction. Actual rollout state, row-level
inconsistencies, whether unused legacy structures can be retired, and production
query performance remain **unverified**, not assumed problems.
