# Test the local authentication API

1. Start the API: `dotnet run --project CampionsSystem.API --launch-profile http`.
2. Import `Campions.Auth.postman_collection.json` into Postman.
3. Import and select `Campions.Auth.local.postman_environment.json` (generated locally, ignored by Git).
4. Run the collection in order. Successful logins automatically save bearer tokens for `/api/auth/me`.

The local environment already contains the demo password. Keep it private; do not sync, share or commit passwords/tokens. The committed environment is a blank template, usable after supplying your own credentials.

Accounts: `dev.owner`, `dev.reception`, `dev.no-permissions`, `dev.inactive`, `dev.suspended`. Owner has no employee/branch. Reception has Demo Reception and Auth Demo Branch. The last two accounts cannot log in.

See [local setup instructions](../docs/authentication/LOCAL_API_TESTING.md) to regenerate the environment or seed a different local development database. No migrations are applied by the seeder.

Twelve requests check login/current context, invalid password, inactive user, suspended employee, invalid input, missing authentication and invalid token. No business/debug endpoint was added. Permission-protected 200/403 behavior is verified by the automated API tests until business endpoints exist.

Collection format: [official Postman v2.1 schema](https://schema.postman.com/).
