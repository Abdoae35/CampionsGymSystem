# Test the auth API with persistent local demo data

Automated tests create temporary users/roles/employees/branches inside isolated
transaction-scoped schemas, then roll everything back. Their passing results do
not leave login accounts in your actual development database. This explicit demo
setup supplies persistent auth data for manual API testing instead.

```sh
python3 scripts/setup-local-auth-demo.py
dotnet run --project CampionsSystem.API --launch-profile http -- --seed-auth-demo
dotnet run --project CampionsSystem.API --launch-profile http
```

The helper generates missing Jwt:Key and DemoAuth:Password in existing .NET user
secrets, preserving existing values. Passwords are not displayed or hardcoded.
It writes postman/Campions.Auth.local.postman_environment.json, ignored by Git and
restricted to its owner. Import it locally into Postman; do not share/sync/export
its secret values. Keep tracked collection/template files free of credentials.

The seed command is allowed only in Development against localhost. It uses Identity
password/role APIs and one transaction; it does not apply migrations or erase data.
Reruns retain existing demo passwords, activation, lockout, assignments, and role
customizations. Occupied demo usernames owned by another account cause rollback.
Existing roles are not re-granted permissions; therefore customized roles may make
manual expectations differ from the fresh-demo baseline. Changing a stored demo
password later does not reset existing accounts; use a reviewed Identity operation.

| Username | Purpose |
| --- | --- |
| dev.owner | Active Owner, all baseline permissions, no Employee/branch |
| dev.reception | Active Reception with Employee and Auth Demo Branch |
| dev.no-permissions | Active technical account with no role grants |
| dev.inactive | Inactive Identity account, login must return 401 |
| dev.suspended | Active user linked to suspended Employee, login returns 401 |

Fresh setup creates the six documented roles, 45 baseline permission codes, one
Auth Demo City/Branch, five users, two employees and their assignments. No Member,
payment, membership or other business data is seeded. The prior pending schema
migration is not needed for these existing auth tables.

Use http://localhost:5236 only for this loopback development test profile. Use the
HTTPS launch profile and a trusted development certificate when testing HTTPS;
production always requires HTTPS. There is no public bootstrap endpoint or automatic
startup seed. Owner/Reception tokens and /me exercise available API endpoints;
permission-protected 403 behavior remains covered by automated test-host-only probes
until a genuine protected business action exists.
