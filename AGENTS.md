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

- Branch feature complete on BranchSepration; eight prior commits verified pushed through
  a69af6a. Latest follow-up adds all eight branch/assignment/transfer endpoints to existing
  postman/Campions.Auth.postman_collection.json; displayed name Auth and Branches.
- Re-import collection; primary Login supplies accessToken. Set selected IDs and calendar
  dates deliberately. README explains permissions, scope and explicit AccessAll setup.
- Follow-up build 0 warnings/errors; 33 auth/branch HTTP tests passed; collection JSON,
  exact route coverage and bodies validated. Prior full suite 202 passed. No live
  Postman/Newman run, application code/schema changes or application database writes.
- Postman commit 3adb493 is local and NOT pushed. Initial push and retry both failed
  with GitHub Internal Server Error; origin remains a69af6a. Separate local history
  checkpoint records failure. Retry git push origin HEAD later; no force push/amend.
  Private environment intact.
- Explicit global branch permission setup is still not automatically run. Other branch
  rules and deferred workflows remain in docs/branches and PROJECT_HISTORY.md.
- Await next user-selected task; no unrelated module started.
- Portable history skill: skills/project-history-update/SKILL.md.
