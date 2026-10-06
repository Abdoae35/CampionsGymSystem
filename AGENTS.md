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

- Active task: Authentication + Authorization + current user/branch only.
  User requests commit/push of each verified small subtask to Auth.
- BLL/context subtask dc933b0 is committed/pushed to origin/Auth (30 tests passed).
- JWT/API subtask implemented: /api/auth/login, /api/auth/me, startup JWT validation,
  active user/employee checks on bearer requests. Build zero warnings/errors;
  13 HTTP and all 50 solution tests passed after fixing host configuration timing.
- JWT signing secret must be supplied externally: committed Jwt:Key is empty.
  No auth migration/data changes or real users/roles seeded.
- Next: full solution tests and publish JWT/API subtask; dynamic permission
  policies/tests, then setup/authorization docs. No other business endpoints.
- Prior Private/Freelance migration rollout remains separate; do not apply it.
- History skill portable source: skills/project-history-update/SKILL.md.
