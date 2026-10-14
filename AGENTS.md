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

- Owner employee-lifecycle protection complete on Permissions&Administration; source checkpoint 15c6588 committed and pushed. Only backend audit finding #1 addressed; the other five findings remain unchanged pending Owner direction.
- Status and hire-date changes affecting Owner-linked authentication now use authoritative stored Owner membership and the existing usable-Owner/recovery-capability checks. Non-Owners cannot administer protected lifecycle changes; self-lockout and loss of the last usable Owner are rejected. Branch scope and effective Allow/Deny behavior remain authoritative.
- Access Administration and lifecycle changes share ReadCommitted advisory coordination before sorted row locks and recheck authorization after waiting. Account locks serialize eligibility changes while allowing foreign-key evidence writes; failed changes roll back lifecycle, ownership/entitlement side effects and audit evidence.
- Added 17 regression cases. Final complete suite 904/904 passed, zero failures/skips; expanded targeted run 132/132 plus deterministic foreign-key locking regression 1/1 passed. Final clean rebuild: zero warnings/errors; EF model drift none. No application migrations, application data changes or Production operations.
- Existing local migration deployment checkpoint, frontend-handoff documentation and untracked docs/database-guide.zip preserved outside this task's staging. Documentation checkpoint records verified results for the authorized publication workflow. Stop after this fix; await the next Owner-approved task.
