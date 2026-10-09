START TRANSACTION;
ALTER TABLE "SubscriptionFreezes" ADD "DaysAlreadyCompensated" integer NOT NULL DEFAULT 0;

ALTER TABLE "SubscriptionFreezes" ADD CONSTRAINT "CK_SubscriptionFreezes_CompensatedDays" CHECK ("DaysAlreadyCompensated" >= 0 AND "DaysAlreadyCompensated" <= "DaysApplied");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261009164635_PreventDuplicateFreezeHolidayCompensation', '10.0.8');

COMMIT;

