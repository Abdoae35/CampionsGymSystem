# Owner permissions administration V1

Baseline: clean user-created `Permissions&Administration`, HEAD `da601e0`.
Existing Identity memberships, Permission/RolePermission/UserPermissionOverride and
AuditLog persistence are sufficient. Runtime permissions and roles already reload
from the database for every authenticated request; Deny wins. Canonical Owner
recognition uses the existing role store's normalized `OWNER` name.

Implement `/api/access-administration`: grouped catalog, roles/grants, paginated
users with employee/branch context, effective permission explanations, paginated
change history, and three desired-state PUTs for membership, grant and override.
Reads and writes require a current active/unlocked eligible Owner plus existing
Users.View/Manage or Roles.View/Manage capabilities. Generic management or global
branch capabilities never authorize access administration.

Each PUT requires operation ID, current opaque resource version and reason.
Serialize access mutations with a PostgreSQL transaction advisory lock, recheck
actor after acquiring it, lock affected account/Owner lifecycle rows, enforce
actor recovery and at least one usable Owner with all four administration
capabilities, and atomically persist audit evidence and result. Retries return
the original result; conflicting operation reuse or stale versions return 409.
Only existing roles/catalog permissions are accepted; no Owner provisioning,
role creation or account/employee lifecycle administration. Removing another
Owner requires another usable Owner; self-removal is prohibited.

Add only a forward unique filtered operation index on access AuditLogs and a
tracked append-only guard. Preserve all old migrations and role seeds/grants.
No branch assignments, member/lead ownership, commissions or financial facts
are changed by access configuration.

Verify Owner/Manager boundaries, multi-role union, Allow/Deny/clear, immediate
revocation, version conflicts/retries/concurrency, self/last-Owner protection,
audit immutability, branch/history preservation and invalid inputs. Run full
regression, clean build and EF drift; migrate only disposable localhost test
schemas. Document actual APIs, operational prerequisite grants and controlled
offline recovery, then update Postman/history and publish verified checkpoints.
