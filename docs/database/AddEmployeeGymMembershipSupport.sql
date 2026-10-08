START TRANSACTION;
ALTER TABLE "MembershipPlans" DROP CONSTRAINT "CK_MembershipPlans_DurationDays";

ALTER TABLE "MembershipPlans" DROP CONSTRAINT "CK_MembershipPlans_PlanCategoryCoach";

ALTER TABLE "MemberSubscriptions" ALTER COLUMN "EndDate" DROP NOT NULL;

ALTER TABLE "MemberSubscriptions" ADD "EmployeeId" uuid;

ALTER TABLE "Employees" ADD "MemberId" uuid;

CREATE UNIQUE INDEX "IX_MemberSubscriptions_EmployeeId" ON "MemberSubscriptions" ("EmployeeId") WHERE "EmployeeId" IS NOT NULL AND "EndDate" IS NULL;

CREATE INDEX "IX_MemberSubscriptions_EmployeeId_StartDate" ON "MemberSubscriptions" ("EmployeeId", "StartDate");

CREATE UNIQUE INDEX "IX_MemberSubscriptions_OpenEmployeeMember" ON "MemberSubscriptions" ("MemberId") WHERE "EmployeeId" IS NOT NULL AND "EndDate" IS NULL;

ALTER TABLE "MemberSubscriptions" ADD CONSTRAINT "CK_MemberSubscriptions_EmploymentPeriod" CHECK (("EmployeeId" IS NULL AND "EndDate" IS NOT NULL) OR ("EmployeeId" IS NOT NULL AND "AccessMode" = 'Unlimited' AND (("EndDate" IS NULL AND "Status" IN ('Scheduled', 'Active')) OR ("EndDate" IS NOT NULL AND "Status" IN ('Expired', 'Cancelled')))));

ALTER TABLE "MembershipPlans" ADD CONSTRAINT "CK_MembershipPlans_DurationDays" CHECK (("PlanCategory" = 'Employee' AND "DurationDays" = 0) OR ("PlanCategory" <> 'Employee' AND "DurationDays" > 0));

ALTER TABLE "MembershipPlans" ADD CONSTRAINT "CK_MembershipPlans_Employee" CHECK ("PlanCategory" <> 'Employee' OR ("Price" = 0 AND "AccessMode" = 'Unlimited' AND "SessionCount" IS NULL));

ALTER TABLE "MembershipPlans" ADD CONSTRAINT "CK_MembershipPlans_PlanCategoryCoach" CHECK (("PlanCategory" IN ('Normal', 'Employee') AND "FreelanceCoachEmployeeId" IS NULL) OR ("PlanCategory" = 'Freelance' AND "FreelanceCoachEmployeeId" IS NOT NULL));

CREATE UNIQUE INDEX "IX_Employees_MemberId" ON "Employees" ("MemberId");

ALTER TABLE "Employees" ADD CONSTRAINT "FK_Employees_Members_MemberId" FOREIGN KEY ("MemberId") REFERENCES "Members" ("Id") ON DELETE RESTRICT;

ALTER TABLE "MemberSubscriptions" ADD CONSTRAINT "FK_MemberSubscriptions_Employees_EmployeeId" FOREIGN KEY ("EmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261008014844_AddEmployeeGymMembershipSupport', '10.0.8');

COMMIT;
