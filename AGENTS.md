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

- Private/Classes implementation in progress on PrivateCoaching-Classes-Implementation from clean verified baseline 0bc4476 (772/772 passed; EF baseline drift none).
- Domain checkpoint adds shared Classes packages/balances and immutable Private attribution, session/remainder, independent coach targets, earnings and partial payout evidence; migration 20261012101654_ImplementPrivateClassesAndCoachEarnings is generated only, not applied to application/Production.
- Owner confirmed: expiry unused sessions all belong to coach assigned at expiry; incoming transferred package fixed 50%; Private target uses actual net funded delivered/expiry value; partial collections defer unfunded value. Classes target is average actual attendees; compensation remains disabled until defined.
- Owner confirmed rejection of refunds affecting finalized Private earnings until adjustment policy exists. Original Private rate is 50/60 with explicit per-coach/branch-period target; no defaults, attendance unblocked by missing targets.
- Domain build zero warnings/errors, three model/forward-migration checks passed; old migration files unchanged. Operational API/services/tests are uncommitted work in progress; initial operational failures corrected, focused rerun pending.
- User-created docs/frontend-handoff/ appeared during work and is preserved outside assistant commits. Continue focused validation, operational/API checkpoints and full regression; user explicitly authorized checkpoint commits/pushes. No application DB update, no grants seeded.
