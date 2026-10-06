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

Work one user-selected task at a time. Complete its implementation, relevant
verification, and history checkpoint before moving on. Do not expand a task into
whole-project implementation or start the next task without user direction.

For tools that do not automatically read `AGENTS.md`, the user can ask them to
read this file and `PROJECT_HISTORY.md` before working.

## Current handoff

- Latest operation (2026-10-06): completed targeted Private/Freelance schema and
  flow update; see PROJECT_HISTORY.md and docs/database/UPDATE_PRIVATE_AND_FREELANCE.md.
- New migration: DAL/Migrations/20261006014644_UpdatePrivateAndFreelanceBusinessRules.cs;
  review SQL in docs/database/. InitialCreate unchanged. NOT applied to the existing
  local database; local DB still has 61 application tables, updated model has 62.
- Added session batch/period FKs, batch remaining counter, unique expiry settlements,
  Freelance plan category/Coach and purchase snapshots. Legacy assignment financial
  fields preserved/deprecated; migration rejects ambiguous legacy private history.
- Verification: builds zero warnings/errors; 20 PostgreSQL tests passed through the
  solution; no model drift, no Up table/column drops or new cascades. Test schemas
  and data roll back; existing gym database untouched.
- Next: await user-selected task. BLL flows are documented, not implemented.
  Require explicit permission before applying to existing development DB; reconcile
  legacy data if preflight rejects it. No commit/push; do not record credentials.
- History skill portable source: skills/project-history-update/SKILL.md.
