# Campions Gym Management System Documentation

Campions is a permission-based, branch-isolated gym backend built as a .NET modular
monolith. This index describes the implemented management features, not the entire
future operational system.

## Core Documentation

- [Architecture](ARCHITECTURE.md)
- [Authentication & Authorization](AUTHORIZATION.md)
- [Branch Management](BRANCH_MANAGEMENT.md)
- [Employee Management](EMPLOYEE_MANAGEMENT.md)
- [Member Management](MEMBER_MANAGEMENT.md)
- [CRM / Leads](CRM.md)
- [Membership Plans](MEMBERSHIP_PLANS.md)
- [Subscriptions / Renewals / Freeze](SUBSCRIPTIONS.md)
- [Check-In / Access Management](CHECK_INS.md)
- [Sales / Payments](SALES_PAYMENTS.md)
- [Shifts / Shared Vault / Expenses / Advances](SHIFTS_VAULT_EXPENSES.md)
- [Sales Commissions & Independent Targets](SALES_COMMISSIONS.md)
- [Private Coaching / Classes / Coach Earnings / Partial Payouts](PRIVATE_CLASSES.md)

## Current Implementation Status

| Feature | Status |
|---|---|
| Authentication & Authorization | Complete |
| Branch Management | Complete |
| Employee Management | Complete |
| Member Management | Complete |
| CRM / Leads | Complete |
| Membership Plans | Complete |
| Subscriptions / Renewals / Freeze | Complete within documented entitlement scope |
| Check-In / Access Management | Complete within documented gym-entry scope |
| Sales / Payments | Complete within documented customer gym-finance scope |
| Shifts / Shared Vault / Expenses / Advances | Complete within documented manual operational scope |
| Sales Commissions & Independent Targets | Complete within documented calculation / statement scope; Sales payouts deferred |
| Private / Classes / Coach Earnings / Partial Payouts | Implemented within documented operational scope; Classes compensation awaits Owner formula |

Complete refers to the management scope described in each document. Persistence
models and some narrower operations exist outside these scopes; they do not imply
complete Finance or attendance lifecycles.

## Planned / Deferred Areas

- Classes compensation formula; remaining Activities workflows
- Payroll / Sales commission payouts; finalized earnings adjustment policy
- Contracts / Messaging
- Reports / Hardening
- Frontend; optional Offline Sync

Existing limited activity/Freelance purchase, class and coach operations are identified
in [Architecture](ARCHITECTURE.md); the broader areas above remain incomplete.

These documents represent the implemented system state at the time of the latest
documentation refresh. Documentation is refreshed explicitly when requested.

Latest refresh: 2026-10-12. Earlier technical notes remain available in the existing
feature and database subdirectories; this index is the main entry point.
