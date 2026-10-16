# V1 module coverage against current master

Every controller below has a current per-action contract in `api/`, with the exact route, method, controller policies, request type and success type. The [catalog](API_ENDPOINT_CATALOG.md) lists all **227** actions. The raw OpenAPI contains those plus Development-only health. Four legacy Operations writes are explicitly disabled (410).

| Module | Controllers | Current behavior to use |
|---|---|---|
| Identity and access | Auth, AccessAdministration, Employees, EmployeeBranches, Branches | Login/me, Owner-only role/grant/override administration, assignment and effective Deny precedence; lifecycle guards protect Owner account. |
| Members and CRM | Members, Leads, Crm | Member identity, personal commercial ownership, leads, distribution and conversion. |
| Plans and visits | MembershipPlanCategories, MembershipBenefits, MembershipPlans, Subscriptions, CheckIns | Catalog, sale entitlement, freezes, reception check-in and eligibility. |
| Sales and refunds | Sales, Offers | Purchase, collections/installments, optional provider references, 24-hour reception Quick Cancel and elevated refund. |
| Shift and vault | Shifts, BusinessDays, Vault, FinancialAccounts, Expenses, EmployeeAdvances | Manual shift, override close, own/branch-period movement review, shared drawer and actual Owner expense without shift. |
| Sales commission | Commissions | Separate New/Renewal progress, rules, periods, sources and finalized statements. |
| Private and Classes | Coaching, Operations | 36 current coaching operations for packages/attendance/transfers/earnings/payouts, plus limited legacy Operations. Classes compensation is not configured. |
| Month and payroll | MonthlyFinance, EmployeeSalaries, Payroll | Branch close/reopen/revisions, report/XLSX, salary history, commission recognition and payout evidence. |
| Informational plans | MaintenancePlans | Five plan/summary/reminder/history operations, no ledger movement. |

Use [workflow guide](V1_WORKFLOWS.md) and [setup](INTEGRATION_SETUP_AND_EXAMPLES.md) for cross-module conditions. This checks source contracts; it does not claim every integration environment is activated.
