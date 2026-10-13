START TRANSACTION;
CREATE UNIQUE INDEX "UX_AccessAdministration_Operation" ON "AuditLogs" ("EntityId") WHERE "Action" = 'AccessAdministration.Changed';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261013125623_ImplementOwnerAccessAdministration', '10.0.8');

COMMIT;
