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

- Latest operation (2026-10-07): completed Activities/Classes/Freelance update;
  ActivityCoach supports internal/external, purchase coach/rate snapshots, actual
  collected revenue and explicit period settlements; Class occurrences/stats only;
  Freelance eligibility and House purchases with preserved Sales history.
- New migration 20261007000549_UpdateActivitiesClassesAndFreelanceRules; four tables,
  six nullable added columns, legacy FK made nullable. SQL reviewed; no user DB update.
  Model now 66 application + seven Identity tables. Old migrations unchanged.
- Final build zero warnings/errors; 103 tests passed, zero failed/skipped; no model drift.
  Legacy upgrade/constraints/report SQL and HTTP authorization tested with rollback.
- Eight new permissions; Owner grants through migration; other role grants preserved.
  No Class compensation, payment engine, payroll redesign or unrelated CRUD added.
- See docs/database/UPDATE_ACTIVITIES_CLASSES_AND_FREELANCE.md for exact files, API,
  assumptions and deployment. Unknown legacy coaches/rates/eligibility remain explicit
  reconciliation tasks; Class payout and late closed-period adjustments unresolved.
- Current branch DataBaseInit. Changes/checkpoint uncommitted/unpushed; previous
  incremental Auth publication instruction related to the authentication task.
  Await next user-selected task; no automatic migration/publication or branch switch.
- History skill portable source: skills/project-history-update/SKILL.md.
