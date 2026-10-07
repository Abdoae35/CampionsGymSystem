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

- Latest task: add Member.ClientCode only; complete on EmployeeManagement branch.
- Required long/bigint chosen after verifying configured development Members empty.
  Unique IX_Members_ClientCode and positive check; no default or invented real codes.
- New 20261007182227_AddMemberClientCode applied successfully to localhost development.
  Member count remains zero; production untouched. Guid PK/FKs and MemberCode unchanged.
- Build 0 warnings/errors; full suite 229 passed, zero failed/skipped; EF no drift.
  Eight new identifier tests; necessary synthetic/legacy migration fixtures updated.
- Schema/local deployment docs updated. Old migrations untouched. Commit/push checked
  after checkpoint; see PROJECT_HISTORY.md. Other databases with uncoded members need
  separate nullable transition review before deploying this required migration.
- Employee Management completed/pushed through 1d10a04. No Member Management feature
  or import implementation started; await next user-selected task.
- Portable history skill: skills/project-history-update/SKILL.md.
