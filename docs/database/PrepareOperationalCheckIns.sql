START TRANSACTION;
DROP INDEX "IX_BorrowedGymSessions_CheckInId";

DROP INDEX "IX_BorrowedGymSessions_ExpiredSubscriptionId";

DROP INDEX "IX_BorrowedGymSessions_MemberId";

ALTER TABLE "CheckIns" ADD "BusinessDate" date;

ALTER TABLE "CheckIns" ADD "OperationId" uuid;

ALTER TABLE "CheckIns" ADD "SessionsAfter" integer;

ALTER TABLE "CheckIns" ADD "SessionsBefore" integer;

ALTER TABLE "BorrowedGymSessions" ADD "SettledByTransactionId" uuid;

CREATE INDEX "IX_CheckIns_BranchId_BusinessDate_MemberId" ON "CheckIns" ("BranchId", "BusinessDate", "MemberId");

CREATE UNIQUE INDEX "IX_CheckIns_OperationId" ON "CheckIns" ("OperationId");

ALTER TABLE "CheckIns" ADD CONSTRAINT "CK_CheckIns_OperationalDate" CHECK ("OperationId" IS NULL OR "BusinessDate" IS NOT NULL);

ALTER TABLE "CheckIns" ADD CONSTRAINT "CK_CheckIns_SessionSnapshot" CHECK (("SessionsBefore" IS NULL AND "SessionsAfter" IS NULL) OR ("SessionsBefore" IS NOT NULL AND "SessionsAfter" IS NOT NULL AND "SessionsBefore" > 0 AND "SessionsAfter" = "SessionsBefore" - 1 AND "CheckInType" = 'Gym' AND "SessionConsumed"));

CREATE UNIQUE INDEX "IX_BorrowedGymSessions_CheckInId" ON "BorrowedGymSessions" ("CheckInId");

CREATE INDEX "IX_BorrowedGymSessions_ExpiredSubscriptionId_Status" ON "BorrowedGymSessions" ("ExpiredSubscriptionId", "Status");

CREATE INDEX "IX_BorrowedGymSessions_MemberId_Status" ON "BorrowedGymSessions" ("MemberId", "Status");

CREATE INDEX "IX_BorrowedGymSessions_SettledByTransactionId" ON "BorrowedGymSessions" ("SettledByTransactionId");

ALTER TABLE "BorrowedGymSessions" ADD CONSTRAINT "CK_BorrowedGymSessions_GrantSettlement" CHECK ("SettledByTransactionId" IS NULL OR ("Status" = 'Settled' AND "SettledAt" IS NOT NULL AND "SettledBySubscriptionId" IS NOT NULL));

ALTER TABLE "BorrowedGymSessions" ADD CONSTRAINT "FK_BorrowedGymSessions_MembershipSubscriptionTransactions_Sett~" FOREIGN KEY ("SettledByTransactionId") REFERENCES "MembershipSubscriptionTransactions" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261009200017_PrepareOperationalCheckIns', '10.0.8');

COMMIT;

