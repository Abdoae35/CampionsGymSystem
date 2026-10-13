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

- Monthly Closing complete on MonthlyClosing&FinancialReports from clean 1a9c811; domain 9f5cedc and operational 6bbe6a8 pushed; frontend handoff unchanged.
- Independent branch months, serialized close/Shift-open, optional nonblocking counts, immutable revisions and six-sheet XLSX; late posts next Open month, finalized commissions untouched. Effective salary history/proration excludes suspension and pays approved leave; branch advance caps/deductions, external partial employee payment records and informational maintenance implemented.
- 22 API operations with effective capabilities/branch scope; Sales/coach evidence reused, cashier versus commercial owner preserved, historical outstanding and ledger amounts counted once. Classes compensation NotConfigured. New capability catalog only, no grants.
- Full suite 855/855 passed, zero failures/skips, 27m20s (33 new cases); focused 36/36 and follow-up 7/7 passed. Clean rebuild zero warnings/errors; final EF drift none; forward SQL matches reviewed output; 50 historical migration files unchanged. Earlier fixture errors and superseded-run cancellation are recorded in history.
- Migration 20261012164313_ImplementMonthlyClosingAndEmployeePayments exercised only in disposable localhost schemas; no application/Production data/migration/grant changes. docs/MONTHLY_CLOSING.md, module notices and 22 Postman examples verified; final documentation checkpoint includes this handoff. Verify actual Git HEAD/upstream before continuing.
- Rollout: controlled prerequisite migrations, explicit complete-report/action capabilities, branch drawer/month dates and salary history (including explicit zero). Classes compensation, finalized-earnings/payout corrections and ambiguous legacy reconciliation remain outside confirmed rules. No further feature work without direction.
