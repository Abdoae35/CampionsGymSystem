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

- Membership Plans Management completed on MembershipPlansManagement; stop here, no Subscriptions started.
- BranchId owns/offers/administers plans; CityId retains access meaning. New plans require
  an active owner branch; legacy null stays global-only until explicit mapping.
- Global catalogs require management + Branches.AccessAll. Freelance eligibility permits
  Members from other registration branches and preserves Sales/House ownership.
- All creation/update/status/benefit/eligibility APIs and compatibility paths enforce scope;
  FreelanceHouse registration cannot bypass plan ownership. Snapshot history is untouched.
- Final build: zero warnings/errors. Full suite: 414 passed, zero failed/skipped. EF drift clean.
- Approved additive ownership migration applied/verified only in Development; zero plans,
  no backfill and no production operations. Postman covers 20 canonical + four compatibility routes.
- Implementation commits through a5ed361 pushed. Final documentation/Postman commit and push
  accompany this checkpoint; remote HEAD/clean status are verified before the final response.
- No known unresolved Plans issues; purchase/grants, Check-In, commissions and imports remain separate.
- See PROJECT_HISTORY.md and docs/MEMBERSHIP_PLANS.md; portable history skill:
  skills/project-history-update/SKILL.md.
