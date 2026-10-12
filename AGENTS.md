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

- Sales Commissions implemented on SalesCommissions&Targets from verified f8530e9 / completed backend 5244d48; source integration 1ba9a14 pushed after domain 974f116 and rules/periods 5bf9bd9.
- Independent future-effective New/Renewal rules, explicit branch periods, immutable collection attribution, newest-first refunds and finalized source/rate statements implemented; no payroll/payout.
- Retained eligible cancellation cash remains eligible; post-employment collections become House. Refunds reaching finalized sources reject atomically. No unresolved policy.
- Latest commission plus historical migration tests 44/44 passed; four concurrency cases rerun 4/4. Build zero warnings/errors. API authorization included four passing HTTP cases; Postman ten authenticated examples validated.
- Two migrations applied only to guarded localhost Development: three tables, seven indexes, 14 validated constraints, restrictive FKs and exact precision. No app data or grants seeded; unrelated Sales permission gap preserved.
- First full run 769 passed / one old-schema projection failed; corrected and targeted rerun passed. Fresh complete final suite running; final EF/build/HEAD/docs checks next.
