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

- CRM Sales ownership correction completed on CRM-/-Leads-&-Follow-up-Management.
- Transfers preserve Member/Lead assignments and owned old-branch follow-up visibility;
  new Lead distribution still validates the employee's current operational branch.
- Non-active employment closes current ownership to House/unassigned management work;
  history remains, no random reassignment, reactivation does not restore closed ownership.
- Active authorized employees may convert another owner's Lead; locked current owner wins,
  destination/referral scope applies, converter is creation/audit actor. Phone lookup can
  return an ID-only conversion reference without exposing private Lead details.
- Final build zero warnings/errors; full suite 370 passed, zero failed/skipped; EF drift clean.
- First correction 52a58a5 pushed; final conversion/docs/Postman commit accompanies this
  checkpoint. No schema changes/production operations. Stop here; no next feature started.
- CRM automatic Member status/expiry/attendance task generation remains deferred.
- See PROJECT_HISTORY.md; portable history skill: skills/project-history-update/SKILL.md.
