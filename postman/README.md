# Simple manual API testing

1. Start the API: `dotnet run --project CampionsSystem.API --launch-profile http`.
2. Import `Campions.Auth.postman_collection.json`. Open **Campions API - Auth and Branches**.
3. Select the **Campions Auth - LOCAL PRIVATE** environment (contains demoPassword).
4. Send **1. Login**. Default username is `dev.reception`; change it in Body for another account.
5. Send **2. Current user**. Login saves the token in this collection and Current user sends it automatically. No copying/pasting.

The authentication endpoints are POST /api/auth/login and GET /api/auth/me.
The collection also includes all eight implemented branch/employee assignment routes.
Leave **Optional automated test cases** collapsed. They are examples of successful/failed requests to the same endpoints, not separate APIs. Some intentionally return 401.

If you previously imported the old collection, use the new **Campions API - Auth and Branches** collection. Your imported Postman copy does not update when repository files change.

Login must return 200 and run its Scripts / Post-response script. Current user sends the latest login token; logging in as another account replaces it. If token expires, send Login again. The collection starts with an empty token and passwords are never committed. Do not share/export a collection after login without clearing accessToken.

Private local environment is ignored by Git. Regenerate it with `python3 scripts/setup-local-auth-demo.py` if needed. See [local setup](../docs/authentication/LOCAL_API_TESTING.md).

Automated API permission/isolation tests remain in the .NET test suite; no debug endpoints were added.

## Branch feature requests

Re-import the updated collection file: existing Postman copies do not update automatically.
Login as an appropriately authorized account using **1. Login**; its saved accessToken is
used by every request in folders **3. Branch management** and **4. Employee branch
assignments and transfers**. Optional Owner login tests use their own ownerToken, so they
do not replace the primary accessToken; use 1. Login for management requests.

Set these **collection variables** (leave real passwords/tokens in private local storage):

| Variable | Source |
| --- | --- |
| cityId | Existing active City ID from your development database; no City CRUD endpoint yet |
| branchId | Branch being read/updated/assigned; Create branch saves its returned ID here |
| employeeId | Existing Employee ID selected deliberately; no Employee CRUD endpoint yet |
| targetBranchId | Active destination for transfer, selected deliberately |
| branchName | Desired branch name for create/update |
| assignmentStartDate | YYYY-MM-DD, today or later in configured gym timezone |
| transferEffectiveDate | YYYY-MM-DD, today or later and after current assignment StartDate |

All eight routes have sample requests. PUT is a complete settings update; edit all desired
fields and set isActive false to deactivate. There is no hard-delete request. An employee
with an overlapping current assignment must use transfer, not initial assignment.
GET current assignment can return 204 for an unassigned employee queried globally.

Reception's existing demo grants do not include Branches.View/Manage or employee management.
Use an account with the relevant action permissions. Global operations additionally need
Branches.AccessAll. The existing Owner role needs the explicit local setup described in
[branch isolation](../docs/branches/BRANCH_ISOLATION.md); this collection does not seed
permissions, accounts, employees or cities. A 403 indicates missing permission or scope;
400 may indicate invalid date, inactive target, overlap or same-branch transfer.

Send modifying requests individually against your intended development records; do not run
the entire collection indiscriminately. No Postman requests were sent against your database
as part of adding these examples. The .NET integration tests verify endpoint behavior.
