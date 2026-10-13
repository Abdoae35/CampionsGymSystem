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

- Owner-only Permissions & Administration V1 implemented on user-created Permissions&Administration from clean da601e0; verified API/security checkpoint pending publication. Frontend handoff and 52 historical migration files unchanged.
- Nine access-administration routes reuse existing Identity memberships, canonical catalog, role grants, user Allow/Deny overrides and PermissionService; API/BLL require normalized OWNER store membership plus effective Users/Roles capabilities. No seeder/grant/financial/branch/ownership changes.
- Versioned/idempotent desired-state writes, current actor revalidation after serialized lock, self/last-Owner recovery safeguards and immutable audit evidence. Owner provisioning unavailable; trusted offline bootstrap/recovery required if no usable configured Owner.
- Forward migration 20261013125623_ImplementOwnerAccessAdministration adds one unique filtered audit operation index only. Exercised in disposable localhost schemas; application/Production migrations and data untouched.
- Verification: authentication API 125/125; corrected concurrency 5/5; final focused 29/29; final follow-up 4/4; clean rebuild zero warnings/errors; EF drift none; SQL/contract/Postman checks verified. Fixture and overlapping-rebuild test-host failures corrected and recorded below. Full regression now running, not yet complete.
- docs/PERMISSIONS_ADMINISTRATION.md and ten Postman examples cover all nine routes; final documentation publication awaits complete regression. Remaining rollout: existing active Owner and four core administration capabilities, reviewed migration application and deliberate role/override configuration. No defaults silently expanded.
