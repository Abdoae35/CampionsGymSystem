START TRANSACTION;
ALTER TABLE "MembershipPlans" ADD "BranchId" uuid;

CREATE INDEX "IX_MembershipPlans_BranchId" ON "MembershipPlans" ("BranchId");

ALTER TABLE "MembershipPlans" ADD CONSTRAINT "FK_MembershipPlans_GymBranches_BranchId" FOREIGN KEY ("BranchId") REFERENCES "GymBranches" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261009013415_AddMembershipPlanBranchOwnership', '10.0.8');

COMMIT;

