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

- Shifts / Vault / Expenses on Shifts-Vault&Expenses-; registration/Sales/payment/refund integration verified.
- Independent own employee shifts may overlap; one shared drawer, explicit cash counts and discrepancy carry-forward, no automatic ledger corrections.
- Cash payouts enforce tracked physical availability; InstaPay has no inferred bank-balance check.
- New operational Member creation, Sales/collections/refunds require own open shift; committed financial retries survive closure; historical null links preserved.
- Employee transfer requires own manual shift closure; explicit BusinessDay rollover may proceed while employee shifts remain open.
- CashDrawer locks coordinate money/count/closure; employee lock protects transfer/opening. No automatic recovery or manager approval.
- Policy/branch concurrency 32 tests and HTTP regressions 92 passed. Prior integration run 210 passed / 1 fixture failure corrected; full suite pending.
- Latest published a6d871c; integration commit/push next. Reporting/API implementation underway and not yet validated.
- Two new migrations applied only guarded localhost Development; prior EF drift clean.
- Sales Offers.Manage/Installments.Manage/Sales.QuickCancel provisioning gap remains; no unrelated grants/overrides mutated.
- Payroll/advance settlement, commission, monthly close, automation, Private/Classes, frontend and sync deferred. See PROJECT_HISTORY.md.
