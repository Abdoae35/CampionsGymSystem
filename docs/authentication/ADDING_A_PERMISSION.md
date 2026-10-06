# Add a permission

1. Choose a stable exact code, such as Members.View. Through trusted configuration,
   add Permission with human Name, Module, optional Description and IsActive=true.
   Code must be unique/case-sensitive. No enum/JWT change or data-only migration.
2. Add it to controlled bootstrap/configuration input where appropriate. No automatic
   seeder/admin endpoint exists; preserve deliberate revocations/inactive codes.
3. Add RolePermission mappings to reviewed Identity roles. Users receive their role
   union. Owner needs explicit grants too. Use RoleManager/UserManager for roles.
4. Protect the future endpoint:

   ```csharp
   [Authorize(Policy = "Permission:Members.View")]
   ```

   Branch-scoped actions must additionally compare resource BranchId with current
   branch in BLL/API, explicitly define Owner/global behavior and handle null safely.
5. Test HTTP outcomes: anonymous 401, authenticated without grant 403, active grant
   200. Reuse the JWT after revocation/deactivation and require 403; test multiple
   roles, duplicate grants and future branch rules when implementing that action.

Dynamic PermissionPolicyProvider and DB-backed handler resolve Permission:<code>;
no per-code registration is needed, and ordinary policies continue working. Do not
embed permissions in JWT or use role-only checks for normal business actions.

Run `dotnet test CampionsGymSystem.slnx` using a disposable local PostgreSQL
GYM_SCHEMA_TEST_CONNECTION. Probe controllers are test-host-only; no permanent debug
API routes or Members CRUD are part of this feature.
