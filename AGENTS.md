# Instructions for coding assistants

Read `PROJECT_HISTORY.md` before starting work. It contains the project overview,
user preferences, decisions, completed work, and current handoff state.
For approved skill names, usage, source revisions, and limitations, consult
`SKILLS_GUIDE.md` when relevant. Load only the skills needed for the task.

Before the final response after every project operation (changes, setup,
investigations, or decisions), apply `project-history-update`. Its portable source
is `skills/project-history-update/SKILL.md`; read that file if the installed skill
is unavailable. Group related commands into one operation, not one entry per
tool call. Pure acknowledgments need no entry unless they change project context.

Update `PROJECT_HISTORY.md` with:
- What was requested and what changed.
- Important decisions and their reasons.
- Verification performed and any limitations.
- Remaining work and the current commit/push state.

Also refresh the `Current handoff` section below after each operation. Keep the
full chronological record in history and permanent workflow rules here.
The checkpoint includes failed/blocked work; documentation edits do not trigger
recursive logging. If a write is blocked, explain the missing update to the user.

Keep the current-state section concise and accurate. Append dated history entries
rather than repeating the whole conversation. Never record secrets, credentials,
or private environment values. Do not claim checks or actions that were not run.

Preserve existing user changes. Do not commit or push unless requested.
For the Authentication task (2026-10-06), the user explicitly requested committing
and pushing each completed, verified small subtask to the current Auth branch.
For the Branch feature (2026-10-07), the user explicitly requested small verified
commits and pushes to the current BranchSepration branch after each meaningful task.

Work one user-selected task at a time. Complete its implementation, relevant
verification, and history checkpoint before moving on. Do not expand a task into
whole-project implementation or start the next task without user direction.

For tools that do not automatically read `AGENTS.md`, the user can ask them to
read this file and `PROJECT_HISTORY.md` before working.

## Current handoff

- Shifts / Vault / Expenses complete within confirmed manual scope on Shifts-Vault&Expenses-; implementation through 0ac425c pushed; final documentation/results recorded in this checkpoint.
- Own manual overlapping employee shifts; one shared branch drawer. Actual counts carry forward with unresolved discrepancies, no ledger adjustments/blame.
- Cash payouts reject above physical availability; InstaPay has no inferred bank balance. Expenses/advances are direct paid facts; returns require real returned funds.
- New operational registration, Sales/payments/refunds and payouts require own active shift; committed financial retries survive closure; historical null links remain unchanged.
- Employee transfer requires own manual closure; explicit BusinessDay rollover may leave shifts open and later transactions use the new date.
- Final full suite 733 passed / zero failures/skips (55 added over baseline). Build zero warnings/errors; EF drift none. Earlier nine fixture/resource failures corrected and rerun green.
- Two new migrations verified only localhost Development: five support tables, 36 indexes, 38 validated constraints, restrictive FKs and exact 18,2 money.
- Seven missing capability catalog entries added only; existing grants/overrides preserved. Prior Offers.Manage/Installments.Manage/Sales.QuickCancel provisioning gap remains.
- Rollout needs explicit existing-user grants and account/drawer/category initialization; Development still has no application operational/financial setup data.
- docs/SHIFTS_VAULT_EXPENSES.md documents actual routes and policies; Postman adds 28 examples. No unresolved business-policy blocker.
- Payroll/advance settlement, discrepancy resolution, forced/automatic closing, commission/monthly closing, Private/Classes finance, frontend and sync deferred. Stop after this feature.
