START TRANSACTION;
DROP INDEX "IX_SubscriptionFreezes_MemberSubscriptionId";

DROP INDEX "IX_MembershipSubscriptionTransactions_MemberSubscriptionId";

ALTER TABLE "MembershipSubscriptionTransactions" DROP CONSTRAINT "CK_MembershipSubscriptionTransactions_PlanCategoryCoachSnapshot";

ALTER TABLE "MembershipSubscriptionTransactions" DROP CONSTRAINT "CK_MembershipSubscriptionTransactions_PurchasedDurationDays";

ALTER TABLE "SubscriptionFreezes" ADD "CompletedAt" timestamp with time zone;

ALTER TABLE "SubscriptionFreezes" ADD "CompletedByUserId" uuid;

ALTER TABLE "SubscriptionFreezes" ADD "NewSubscriptionEndDate" date;

ALTER TABLE "SubscriptionFreezes" ADD "OperationId" uuid;

ALTER TABLE "SubscriptionFreezes" ADD "PreviousSubscriptionEndDate" date;

ALTER TABLE "MemberSubscriptions" ADD "FollowsSubscriptionId" uuid;

ALTER TABLE "MemberSubscriptions" ADD "Version" integer NOT NULL DEFAULT 0;

ALTER TABLE "MembershipSubscriptionTransactions" ALTER COLUMN "SaleTransactionId" DROP NOT NULL;

ALTER TABLE "MembershipSubscriptionTransactions" ALTER COLUMN "NewEndDate" DROP NOT NULL;

ALTER TABLE "MembershipSubscriptionTransactions" ADD "AccessModeSnapshot" character varying(40);

ALTER TABLE "MembershipSubscriptionTransactions" ADD "BusinessDayId" uuid;

ALTER TABLE "MembershipSubscriptionTransactions" ADD "CityIdSnapshot" uuid;

ALTER TABLE "MembershipSubscriptionTransactions" ADD "CreatedByUserId" uuid;

ALTER TABLE "MembershipSubscriptionTransactions" ADD "FreezeDaysSnapshot" integer;

ALTER TABLE "MembershipSubscriptionTransactions" ADD "OperationId" uuid;

ALTER TABLE "MembershipSubscriptionTransactions" ADD "PlanBranchIdSnapshot" uuid;

ALTER TABLE "MembershipSubscriptionTransactions" ADD "PlanNameSnapshot" character varying(256);

ALTER TABLE "MembershipSubscriptionTransactions" ADD "PreviousRemainingSessions" integer;

ALTER TABLE "MembershipSubscriptionTransactions" ADD "RenewedFromSubscriptionId" uuid;

ALTER TABLE "MembershipSubscriptionTransactions" ADD "StartDateSnapshot" date;

CREATE TABLE "SubscriptionHolidayExtensions" (
    "Id" uuid NOT NULL,
    "MemberSubscriptionId" uuid NOT NULL,
    "BranchClosureId" uuid NOT NULL,
    "ProtectedDate" date NOT NULL,
    "DaysApplied" integer NOT NULL,
    "PreviousEndDate" date NOT NULL,
    "NewEndDate" date NOT NULL,
    "CreatedByUserId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_SubscriptionHolidayExtensions" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_SubscriptionHolidayExtensions_Days" CHECK ("DaysApplied" IN (0, 1) AND "NewEndDate" = "PreviousEndDate" + "DaysApplied"),
    CONSTRAINT "FK_SubscriptionHolidayExtensions_AspNetUsers_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_SubscriptionHolidayExtensions_BranchClosures_BranchClosureId" FOREIGN KEY ("BranchClosureId") REFERENCES "BranchClosures" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_SubscriptionHolidayExtensions_MemberSubscriptions_MemberSub~" FOREIGN KEY ("MemberSubscriptionId") REFERENCES "MemberSubscriptions" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_SubscriptionFreezes_CompletedByUserId" ON "SubscriptionFreezes" ("CompletedByUserId");

CREATE INDEX "IX_SubscriptionFreezes_MemberSubscriptionId_StartDate" ON "SubscriptionFreezes" ("MemberSubscriptionId", "StartDate");

CREATE UNIQUE INDEX "IX_SubscriptionFreezes_OperationId" ON "SubscriptionFreezes" ("OperationId");

CREATE UNIQUE INDEX "IX_MemberSubscriptions_FollowsSubscriptionId" ON "MemberSubscriptions" ("FollowsSubscriptionId");

CREATE INDEX "IX_MembershipSubscriptionTransactions_BusinessDayId" ON "MembershipSubscriptionTransactions" ("BusinessDayId");

CREATE INDEX "IX_MembershipSubscriptionTransactions_CityIdSnapshot" ON "MembershipSubscriptionTransactions" ("CityIdSnapshot");

CREATE INDEX "IX_MembershipSubscriptionTransactions_CreatedByUserId" ON "MembershipSubscriptionTransactions" ("CreatedByUserId");

CREATE INDEX "IX_MembershipSubscriptionTransactions_MemberSubscriptionId_Cre~" ON "MembershipSubscriptionTransactions" ("MemberSubscriptionId", "CreatedAt");

CREATE UNIQUE INDEX "IX_MembershipSubscriptionTransactions_OperationId" ON "MembershipSubscriptionTransactions" ("OperationId");

CREATE INDEX "IX_MembershipSubscriptionTransactions_PlanBranchIdSnapshot" ON "MembershipSubscriptionTransactions" ("PlanBranchIdSnapshot");

CREATE INDEX "IX_MembershipSubscriptionTransactions_RenewedFromSubscriptionId" ON "MembershipSubscriptionTransactions" ("RenewedFromSubscriptionId");

ALTER TABLE "MembershipSubscriptionTransactions" ADD CONSTRAINT "CK_MembershipSubscriptionTransactions_FreezeSnapshot" CHECK ("FreezeDaysSnapshot" >= 0);

ALTER TABLE "MembershipSubscriptionTransactions" ADD CONSTRAINT "CK_MembershipSubscriptionTransactions_PlanCategoryCoachSnapshot" CHECK (("PlanCategorySnapshot" IN ('Normal', 'Employee') AND "FreelanceCoachEmployeeIdSnapshot" IS NULL) OR ("PlanCategorySnapshot" = 'Freelance' AND "FreelanceCoachEmployeeIdSnapshot" IS NOT NULL));

ALTER TABLE "MembershipSubscriptionTransactions" ADD CONSTRAINT "CK_MembershipSubscriptionTransactions_PurchasedDurationDays" CHECK (("PlanCategorySnapshot" = 'Employee' AND "PurchasedDurationDays" = 0 AND "NewEndDate" IS NULL AND "SaleTransactionId" IS NULL AND "PlanPriceSnapshot" = 0 AND "NetPriceSnapshot" = 0 AND "PurchasedSessions" IS NULL) OR ("PlanCategorySnapshot" <> 'Employee' AND "PurchasedDurationDays" > 0 AND "NewEndDate" IS NOT NULL));

CREATE INDEX "IX_SubscriptionHolidayExtensions_BranchClosureId" ON "SubscriptionHolidayExtensions" ("BranchClosureId");

CREATE INDEX "IX_SubscriptionHolidayExtensions_CreatedByUserId" ON "SubscriptionHolidayExtensions" ("CreatedByUserId");

CREATE UNIQUE INDEX "IX_SubscriptionHolidayExtensions_MemberSubscriptionId_Protecte~" ON "SubscriptionHolidayExtensions" ("MemberSubscriptionId", "ProtectedDate");

ALTER TABLE "MembershipSubscriptionTransactions" ADD CONSTRAINT "FK_MembershipSubscriptionTransactions_AspNetUsers_CreatedByUse~" FOREIGN KEY ("CreatedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT;

ALTER TABLE "MembershipSubscriptionTransactions" ADD CONSTRAINT "FK_MembershipSubscriptionTransactions_BusinessDays_BusinessDay~" FOREIGN KEY ("BusinessDayId") REFERENCES "BusinessDays" ("Id") ON DELETE RESTRICT;

ALTER TABLE "MembershipSubscriptionTransactions" ADD CONSTRAINT "FK_MembershipSubscriptionTransactions_Cities_CityIdSnapshot" FOREIGN KEY ("CityIdSnapshot") REFERENCES "Cities" ("Id") ON DELETE RESTRICT;

ALTER TABLE "MembershipSubscriptionTransactions" ADD CONSTRAINT "FK_MembershipSubscriptionTransactions_GymBranches_PlanBranchId~" FOREIGN KEY ("PlanBranchIdSnapshot") REFERENCES "GymBranches" ("Id") ON DELETE RESTRICT;

ALTER TABLE "MembershipSubscriptionTransactions" ADD CONSTRAINT "FK_MembershipSubscriptionTransactions_MemberSubscriptions_Rene~" FOREIGN KEY ("RenewedFromSubscriptionId") REFERENCES "MemberSubscriptions" ("Id") ON DELETE RESTRICT;

ALTER TABLE "MemberSubscriptions" ADD CONSTRAINT "FK_MemberSubscriptions_MemberSubscriptions_FollowsSubscription~" FOREIGN KEY ("FollowsSubscriptionId") REFERENCES "MemberSubscriptions" ("Id") ON DELETE RESTRICT;

ALTER TABLE "SubscriptionFreezes" ADD CONSTRAINT "FK_SubscriptionFreezes_AspNetUsers_CompletedByUserId" FOREIGN KEY ("CompletedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261009152545_PrepareSubscriptionLifecycle', '10.0.8');

COMMIT;

