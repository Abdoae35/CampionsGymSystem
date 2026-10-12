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

- Sales Commissions & Independent Targets complete on SalesCommissions&Targets, continued from verified planning f8530e9 and completed backend 5244d48; no restart or discarded work.
- Published implementation checkpoints: 974f116 domain, 5bf9bd9 rules/periods, 1ba9a14 collections/refunds, 71e0331 scoped API/statements/Postman. Final documentation checkpoint records verification below; verify actual Git HEAD/status before continuing.
- Independent configurable New/Renewal targets/rates, first-ever purchase history, collection-time commercial employee/House facts, retained cancellation eligibility and newest-first refund allocations implemented. Finalized-source refunds reject atomically; no unresolved business-policy issue.
- Final full suite 772/772 passed, zero failures/skips (39 new cases over verified 733). Targeted 44/44 and concurrency rerun 4/4 passed; build zero warnings/errors; EF drift none.
- Two new migrations applied only to guarded localhost Development: three evidence tables, seven indexes, 14 validated constraints, restrictive FKs/exact precision. All 42 old migration files unchanged. No operational data/rules/periods/grants seeded; Production untouched.
- Rollout: explicitly configure branch fallback/employee rules and branch periods. Legacy undated sources/periods need explicit historical reconciliation. Existing Offers.Manage/Installments.Manage/Sales.QuickCancel active-but-ungranted gap preserved.
- docs/SALES_COMMISSIONS.md contains actual nine routes, capabilities, setup and examples. Payroll, actual commission payouts, full monthly closing, late-refund adjustments, Private/Classes finance, frontend and sync remain deferred. Stop after this feature.
