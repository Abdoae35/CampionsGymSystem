START TRANSACTION;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('5932dca8-2dcc-57b2-b0b3-aeac0c4dfdb3', 'Private.RegisterSession', 'Private.RegisterSession', 'Private', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('ccca7368-03c0-57c9-a7aa-ca853840140b', 'Private.Transfer', 'Private.Transfer', 'Private', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('374c9757-42d1-52dd-858b-23f1cb0b6f97', 'Private.SettleExpiry', 'Private.SettleExpiry', 'Private', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('94897b97-5f90-5e85-a4fd-345335a12d05', 'Classes.RegisterAttendance', 'Classes.RegisterAttendance', 'Classes', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('9926fc9e-28e4-59ae-9c05-4118819c392d', 'CoachEarnings.View', 'CoachEarnings.View', 'CoachEarnings', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('50cd7ee3-397b-5935-b068-421dc557a932', 'CoachEarnings.ManageTargets', 'CoachEarnings.ManageTargets', 'CoachEarnings', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('20d0bdd5-4c22-5fbe-b5ec-5d857735f34e', 'CoachEarnings.Recognize', 'CoachEarnings.Recognize', 'CoachEarnings', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('614dfe9a-fc31-5121-927d-3cfdfec83150', 'CoachEarnings.Pay', 'CoachEarnings.Pay', 'CoachEarnings', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;

ALTER TABLE "SaleTransactions" DROP CONSTRAINT "CK_SaleTransactions_SaleType";

DROP INDEX "IX_PrivateSubscriptionTransactions_SaleTransactionId";

DROP INDEX "IX_PrivateSessions_GymCheckInId";

DROP INDEX "IX_PrivateSessions_PrivateSubscriptionTransactionId";

ALTER TABLE "PrivateExpirySettlements" DROP CONSTRAINT "CK_PrivateExpirySettlements_EligibleAmount";

ALTER TABLE "CheckIns" DROP CONSTRAINT "CK_CheckIns_SessionSnapshot";

ALTER TABLE "PrivateSubscriptionTransactions" ADD "OriginalAssignmentId" uuid;

ALTER TABLE "PrivateSubscriptionTransactions" ADD "OriginalCoachEmployeeId" uuid;

ALTER TABLE "PrivateSubscriptionTransactions" ADD "PackageNameSnapshot" character varying(256);

ALTER TABLE "PrivateSubscriptionTransactions" ADD "RemainderValue" numeric(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE "PrivateSubscriptionTransactions" ADD "StartDateSnapshot" date;

ALTER TABLE "PrivateSubscriptions" ADD "Version" integer NOT NULL DEFAULT 0;

ALTER TABLE "PrivateSessions" ALTER COLUMN "CommissionPeriodId" DROP NOT NULL;

ALTER TABLE "PrivateSessions" ADD "BusinessDate" date;

ALTER TABLE "PrivateSessions" ADD "OperationId" uuid;

ALTER TABLE "PrivateSessions" ADD "PrivateCoachAssignmentId" uuid;

ALTER TABLE "PrivateSessions" ADD "RequestHash" character varying(64);

ALTER TABLE "PrivateSessions" ADD "SessionNumber" integer;

ALTER TABLE "PrivateSessions" ADD "SessionValue" numeric(18,2);

ALTER TABLE "PrivateSessions" ADD "TransferredPackage" boolean;

ALTER TABLE "PrivateExpirySettlements" ADD "BusinessDate" date;

ALTER TABLE "PrivateExpirySettlements" ADD "OperationId" uuid;

ALTER TABLE "PrivateExpirySettlements" ADD "PrivateCoachAssignmentId" uuid;

ALTER TABLE "PrivateExpirySettlements" ADD "Reason" character varying(2000);

ALTER TABLE "PrivateExpirySettlements" ADD "RemainderValue" numeric(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE "PrivateExpirySettlements" ADD "RequestHash" character varying(64);

ALTER TABLE "PrivateExpirySettlements" ADD "TransferredPackage" boolean;

ALTER TABLE "PrivateCoachAssignments" ADD "OperationId" uuid;

ALTER TABLE "PrivateCoachAssignments" ADD "RequestHash" character varying(64);

ALTER TABLE "ClassSessions" ADD "BusinessDate" date;

ALTER TABLE "ClassSessions" ADD "CreatedByUserId" uuid;

ALTER TABLE "ClassSessions" ADD "OperationId" uuid;

ALTER TABLE "ClassSessions" ADD "RequestHash" character varying(64);

ALTER TABLE "ClassSessions" ADD "Version" integer NOT NULL DEFAULT 0;

ALTER TABLE "CheckIns" ADD "ClassesSubscriptionId" uuid;

ALTER TABLE "CheckIns" ADD "RequestHash" character varying(64);

CREATE TABLE "ClassesPackages" (
    "Id" uuid NOT NULL,
    "BranchId" uuid NOT NULL,
    "Name" character varying(256) NOT NULL,
    "SessionCount" integer NOT NULL,
    "DurationDays" integer NOT NULL,
    "Price" numeric(18,2) NOT NULL,
    "IsActive" boolean NOT NULL,
    CONSTRAINT "PK_ClassesPackages" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_ClassesPackages_Terms" CHECK (length(btrim("Name")) > 0 AND "SessionCount" > 0 AND "DurationDays" > 0 AND "Price" > 0),
    CONSTRAINT "FK_ClassesPackages_GymBranches_BranchId" FOREIGN KEY ("BranchId") REFERENCES "GymBranches" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "CoachPayouts" (
    "Id" uuid NOT NULL,
    "BranchId" uuid NOT NULL,
    "CoachEmployeeId" uuid NOT NULL,
    "ExpenseId" uuid NOT NULL,
    "OperationId" uuid NOT NULL,
    "RequestHash" character varying(64) NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    "BalanceBefore" numeric(18,2) NOT NULL,
    "BalanceAfter" numeric(18,2) NOT NULL,
    "AuthorizedByUserId" uuid NOT NULL,
    "PaidAt" timestamp with time zone NOT NULL,
    "BusinessDate" date NOT NULL,
    CONSTRAINT "PK_CoachPayouts" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_CoachPayouts_Balance" CHECK ("Amount" > 0 AND "BalanceBefore" >= "Amount" AND "BalanceAfter" = "BalanceBefore" - "Amount"),
    CONSTRAINT "FK_CoachPayouts_AspNetUsers_AuthorizedByUserId" FOREIGN KEY ("AuthorizedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CoachPayouts_Employees_CoachEmployeeId" FOREIGN KEY ("CoachEmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CoachPayouts_Expenses_ExpenseId" FOREIGN KEY ("ExpenseId") REFERENCES "Expenses" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CoachPayouts_GymBranches_BranchId" FOREIGN KEY ("BranchId") REFERENCES "GymBranches" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "CoachTargetRules" (
    "Id" uuid NOT NULL,
    "CommissionPeriodId" uuid NOT NULL,
    "CoachEmployeeId" uuid NOT NULL,
    "PrivateTarget" numeric(18,2),
    "ClassesAverageTarget" numeric(18,4),
    "OperationId" uuid NOT NULL,
    "RequestHash" character varying(64) NOT NULL,
    "CreatedByUserId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_CoachTargetRules" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_CoachTargetRules_Targets" CHECK (("PrivateTarget" IS NULL OR "PrivateTarget" > 0) AND ("ClassesAverageTarget" IS NULL OR "ClassesAverageTarget" > 0) AND ("PrivateTarget" IS NOT NULL OR "ClassesAverageTarget" IS NOT NULL)),
    CONSTRAINT "FK_CoachTargetRules_AspNetUsers_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CoachTargetRules_CommissionPeriods_CommissionPeriodId" FOREIGN KEY ("CommissionPeriodId") REFERENCES "CommissionPeriods" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CoachTargetRules_Employees_CoachEmployeeId" FOREIGN KEY ("CoachEmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "ClassesSubscriptions" (
    "Id" uuid NOT NULL,
    "MemberId" uuid NOT NULL,
    "BranchId" uuid NOT NULL,
    "ClassesPackageId" uuid NOT NULL,
    "SaleTransactionId" uuid NOT NULL,
    "PackageName" character varying(256) NOT NULL,
    "StartDate" date NOT NULL,
    "EndDate" date NOT NULL,
    "PurchasedSessions" integer NOT NULL,
    "RemainingSessions" integer NOT NULL,
    "Price" numeric(18,2) NOT NULL,
    "UnitSessionValue" numeric(18,2) NOT NULL,
    "RemainderValue" numeric(18,2) NOT NULL,
    "Version" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ClassesSubscriptions" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_ClassesSubscriptions_Balance" CHECK ("PurchasedSessions" > 0 AND "RemainingSessions" >= 0 AND "RemainingSessions" <= "PurchasedSessions" AND "EndDate" >= "StartDate"),
    CONSTRAINT "CK_ClassesSubscriptions_Value" CHECK ("Price" > 0 AND "UnitSessionValue" >= 0 AND "RemainderValue" >= 0 AND "RemainderValue" < "PurchasedSessions" * 0.01 AND "Price" = "PurchasedSessions" * "UnitSessionValue" + "RemainderValue"),
    CONSTRAINT "FK_ClassesSubscriptions_ClassesPackages_ClassesPackageId" FOREIGN KEY ("ClassesPackageId") REFERENCES "ClassesPackages" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ClassesSubscriptions_GymBranches_BranchId" FOREIGN KEY ("BranchId") REFERENCES "GymBranches" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ClassesSubscriptions_Members_MemberId" FOREIGN KEY ("MemberId") REFERENCES "Members" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ClassesSubscriptions_SaleTransactions_SaleTransactionId" FOREIGN KEY ("SaleTransactionId") REFERENCES "SaleTransactions" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "CoachEarnings" (
    "Id" uuid NOT NULL,
    "BranchId" uuid NOT NULL,
    "CoachEmployeeId" uuid NOT NULL,
    "CommissionPeriodId" uuid NOT NULL,
    "CoachTargetRuleId" uuid,
    "SaleTransactionId" uuid NOT NULL,
    "PrivateSessionId" uuid,
    "PrivateExpirySettlementId" uuid,
    "RecognitionOperationId" uuid NOT NULL,
    "RequestHash" character varying(64) NOT NULL,
    "EligibleValue" numeric(18,2) NOT NULL,
    "Rate" numeric(5,2) NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    "TransferredPackage" boolean NOT NULL,
    "TargetProgress" numeric(18,2) NOT NULL,
    "Target" numeric(18,2),
    "BusinessDate" date NOT NULL,
    "CreatedByUserId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_CoachEarnings" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_CoachEarnings_Amounts" CHECK ("EligibleValue" > 0 AND "Amount" >= 0 AND "Amount" <= "EligibleValue" AND "TargetProgress" >= 0),
    CONSTRAINT "CK_CoachEarnings_Rate" CHECK (("TransferredPackage" AND "Rate" = 50) OR (NOT "TransferredPackage" AND "CoachTargetRuleId" IS NOT NULL AND "Target" > 0 AND (("TargetProgress" >= "Target" AND "Rate" = 60) OR ("TargetProgress" < "Target" AND "Rate" = 50)))),
    CONSTRAINT "CK_CoachEarnings_Source" CHECK (("PrivateSessionId" IS NOT NULL) <> ("PrivateExpirySettlementId" IS NOT NULL)),
    CONSTRAINT "FK_CoachEarnings_AspNetUsers_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CoachEarnings_CoachTargetRules_CoachTargetRuleId" FOREIGN KEY ("CoachTargetRuleId") REFERENCES "CoachTargetRules" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CoachEarnings_CommissionPeriods_CommissionPeriodId" FOREIGN KEY ("CommissionPeriodId") REFERENCES "CommissionPeriods" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CoachEarnings_Employees_CoachEmployeeId" FOREIGN KEY ("CoachEmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CoachEarnings_GymBranches_BranchId" FOREIGN KEY ("BranchId") REFERENCES "GymBranches" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CoachEarnings_PrivateExpirySettlements_PrivateExpirySettlem~" FOREIGN KEY ("PrivateExpirySettlementId") REFERENCES "PrivateExpirySettlements" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CoachEarnings_PrivateSessions_PrivateSessionId" FOREIGN KEY ("PrivateSessionId") REFERENCES "PrivateSessions" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CoachEarnings_SaleTransactions_SaleTransactionId" FOREIGN KEY ("SaleTransactionId") REFERENCES "SaleTransactions" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "CoachPayoutAllocations" (
    "CoachPayoutId" uuid NOT NULL,
    "CoachEarningId" uuid NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    CONSTRAINT "PK_CoachPayoutAllocations" PRIMARY KEY ("CoachPayoutId", "CoachEarningId"),
    CONSTRAINT "CK_CoachPayoutAllocations_Amount" CHECK ("Amount" > 0),
    CONSTRAINT "FK_CoachPayoutAllocations_CoachEarnings_CoachEarningId" FOREIGN KEY ("CoachEarningId") REFERENCES "CoachEarnings" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CoachPayoutAllocations_CoachPayouts_CoachPayoutId" FOREIGN KEY ("CoachPayoutId") REFERENCES "CoachPayouts" ("Id") ON DELETE RESTRICT
);

ALTER TABLE "SaleTransactions" ADD CONSTRAINT "CK_SaleTransactions_SaleType" CHECK ("SaleType" IN ('GymMembership', 'Private', 'Activity', 'Other', 'Classes'));

CREATE INDEX "IX_PrivateSubscriptionTransactions_OriginalAssignmentId" ON "PrivateSubscriptionTransactions" ("OriginalAssignmentId");

CREATE INDEX "IX_PrivateSubscriptionTransactions_OriginalCoachEmployeeId" ON "PrivateSubscriptionTransactions" ("OriginalCoachEmployeeId");

CREATE UNIQUE INDEX "IX_PrivateSubscriptionTransactions_SaleTransactionId" ON "PrivateSubscriptionTransactions" ("SaleTransactionId");

ALTER TABLE "PrivateSubscriptionTransactions" ADD CONSTRAINT "CK_PrivateSubscriptionTransactions_Snapshot" CHECK ("OriginalCoachEmployeeId" IS NULL OR ("OriginalAssignmentId" IS NOT NULL AND "StartDateSnapshot" IS NOT NULL AND "PackageNameSnapshot" IS NOT NULL AND "NetAmount" = "SessionsAdded" * "UnitSessionValue" + "RemainderValue" AND "RemainderValue" >= 0 AND "RemainderValue" < "SessionsAdded" * 0.01));

CREATE UNIQUE INDEX "IX_PrivateSessions_GymCheckInId" ON "PrivateSessions" ("GymCheckInId") WHERE "OperationId" IS NOT NULL;

CREATE UNIQUE INDEX "IX_PrivateSessions_OperationId" ON "PrivateSessions" ("OperationId");

CREATE INDEX "IX_PrivateSessions_PrivateCoachAssignmentId" ON "PrivateSessions" ("PrivateCoachAssignmentId");

CREATE UNIQUE INDEX "IX_PrivateSessions_PrivateSubscriptionTransactionId_SessionNum~" ON "PrivateSessions" ("PrivateSubscriptionTransactionId", "SessionNumber") WHERE "OperationId" IS NOT NULL;

ALTER TABLE "PrivateSessions" ADD CONSTRAINT "CK_PrivateSessions_Operational" CHECK ("OperationId" IS NULL OR ("BusinessDate" IS NOT NULL AND "RequestHash" IS NOT NULL AND "PrivateCoachAssignmentId" IS NOT NULL AND "SessionValue" >= 0 AND "SessionNumber" > 0 AND "TransferredPackage" IS NOT NULL));

CREATE UNIQUE INDEX "IX_PrivateExpirySettlements_OperationId" ON "PrivateExpirySettlements" ("OperationId");

CREATE INDEX "IX_PrivateExpirySettlements_PrivateCoachAssignmentId" ON "PrivateExpirySettlements" ("PrivateCoachAssignmentId");

ALTER TABLE "PrivateExpirySettlements" ADD CONSTRAINT "CK_PrivateExpirySettlements_EligibleAmount" CHECK ("EligibleAmount" = "RemainingSessions" * "UnitSessionValue" + "RemainderValue");

CREATE UNIQUE INDEX "IX_PrivateCoachAssignments_OperationId" ON "PrivateCoachAssignments" ("OperationId");

CREATE INDEX "IX_ClassSessions_CreatedByUserId" ON "ClassSessions" ("CreatedByUserId");

CREATE UNIQUE INDEX "IX_ClassSessions_OperationId" ON "ClassSessions" ("OperationId");

CREATE INDEX "IX_CheckIns_ClassesSubscriptionId" ON "CheckIns" ("ClassesSubscriptionId");

ALTER TABLE "CheckIns" ADD CONSTRAINT "CK_CheckIns_ClassesSubscription" CHECK ("ClassesSubscriptionId" IS NULL OR ("CheckInType" = 'Class' AND "ClassSessionId" IS NOT NULL AND "SessionConsumed"));

ALTER TABLE "CheckIns" ADD CONSTRAINT "CK_CheckIns_SessionSnapshot" CHECK (("SessionsBefore" IS NULL AND "SessionsAfter" IS NULL) OR ("SessionsBefore" IS NOT NULL AND "SessionsAfter" IS NOT NULL AND "SessionsBefore" > 0 AND "SessionsAfter" = "SessionsBefore" - 1 AND "CheckInType" IN ('Gym', 'Class') AND "SessionConsumed"));

CREATE UNIQUE INDEX "IX_ClassesPackages_BranchId_Name" ON "ClassesPackages" ("BranchId", "Name");

CREATE INDEX "IX_ClassesSubscriptions_BranchId_MemberId_EndDate" ON "ClassesSubscriptions" ("BranchId", "MemberId", "EndDate");

CREATE INDEX "IX_ClassesSubscriptions_ClassesPackageId" ON "ClassesSubscriptions" ("ClassesPackageId");

CREATE INDEX "IX_ClassesSubscriptions_MemberId" ON "ClassesSubscriptions" ("MemberId");

CREATE UNIQUE INDEX "IX_ClassesSubscriptions_SaleTransactionId" ON "ClassesSubscriptions" ("SaleTransactionId");

CREATE INDEX "IX_CoachEarnings_BranchId_CoachEmployeeId_BusinessDate" ON "CoachEarnings" ("BranchId", "CoachEmployeeId", "BusinessDate");

CREATE INDEX "IX_CoachEarnings_CoachEmployeeId" ON "CoachEarnings" ("CoachEmployeeId");

CREATE INDEX "IX_CoachEarnings_CoachTargetRuleId" ON "CoachEarnings" ("CoachTargetRuleId");

CREATE UNIQUE INDEX "IX_CoachEarnings_CommissionPeriodId_PrivateExpirySettlementId" ON "CoachEarnings" ("CommissionPeriodId", "PrivateExpirySettlementId") WHERE "PrivateExpirySettlementId" IS NOT NULL;

CREATE UNIQUE INDEX "IX_CoachEarnings_CommissionPeriodId_PrivateSessionId" ON "CoachEarnings" ("CommissionPeriodId", "PrivateSessionId") WHERE "PrivateSessionId" IS NOT NULL;

CREATE INDEX "IX_CoachEarnings_CreatedByUserId" ON "CoachEarnings" ("CreatedByUserId");

CREATE INDEX "IX_CoachEarnings_PrivateExpirySettlementId" ON "CoachEarnings" ("PrivateExpirySettlementId");

CREATE INDEX "IX_CoachEarnings_PrivateSessionId" ON "CoachEarnings" ("PrivateSessionId");

CREATE INDEX "IX_CoachEarnings_RecognitionOperationId" ON "CoachEarnings" ("RecognitionOperationId");

CREATE INDEX "IX_CoachEarnings_SaleTransactionId" ON "CoachEarnings" ("SaleTransactionId");

CREATE INDEX "IX_CoachPayoutAllocations_CoachEarningId" ON "CoachPayoutAllocations" ("CoachEarningId");

CREATE INDEX "IX_CoachPayouts_AuthorizedByUserId" ON "CoachPayouts" ("AuthorizedByUserId");

CREATE INDEX "IX_CoachPayouts_BranchId_CoachEmployeeId_PaidAt" ON "CoachPayouts" ("BranchId", "CoachEmployeeId", "PaidAt");

CREATE INDEX "IX_CoachPayouts_CoachEmployeeId" ON "CoachPayouts" ("CoachEmployeeId");

CREATE UNIQUE INDEX "IX_CoachPayouts_ExpenseId" ON "CoachPayouts" ("ExpenseId");

CREATE UNIQUE INDEX "IX_CoachPayouts_OperationId" ON "CoachPayouts" ("OperationId");

CREATE INDEX "IX_CoachTargetRules_CoachEmployeeId" ON "CoachTargetRules" ("CoachEmployeeId");

CREATE UNIQUE INDEX "IX_CoachTargetRules_CommissionPeriodId_CoachEmployeeId" ON "CoachTargetRules" ("CommissionPeriodId", "CoachEmployeeId");

CREATE INDEX "IX_CoachTargetRules_CreatedByUserId" ON "CoachTargetRules" ("CreatedByUserId");

CREATE UNIQUE INDEX "IX_CoachTargetRules_OperationId" ON "CoachTargetRules" ("OperationId");

ALTER TABLE "CheckIns" ADD CONSTRAINT "FK_CheckIns_ClassesSubscriptions_ClassesSubscriptionId" FOREIGN KEY ("ClassesSubscriptionId") REFERENCES "ClassesSubscriptions" ("Id") ON DELETE RESTRICT;

ALTER TABLE "ClassSessions" ADD CONSTRAINT "FK_ClassSessions_AspNetUsers_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT;

ALTER TABLE "PrivateExpirySettlements" ADD CONSTRAINT "FK_PrivateExpirySettlements_PrivateCoachAssignments_PrivateCoa~" FOREIGN KEY ("PrivateCoachAssignmentId") REFERENCES "PrivateCoachAssignments" ("Id") ON DELETE RESTRICT;

ALTER TABLE "PrivateSessions" ADD CONSTRAINT "FK_PrivateSessions_PrivateCoachAssignments_PrivateCoachAssignm~" FOREIGN KEY ("PrivateCoachAssignmentId") REFERENCES "PrivateCoachAssignments" ("Id") ON DELETE RESTRICT;

ALTER TABLE "PrivateSubscriptionTransactions" ADD CONSTRAINT "FK_PrivateSubscriptionTransactions_Employees_OriginalCoachEmpl~" FOREIGN KEY ("OriginalCoachEmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT;

ALTER TABLE "PrivateSubscriptionTransactions" ADD CONSTRAINT "FK_PrivateSubscriptionTransactions_PrivateCoachAssignments_Ori~" FOREIGN KEY ("OriginalAssignmentId") REFERENCES "PrivateCoachAssignments" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261012101654_ImplementPrivateClassesAndCoachEarnings', '10.0.8');

COMMIT;

