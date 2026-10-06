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

- Latest feature (2026-10-06): Authentication + Authorization + current user/branch
  implemented and verified; stop here, await next user-selected task.
- User requested incremental commit/push to Auth. Implementation commits dc933b0,
  e72ff74 and 24ee95b are pushed to origin/Auth; final docs/checkpoint ready to publish.
- Endpoints: POST /api/auth/login; authenticated GET /api/auth/me. Dynamic policy
  syntax [Authorize(Policy = "Permission:Members.View")]; live role/permission checks.
- Configure JWT key externally (empty in Git). No auth migration, automatic seeding,
  existing DB updates, refresh tokens, branch switching, MFA challenge or CRUD added.
- Final build zero warnings/errors; 58 tests passed, 0 failed/skipped; no model drift.
  All PostgreSQL test schemas/data rolled back. Docs in docs/authentication/.
- Preserve concurrent uncommitted Program.cs weather-route removal by user; excluded
  from assistant final docs commit. Prior Private/Freelance rollout stays separate.
- History skill portable source: skills/project-history-update/SKILL.md.
