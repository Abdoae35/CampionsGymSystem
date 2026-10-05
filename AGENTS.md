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

For tools that do not automatically read `AGENTS.md`, the user can ask them to
read this file and `PROJECT_HISTORY.md` before working.

## Current handoff

- Latest operation (2026-10-05): added API -> BLL and BLL -> DAL project references.
  API also references DAL for database registration and the local diagnostic.
  See `LOCAL_DATABASE.md` for database run instructions and secret handling.
- Portable skill source: `skills/project-history-update/SKILL.md`.
- Installed locally in `~/.codex/skills/project-history-update`; basic manifest
  checks and source/copy comparison passed. The standard validator lacks PyYAML.
- Verification: solution build passed with zero warnings/errors after reference
  changes. Earlier database check returned healthy; app role requires a password.
- Next step: agree gym schema and migrations; payment provider undecided.
  No commit or push requested. Do not log local credentials.
