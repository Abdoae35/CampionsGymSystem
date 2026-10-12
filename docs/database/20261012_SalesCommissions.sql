START TRANSACTION;
ALTER TABLE "EmployeeCommissionSummaries" ADD "BelowTargetRateSnapshot" numeric(5,2);

ALTER TABLE "EmployeeCommissionSummaries" ADD "CommissionRuleId" uuid;

ALTER TABLE "EmployeeCommissionSummaries" ADD "ReachedTargetRateSnapshot" numeric(5,2);

ALTER TABLE "CommissionRules" ADD "OperationId" uuid;

ALTER TABLE "CommissionRules" ADD "RequestHash" character varying(64);

ALTER TABLE "CommissionRules" ADD "Revision" integer NOT NULL DEFAULT 0;

ALTER TABLE "CommissionPeriods" ADD "CloseOperationId" uuid;

ALTER TABLE "CommissionPeriods" ADD "CloseRequestHash" character varying(64);

ALTER TABLE "CommissionPeriods" ADD "EndBusinessDate" date;

ALTER TABLE "CommissionPeriods" ADD "OperationId" uuid;

ALTER TABLE "CommissionPeriods" ADD "RequestHash" character varying(64);

ALTER TABLE "CommissionPeriods" ADD "StartBusinessDate" date;

ALTER TABLE "CommissionPeriods" ADD "Version" integer NOT NULL DEFAULT 0;

CREATE TABLE "CommissionCollections" (
    "PaymentId" uuid NOT NULL,
    "EmployeeId" uuid,
    "Category" character varying(40),
    "EligibilityReason" character varying(80) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_CommissionCollections" PRIMARY KEY ("PaymentId"),
    CONSTRAINT "CK_CommissionCollections_Category" CHECK ("Category" IN ('NewMembership','Renewal')),
    CONSTRAINT "FK_CommissionCollections_Employees_EmployeeId" FOREIGN KEY ("EmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CommissionCollections_Payments_PaymentId" FOREIGN KEY ("PaymentId") REFERENCES "Payments" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "CommissionRefundAllocations" (
    "RefundId" uuid NOT NULL,
    "PaymentId" uuid NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    CONSTRAINT "PK_CommissionRefundAllocations" PRIMARY KEY ("RefundId", "PaymentId"),
    CONSTRAINT "CK_CommissionRefundAllocations_Amount" CHECK ("Amount" > 0),
    CONSTRAINT "FK_CommissionRefundAllocations_Payments_PaymentId" FOREIGN KEY ("PaymentId") REFERENCES "Payments" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CommissionRefundAllocations_Refunds_RefundId" FOREIGN KEY ("RefundId") REFERENCES "Refunds" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "CommissionStatementLines" (
    "CommissionPeriodId" uuid NOT NULL,
    "PaymentId" uuid NOT NULL,
    "EmployeeId" uuid,
    "Category" character varying(40),
    "EligibilityReason" character varying(80) NOT NULL,
    "GrossAmount" numeric(18,2) NOT NULL,
    "RefundedAmount" numeric(18,2) NOT NULL,
    "EligibleAmount" numeric(18,2) NOT NULL,
    CONSTRAINT "PK_CommissionStatementLines" PRIMARY KEY ("CommissionPeriodId", "PaymentId"),
    CONSTRAINT "CK_CommissionStatementLines_Amounts" CHECK ("GrossAmount" >= 0 AND "RefundedAmount" >= 0 AND "RefundedAmount" <= "GrossAmount" AND "EligibleAmount" >= 0 AND "EligibleAmount" <= "GrossAmount" - "RefundedAmount"),
    CONSTRAINT "CK_CommissionStatementLines_Category" CHECK ("Category" IN ('NewMembership','Renewal')),
    CONSTRAINT "FK_CommissionStatementLines_CommissionPeriods_CommissionPeriod~" FOREIGN KEY ("CommissionPeriodId") REFERENCES "CommissionPeriods" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CommissionStatementLines_Employees_EmployeeId" FOREIGN KEY ("EmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CommissionStatementLines_Payments_PaymentId" FOREIGN KEY ("PaymentId") REFERENCES "Payments" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_EmployeeCommissionSummaries_CommissionRuleId" ON "EmployeeCommissionSummaries" ("CommissionRuleId");

CREATE UNIQUE INDEX "IX_CommissionRules_OperationId_CommissionType" ON "CommissionRules" ("OperationId", "CommissionType");

CREATE INDEX "IX_CommissionPeriods_BranchId_StartBusinessDate" ON "CommissionPeriods" ("BranchId", "StartBusinessDate");

CREATE UNIQUE INDEX "IX_CommissionPeriods_CloseOperationId" ON "CommissionPeriods" ("CloseOperationId");

CREATE UNIQUE INDEX "IX_CommissionPeriods_OperationId" ON "CommissionPeriods" ("OperationId");

ALTER TABLE "CommissionPeriods" ADD CONSTRAINT "CK_CommissionPeriods_BusinessDates" CHECK (("StartBusinessDate" IS NULL AND "EndBusinessDate" IS NULL) OR ("StartBusinessDate" IS NOT NULL AND "EndBusinessDate" IS NOT NULL AND "EndBusinessDate" >= "StartBusinessDate"));

CREATE INDEX "IX_CommissionCollections_EmployeeId" ON "CommissionCollections" ("EmployeeId");

CREATE INDEX "IX_CommissionRefundAllocations_PaymentId" ON "CommissionRefundAllocations" ("PaymentId");

CREATE INDEX "IX_CommissionStatementLines_EmployeeId" ON "CommissionStatementLines" ("EmployeeId");

CREATE INDEX "IX_CommissionStatementLines_PaymentId" ON "CommissionStatementLines" ("PaymentId");

ALTER TABLE "EmployeeCommissionSummaries" ADD CONSTRAINT "FK_EmployeeCommissionSummaries_CommissionRules_CommissionRuleId" FOREIGN KEY ("CommissionRuleId") REFERENCES "CommissionRules" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261012013412_AddSalesCommissionEvidence', '10.0.8');

COMMIT;

START TRANSACTION;
ALTER TABLE "SaleTransactions" DROP CONSTRAINT "CK_SaleTransactions_Attribution";

ALTER TABLE "SaleTransactions" ADD CONSTRAINT "CK_SaleTransactions_Attribution" CHECK ("SalesAttributionSnapshot" IS NULL OR ("SalesAttributionSnapshot" = 'House' AND "SalesEmployeeId" IS NULL) OR ("SalesAttributionSnapshot" = 'Sales' AND "SalesEmployeeId" IS NOT NULL));

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261012015229_SeparatePurchaseCategoryFromOwnership', '10.0.8');

COMMIT;
