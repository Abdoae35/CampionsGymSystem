START TRANSACTION;
ALTER TABLE "CashCounts" ADD "ObservationOrder" bigint GENERATED ALWAYS AS IDENTITY;

CREATE INDEX "IX_CashCounts_BranchId_ObservationOrder" ON "CashCounts" ("BranchId", "ObservationOrder");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261011133824_OrderSharedDrawerObservations', '10.0.8');

COMMIT;

