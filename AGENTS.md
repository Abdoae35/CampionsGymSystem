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

- Latest operation (2026-10-06): simplified Postman to Login and Current user;
  optional negative/account-specific tests moved to one folder. Token automatically
  saved in collection variables and explicitly sent by Current user.
- Live Newman verification passed 14 requests/21 assertions. No backend/schema changes.
  Prior build clean and 61 automated tests passed. User's API remains running.
- Reimport postman/Campions.Auth.postman_collection.json; open Campions Auth - Simple Login.
  Select private environment, send Login then Current user. See postman/README.md.
- Verified collection update prepared for commit/push to Auth per user's workflow.
  Previous seed/collection pushes 8f73967 and 3e8afee; no secrets committed.
- No business CRUD implemented. Preserve user cleanup c3752e7.
- History skill portable source: skills/project-history-update/SKILL.md.
