START TRANSACTION;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('de85d8e1-21f4-51b5-a069-69b9e7e7d6ac', 'FinancialMonths.View', 'FinancialMonths.View', 'FinancialMonths', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('1cf4e309-7d78-599a-8b63-436a34d9a26d', 'FinancialMonths.Manage', 'FinancialMonths.Manage', 'FinancialMonths', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('b5d3fa90-d5dd-5373-8242-a6772bf618ba', 'FinancialMonths.Close', 'FinancialMonths.Close', 'FinancialMonths', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('31a9a93e-c89d-50e9-bab4-9f709c3558a8', 'FinancialMonths.Reopen', 'FinancialMonths.Reopen', 'FinancialMonths', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('764ba898-c62c-55e2-85f3-77b5b662aca4', 'FinancialMonths.Export', 'FinancialMonths.Export', 'FinancialMonths', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('74a6cfd3-235e-525a-a929-4945031628b2', 'FinancialMonths.ViewConsolidated', 'FinancialMonths.ViewConsolidated', 'FinancialMonths', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('b32ae1e9-2adc-5534-bf44-d8360d03969b', 'Salaries.View', 'Salaries.View', 'Salaries', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('597361a4-2524-596b-8349-184d7b8b737e', 'Salaries.Manage', 'Salaries.Manage', 'Salaries', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('59550144-7668-5e5a-b650-81526d971fb3', 'Payroll.RecordPayment', 'Payroll.RecordPayment', 'Payroll', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('2d5889d7-a521-58f3-a228-b8da9cdde687', 'Maintenance.View', 'Maintenance.View', 'Maintenance', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('f8a4db14-52da-5bdb-bea0-9da0a6d6d638', 'Maintenance.Manage', 'Maintenance.Manage', 'Maintenance', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;

DROP INDEX "IX_EmployeeSalarySettings_EmployeeId";

ALTER TABLE "Payrolls" ALTER COLUMN "CommissionPeriodId" DROP NOT NULL;

ALTER TABLE "Payrolls" ADD "CommissionSummaryId" uuid;

ALTER TABLE "Payrolls" ADD "FinancialMonthId" uuid;

ALTER TABLE "Payrolls" ADD "RecognizedByUserId" uuid;

ALTER TABLE "Payrolls" ADD "SalaryEvidenceJson" jsonb;

ALTER TABLE "PayrollDeductions" ADD "EmployeeAdvanceId" uuid;

ALTER TABLE "FinancialTransactions" ADD "FinancialMonthId" uuid;

ALTER TABLE "FinancialTransactions" ADD "PostedAt" timestamp with time zone;

ALTER TABLE "EmployeeSalarySettings" ADD "OperationId" uuid;

ALTER TABLE "EmployeeSalarySettings" ADD "RequestHash" character varying(64);

ALTER TABLE "EmployeeAdvances" ADD "BasicEntitlementSnapshot" numeric(18,2);

ALTER TABLE "EmployeeAdvances" ADD "FinancialMonthId" uuid;

ALTER TABLE "CoachEarnings" ADD "FinancialMonthId" uuid;

CREATE TABLE "FinancialMonths" (
    "Id" uuid NOT NULL,
    "BranchId" uuid NOT NULL,
    "Name" character varying(256) NOT NULL,
    "StartDate" date NOT NULL,
    "EndDate" date NOT NULL,
    "Status" character varying(16) NOT NULL,
    "Version" integer NOT NULL,
    "LastRevision" integer NOT NULL,
    "FirstClosedAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedByUserId" uuid NOT NULL,
    CONSTRAINT "PK_FinancialMonths" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_FinancialMonths_Bounds" CHECK ("EndDate" >= "StartDate" AND "EndDate" - "StartDate" < 62),
    CONSTRAINT "CK_FinancialMonths_Status" CHECK ("Status" IN ('Open','Closed','Reopened') AND "Version" >= 0 AND "LastRevision" >= 0 AND ("Status" = 'Open' AND "FirstClosedAt" IS NULL OR "Status" <> 'Open' AND "FirstClosedAt" IS NOT NULL)),
    CONSTRAINT "FK_FinancialMonths_AspNetUsers_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_FinancialMonths_GymBranches_BranchId" FOREIGN KEY ("BranchId") REFERENCES "GymBranches" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "FinancialOccurrenceEvidence" (
    "Id" uuid NOT NULL,
    "FinancialTransactionId" uuid NOT NULL,
    "OperationId" uuid NOT NULL,
    "RequestHash" character varying(64) NOT NULL,
    "OriginalOccurredAt" timestamp with time zone NOT NULL,
    "RecordedAt" timestamp with time zone NOT NULL,
    "ActorUserId" uuid NOT NULL,
    "Reason" character varying(2000) NOT NULL,
    CONSTRAINT "PK_FinancialOccurrenceEvidence" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_FinancialOccurrenceEvidence_Date" CHECK ("OriginalOccurredAt" <= "RecordedAt"),
    CONSTRAINT "FK_FinancialOccurrenceEvidence_AspNetUsers_ActorUserId" FOREIGN KEY ("ActorUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_FinancialOccurrenceEvidence_FinancialTransactions_Financial~" FOREIGN KEY ("FinancialTransactionId") REFERENCES "FinancialTransactions" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "MaintenancePlanRevisions" (
    "Id" uuid NOT NULL,
    "PlanId" uuid NOT NULL,
    "BranchId" uuid NOT NULL,
    "Revision" integer NOT NULL,
    "Name" character varying(256) NOT NULL,
    "ExpectedCost" numeric(18,2) NOT NULL,
    "FrequencyMonths" integer NOT NULL,
    "FirstDueDate" date NOT NULL,
    "IsActive" boolean NOT NULL,
    "OperationId" uuid NOT NULL,
    "RequestHash" character varying(64) NOT NULL,
    "ActorUserId" uuid NOT NULL,
    "RecordedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_MaintenancePlanRevisions" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_MaintenancePlanRevisions_Values" CHECK ("ExpectedCost" > 0 AND "FrequencyMonths" BETWEEN 1 AND 120 AND "Revision" > 0),
    CONSTRAINT "FK_MaintenancePlanRevisions_AspNetUsers_ActorUserId" FOREIGN KEY ("ActorUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_MaintenancePlanRevisions_GymBranches_BranchId" FOREIGN KEY ("BranchId") REFERENCES "GymBranches" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "FinancialClosingRevisions" (
    "Id" uuid NOT NULL,
    "FinancialMonthId" uuid NOT NULL,
    "Revision" integer NOT NULL,
    "SchemaVersion" integer NOT NULL,
    "SnapshotAt" timestamp with time zone NOT NULL,
    "ActorUserId" uuid NOT NULL,
    "Reason" character varying(2000) NOT NULL,
    "ReportJson" jsonb NOT NULL,
    "Workbook" bytea NOT NULL,
    "WorkbookSha256" character varying(64) NOT NULL,
    CONSTRAINT "PK_FinancialClosingRevisions" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_FinancialClosingRevisions_Version" CHECK ("Revision" > 0 AND "SchemaVersion" > 0),
    CONSTRAINT "FK_FinancialClosingRevisions_AspNetUsers_ActorUserId" FOREIGN KEY ("ActorUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_FinancialClosingRevisions_FinancialMonths_FinancialMonthId" FOREIGN KEY ("FinancialMonthId") REFERENCES "FinancialMonths" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "FinancialMonthEvents" (
    "Id" uuid NOT NULL,
    "FinancialMonthId" uuid NOT NULL,
    "OperationId" uuid NOT NULL,
    "RequestHash" character varying(64) NOT NULL,
    "Kind" character varying(16) NOT NULL,
    "Reason" character varying(2000) NOT NULL,
    "ResultVersion" integer NOT NULL,
    "Revision" integer,
    "ActorUserId" uuid NOT NULL,
    "RecordedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_FinancialMonthEvents" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_FinancialMonthEvents_Kind" CHECK ("Kind" IN ('Opened','Closed','Reopened') AND "ResultVersion" >= 0),
    CONSTRAINT "FK_FinancialMonthEvents_AspNetUsers_ActorUserId" FOREIGN KEY ("ActorUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_FinancialMonthEvents_FinancialMonths_FinancialMonthId" FOREIGN KEY ("FinancialMonthId") REFERENCES "FinancialMonths" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "PayrollPaymentRecords" (
    "Id" uuid NOT NULL,
    "PayrollId" uuid NOT NULL,
    "FinancialMonthId" uuid NOT NULL,
    "OperationId" uuid NOT NULL,
    "RequestHash" character varying(64) NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    "BalanceBefore" numeric(18,2) NOT NULL,
    "BalanceAfter" numeric(18,2) NOT NULL,
    "PaidAt" timestamp with time zone NOT NULL,
    "BusinessDate" date NOT NULL,
    "RecordedAt" timestamp with time zone NOT NULL,
    "ActorUserId" uuid NOT NULL,
    "Reason" character varying(2000) NOT NULL,
    "Reference" character varying(512),
    CONSTRAINT "PK_PayrollPaymentRecords" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_PayrollPaymentRecords_Balance" CHECK ("Amount" > 0 AND "BalanceBefore" >= "Amount" AND "BalanceAfter" = "BalanceBefore" - "Amount" AND "PaidAt" <= "RecordedAt"),
    CONSTRAINT "FK_PayrollPaymentRecords_AspNetUsers_ActorUserId" FOREIGN KEY ("ActorUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_PayrollPaymentRecords_FinancialMonths_FinancialMonthId" FOREIGN KEY ("FinancialMonthId") REFERENCES "FinancialMonths" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_PayrollPaymentRecords_Payrolls_PayrollId" FOREIGN KEY ("PayrollId") REFERENCES "Payrolls" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_Payrolls_CommissionSummaryId" ON "Payrolls" ("CommissionSummaryId") WHERE "CommissionSummaryId" IS NOT NULL;

CREATE UNIQUE INDEX "IX_Payrolls_FinancialMonthId_EmployeeId" ON "Payrolls" ("FinancialMonthId", "EmployeeId") WHERE "FinancialMonthId" IS NOT NULL AND "CommissionSummaryId" IS NULL;

CREATE INDEX "IX_Payrolls_RecognizedByUserId" ON "Payrolls" ("RecognizedByUserId");

ALTER TABLE "Payrolls" ADD CONSTRAINT "CK_Payrolls_Recognized" CHECK ("FinancialMonthId" IS NULL OR ("RecognizedByUserId" IS NOT NULL AND "BonusAmount" = 0 AND "NetAmount" = "BasicSalary" + "CommissionAmount" - "DeductionAmount" AND ("CommissionSummaryId" IS NULL AND "CommissionAmount" = 0 AND "SalaryEvidenceJson" IS NOT NULL OR "CommissionSummaryId" IS NOT NULL AND "CommissionPeriodId" IS NOT NULL AND "BasicSalary" = 0 AND "DeductionAmount" = 0)));

CREATE UNIQUE INDEX "IX_PayrollDeductions_EmployeeAdvanceId" ON "PayrollDeductions" ("EmployeeAdvanceId") WHERE "EmployeeAdvanceId" IS NOT NULL;

CREATE INDEX "IX_FinancialTransactions_FinancialMonthId_OccurredAt_Id" ON "FinancialTransactions" ("FinancialMonthId", "OccurredAt", "Id");

CREATE UNIQUE INDEX "IX_EmployeeSalarySettings_EmployeeId_EffectiveFrom" ON "EmployeeSalarySettings" ("EmployeeId", "EffectiveFrom");

CREATE UNIQUE INDEX "IX_EmployeeSalarySettings_OperationId" ON "EmployeeSalarySettings" ("OperationId") WHERE "OperationId" IS NOT NULL;

CREATE INDEX "IX_EmployeeAdvances_FinancialMonthId_ReceivingEmployeeId" ON "EmployeeAdvances" ("FinancialMonthId", "ReceivingEmployeeId");

ALTER TABLE "EmployeeAdvances" ADD CONSTRAINT "CK_EmployeeAdvances_Entitlement" CHECK ("FinancialMonthId" IS NULL AND "BasicEntitlementSnapshot" IS NULL OR "FinancialMonthId" IS NOT NULL AND "BasicEntitlementSnapshot" IS NOT NULL AND "BasicEntitlementSnapshot" >= "Amount");

CREATE INDEX "IX_CoachEarnings_FinancialMonthId" ON "CoachEarnings" ("FinancialMonthId");

CREATE INDEX "IX_FinancialClosingRevisions_ActorUserId" ON "FinancialClosingRevisions" ("ActorUserId");

CREATE UNIQUE INDEX "IX_FinancialClosingRevisions_FinancialMonthId_Revision" ON "FinancialClosingRevisions" ("FinancialMonthId", "Revision");

CREATE INDEX "IX_FinancialMonthEvents_ActorUserId" ON "FinancialMonthEvents" ("ActorUserId");

CREATE INDEX "IX_FinancialMonthEvents_FinancialMonthId" ON "FinancialMonthEvents" ("FinancialMonthId");

CREATE UNIQUE INDEX "IX_FinancialMonthEvents_OperationId" ON "FinancialMonthEvents" ("OperationId");

CREATE UNIQUE INDEX "IX_FinancialMonths_BranchId" ON "FinancialMonths" ("BranchId") WHERE "Status" = 'Open';

CREATE UNIQUE INDEX "IX_FinancialMonths_BranchId_StartDate" ON "FinancialMonths" ("BranchId", "StartDate");

CREATE INDEX "IX_FinancialMonths_CreatedByUserId" ON "FinancialMonths" ("CreatedByUserId");

CREATE INDEX "IX_FinancialOccurrenceEvidence_ActorUserId" ON "FinancialOccurrenceEvidence" ("ActorUserId");

CREATE UNIQUE INDEX "IX_FinancialOccurrenceEvidence_FinancialTransactionId" ON "FinancialOccurrenceEvidence" ("FinancialTransactionId");

CREATE UNIQUE INDEX "IX_FinancialOccurrenceEvidence_OperationId" ON "FinancialOccurrenceEvidence" ("OperationId");

CREATE INDEX "IX_MaintenancePlanRevisions_ActorUserId" ON "MaintenancePlanRevisions" ("ActorUserId");

CREATE INDEX "IX_MaintenancePlanRevisions_BranchId" ON "MaintenancePlanRevisions" ("BranchId");

CREATE UNIQUE INDEX "IX_MaintenancePlanRevisions_OperationId" ON "MaintenancePlanRevisions" ("OperationId");

CREATE UNIQUE INDEX "IX_MaintenancePlanRevisions_PlanId_Revision" ON "MaintenancePlanRevisions" ("PlanId", "Revision");

CREATE INDEX "IX_PayrollPaymentRecords_ActorUserId" ON "PayrollPaymentRecords" ("ActorUserId");

CREATE INDEX "IX_PayrollPaymentRecords_FinancialMonthId" ON "PayrollPaymentRecords" ("FinancialMonthId");

CREATE UNIQUE INDEX "IX_PayrollPaymentRecords_OperationId" ON "PayrollPaymentRecords" ("OperationId");

CREATE INDEX "IX_PayrollPaymentRecords_PayrollId_RecordedAt" ON "PayrollPaymentRecords" ("PayrollId", "RecordedAt");

ALTER TABLE "CoachEarnings" ADD CONSTRAINT "FK_CoachEarnings_FinancialMonths_FinancialMonthId" FOREIGN KEY ("FinancialMonthId") REFERENCES "FinancialMonths" ("Id") ON DELETE RESTRICT;

ALTER TABLE "EmployeeAdvances" ADD CONSTRAINT "FK_EmployeeAdvances_FinancialMonths_FinancialMonthId" FOREIGN KEY ("FinancialMonthId") REFERENCES "FinancialMonths" ("Id") ON DELETE RESTRICT;

ALTER TABLE "FinancialTransactions" ADD CONSTRAINT "FK_FinancialTransactions_FinancialMonths_FinancialMonthId" FOREIGN KEY ("FinancialMonthId") REFERENCES "FinancialMonths" ("Id") ON DELETE RESTRICT;

ALTER TABLE "PayrollDeductions" ADD CONSTRAINT "FK_PayrollDeductions_EmployeeAdvances_EmployeeAdvanceId" FOREIGN KEY ("EmployeeAdvanceId") REFERENCES "EmployeeAdvances" ("Id") ON DELETE RESTRICT;

ALTER TABLE "Payrolls" ADD CONSTRAINT "FK_Payrolls_AspNetUsers_RecognizedByUserId" FOREIGN KEY ("RecognizedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT;

ALTER TABLE "Payrolls" ADD CONSTRAINT "FK_Payrolls_EmployeeCommissionSummaries_CommissionSummaryId" FOREIGN KEY ("CommissionSummaryId") REFERENCES "EmployeeCommissionSummaries" ("Id") ON DELETE RESTRICT;

ALTER TABLE "Payrolls" ADD CONSTRAINT "FK_Payrolls_FinancialMonths_FinancialMonthId" FOREIGN KEY ("FinancialMonthId") REFERENCES "FinancialMonths" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261012164313_ImplementMonthlyClosingAndEmployeePayments', '10.0.8');

COMMIT;

