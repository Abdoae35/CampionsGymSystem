START TRANSACTION;
DO $rules$
BEGIN
    IF EXISTS (SELECT 1 FROM "PrivateSessions") OR EXISTS (
        SELECT 1 FROM "PrivateSubscriptions" s
        WHERE s."RemainingSessions" <> COALESCE((
            SELECT SUM(t."SessionsAdded") FROM "PrivateSubscriptionTransactions" t
            WHERE t."PrivateSubscriptionId" = s."Id"), 0)
           OR s."TotalSessions" <> COALESCE((
            SELECT SUM(t."SessionsAdded") FROM "PrivateSubscriptionTransactions" t
            WHERE t."PrivateSubscriptionId" = s."Id"), 0)
    ) THEN
        RAISE EXCEPTION 'Private history requires explicit batch/period/counter reconciliation before UpdatePrivateAndFreelanceBusinessRules. No history was changed.';
    END IF;
END $rules$;

DROP INDEX "IX_PrivateSubscriptionTransactions_PrivateSubscriptionId";

DROP INDEX "IX_PrivateSessions_CoachEmployeeId";

ALTER TABLE "PrivateSubscriptionTransactions" ADD "RemainingSessions" integer NOT NULL DEFAULT 0;

ALTER TABLE "PrivateSessions" ADD "CommissionPeriodId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE "PrivateSessions" ADD "PrivateSubscriptionTransactionId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE "PrivateCoachAssignments" ALTER COLUMN "SessionsAllocated" SET DEFAULT 0;

ALTER TABLE "PrivateCoachAssignments" ALTER COLUMN "CommissionBasisAmount" SET DEFAULT 0.0;

ALTER TABLE "MembershipSubscriptionTransactions" ADD "FreelanceCoachEmployeeIdSnapshot" uuid;

ALTER TABLE "MembershipSubscriptionTransactions" ADD "PlanCategorySnapshot" character varying(40) NOT NULL DEFAULT 'Normal';

ALTER TABLE "MembershipPlans" ADD "FreelanceCoachEmployeeId" uuid;

ALTER TABLE "MembershipPlans" ADD "PlanCategory" character varying(40) NOT NULL DEFAULT 'Normal';

UPDATE "PrivateSubscriptionTransactions" SET "RemainingSessions" = "SessionsAdded";
ALTER TABLE "PrivateSubscriptionTransactions" ALTER COLUMN "RemainingSessions" DROP DEFAULT;
ALTER TABLE "PrivateSessions" ALTER COLUMN "CommissionPeriodId" DROP DEFAULT;
ALTER TABLE "PrivateSessions" ALTER COLUMN "PrivateSubscriptionTransactionId" DROP DEFAULT;
ALTER TABLE "MembershipPlans" ALTER COLUMN "PlanCategory" DROP DEFAULT;
ALTER TABLE "MembershipSubscriptionTransactions" ALTER COLUMN "PlanCategorySnapshot" DROP DEFAULT;

CREATE TABLE "PrivateExpirySettlements" (
    "Id" uuid NOT NULL,
    "PrivateSubscriptionId" uuid NOT NULL,
    "PrivateSubscriptionTransactionId" uuid NOT NULL,
    "CoachEmployeeId" uuid NOT NULL,
    "CommissionPeriodId" uuid NOT NULL,
    "RemainingSessions" integer NOT NULL,
    "UnitSessionValue" numeric(18,2) NOT NULL,
    "EligibleAmount" numeric(18,2) NOT NULL,
    "SettledAt" timestamp with time zone NOT NULL,
    "CreatedByUserId" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_PrivateExpirySettlements" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_PrivateExpirySettlements_EligibleAmount" CHECK ("EligibleAmount" = "RemainingSessions" * "UnitSessionValue"),
    CONSTRAINT "CK_PrivateExpirySettlements_RemainingSessions" CHECK ("RemainingSessions" > 0),
    CONSTRAINT "CK_PrivateExpirySettlements_UnitSessionValue" CHECK ("UnitSessionValue" >= 0),
    CONSTRAINT "FK_PrivateExpirySettlements_AspNetUsers_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_PrivateExpirySettlements_CommissionPeriods_CommissionPeriod~" FOREIGN KEY ("CommissionPeriodId") REFERENCES "CommissionPeriods" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_PrivateExpirySettlements_Employees_CoachEmployeeId" FOREIGN KEY ("CoachEmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_PrivateExpirySettlements_PrivateSubscriptionTransactions_Pr~" FOREIGN KEY ("PrivateSubscriptionTransactionId") REFERENCES "PrivateSubscriptionTransactions" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_PrivateExpirySettlements_PrivateSubscriptions_PrivateSubscr~" FOREIGN KEY ("PrivateSubscriptionId") REFERENCES "PrivateSubscriptions" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_PrivateSubscriptionTransactions_PrivateSubscriptionId_Creat~" ON "PrivateSubscriptionTransactions" ("PrivateSubscriptionId", "CreatedAt", "Id");

ALTER TABLE "PrivateSubscriptionTransactions" ADD CONSTRAINT "CK_PrivateSubscriptionTransactions_RemainingSessions" CHECK ("RemainingSessions" >= 0 AND "RemainingSessions" <= "SessionsAdded");

CREATE INDEX "IX_PrivateSessions_CoachEmployeeId_SessionAt" ON "PrivateSessions" ("CoachEmployeeId", "SessionAt");

CREATE INDEX "IX_PrivateSessions_CommissionPeriodId" ON "PrivateSessions" ("CommissionPeriodId");

CREATE INDEX "IX_PrivateSessions_PrivateSubscriptionTransactionId" ON "PrivateSessions" ("PrivateSubscriptionTransactionId");

CREATE INDEX "IX_PrivateCoachAssignments_PrivateSubscriptionId_EndedAt" ON "PrivateCoachAssignments" ("PrivateSubscriptionId", "EndedAt");

CREATE INDEX "IX_MembershipSubscriptionTransactions_FreelanceCoachEmployeeId~" ON "MembershipSubscriptionTransactions" ("FreelanceCoachEmployeeIdSnapshot");

ALTER TABLE "MembershipSubscriptionTransactions" ADD CONSTRAINT "CK_MembershipSubscriptionTransactions_PlanCategoryCoachSnapshot" CHECK (("PlanCategorySnapshot" = 'Normal' AND "FreelanceCoachEmployeeIdSnapshot" IS NULL) OR ("PlanCategorySnapshot" = 'Freelance' AND "FreelanceCoachEmployeeIdSnapshot" IS NOT NULL));

CREATE INDEX "IX_MembershipPlans_FreelanceCoachEmployeeId" ON "MembershipPlans" ("FreelanceCoachEmployeeId");

CREATE INDEX "IX_MembershipPlans_PlanCategory_FreelanceCoachEmployeeId_IsAct~" ON "MembershipPlans" ("PlanCategory", "FreelanceCoachEmployeeId", "IsActive");

ALTER TABLE "MembershipPlans" ADD CONSTRAINT "CK_MembershipPlans_PlanCategoryCoach" CHECK (("PlanCategory" = 'Normal' AND "FreelanceCoachEmployeeId" IS NULL) OR ("PlanCategory" = 'Freelance' AND "FreelanceCoachEmployeeId" IS NOT NULL));

CREATE INDEX "IX_PrivateExpirySettlements_CoachEmployeeId" ON "PrivateExpirySettlements" ("CoachEmployeeId");

CREATE INDEX "IX_PrivateExpirySettlements_CommissionPeriodId" ON "PrivateExpirySettlements" ("CommissionPeriodId");

CREATE INDEX "IX_PrivateExpirySettlements_CreatedByUserId" ON "PrivateExpirySettlements" ("CreatedByUserId");

CREATE INDEX "IX_PrivateExpirySettlements_PrivateSubscriptionId" ON "PrivateExpirySettlements" ("PrivateSubscriptionId");

CREATE UNIQUE INDEX "IX_PrivateExpirySettlements_PrivateSubscriptionTransactionId" ON "PrivateExpirySettlements" ("PrivateSubscriptionTransactionId");

ALTER TABLE "MembershipPlans" ADD CONSTRAINT "FK_MembershipPlans_Employees_FreelanceCoachEmployeeId" FOREIGN KEY ("FreelanceCoachEmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT;

ALTER TABLE "MembershipSubscriptionTransactions" ADD CONSTRAINT "FK_MembershipSubscriptionTransactions_Employees_FreelanceCoach~" FOREIGN KEY ("FreelanceCoachEmployeeIdSnapshot") REFERENCES "Employees" ("Id") ON DELETE RESTRICT;

ALTER TABLE "PrivateSessions" ADD CONSTRAINT "FK_PrivateSessions_CommissionPeriods_CommissionPeriodId" FOREIGN KEY ("CommissionPeriodId") REFERENCES "CommissionPeriods" ("Id") ON DELETE RESTRICT;

ALTER TABLE "PrivateSessions" ADD CONSTRAINT "FK_PrivateSessions_PrivateSubscriptionTransactions_PrivateSubs~" FOREIGN KEY ("PrivateSubscriptionTransactionId") REFERENCES "PrivateSubscriptionTransactions" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261006014644_UpdatePrivateAndFreelanceBusinessRules', '10.0.8');

COMMIT;

