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

Work one user-selected task at a time. Complete its implementation, relevant
verification, and history checkpoint before moving on. Do not expand a task into
whole-project implementation or start the next task without user direction.

For tools that do not automatically read `AGENTS.md`, the user can ask them to
read this file and `PROJECT_HISTORY.md` before working.

## Current handoff

- Latest operation (2026-10-07): Membership Benefits schema-only alignment complete;
  75 application + seven Identity tables. Dynamic catalog, per-plan Quantity/Access,
  purchase-batch snapshots and usage history; no API/BLL feature implementation.
- New migration 20261007153001_AddMembershipBenefits and reviewed forward SQL create
  four tables only; all FKs Restrict. Previous FinalBusinessSchemaAlignment and all
  older migration files unchanged (SHA-256 checked). No application database update.
- Final build zero warnings/errors; full suite 156 passed, zero failed/skipped;
  no EF model drift. PostgreSQL test schemas/data rolled back. Sorting-only test
  failure corrected before successful final full run.
- No redundant subscription FK, independent validity engine, mutable balance or trigger.
  Benefit carryover/expiry/FIFO/reset policy NOT FINALIZED. Snapshot creation, usage
  validation, concurrency and auditing remain future BLL. Other features preserved.
- Gap analysis, paths, relationships and decisions: docs/database/MEMBERSHIP_BENEFITS.md.
  Chronological record and previous feature limitations: PROJECT_HISTORY.md.
- Branch DataBaseInit; started clean at user commit 8f622d8. Benefits changes/checkpoint
  uncommitted/unpushed. Await user review/next selected task; do not start automatically.
- History skill portable source: skills/project-history-update/SKILL.md.
