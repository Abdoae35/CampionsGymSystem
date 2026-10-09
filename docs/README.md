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

## Current Implementation Status

| Feature | Status |
|---|---|
| Authentication & Authorization | Complete |
| Branch Management | Complete |
| Employee Management | Complete |
| Member Management | Complete |
| CRM / Leads | Complete |
| Membership Plans | Complete |

Complete refers to the management scope described in each document. Persistence
models and some narrower operations exist outside these scopes; they do not imply
complete Subscription, Finance, attendance or employee-entitlement lifecycles.

## Planned / Deferred Areas

- Subscriptions / Renewals / Freeze and employee entitlement lifecycle
- Check-In / Access
- Complete Sales / Payments; Installments / Refunds
- Shifts / Vault / Expenses
- Remaining Private Training and Classes / Activities workflows
- Commission / Payroll
- Contracts / Messaging
- Reports / Hardening
- Frontend; optional Offline Sync

Existing limited activity/Freelance purchase, class and coach operations are identified
in [Architecture](ARCHITECTURE.md); the broader areas above remain incomplete.

These documents represent the implemented system state at the time of the latest
documentation refresh. Documentation is refreshed explicitly when requested.

Latest refresh: 2026-10-09. Earlier technical notes remain available in the existing
feature and database subdirectories; this index is the main entry point.
