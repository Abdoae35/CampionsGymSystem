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

- Latest operation (2026-10-05): implemented Guid Identity and 61 application
  persistence tables in DAL, generated InitialCreate, and applied it locally.
  Context: `CampionsSystem.DAL/Data/ApplicationDbContext.cs`; secret key is now
  `ConnectionStrings:DefaultConnection`. See `docs/database/` and LOCAL_DATABASE.md.
- Portable skill source: `skills/project-history-update/SKILL.md`.
- Installed locally in `~/.codex/skills/project-history-update`; basic manifest
  checks and source/copy comparison passed. The standard validator lacks PyYAML.
- Verification: build zero warnings/errors; no model drift; local migration and
  rolled-back constraint checks passed. No cascading business FKs or shadow FKs.
- Next step: implement authorized BLL workflows against the documented rules;
  payment provider undecided. No controllers/business services added in this task.
  No commit or push requested. Do not log local credentials.
