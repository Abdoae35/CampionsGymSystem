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

- Sales / Payments complete on Sales/Payments; Tasks 1–12 plus final queued-successor renewal fix verified.
- Reuse existing Sale/Payment/Refund/accounts/ledger, Offer, Installment, Subscription and Check-In domains; no raw paid-grant API.
- Full suite: 678 passed, zero failed/skipped; build zero warnings/errors; EF drift clean. Three new migrations applied to localhost Development only; Production untouched.
- Normal cancellation reverses only its Sale grant and reopens debt settled by that Sale through append-only history; Quick Cancel rejects debt-settling grants.
- New management capabilities are cataloged without inferred existing-role grants; explicitly assign permissions when configuring staff.
- Implementation/review commits through f52e997 pushed; the final documentation checkpoint contains the completed feature record. Closing publication and HEAD/status results are reported at task completion.
- Stop after this feature. Commission, payroll, Shift/Vault/Expenses, Private/Classes finance, Member branch settlement, frontend and offline sync remain deferred.
- See PROJECT_HISTORY.md; portable history skill: skills/project-history-update/SKILL.md.
