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

- Frontend handoff finalized documentation-only on actual clean-start master at 476f0a77bf4f0a75839ab3b9a7f5e65dbc284b80 (FrontEndDoc), containing completed backend a2a0c56; no branch switch/reset/commit/push. Existing handoff was tracked at inspection and preserved/reused; historical branch-only captures retained.
- docs/frontend-handoff/README_START_HERE.md is the current entry. 191 documented controller operations across 23 controllers, including all 36 Private/Classes/coach-finance operations; 37 Coaching Postman examples. Root contracts supersede earlier draft/baseline briefings.
- Fresh isolated real OpenAPI 3.1.1: 153 paths/192 operations/102 schemas. openapi.integration.json adds source-derived typed responses, auth and property enum converters (243 total schemas); raw export unchanged. Catalog/compiler supplement supplies BLL validation/scope details. Backend projects built; temporary exporter alone has three nullable warnings. No business tests rerun; implementation history remains 822/822.
- Portable ZIP: artifacts/Campeones-Frontend-Handoff-2026-10-12-476f0a7.zip, 111 files, 660249 bytes. Route/DTO/permission/reference/example checks, 2240 portable links, ZIP CRC/exact contents and manifest hashes passed. Artifacts directory is ignored; documentation changes remain uncommitted.
- No backend source/controllers/services/DTOs/migrations, application/Production database, grants or operational data changed. Two Coaching migrations remain generated-only per implementation handoff; actual deployment schema/grants/configuration unqueried. Explicit rollout periods/thresholds/accounts/drawer/category/own shifts and CORS/proxy/host configuration remain operational prerequisites.
- Classes compensation remains unimplemented until Owner formula definition. Finalized-earnings refund adjustments, coach-payout correction and Sales commission payouts remain unavailable. No next feature starts automatically; see PROJECT_HISTORY.md for full verification and limits.
