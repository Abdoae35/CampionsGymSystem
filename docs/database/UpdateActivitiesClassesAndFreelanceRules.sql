START TRANSACTION;
DROP INDEX "IX_ActivityCoachAssignments_SportsActivityId";

ALTER TABLE "SaleTransactions" ADD "SalesAttributionSnapshot" character varying(40);

ALTER TABLE "CheckIns" ADD "ClassSessionId" uuid;

ALTER TABLE "ActivitySubscriptionTransactions" ADD "ActivityCoachIdSnapshot" uuid;

ALTER TABLE "ActivitySubscriptionTransactions" ADD "CoachPercentageSnapshot" numeric(5,2);

ALTER TABLE "ActivitySubscriptions" ADD "ActivityCoachId" uuid;

ALTER TABLE "ActivityCoachAssignments" ALTER COLUMN "CoachEmployeeId" DROP NOT NULL;

ALTER TABLE "ActivityCoachAssignments" ADD "ActivityCoachId" uuid;

CREATE TABLE "ActivityCoaches" (
    "Id" uuid NOT NULL,
    "CoachType" character varying(40) NOT NULL,
    "EmployeeId" uuid,
    "ExternalName" character varying(256),
    "ExternalPhone" character varying(64),
    "Percentage" numeric(5,2) NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ActivityCoaches" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_ActivityCoaches_Percentage" CHECK ("Percentage" BETWEEN 0 AND 100),
    CONSTRAINT "CK_ActivityCoaches_Type" CHECK (("CoachType" = 'Internal' AND "EmployeeId" IS NOT NULL AND "ExternalName" IS NULL AND "ExternalPhone" IS NULL) OR ("CoachType" = 'External' AND "EmployeeId" IS NULL AND length(trim("ExternalName")) > 0 AND "ExternalName" IS NOT NULL)),
    CONSTRAINT "FK_ActivityCoaches_Employees_EmployeeId" FOREIGN KEY ("EmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "ClassSessions" (
    "Id" uuid NOT NULL,
    "GroupClassId" uuid NOT NULL,
    "ClassScheduleId" uuid,
    "BranchId" uuid NOT NULL,
    "CoachEmployeeId" uuid NOT NULL,
    "StartedAt" timestamp with time zone NOT NULL,
    "EndedAt" timestamp with time zone,
    "Status" character varying(40) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ClassSessions" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_ClassSessions_Status" CHECK ("Status" IN ('Scheduled', 'Completed', 'Cancelled')),
    CONSTRAINT "CK_ClassSessions_Times" CHECK (("EndedAt" IS NULL OR "EndedAt" >= "StartedAt") AND ("Status" <> 'Completed' OR "EndedAt" IS NOT NULL)),
    CONSTRAINT "FK_ClassSessions_ClassSchedules_ClassScheduleId" FOREIGN KEY ("ClassScheduleId") REFERENCES "ClassSchedules" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ClassSessions_Employees_CoachEmployeeId" FOREIGN KEY ("CoachEmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ClassSessions_GroupClasses_GroupClassId" FOREIGN KEY ("GroupClassId") REFERENCES "GroupClasses" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ClassSessions_GymBranches_BranchId" FOREIGN KEY ("BranchId") REFERENCES "GymBranches" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "MembershipPlanEligibleMembers" (
    "Id" uuid NOT NULL,
    "MembershipPlanId" uuid NOT NULL,
    "MemberId" uuid NOT NULL,
    "CreatedByUserId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_MembershipPlanEligibleMembers" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_MembershipPlanEligibleMembers_AspNetUsers_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_MembershipPlanEligibleMembers_Members_MemberId" FOREIGN KEY ("MemberId") REFERENCES "Members" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_MembershipPlanEligibleMembers_MembershipPlans_MembershipPla~" FOREIGN KEY ("MembershipPlanId") REFERENCES "MembershipPlans" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "ActivityCoachSettlements" (
    "Id" uuid NOT NULL,
    "CommissionPeriodId" uuid NOT NULL,
    "ActivityCoachId" uuid NOT NULL,
    "SportsActivityId" uuid NOT NULL,
    "EmployeeId" uuid,
    "CoachTypeSnapshot" character varying(40) NOT NULL,
    "CoachNameSnapshot" character varying(256) NOT NULL,
    "ActivityNameSnapshot" character varying(256) NOT NULL,
    "SubscribersCount" integer NOT NULL,
    "EligibleRevenue" numeric(18,2) NOT NULL,
    "Percentage" numeric(5,2) NOT NULL,
    "Entitlement" numeric(18,2) NOT NULL,
    "FinalizedAt" timestamp with time zone NOT NULL,
    "CreatedByUserId" uuid NOT NULL,
    CONSTRAINT "PK_ActivityCoachSettlements" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_ActivityCoachSettlements_Amounts" CHECK ("EligibleRevenue" >= 0 AND "Entitlement" >= 0 AND "SubscribersCount" >= 0 AND "Percentage" BETWEEN 0 AND 100),
    CONSTRAINT "CK_ActivityCoachSettlements_Employee" CHECK (("CoachTypeSnapshot" = 'Internal' AND "EmployeeId" IS NOT NULL) OR ("CoachTypeSnapshot" = 'External' AND "EmployeeId" IS NULL)),
    CONSTRAINT "FK_ActivityCoachSettlements_ActivityCoaches_ActivityCoachId" FOREIGN KEY ("ActivityCoachId") REFERENCES "ActivityCoaches" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ActivityCoachSettlements_AspNetUsers_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ActivityCoachSettlements_CommissionPeriods_CommissionPeriod~" FOREIGN KEY ("CommissionPeriodId") REFERENCES "CommissionPeriods" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ActivityCoachSettlements_Employees_EmployeeId" FOREIGN KEY ("EmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ActivityCoachSettlements_SportsActivities_SportsActivityId" FOREIGN KEY ("SportsActivityId") REFERENCES "SportsActivities" ("Id") ON DELETE RESTRICT
);

ALTER TABLE "SaleTransactions" ADD CONSTRAINT "CK_SaleTransactions_Attribution" CHECK ("SalesAttributionSnapshot" IS NULL OR ("SalesAttributionSnapshot" = 'House' AND "SalesEmployeeId" IS NULL AND "SaleCategory" IS NULL) OR ("SalesAttributionSnapshot" = 'Sales' AND "SalesEmployeeId" IS NOT NULL));

CREATE UNIQUE INDEX "IX_CheckIns_ClassSessionId_MemberId" ON "CheckIns" ("ClassSessionId", "MemberId") WHERE "ClassSessionId" IS NOT NULL;

ALTER TABLE "CheckIns" ADD CONSTRAINT "CK_CheckIns_ClassSession" CHECK ("ClassSessionId" IS NULL OR "CheckInType" = 'Class');

CREATE INDEX "IX_ActivitySubscriptionTransactions_ActivityCoachIdSnapshot" ON "ActivitySubscriptionTransactions" ("ActivityCoachIdSnapshot");

ALTER TABLE "ActivitySubscriptionTransactions" ADD CONSTRAINT "CK_ActivitySubscriptionTransactions_CoachSnapshot" CHECK (("ActivityCoachIdSnapshot" IS NULL AND "CoachPercentageSnapshot" IS NULL) OR ("ActivityCoachIdSnapshot" IS NOT NULL AND "CoachPercentageSnapshot" IS NOT NULL AND "CoachPercentageSnapshot" BETWEEN 0 AND 100));

CREATE INDEX "IX_ActivitySubscriptions_ActivityCoachId_CreatedAt" ON "ActivitySubscriptions" ("ActivityCoachId", "CreatedAt");

CREATE INDEX "IX_ActivityCoachAssignments_ActivityCoachId" ON "ActivityCoachAssignments" ("ActivityCoachId");

CREATE INDEX "IX_ActivityCoachAssignments_SportsActivityId_ActivityCoachId_S~" ON "ActivityCoachAssignments" ("SportsActivityId", "ActivityCoachId", "StartDate");

ALTER TABLE "ActivityCoachAssignments" ADD CONSTRAINT "CK_ActivityCoachAssignments_Coach" CHECK (("ActivityCoachId" IS NOT NULL AND "CoachEmployeeId" IS NULL) OR ("ActivityCoachId" IS NULL AND "CoachEmployeeId" IS NOT NULL));

CREATE UNIQUE INDEX "IX_ActivityCoaches_EmployeeId" ON "ActivityCoaches" ("EmployeeId") WHERE "EmployeeId" IS NOT NULL;

CREATE INDEX "IX_ActivityCoachSettlements_ActivityCoachId" ON "ActivityCoachSettlements" ("ActivityCoachId");

CREATE UNIQUE INDEX "IX_ActivityCoachSettlements_CommissionPeriodId_ActivityCoachId~" ON "ActivityCoachSettlements" ("CommissionPeriodId", "ActivityCoachId", "SportsActivityId", "Percentage");

CREATE INDEX "IX_ActivityCoachSettlements_CreatedByUserId" ON "ActivityCoachSettlements" ("CreatedByUserId");

CREATE INDEX "IX_ActivityCoachSettlements_EmployeeId" ON "ActivityCoachSettlements" ("EmployeeId");

CREATE INDEX "IX_ActivityCoachSettlements_SportsActivityId" ON "ActivityCoachSettlements" ("SportsActivityId");

CREATE INDEX "IX_ClassSessions_BranchId_StartedAt" ON "ClassSessions" ("BranchId", "StartedAt");

CREATE INDEX "IX_ClassSessions_ClassScheduleId" ON "ClassSessions" ("ClassScheduleId");

CREATE INDEX "IX_ClassSessions_CoachEmployeeId_StartedAt" ON "ClassSessions" ("CoachEmployeeId", "StartedAt");

CREATE INDEX "IX_ClassSessions_GroupClassId_StartedAt" ON "ClassSessions" ("GroupClassId", "StartedAt");

CREATE INDEX "IX_MembershipPlanEligibleMembers_CreatedByUserId" ON "MembershipPlanEligibleMembers" ("CreatedByUserId");

CREATE INDEX "IX_MembershipPlanEligibleMembers_MemberId" ON "MembershipPlanEligibleMembers" ("MemberId");

CREATE UNIQUE INDEX "IX_MembershipPlanEligibleMembers_MembershipPlanId_MemberId" ON "MembershipPlanEligibleMembers" ("MembershipPlanId", "MemberId");

ALTER TABLE "ActivityCoachAssignments" ADD CONSTRAINT "FK_ActivityCoachAssignments_ActivityCoaches_ActivityCoachId" FOREIGN KEY ("ActivityCoachId") REFERENCES "ActivityCoaches" ("Id") ON DELETE RESTRICT;

ALTER TABLE "ActivitySubscriptions" ADD CONSTRAINT "FK_ActivitySubscriptions_ActivityCoaches_ActivityCoachId" FOREIGN KEY ("ActivityCoachId") REFERENCES "ActivityCoaches" ("Id") ON DELETE RESTRICT;

ALTER TABLE "ActivitySubscriptionTransactions" ADD CONSTRAINT "FK_ActivitySubscriptionTransactions_ActivityCoaches_ActivityCo~" FOREIGN KEY ("ActivityCoachIdSnapshot") REFERENCES "ActivityCoaches" ("Id") ON DELETE RESTRICT;

ALTER TABLE "CheckIns" ADD CONSTRAINT "FK_CheckIns_ClassSessions_ClassSessionId" FOREIGN KEY ("ClassSessionId") REFERENCES "ClassSessions" ("Id") ON DELETE RESTRICT;

INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('fd77a297-006f-5f22-81d4-7d8b8716ca4b', 'Activities.ManageCoaches', 'Activities.ManageCoaches', 'Activities', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('ac5ac767-448b-5edf-bfa5-4ae7beafa5fa', 'Activities.ViewCoachRevenue', 'Activities.ViewCoachRevenue', 'Activities', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('5152272e-0e95-53b8-a799-bbb9848df3f6', 'Classes.View', 'Classes.View', 'Classes', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('d6218055-793c-5802-96df-30823241a8bb', 'Classes.Manage', 'Classes.Manage', 'Classes', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('b43efd0e-04a7-5ede-8d89-465057b0913e', 'Classes.ViewAttendanceReport', 'Classes.ViewAttendanceReport', 'Classes', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('82f51aa3-84dd-57c8-bad2-1816f227cfd7', 'FreelancePlans.View', 'FreelancePlans.View', 'FreelancePlans', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('047a26aa-9a56-54db-a40c-8968563fc2dd', 'FreelancePlans.Manage', 'FreelancePlans.Manage', 'FreelancePlans', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('0bdc7160-c718-57cd-809d-f38f26e24e4e', 'FreelancePlans.ManageEligibleMembers', 'FreelancePlans.ManageEligibleMembers', 'FreelancePlans', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "RolePermissions" ("RoleId", "PermissionId") SELECT r."Id", p."Id" FROM "AspNetRoles" r CROSS JOIN "Permissions" p WHERE r."NormalizedName" = 'OWNER' AND p."Code" IN ('Activities.ManageCoaches', 'Activities.ViewCoachRevenue', 'Classes.View', 'Classes.Manage', 'Classes.ViewAttendanceReport', 'FreelancePlans.View', 'FreelancePlans.Manage', 'FreelancePlans.ManageEligibleMembers') ON CONFLICT DO NOTHING;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261007000549_UpdateActivitiesClassesAndFreelanceRules', '10.0.8');

COMMIT;

