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

- Private/Classes operational implementation on PrivateCoaching-Classes-Implementation from verified baseline 0bc4476 (772/772 passed); domain bf733e2 and operational eb4b538 committed and pushed.
- Private sale snapshots/reception-required attendance/transfers/Owner expiry; shared Classes purchases/direct attendance/history/averages; explicit independent coach targets, net funded earning slices and Owner partial Expense/ledger payouts implemented with effective branch permissions, retry/row locks and refund finalization guards.
- Confirmed: expiry unused sessions belong to assigned coach; incoming transferred assignment fixed 50%; normal Private 50/60 on explicit period target; partial collections defer unfunded sources. Classes average targets configurable; compensation remains disabled pending formula. Finalized earnings refund/correction rejected pending adjustment policy.
- Latest focused run 84/84 passed (47 new cases plus 37 existing); clean rebuild zero warnings/errors. Initial HTTP query translation failure corrected and rerun green. New forward completeness constraints close nullable evidence loopholes; no old migration edits.
- Migrations 20261012101654_ImplementPrivateClassesAndCoachEarnings and 20261012104721_EnforceCoachingEvidenceCompleteness generated only, not applied to application/Production; eight catalog capabilities only, no role grants or default rules/money seeded.
- Additional expiry/new-package and corrected legacy migration cases passed 29/29; managed class immutability/completion cases passed 10/10. First full run has five historical-fixture failures now corrected; final full rerun remains required. docs/PRIVATE_CLASSES.md and 37 Postman examples draft validated across 36 routes. Concurrent user docs/frontend-handoff/ preserved outside commits.
