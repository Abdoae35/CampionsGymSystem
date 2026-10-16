# Campeones backend → React frontend handoff

Finalized 2026-10-12 against clean Git **`master` at `476f0a77bf4f0a75839ab3b9a7f5e65dbc284b80`** (`FrontEndDoc`), containing the completed backend commit **a2a0c56**. The branch differs from the earlier implementation-session handoff; no branch was switched. This package replaces the earlier baseline/draft integration briefing.

**191 documented controller API operations across 23 controllers, including all 36 completed Private Coaching, Classes and coach-finance routes.** The actual OpenAPI has **192 operations** including Development-only database health. The separate Development document-serving registration makes 193 exposed registrations. Four legacy routes return 410; route count includes these compatibility contracts.

Read in order:

**Current feature addendum:** [Planned periodic expenses V1](PLANNED_PERIODIC_EXPENSES.md) documents the new maintenance summary/reminders and revised plan contract. The older full OpenAPI capture below predates later backend features and is not authoritative for this feature.

1. [Authentication, permissions and branch context](AUTH_PERMISSIONS_BRANCH_CONTEXT.md).
2. [React/API setup and examples](INTEGRATION_SETUP_AND_EXAMPLES.md).
3. [Full endpoint catalog](API_ENDPOINT_CATALOG.md), [DTOs/enums](DTO_AND_ENUM_REFERENCE.md), and [raw OpenAPI](openapi.json), [source-derived frontend specification](openapi.integration.json), with their [provenance/limits](OPENAPI_STATUS.md).
4. [All business workflows](FRONTEND_WORKFLOWS_AND_BUSINESS_RULES.md), [Private/Classes confirmed rules](PRIVATE_CLASSES.md), and [React Coaching integration](REACT_COACHING_INTEGRATION.md).
5. [Operational rollout and unresolved limitations](OPEN_QUESTIONS_AND_GAPS.md), [verification](COMPLETENESS_AUDIT.md), and [Postman examples](postman/README.md).
6. [Starter for a separate frontend conversation](HANDOFF_FOR_NEW_CHAT.txt).

| Implemented area | Available contracts |
|---|---|
| Auth, capabilities, branches, employees | Login/me, effective Allow/Deny, scoped administration and assignments |
| Members and CRM | Registration, lookup, ownership, Leads/conversion/interactions/follow-ups |
| Plans, benefits, subscriptions, reception | Catalogs, paid lifecycle, freezes/extensions/holidays, employee entitlement, gym/trial/borrowed entrance |
| Member Sales and finance | Collections, split tender/installments, Offers, cancellation/refunds, shifts/drawer/expenses/advances/reports |
| Independent Sales commissions | Explicit rules/periods, actual collections/refund allocations, statements and finalization; Sales commission payout remains deferred |
| Private coaching | Catalog/purchase, search/history, reception-required session delivery, transfers, Owner expiry authorization |
| Classes | Shared packages, type/occurrence management, individual attendance without reception, histories and completed-occurrence averages/targets |
| Coach finance | Independent targets, net-funded Private earnings recognition, deferred funding, balances, arbitrary partial payouts/history |
| Legacy operations | Narrow Activity/Freelance and historical class reports; four explicitly disabled compatibility writes |

**Classes compensation remains unimplemented until the Owner defines its formula.** Attendance/statistics/configurable average targets are implemented. No frontend commission rate or payout calculation should be invented. Finalized-earnings refund adjustments and coach-payout corrections are also unavailable pending policy.

Implementation does not prove deployment readiness. The two Coaching migrations were generated and tested in disposable schemas, but the implementation handoff reports they were **not applied to application/Production**. No new capability grants/default targets/seed cash were created. Deployment schema/grants/accounts and CORS/host must be verified by the authorized backend operator. This documentation task did not access any database or run operational requests.

The existing substantial handoff was reused; accurate DTOs and workflows were retained, the late member-attendance route was integrated, and OpenAPI/metadata were recaptured from committed isolated source. The reported implementation suite is **822/822 passed**; it was not rerun for documentation. [Compiler supplement](endpoint-contracts.json) supplies IActionResult response types/policies missing from generated OpenAPI. Use openapi.integration.json for response/auth/property-converter supplements and review the catalog/BLL constraints when generating clients.

All current integration links are portable within this folder. C# paths are provenance, not required external files. Examples are fabricated and require configured authorized fixtures. [branch-only/](branch-only/README_ARCHIVE.md) preserves historical captures, clearly superseded by the root contracts. [File index](PORTABLE_FILE_INDEX.md).
