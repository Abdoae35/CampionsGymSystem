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
For future feature work, the user creates and checks out the branch. Do not
create or switch to a new feature branch on your own. Complete and push each
user-assigned task on that branch after verification; the user handles merging
it into master and creating the next branch.
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

- Planned Periodic Expenses V1 is implemented and pushed as `933ff78` on the user-created `PlannedPeriodicExpenses` branch. `master` was not merged or switched; the user handles merging. Audit Fixes #1–#6 remain in the branch ancestry.
- Existing maintenance revisions now include optional description and explicit next due date; Owner-only writes, active-branch plan locking, rounded branch monthly suggestions, and paginated overdue/upcoming reminders are implemented. Plans remain informational and do not create Expenses, cash movements or reserve balances. Frontend contract addendum is `docs/frontend-handoff/PLANNED_PERIODIC_EXPENSES.md`; the older all-module OpenAPI capture still needs final recapture.
- Five focused tests and the final full isolated suite 943/943 passed; clean build had zero warnings/errors, EF model drift was none, and staged diff check passed. The additive migration `20261016114618_AddPlannedPeriodicExpenseDescriptionAndDueDate` was tested in isolated schemas only; Development and Production were untouched. This history/handoff-only checkpoint is uncommitted; see latest `PROJECT_HISTORY.md` entry. Wait for user direction on migration activation and final frontend handoff.

### Retained prior local deployment checkpoint (historical)

- Authorized local Development migration deployment complete on Permissions&Administration at c74c122: verified localhost:5432/campions_gym, Homebrew PostgreSQL 14.20, no other database clients; 23 applied migrations/100 tables before deployment.
- Private/Classes, coaching evidence completeness, Monthly Closing/payments and Owner administration migrations applied individually in chronological order through EF Core. All 27 migrations now applied, zero pending; 112 tables including 7 Identity and EF history. Production/shared live databases untouched; no seeder, reset, truncation, source/migration edit, commit or push.
- Timestamped custom-format backup: /Users/macbookpro/Backups/Campeones/20261013T165740Z/campions_gym_20261013T165740Z.dump (471,042 bytes); pg_dump succeeded, archive lists 100 tables/100 data entries, full archive read/decompression and SHA-256 verified. No restore was performed.
- Build passed with zero warnings/errors; EF pending-model check passed with local execution permissions after sandboxed design-time check timed out. Live column/type/nullability/PK and 355 FK comparison passed; CHECK names and index columns/uniqueness match. Pre-existing values/counts preserved across all 100 original tables. Existing commission-summary index naming truncation remains nonblocking.
- At that earlier deployment checkpoint, source, historical migrations, frontend-handoff files and the database-guide ZIP were preserved; only its history/handoff text changed then. No role grants, coach targets or financial months were configured by that deployment. Its prior regression result was 887 tests; no full suite was rerun for that schema-only operation.
