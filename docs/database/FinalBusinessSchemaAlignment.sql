START TRANSACTION;
ALTER TABLE "Shifts" ADD "BusinessDayId" uuid;

ALTER TABLE "SaleTransactions" ADD "BusinessDayId" uuid;

ALTER TABLE "Refunds" ADD "BusinessDayId" uuid;

ALTER TABLE "Payments" ADD "BusinessDayId" uuid;

ALTER TABLE "MembershipPlans" ADD "MembershipPlanCategoryId" uuid;

ALTER TABLE "FinancialTransactions" ADD "BusinessDayId" uuid;

ALTER TABLE "Expenses" ADD "BusinessDayId" uuid;

ALTER TABLE "ClassSessions" ADD "BusinessDayId" uuid;

ALTER TABLE "CheckIns" ADD "BusinessDayId" uuid;

CREATE TABLE "BusinessDays" (
    "Id" uuid NOT NULL,
    "BranchId" uuid NOT NULL,
    "BusinessDate" date NOT NULL,
    "OpenedAt" timestamp with time zone NOT NULL,
    "ClosedAt" timestamp with time zone,
    "Status" character varying(40) NOT NULL,
    "OpenedByUserId" uuid,
    "ClosedByUserId" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_BusinessDays" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_BusinessDays_Lifecycle" CHECK (("Status" = 'Open' AND "ClosedAt" IS NULL AND "ClosedByUserId" IS NULL) OR ("Status" = 'Closed' AND "ClosedAt" IS NOT NULL AND "ClosedAt" >= "OpenedAt")),
    CONSTRAINT "CK_BusinessDays_Status" CHECK ("Status" IN ('Open', 'Closed')),
    CONSTRAINT "FK_BusinessDays_AspNetUsers_ClosedByUserId" FOREIGN KEY ("ClosedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_BusinessDays_AspNetUsers_OpenedByUserId" FOREIGN KEY ("OpenedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_BusinessDays_GymBranches_BranchId" FOREIGN KEY ("BranchId") REFERENCES "GymBranches" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "EmployeeLeaveRequests" (
    "Id" uuid NOT NULL,
    "EmployeeId" uuid NOT NULL,
    "FromDate" date NOT NULL,
    "ToDate" date NOT NULL,
    "Reason" text,
    "Status" character varying(40) NOT NULL,
    "ReviewedByUserId" uuid,
    "ReviewedAt" timestamp with time zone,
    "ManagerNotes" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_EmployeeLeaveRequests" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_EmployeeLeaveRequests_Dates" CHECK ("ToDate" >= "FromDate"),
    CONSTRAINT "CK_EmployeeLeaveRequests_Review" CHECK (("Status" = 'Pending' AND "ReviewedAt" IS NULL AND "ReviewedByUserId" IS NULL) OR ("Status" IN ('Approved', 'Rejected') AND "ReviewedAt" IS NOT NULL AND "ReviewedAt" >= "CreatedAt")),
    CONSTRAINT "CK_EmployeeLeaveRequests_Status" CHECK ("Status" IN ('Pending', 'Approved', 'Rejected')),
    CONSTRAINT "FK_EmployeeLeaveRequests_AspNetUsers_ReviewedByUserId" FOREIGN KEY ("ReviewedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_EmployeeLeaveRequests_Employees_EmployeeId" FOREIGN KEY ("EmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "MembershipPlanCategories" (
    "Id" uuid NOT NULL,
    "Name" character varying(256) NOT NULL,
    "Description" text,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_MembershipPlanCategories" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_MembershipPlanCategories_Name" CHECK (length(trim("Name")) > 0)
);

CREATE TABLE "ShiftEmployeeChanges" (
    "Id" uuid NOT NULL,
    "ShiftId" uuid NOT NULL,
    "OriginalEmployeeId" uuid NOT NULL,
    "ReplacementEmployeeId" uuid NOT NULL,
    "Reason" text,
    "ChangedByUserId" uuid NOT NULL,
    "ChangedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ShiftEmployeeChanges" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_ShiftEmployeeChanges_DifferentEmployees" CHECK ("OriginalEmployeeId" <> "ReplacementEmployeeId"),
    CONSTRAINT "FK_ShiftEmployeeChanges_AspNetUsers_ChangedByUserId" FOREIGN KEY ("ChangedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ShiftEmployeeChanges_Employees_OriginalEmployeeId" FOREIGN KEY ("OriginalEmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ShiftEmployeeChanges_Employees_ReplacementEmployeeId" FOREIGN KEY ("ReplacementEmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ShiftEmployeeChanges_Shifts_ShiftId" FOREIGN KEY ("ShiftId") REFERENCES "Shifts" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "UserPermissionOverrides" (
    "UserId" uuid NOT NULL,
    "PermissionId" uuid NOT NULL,
    "Effect" character varying(40) NOT NULL,
    "CreatedByUserId" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_UserPermissionOverrides" PRIMARY KEY ("UserId", "PermissionId"),
    CONSTRAINT "CK_UserPermissionOverrides_Effect" CHECK ("Effect" IN ('Allow', 'Deny')),
    CONSTRAINT "FK_UserPermissionOverrides_AspNetUsers_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_UserPermissionOverrides_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_UserPermissionOverrides_Permissions_PermissionId" FOREIGN KEY ("PermissionId") REFERENCES "Permissions" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_Shifts_BusinessDayId" ON "Shifts" ("BusinessDayId");

CREATE INDEX "IX_SaleTransactions_BusinessDayId" ON "SaleTransactions" ("BusinessDayId");

CREATE INDEX "IX_Refunds_BusinessDayId" ON "Refunds" ("BusinessDayId");

CREATE INDEX "IX_Payments_BusinessDayId" ON "Payments" ("BusinessDayId");

CREATE INDEX "IX_MembershipPlans_MembershipPlanCategoryId" ON "MembershipPlans" ("MembershipPlanCategoryId");

CREATE INDEX "IX_FinancialTransactions_BusinessDayId" ON "FinancialTransactions" ("BusinessDayId");

CREATE INDEX "IX_Expenses_BusinessDayId" ON "Expenses" ("BusinessDayId");

CREATE INDEX "IX_ClassSessions_BusinessDayId" ON "ClassSessions" ("BusinessDayId");

CREATE INDEX "IX_CheckIns_BusinessDayId" ON "CheckIns" ("BusinessDayId");

CREATE UNIQUE INDEX "IX_BusinessDays_BranchId" ON "BusinessDays" ("BranchId") WHERE "Status" = 'Open';

CREATE UNIQUE INDEX "IX_BusinessDays_BranchId_BusinessDate" ON "BusinessDays" ("BranchId", "BusinessDate");

CREATE INDEX "IX_BusinessDays_ClosedByUserId" ON "BusinessDays" ("ClosedByUserId");

CREATE INDEX "IX_BusinessDays_OpenedByUserId" ON "BusinessDays" ("OpenedByUserId");

CREATE INDEX "IX_EmployeeLeaveRequests_EmployeeId_FromDate_ToDate" ON "EmployeeLeaveRequests" ("EmployeeId", "FromDate", "ToDate");

CREATE INDEX "IX_EmployeeLeaveRequests_ReviewedByUserId" ON "EmployeeLeaveRequests" ("ReviewedByUserId");

CREATE INDEX "IX_EmployeeLeaveRequests_Status" ON "EmployeeLeaveRequests" ("Status");

CREATE UNIQUE INDEX "IX_MembershipPlanCategories_Name" ON "MembershipPlanCategories" ("Name");

CREATE INDEX "IX_ShiftEmployeeChanges_ChangedByUserId" ON "ShiftEmployeeChanges" ("ChangedByUserId");

CREATE INDEX "IX_ShiftEmployeeChanges_OriginalEmployeeId" ON "ShiftEmployeeChanges" ("OriginalEmployeeId");

CREATE INDEX "IX_ShiftEmployeeChanges_ReplacementEmployeeId" ON "ShiftEmployeeChanges" ("ReplacementEmployeeId");

CREATE INDEX "IX_ShiftEmployeeChanges_ShiftId_ChangedAt" ON "ShiftEmployeeChanges" ("ShiftId", "ChangedAt");

CREATE INDEX "IX_UserPermissionOverrides_CreatedByUserId" ON "UserPermissionOverrides" ("CreatedByUserId");

CREATE INDEX "IX_UserPermissionOverrides_PermissionId" ON "UserPermissionOverrides" ("PermissionId");

ALTER TABLE "CheckIns" ADD CONSTRAINT "FK_CheckIns_BusinessDays_BusinessDayId" FOREIGN KEY ("BusinessDayId") REFERENCES "BusinessDays" ("Id") ON DELETE RESTRICT;

ALTER TABLE "ClassSessions" ADD CONSTRAINT "FK_ClassSessions_BusinessDays_BusinessDayId" FOREIGN KEY ("BusinessDayId") REFERENCES "BusinessDays" ("Id") ON DELETE RESTRICT;

ALTER TABLE "Expenses" ADD CONSTRAINT "FK_Expenses_BusinessDays_BusinessDayId" FOREIGN KEY ("BusinessDayId") REFERENCES "BusinessDays" ("Id") ON DELETE RESTRICT;

ALTER TABLE "FinancialTransactions" ADD CONSTRAINT "FK_FinancialTransactions_BusinessDays_BusinessDayId" FOREIGN KEY ("BusinessDayId") REFERENCES "BusinessDays" ("Id") ON DELETE RESTRICT;

ALTER TABLE "MembershipPlans" ADD CONSTRAINT "FK_MembershipPlans_MembershipPlanCategories_MembershipPlanCate~" FOREIGN KEY ("MembershipPlanCategoryId") REFERENCES "MembershipPlanCategories" ("Id") ON DELETE RESTRICT;

ALTER TABLE "Payments" ADD CONSTRAINT "FK_Payments_BusinessDays_BusinessDayId" FOREIGN KEY ("BusinessDayId") REFERENCES "BusinessDays" ("Id") ON DELETE RESTRICT;

ALTER TABLE "Refunds" ADD CONSTRAINT "FK_Refunds_BusinessDays_BusinessDayId" FOREIGN KEY ("BusinessDayId") REFERENCES "BusinessDays" ("Id") ON DELETE RESTRICT;

ALTER TABLE "SaleTransactions" ADD CONSTRAINT "FK_SaleTransactions_BusinessDays_BusinessDayId" FOREIGN KEY ("BusinessDayId") REFERENCES "BusinessDays" ("Id") ON DELETE RESTRICT;

ALTER TABLE "Shifts" ADD CONSTRAINT "FK_Shifts_BusinessDays_BusinessDayId" FOREIGN KEY ("BusinessDayId") REFERENCES "BusinessDays" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261007151235_FinalBusinessSchemaAlignment', '10.0.8');

COMMIT;

