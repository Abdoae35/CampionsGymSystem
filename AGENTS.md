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

- Active task (2026-10-06): Authentication + Authorization + current user/branch
  only. Implementing in verified small subtasks, then committing/pushing each to
  Auth as explicitly requested. No unrelated business endpoints or DB rollout.
- First subtask: BLL services/contracts implemented; Identity password/lockout,
  optional Employee, date-valid primary Branch and live permission union.
  Build zero warnings/errors; 10 focused auth and all 30 solution tests passed.
- Next: verify solution regressions and publish first subtask, then JWT API,
  dynamic permission policies/tests, and authentication setup documentation.
- ApplicationUser.IsActive and all persistence needed already exist: no new
  authentication migration. Previous Private/Freelance work is already committed
  by user (6c305d2); its pending rollout remains separate.
- History skill portable source: skills/project-history-update/SKILL.md.
