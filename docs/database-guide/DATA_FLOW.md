# Registration to monthly closing

[Navigation](README.md) · [Overview ERD](OVERVIEW.md)

The requested reading path is Member Registration → Subscription → Payment →
Check-In → Commission → Monthly Closing. It is a useful business tour, but it is
**not the exact execution order**: a sale records the chosen subscription terms,
actual collection occurs, and activation follows the selected mode. Commission
source evidence is recorded with each eligible collection, without waiting for attendance.

| Stage | Actual tables | Meaning and important distinction |
|---|---|---|
| Register or convert a lead | Members, Leads, LeadAssignments, MemberSalesAssignments | Register a member and preserve explicit commercial ownership; the actor creating the record can be a different user. |
| Choose and sell a plan | MembershipPlans, SaleTransactions, SaleMembershipSnapshots, SaleBenefitSnapshots | Freeze the purchased terms, prices, eligibility and Sales attribution, independently of future catalog edits. |
| Collect money | Payments, PaymentParts, FinancialTransactions, InstallmentPaymentAllocations | Record actual collection and its account splits; create ledger credit(s), grouped by financial account, and explain installment allocation. |
| Grant or activate access | MemberSubscriptions, MembershipSubscriptionTransactions, MemberSubscriptionBenefits | Apply purchased entitlement now, at first eligible entry or on a configured date; pending sales must not be mistaken for active subscriptions. |
| Attend | CheckIns, SubscriptionFreezes, BorrowedGymSessions | Validate member access; consume exactly the relevant session where applicable and retain attendance/debt evidence. |
| Attribute and finalize Sales earnings | CommissionCollections, CommissionRefundAllocations, CommissionStatementLines, EmployeeCommissionSummaries | Attribute eligible collections to the commercial employee or exclusion/House reason, then freeze configured category targets/rates and results for an independent commission period. |
| Recognize employee obligations | EmployeeSalarySettings, Payrolls, PayrollDeductions | Recognize branch salary entitlement and finalized Sales summary amounts; apply advances through a unique deduction link. |
| Close the branch financial month | FinancialMonths, FinancialMonthEvents, FinancialClosingRevisions | Require closed shifts, preserve optional physical counts/discrepancies, and freeze the report plus exact workbook revision. |
| Pay later | PayrollPaymentRecords, CoachPayouts, CoachPayoutAllocations | Record payments separately from earned amounts; employee payment declarations infer no funding source, while coach payouts use an actual paid Expense. |

## Follow the identifiers

- `SaleTransactions.MemberId` identifies the customer; `SalesEmployeeId` is the
  sale's commercial employee snapshot; `CreatedByUserId` is the performing login.
- `Payments.ReceivedByUserId` identifies the cashier. `CommissionCollections.EmployeeId`
  identifies the commission beneficiary for that collection; these are different identities.
- `MembershipSubscriptionTransactions` joins the purchased plan and optional sale
  to the subscription grant. Historical plan/price fields must not be replaced
  with today's MembershipPlans values.
- `PaymentParts` and `RefundParts` reference accounts but are supporting split
  evidence. Sum actual FinancialTransactions once for account movement; do not
  add the payment header, parts and ledger as three separate collections.
- `CommissionStatementLines` links a period to actual payment sources;
  `EmployeeCommissionSummaries` holds employee/category finalized totals.
- `Payrolls.CommissionSummaryId` links recognized Sales commission obligations to
  those existing summaries. Salary payroll has separate SalaryEvidenceJson and
  may have no CommissionPeriodId. `PayrollPaymentRecords.PayrollId` settles the
  payable record; it does not recalculate commission.

## The coaching paths

Private purchases use SaleTransactions/Payments plus PrivateSubscriptions and
PrivateSubscriptionTransactions. PrivateSessions links to the actual reception
CheckIn and historical coach assignment. The registration also requires valid
regular membership through service validation; there is no direct
PrivateSubscriptions → MemberSubscriptions FK.

PrivateExpirySettlements records explicit unused-session entitlement without fake
attendance. CoachEarnings has exactly one of PrivateSessionId or
PrivateExpirySettlementId, plus funded eligible value and a snapshotted rate.
CoachPayoutAllocations allocates payments against earnings; CoachPayouts links to
Expenses, which posts the actual debit through the existing ledger.

ClassesSubscriptions holds one shared purchased balance across GroupClasses types.
ClassSessions is an occurrence; a CheckIns row with class type links the member,
occurrence and ClassesSubscription. That row is class attendance, **not a required
reception check-in**. Completed occurrences and actual attendance support coach
averages. Classes compensation remains **NotConfigured**; activity coach settlement
tables do not establish a Classes compensation formula.

## Dates and final reports

BusinessDays controls an operational branch BusinessDate. CommissionPeriods defines
commission evaluation dates. FinancialMonths defines financial posting/reporting
boundaries. These are separate tables and lifecycles, not three names for one month.

FinancialTransactions retains BusinessDate, OccurredAt and optional PostedAt/month
attribution. FinancialOccurrenceEvidence separately records a known original
occurrence date/time. Where a configured financial history exists, posting requires
an Open destination month; eligible late postings use the next Open month. Existing
closed-commission collection and finalized-source refund protections still apply.

Reopening creates FinancialMonthEvents and preserves earlier FinancialClosingRevisions.
It does not silently reopen/recalculate finalized Sales or Coach statements.
MaintenancePlanRevisions is a forecast; only actual maintenance paid through the
Expenses workflow affects the ledger. CashCounts records discrepancies without
inventing an expense or adjusting ledger money automatically.

## Source checks

- [Member management](../../CampionsSystem.BLL/Members/MemberManagementService.cs)
- [Sale activation](../../CampionsSystem.BLL/Sales/SalesService.Activation.cs) and
  [collection posting](../../CampionsSystem.BLL/Sales/SalesService.cs)
- [Paid entitlement activation](../../CampionsSystem.BLL/Subscriptions/SubscriptionService.PaidSales.cs)
  and [entry](../../CampionsSystem.BLL/CheckIns/CheckInService.Entry.cs)
- [Commission source attribution](../../CampionsSystem.BLL/Commissions/CommissionSources.cs)
  and [finalization](../../CampionsSystem.BLL/Commissions/CommissionService.Calculation.cs)
- [Private/Classes operations](../../CampionsSystem.BLL/Coaching/CoachingService.cs)
- [Payroll recognition/payment](../../CampionsSystem.BLL/MonthlyFinance/PayrollService.cs)
- [Financial posting boundary](../../CampionsSystem.BLL/MonthlyFinance/FinancialMonthPosting.cs)
  and [closing/revisions](../../CampionsSystem.BLL/MonthlyFinance/MonthlyFinanceService.cs)
