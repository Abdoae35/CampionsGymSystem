# Project skills

## Required history checkpoint

Custom `project-history-update` is installed in `~/.codex/skills/` and its portable
source is `skills/project-history-update/SKILL.md`. `AGENTS.md` requires it before
final responses after project operations. It records work in `PROJECT_HISTORY.md`
and refreshes the short handoff in `AGENTS.md`. You can explicitly request:
"Use $project-history-update to record this operation."

Another provider can read the repository source without a local installation.
This is an instruction workflow rather than an executable hook. It records one
concise entry per operation, not every command, to limit repeated context.

Four approved skills were installed on 2026-10-04 in
`~/.codex/skills/`. They will be available on the next turn. These are local
assistant instructions, not application dependencies or security controls.
Changing machines requires reinstalling them; another provider must support
Agent Skills or read the instructions explicitly.

## How to use them

Ask for the task naturally, or name a skill explicitly:

| Skill | Example request |
| --- | --- |
| `aspnet-core` | Use $aspnet-core to implement staff login and authorization policies for our .NET 10 API. |
| `sql-schema-designer` | Use $sql-schema-designer to design members, memberships, leads, invoices, payments, and refunds. Confirm the database engine first. |
| `security-threat-model` | Use $security-threat-model to identify abuse paths in our API, with special attention to staff permissions and payment integrity. |
| `scaffold-dotnet-test-project` | Use $scaffold-dotnet-test-project to create and register our first test project in CampionsGymSystem.slnx. |

Use the skill relevant to the task instead of requesting all four every time.
The threat-model skill is invoked for explicit threat-modeling requests; it may
ask about deployment, data sensitivity, and users before finishing its report.
The test-project skill sets up test wiring; it does not replace ongoing test
authoring for authentication, money calculations, and transaction behavior.

## Token usage

Skill names and descriptions add context. Using a skill loads its instructions
and relevant references, which uses additional input tokens. Only needed
references should be loaded. Extra steps and reports can also increase usage.
Skills can reduce repeated explanation or rework, but lower total token usage is
not guaranteed. Account billing/usage depends on the provider and plan.

Official explanation:
https://developers.openai.com/blog/rethinking-skills-and-prompts-for-gpt-6-astra

## Sources and pinned revisions

Use these repository paths and revisions when reinstalling with skill-installer:

| Repository | Skill path | Revision |
| --- | --- | --- |
| https://github.com/openai/skills | skills/.curated/aspnet-core | 49f948faa9258a0c61caceaf225e179651397431 |
| https://github.com/openai/skills | skills/.curated/security-threat-model | 49f948faa9258a0c61caceaf225e179651397431 |
| https://github.com/jaskirat1616/grok-skills | skills/sql-schema-designer | dfadd540f887c3834e9043a29e9ff402b48aaf66 |
| https://github.com/dotnet/skills | plugins/dotnet-test/skills/scaffold-dotnet-test-project | e87c5da26cbf0f8c701e91247c4817df0adc12f7 |

## Inspection notes

- Downloaded using the built-in skill-installer into a temporary folder,
  inspected manifests and supporting text, then copied the reviewed folders
  into the local Codex skills directory. Verified copied files by SHA-256.
- These four folders contain no executable scripts or installation hooks at
  these revisions. No downloaded code was executed.
- `sql-schema-designer` contains only `SKILL.md`; its referenced examples,
  references, template, and optional validator are absent upstream. Use the
  available instructions and official database documentation; do not claim
  the missing validator was run.
- ASP.NET skill UI metadata says "Windows only", but its instructions cover
  cross-platform ASP.NET Core. Do not treat that label as a platform restriction.
- No extra skills, payment integrations, database services, or .NET packages
  were installed. Database engine and payment provider remain undecided.
