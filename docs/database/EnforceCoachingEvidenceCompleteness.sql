START TRANSACTION;
ALTER TABLE "PrivateSessions" DROP CONSTRAINT "CK_PrivateSessions_Operational";

ALTER TABLE "CoachEarnings" DROP CONSTRAINT "CK_CoachEarnings_Rate";

ALTER TABLE "CheckIns" DROP CONSTRAINT "CK_CheckIns_ClassesSubscription";

ALTER TABLE "PrivateSessions" ADD CONSTRAINT "CK_PrivateSessions_Operational" CHECK ("OperationId" IS NULL OR ("BusinessDate" IS NOT NULL AND "RequestHash" IS NOT NULL AND "PrivateCoachAssignmentId" IS NOT NULL AND "SessionValue" IS NOT NULL AND "SessionValue" >= 0 AND "SessionNumber" IS NOT NULL AND "SessionNumber" > 0 AND "TransferredPackage" IS NOT NULL));

ALTER TABLE "PrivateExpirySettlements" ADD CONSTRAINT "CK_PrivateExpirySettlements_Operational" CHECK ("OperationId" IS NULL OR ("RequestHash" IS NOT NULL AND "BusinessDate" IS NOT NULL AND "CreatedByUserId" IS NOT NULL AND "PrivateCoachAssignmentId" IS NOT NULL AND "TransferredPackage" IS NOT NULL AND "Reason" IS NOT NULL AND "RemainderValue" >= 0));

ALTER TABLE "CoachEarnings" ADD CONSTRAINT "CK_CoachEarnings_Rate" CHECK (("TransferredPackage" AND "Rate" = 50) OR (NOT "TransferredPackage" AND "CoachTargetRuleId" IS NOT NULL AND "Target" IS NOT NULL AND "Target" > 0 AND (("TargetProgress" >= "Target" AND "Rate" = 60) OR ("TargetProgress" < "Target" AND "Rate" = 50))));

ALTER TABLE "ClassSessions" ADD CONSTRAINT "CK_ClassSessions_Operational" CHECK ("OperationId" IS NULL OR ("RequestHash" IS NOT NULL AND "BusinessDate" IS NOT NULL AND "CreatedByUserId" IS NOT NULL));

ALTER TABLE "CheckIns" ADD CONSTRAINT "CK_CheckIns_ClassesSubscription" CHECK ("ClassesSubscriptionId" IS NULL OR ("CheckInType" = 'Class' AND "ClassSessionId" IS NOT NULL AND "SessionConsumed" AND "OperationId" IS NOT NULL AND "RequestHash" IS NOT NULL AND "CoachEmployeeId" IS NOT NULL AND "SessionsBefore" IS NOT NULL AND "SessionsAfter" IS NOT NULL));

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261012104721_EnforceCoachingEvidenceCompleteness', '10.0.8');

COMMIT;
