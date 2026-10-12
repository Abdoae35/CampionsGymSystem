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

- Private/Classes confirmed backend scope implemented on PrivateCoaching-Classes-Implementation from clean baseline 0bc4476; implementation checkpoints bf733e2 (domain), eb4b538 (operations/APIs), 28e4abc (history guards/migration regression) committed and pushed.
- Final full suite 822/822 passed, zero failures/skips, 50 added cases over independently verified 772; bounded fixture run 32m31s. Focused 84/84, migration/expiry/new-package 29/29 and class history 10/10 passed. Clean rebuild zero warnings/errors; EF drift none; all 46 historical migration files unchanged.
- Two forward migrations generated only: 20261012101654_ImplementPrivateClassesAndCoachEarnings and 20261012104721_EnforceCoachingEvidenceCompleteness. No application/Production migration, operational seed money/rules or role grants/overrides changed; eight catalog capabilities only.
- Private delivered/Owner-expiry sources recognize only retained net funded value; deferred slices preserve original coach. Normal 50/60 requires explicit independent coach/branch-period target, incoming transferred assignment fixed 50%; all expiry unused quantity belongs to coach assigned at expiry without fake attendance. Owner partial payouts use existing Expense/debit and available funds once.
- Classes share one package balance across types, require no reception/gym membership, preserve actual member/occurrence history and completed-occurrence averages. Configurable average targets; Classes compensation disabled pending formula. Finalized-earnings refunds and payout corrections rejected pending Owner adjustment policy.
- docs/PRIVATE_CLASSES.md documents 36 authorized routes, setup, exact financial rules and limitations; Postman folder 18 has 37 validated examples. Test fixture parallelism capped at two to prevent local PostgreSQL lock-table exhaustion; actual competing transaction tests remain parallel.
- Final verification/documentation complete; checkpoint commit/push status is verified from Git at delivery. Concurrent user docs/frontend-handoff/ preserved outside assistant commits. Operational scope complete; no further feature starts automatically.
