# Purchase, attendance and commission flows

These are specifications for later BLL services; no workflows or triggers are
implemented here. Use existing authorization, branch consistency, idempotency and
ledger validation rules. All related writes in each flow use one database transaction.
Prefer deactivation over deleting products/employees. Historical FKs use Restrict.

## 1. Normal gym membership purchase

```mermaid
flowchart LR
    M[Member] --> P[Normal MembershipPlan]
    P --> S[SaleTransaction]
    S --> U[MemberSubscription]
    U --> T[MembershipSubscriptionTransaction]
    T --> Pay[Payment]
    Pay --> Part[PaymentPart]
    Part --> Account[FinancialAccount]
    Account --> Ledger[FinancialTransaction]
```

Load the active plan; validate member/branch/city and any system Offer. Price is
normal gym selling amount. Snapshot price, category Normal and null freelance
Coach; renewals add a new transaction. PaymentParts sum to money actually received;
post consistent ledger entries. Account balance derives from its ledger.

## 2. Freelance gym membership purchase

```mermaid
flowchart LR
    M[Member] --> P[Coach-specific Freelance MembershipPlan]
    P --> V[Validate active Freelance Employee]
    V --> E[Require explicit plan eligibility]
    E --> H[End current Sales assignment and append House]
    H --> S[House SaleTransaction: gym configured Price]
    S --> U[MemberSubscription]
    U --> T[MembershipSubscriptionTransaction: category and Coach snapshots]
    T --> Pay[Payment: gym money only]
    Pay --> Part[PaymentPart]
    Part --> Account[FinancialAccount]
    Account --> Ledger[FinancialTransaction]
    External[Coach external money = OUTSIDE SYSTEM]
```

1. Load the selected plan and detect Freelance category.
2. Require FreelanceCoachEmployeeId and a currently active Freelance Employee.
3. Require explicit plan eligibility. End an existing current Sales assignment and
   append House, preserving all previous assignments/sales. Use MembershipPlan.Price
   as the gym amount due. The focused purchase service does not accept discounts;
   only a future validated system Offer workflow may adjust that amount.
4. Create SaleTransaction, membership/renewal and purchase transaction; snapshot
   PlanCategory and FreelanceCoachEmployeeId independently of future plan edits.
   Set SalesAttributionSnapshot=House; SalesEmployeeId and SaleCategory are null.
5. Record Payment/PaymentParts and account/ledger posting only for gym money.

Example: plan Price=300; Coach charges 400 or 500 externally. Without a system
Offer, gym SaleTransaction.NetAmount=300 and full Payment.Amount=300. Neither the
external total nor Coach margin is stored as gym revenue or commission. Never
ask reception how much the Coach charged the client. A Coach's personal discount
is outside the system and does not change the gym amount. FreelanceAgreement is
agreement metadata only; no pricing or permanent client ownership table is added.

## 3. Private package purchase

```mermaid
flowchart LR
    M[Member] --> S[SaleTransaction]
    S --> U[PrivateSubscription]
    U --> T[PrivateSubscriptionTransaction]
    T --> A[PrivateCoachAssignment]
    A --> P[Payment]
```

Validate package/member/branch and sale amount. Create immutable purchase facts:
SessionsAdded, DurationDaysAdded, NetAmount, UnitSessionValue, date snapshots and
CreatedAt. Initialize batch RemainingSessions=SessionsAdded; update subscription
totals and end date. Initial assignment has EndedAt=null; renewals preserve current
assignment unless explicitly transferring. Payment follows existing split-payment
and ledger rules. No commission is earned solely from assigning a Coach.

Each batch stores its own unit value: 20 sessions at 100 and a renewal of 10 at
120 are separate batches. Never reprice old batches. UnitSessionValue remains
numeric(18,2); define purchase rounding in BLL rather than repricing delivered
sessions. It is the authoritative value for delivered-session basis; no duplicate
session price is stored. Expiry settlement deliberately snapshots its financial
count and unit value.

## 4. Private session attendance

```mermaid
flowchart LR
    C[Gym CheckIn] --> V[Validate active PrivateSubscription and Gym Membership]
    V --> A[Current PrivateCoachAssignment]
    A --> B[Select valid purchased batch FIFO]
    B --> S[PrivateSession: actual Coach and batch]
    S --> D[Decrement batch RemainingSessions]
    D --> U[Decrement subscription RemainingSessions]
    U --> P[Open CommissionPeriod for occurrence]
```

1. Validate active PrivateSubscription and active Gym Membership.
2. Validate same member/date/branch gym CheckIn.
3. Find assignment with EndedAt=null; require authenticated Coach is that Coach.
4. Lock the subscription; select eligible batch with RemainingSessions>0,
   ordered by CreatedAt then Id (FIFO). Validate it belongs to this subscription.
5. Create exactly one PrivateSession referencing subscription, batch, delivering
   Coach, CheckIn, user and open CommissionPeriod covering SessionAt.
6. Decrement batch and subscription counters by one; save all writes atomically.

Do not use triggers. Deduplicate requests; serialize competing attendance/transfer/
expiry/renewal operations. Do not consume a settled batch or reuse a CheckIn to
accidentally duplicate an attendance. Delivered basis = batch.UnitSessionValue.
If the relevant period is closed, use an authorized correction policy rather
than silently attaching backdated delivery to a different period.

## 5. Private Coach transfer

```mermaid
flowchart LR
    A[Current PrivateCoachAssignment] --> E[Set EndedAt]
    E --> N[New PrivateCoachAssignment: Coach B]
    H[Existing PrivateSessions and closed CommissionPeriods stay unchanged]
```

Lock subscription; find current assignment, set EndedAt, then create new Coach
assignment with AssignedAt/reason/created user. Flush the old row before inserting
the new row to respect the partial unique index; both saves belong to the SAME
transaction. Validate active eligible new Coach. Transfer is allowed regardless
of earlier period closure. Existing sessions, their delivering Coach, batch,
period and finalized earnings remain unchanged. Future sessions use Coach B;
there is no automatic movement of already-earned money.

## 6. Private subscription expiration

```mermaid
flowchart LR
    E[Final EndDate reached] --> R{RemainingSessions greater than zero?}
    R -- No --> X[Expire subscription]
    R -- Yes --> B[Find remaining transaction batches]
    B --> C[Find current Coach]
    C --> S[PrivateExpirySettlement per batch]
    S --> P[Currently open CommissionPeriod]
    P --> Z[Zero live batch and subscription counters]
    Z --> X
```

Under the same subscription lock/transaction, recheck final EndDate and no renewal.
If no sessions remain, simply mark Expired. Otherwise find current assignment and
all batches with RemainingSessions>0. For each create a settlement snapshot with
RemainingSessions, UnitSessionValue and EligibleAmount=count*unit value, current
Coach, open period at expiry/settlement, SettledAt and nullable initiating user
(for automation). Verify batch totals equal subscription total; do not guess if
inconsistent. Zero live counters and mark Expired in the same transaction.

Unique PrivateSubscriptionTransactionId prevents double settlement. Retry safely:
never recreate settlements for an already expired/settled batch. Preserve every
session, assignment and settlement. Missing current Coach/open period blocks the
whole settlement for review/retry. Final expiry uses branch-local EndDate semantics.

Example: 20 sessions at 100; Coach A delivered 6 (basis 600), Coach B delivered 10
(basis 1000). At expiry 4 unused sessions add 400 to current Coach B: total basis
A=600, B=1400. Expiry does not change the periods of the 16 delivered sessions.

## 7. Private commission calculation

```mermaid
flowchart LR
    P[CommissionPeriod] --> S[PrivateSessions]
    S --> D[Sum immutable batch unit values per delivering Coach]
    P --> E[PrivateExpirySettlements]
    E --> R[Sum expiry EligibleAmount per settled Coach]
    D --> B[Combined eligible basis]
    R --> B
    B --> Rule[Apply configurable CommissionRule]
    Rule --> Summary[EmployeeCommissionSummary]
```

Group sessions and settlements by their own CommissionPeriodId/CoachEmployeeId.
Resolve existing effective-dated CommissionRules and employee overrides; apply
configured rates/methods and save the existing summary snapshots. Eligible basis
1000 with AppliedRate=60 percent yields final commission 600. Configure 100 percent
in CommissionRule if needed; never hardcode percentages into Private entities.
Closing a period finalizes its history. A later transfer does not recalculate it.

## 8. Sports activity purchase, revenue and settlement

```mermaid
flowchart LR
    M[Member] --> P[Select ActivityPackage]
    P --> C[Select active assigned ActivityCoach]
    C --> U[ActivitySubscription - independent of Gym access]
    U --> S[Create activity SaleTransaction]
    S --> B[Purchase batch - snapshot coach and percentage]
    B --> Pay[Receive Payment and PaymentParts]
    Pay --> R[Count each completed Payment once in its period]
    R --> Refund[Subtract linked eligible refunds]
    Refund --> Rate[Apply batch percentage - no targets]
    Rate --> Report[Coach revenue report]
    Report --> Final[Explicit closed-period snapshot]
    Final --> Internal[Internal EmployeeCommissionSummary]
    Final --> External[External coach amount owed - no Employee payroll]
```

The purchase service atomically creates/renews the subscription, creates the sale,
records batch attribution and audits coach selection. It does not receive payments
or grant Gym membership. New subscriptions are Scheduled; payment/access activation
remains the payment workflow's responsibility. Renewals can select a new assigned
coach for future batches; previous batches/installments retain their original coach/rate.

Report rows group by coach/activity/**purchase percentage**; multiple configured
rates may therefore yield multiple rows. Subscribers are distinct paid members per
row. Eligible revenue is completed Payment.Amount minus linked refunds, not list
price or the sum of joined payment/part rows. Validate parts total. Round each grouped
entitlement to two places, midpoint away from zero. Period intervals are [start,end).

Finalize explicitly after period close, with its final EndAt and reconciled records.
Internal snapshots feed one EmployeeCommissionSummary per employee/activity;
external snapshots remain separate. No external payment/ledger engine is added.
Never recalculate finalized snapshots automatically, including when a period is
reopened. Later refunds require a separately authorized adjustment, not history edits.

## 9. Actual class attendance — statistics only

```mermaid
flowchart LR
    Schedule[ClassSchedule - planned] --> Actual[Actual ClassSession]
    Actual --> Coach[Actual delivering CoachEmployee]
    Coach --> Attendance[CheckIns linked to occurrence]
    Attendance --> Completed[Completed sessions including zero attendance]
    Completed --> Report[Coach attendance report]
    Report --> Average[Total attendance divided by completed sessions]
    Average --> Stop[STOP - financial compensation NOT FINALIZED]
```

Scheduled/cancelled sessions are excluded. A completed empty session contributes
one to the denominator and zero to the numerator. The session's actual coach wins
over GroupClass/planned coach. Unique member/session attendance avoids duplicate
counts. Legacy class CheckIns without session links are excluded until reconciled;
no fake occurrences or manual attendance totals are created.

## 10. Freelance plan configuration, eligibility and House sale

```mermaid
flowchart LR
    Plan[Create Freelance MembershipPlan] --> Coach[Assign active Freelance Employee]
    Coach --> Price[Set MembershipPlan.Price - gym money]
    Price --> Eligible[Explicitly add eligible Members]
    Eligible --> Validate[Validate eligibility in BLL on every purchase or renewal]
    Validate --> History[End current Sales assignment and append House]
    History --> Purchase[Create membership and immutable coach/plan snapshots]
    Purchase --> Sale[House sale - no Sales employee or category]
    Sale --> Stop[No SalesNew or SalesRenewal commission]
```

Removing eligibility affects future purchases only. Historical membership/sale
snapshots survive plan edits and eligibility removal. One member may be listed on
multiple plans; no permanent global coach lock exists. No coach margin, external
client charge, Freelance settlement pricing engine or permanent client table is added.

## 11. Business day — explicit operational date

```mermaid
flowchart LR
    B[Branch] --> Open[Open BusinessDay with BusinessDate]
    Open --> Operations[Shifts - Sales - Payments - Refunds - Expenses - CheckIns]
    Operations --> Actual[Keep actual event timestamps and BusinessDayId]
    Actual --> Close[Explicitly close BusinessDay]
```

After midnight, operations may continue referencing the still-open prior BusinessDate.
No timestamp is overwritten. Purchase batches derive BusinessDay through their sale;
parts derive it through their payment/refund. Ledger adjustments and actual class sessions
also have explicit optional day links. Legacy unknown links remain null for reconciliation.
Future BLL performs assignment/branch checks; this alignment implements no day workflow.

## 12. Employee leave and shift replacement — storage ready

```mermaid
flowchart LR
    E[Employee] --> Request[Leave Request]
    Request --> Review[Authorized Manager Review]
    Review --> Decision[Approved or Rejected]
    Decision --> Validate[Later shift assignment checks approved leave]
    Validate --> Replace[If replacing - preserve original assignment]
    Replace --> History[Append ShiftEmployeeChange and AuditLog]
```

No manager hierarchy or approval/shift API is implemented by this schema alignment.

## 13. Class, activity and Freelance facts ready for vertical features

```mermaid
flowchart LR
    Schedule[ClassSchedule] --> Session[Actual ClassSession]
    Session --> Coach[Actual CoachEmployee]
    Coach --> Checks[Unique member Class CheckIns]
    Checks --> Average[Completed classes - attendance - average]
    Average --> Stop[STOP - class financial rule NOT FINALIZED]
```

```mermaid
flowchart LR
    Package[ActivityPackage] --> Subscription[ActivitySubscription]
    Subscription --> Coach[Selected internal or external ActivityCoach]
    Coach --> Snapshot[Purchase coach and percentage snapshots]
    Snapshot --> Pay[Sale and collected Payments]
    Pay --> Entitlement[Coach entitlement using actual eligible money]
```

```mermaid
flowchart LR
    Plan[Freelance MembershipPlan] --> Eligible[Explicit eligible Members]
    Eligible --> Purchase[Validated member purchase]
    Purchase --> House[House attribution and historical snapshots]
    House --> NoSales[No Sales commission]
```

These reuse existing schema facts and any previously implemented feature code. This
final alignment adds no purchase, attendance, report, financial or authorization service.

Latest Private transfer calculation: preserve delivered/expiry monetary basis, determine
transferred-client context from assignment history, then apply the future BLL 50% rule
for transferred-client work and expiry. Existing ordinary CommissionRule behavior applies
to other Private work; no database trigger hardcodes the transfer percentage.

## Membership benefit configuration, snapshots and usage

```mermaid
flowchart LR
    Catalog[MembershipBenefit catalog] --> Config[MembershipPlanBenefit: type and quantity]
    Config --> Plan[MembershipPlan]
```

```mermaid
flowchart LR
    Plan[MembershipPlan] --> Config[Current plan benefits]
    Config --> Purchase[MembershipSubscriptionTransaction: initial or renewal]
    Purchase --> Grants[Snapshot MemberSubscriptionBenefits]
    Purchase --> Subscription[MemberSubscription and date context]
    Grants --> Separate[Keep each purchase batch separate]
```

Future purchase BLL copies code, name, grant type and granted quantity atomically.
Later configuration edits leave existing snapshots unchanged. Renewals copy the new
configuration into a new batch; no automatic snapshot creation is implemented yet.

```mermaid
flowchart LR
    Grant[Quantity MemberSubscriptionBenefit] --> Validate[Future BLL validates available quantity and validity]
    Validate --> Usage[MemberBenefitUsage: quantity, branch, actor, time]
    Usage --> Balance[Remaining = granted minus summed usage]
    Access[Access grant with NULL quantity] --> Entitlement[Future subscription access validation]
```

Usage is not a payment or sale. Carryover, expiration, FIFO and reset rules remain
**BUSINESS RULE NOT FINALIZED**. This alignment only stores the required facts.
