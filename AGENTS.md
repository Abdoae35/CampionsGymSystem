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

- The user confirmed the feature branch was merged into `master`; live remote `master` and `Fix5-InstaPay-Provider-References` were verified at `96c6239`, containing Fix #5. The current checkout is the user-created `Fix-#6--Monthly-Closing-Historical-Data-Scalability` branch at `96c6239`, with only this uncommitted history/handoff checkpoint. The user creates/checks out branches and handles merges; assistants must not create branches independently and should push completed verified tasks to the user-provided branch.
- Backend audit Fix #5 InstaPay provider-reference export was implemented as `c937219` on `Fix5-InstaPay-Provider-References`; Fixes #1–#4 remain ancestors and finding #6 was not touched.
- New monthly reports, the paginated financial-month transactions API and newly frozen six-sheet XLSX workbooks include nullable `ProviderReference` per ledger/account row. One reference is exported exactly, multiple same-account references are a JSON array, and absent/empty references yield a blank cell. Part-level `SourceEvidence` retains every stored value. Payments/refunds, ledger totals, commissions, optional input and old frozen revisions are unchanged.
- Fix #5 verification: three focused tests and full isolated suite 936/936 passed (38m31s durable rerun), clean build zero warnings/errors, EF model drift none, staged diff clean. No migration or Development/Production data change. Earlier Owner-authorized Fix #4 Development activation completed with 29 migrations, zero pending and a verified backup; see `PROJECT_HISTORY.md`. The prior activation history/handoff edits remain uncommitted locally and were excluded from the Fix #5 commit; this checkpoint extends them without staging or pushing them.

### Retained prior local deployment checkpoint (historical)

- Authorized local Development migration deployment complete on Permissions&Administration at c74c122: verified localhost:5432/campions_gym, Homebrew PostgreSQL 14.20, no other database clients; 23 applied migrations/100 tables before deployment.
- Private/Classes, coaching evidence completeness, Monthly Closing/payments and Owner administration migrations applied individually in chronological order through EF Core. All 27 migrations now applied, zero pending; 112 tables including 7 Identity and EF history. Production/shared live databases untouched; no seeder, reset, truncation, source/migration edit, commit or push.
- Timestamped custom-format backup: /Users/macbookpro/Backups/Campeones/20261013T165740Z/campions_gym_20261013T165740Z.dump (471,042 bytes); pg_dump succeeded, archive lists 100 tables/100 data entries, full archive read/decompression and SHA-256 verified. No restore was performed.
- Build passed with zero warnings/errors; EF pending-model check passed with local execution permissions after sandboxed design-time check timed out. Live column/type/nullability/PK and 355 FK comparison passed; CHECK names and index columns/uniqueness match. Pre-existing values/counts preserved across all 100 original tables. Existing commission-summary index naming truncation remains nonblocking.
- At that earlier deployment checkpoint, source, historical migrations, frontend-handoff files and the database-guide ZIP were preserved; only its history/handoff text changed then. No role grants, coach targets or financial months were configured by that deployment. Its prior regression result was 887 tests; no full suite was rerun for that schema-only operation.
