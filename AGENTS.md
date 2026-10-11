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

- Shifts / Vault / Expenses on Shifts-Vault&Expenses-; operational integration cdb9d94 pushed. Reporting/API and Postman stage verified with targeted tests; final full rerun underway.
- Own manual overlapping employee shifts; one shared branch drawer; counts carry actual physical cash forward with unresolved discrepancies and no ledger correction/blame.
- Cash payouts enforce physical availability; InstaPay has no inferred bank-balance check. Expenses/advances are direct paid facts; returns require actual received funds.
- New Member creation, Sales/collections/refunds/payouts require actor's own open shift; financial retries survive closure. Legacy null associations are preserved.
- Employee transfer requires own manual closure; explicit BusinessDay rollover may leave shifts open; later operations use new effective date.
- Targeted policy/concurrency 32, HTTP regressions 92, reporting 32, API 12/13 and failure-fix 18 test runs passed. Initial full run: 724 passed / 9 fixture-resource failures; corrected and clean full rerun pending.
- Build zero warnings/errors; EF drift none. Two migrations applied only localhost Development: five support tables, 36 indexes, 38 validated constraints, restrictive FKs and 18,2 money verified.
- Development catalog adds seven missing capabilities only; no existing role grants/user overrides changed. Offers.Manage/Installments.Manage/Sales.QuickCancel remain unprovisioned.
- Next: finish full-suite rerun, final checkpoint/doc commit/push and local/remote HEAD verification.
- Payroll/advance settlement, discrepancy resolution, forced/automatic closing, commissions, monthly closing, Private/Classes, frontend and sync deferred. See PROJECT_HISTORY.md.
