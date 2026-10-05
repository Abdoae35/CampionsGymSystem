---
name: project-history-update
description: Record completed project operations in PROJECT_HISTORY.md and refresh the handoff in AGENTS.md before the final response. Use after project changes, investigations, setup, or decisions in repositories that require this history workflow.
---

# Project History Update

For this project's required history workflow, finish every project operation
with a documentation checkpoint before the final response. Group related tool
calls into one operation; do not log each file read or command separately.
Include read-only investigations, failed or blocked attempts, and project
decisions. Pure conversational acknowledgments need no entry unless they change
the project's decisions or preferences.

Read the repository's `AGENTS.md` and `PROJECT_HISTORY.md` before work. At the
checkpoint:

1. Append a concise dated entry to `PROJECT_HISTORY.md`: request, actual actions,
   changed paths, decisions, verification results, limitations, remaining work,
   and commit/push state. Record failures accurately; never claim unrun checks.
2. Refresh its current-state section to match the actual project.
3. Update the bounded `Current handoff` section in `AGENTS.md` with the latest
   operation, result, and next step or blocker. Keep permanent instructions
   intact. Link to history rather than copying the full log into AGENTS.md.
4. Inspect the resulting edits and Git status. Confirm both documents reflect
   this operation before responding. Documentation updates themselves do not
   require another history entry; avoid recursive logging.

Preserve user changes and existing history. Keep credentials, tokens, connection
strings, personal records, and other sensitive values out of both documents.
Do not commit, push, install other skills, or change application code merely to
complete the checkpoint. If documentation cannot be written within permissions,
state that limitation and provide a short handoff in the response.

This skill is an instruction workflow, not an execution hook. Its reliable use
depends on the host reading repository instructions and following them.
