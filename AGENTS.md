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

- Sales Commissions implementation on SalesCommissions&Targets from verified f8530e9 / completed backend 5244d48; initial tree clean and remote verified.
- Owner confirmed newest-first refunds, deterministic PaidAt then Guid chronology; refunds touching finalized source periods reject atomically. No unresolved refund policy.
- Domain prepared: reuse periods/rules/summaries; three append-only collection/allocation/statement evidence tables, nullable legacy-compatible date/operation/snapshot fields; new AddSalesCommissionEvidence migration reviewed.
- Three domain tests passed; build zero warnings/errors before calculation draft. Prior fresh full baseline 733 passed; final complete feature validation pending.
- BLL configuration/source/calculation work is in progress separately; no production or application data/permission mutations. Migration Development application pending.
- Four configuration/lifecycle/scope tests passed. Next: verify actual collection/refund attribution and calculations, APIs/reporting/tests/docs. Existing unrelated Sales permission gap preserved.
