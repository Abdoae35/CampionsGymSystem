START TRANSACTION;
DROP INDEX "IX_Shifts_BranchId";

DROP INDEX "IX_FinancialTransactions_BranchId";

ALTER TABLE "FinancialTransactions" DROP CONSTRAINT "CK_FinancialTransactions_Source";

ALTER TABLE "FinancialTransactions" DROP CONSTRAINT "CK_FinancialTransactions_SourceType";

DROP INDEX "IX_Expenses_BranchId";

ALTER TABLE "Shifts" ADD "CloseOperationId" uuid;

ALTER TABLE "Shifts" ADD "CloseRequestHash" character varying(64);

ALTER TABLE "Shifts" ADD "ClosingBusinessDate" date;

ALTER TABLE "Shifts" ADD "ClosingReason" text;

ALTER TABLE "Shifts" ADD "EmployeeId" uuid;

ALTER TABLE "Shifts" ADD "FinancialAccountId" uuid;

ALTER TABLE "Shifts" ADD "OpeningBusinessDate" date;

ALTER TABLE "Shifts" ADD "OperationId" uuid;

ALTER TABLE "Shifts" ADD "RequestHash" character varying(64);

ALTER TABLE "Shifts" ADD "Version" bigint NOT NULL DEFAULT 0;

ALTER TABLE "FinancialTransactions" ADD "CashMovementId" uuid;

ALTER TABLE "FinancialTransactions" ADD "DrawerSequence" bigint;

ALTER TABLE "FinancialTransactions" ADD "EmployeeAdvanceId" uuid;

ALTER TABLE "FinancialTransactions" ADD "ShiftId" uuid;

ALTER TABLE "Expenses" ADD "BusinessDate" date;

ALTER TABLE "Expenses" ADD "OperationId" uuid;

ALTER TABLE "Expenses" ADD "PaidAt" timestamp with time zone;

ALTER TABLE "Expenses" ADD "ReceiptReference" character varying(512);

ALTER TABLE "Expenses" ADD "RecordingEmployeeId" uuid;

ALTER TABLE "Expenses" ADD "RequestHash" character varying(64);

CREATE TABLE "CashCounts" (
    "Id" uuid NOT NULL,
    "BranchId" uuid NOT NULL,
    "FinancialAccountId" uuid NOT NULL,
    "ShiftId" uuid,
    "BusinessDayId" uuid,
    "BusinessDate" date NOT NULL,
    "OperationId" uuid NOT NULL,
    "RequestHash" character varying(64) NOT NULL,
    "Kind" character varying(40) NOT NULL,
    "MovementSequence" bigint NOT NULL,
    "ExpectedCash" numeric(18,2) NOT NULL,
    "CountedCash" numeric(18,2) NOT NULL,
    "BookCash" numeric(18,2) NOT NULL,
    "Difference" numeric(18,2) NOT NULL,
    "OccurredAt" timestamp with time zone NOT NULL,
    "CreatedByUserId" uuid NOT NULL,
    "Reason" text,
    CONSTRAINT "PK_CashCounts" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_CashCounts_Count" CHECK ("CountedCash" >= 0),
    CONSTRAINT "CK_CashCounts_Difference" CHECK ("Difference" = "CountedCash" - "ExpectedCash"),
    CONSTRAINT "CK_CashCounts_Kind" CHECK ("Kind" IN ('Initial', 'Opening', 'Closing', 'Recount')),
    CONSTRAINT "FK_CashCounts_AspNetUsers_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CashCounts_BusinessDays_BusinessDayId" FOREIGN KEY ("BusinessDayId") REFERENCES "BusinessDays" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CashCounts_FinancialAccounts_FinancialAccountId" FOREIGN KEY ("FinancialAccountId") REFERENCES "FinancialAccounts" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CashCounts_GymBranches_BranchId" FOREIGN KEY ("BranchId") REFERENCES "GymBranches" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CashCounts_Shifts_ShiftId" FOREIGN KEY ("ShiftId") REFERENCES "Shifts" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "CashDrawers" (
    "BranchId" uuid NOT NULL,
    "FinancialAccountId" uuid NOT NULL,
    "MovementSequence" bigint NOT NULL,
    "OperationId" uuid NOT NULL,
    "RequestHash" character varying(64) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedByUserId" uuid NOT NULL,
    CONSTRAINT "PK_CashDrawers" PRIMARY KEY ("BranchId"),
    CONSTRAINT "CK_CashDrawers_Sequence" CHECK ("MovementSequence" >= 0),
    CONSTRAINT "FK_CashDrawers_AspNetUsers_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CashDrawers_FinancialAccounts_FinancialAccountId" FOREIGN KEY ("FinancialAccountId") REFERENCES "FinancialAccounts" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CashDrawers_GymBranches_BranchId" FOREIGN KEY ("BranchId") REFERENCES "GymBranches" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "CashMovements" (
    "Id" uuid NOT NULL,
    "BranchId" uuid NOT NULL,
    "FinancialAccountId" uuid NOT NULL,
    "ShiftId" uuid,
    "BusinessDayId" uuid,
    "BusinessDate" date NOT NULL,
    "OperationId" uuid NOT NULL,
    "RequestHash" character varying(64) NOT NULL,
    "Kind" character varying(40) NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    "Reason" text NOT NULL,
    "OccurredAt" timestamp with time zone NOT NULL,
    "CreatedByUserId" uuid NOT NULL,
    CONSTRAINT "PK_CashMovements" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_CashMovements_Amount" CHECK ("Amount" > 0),
    CONSTRAINT "CK_CashMovements_Kind" CHECK ("Kind" IN ('InitialPosition', 'Withdrawal')),
    CONSTRAINT "FK_CashMovements_AspNetUsers_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CashMovements_BusinessDays_BusinessDayId" FOREIGN KEY ("BusinessDayId") REFERENCES "BusinessDays" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CashMovements_FinancialAccounts_FinancialAccountId" FOREIGN KEY ("FinancialAccountId") REFERENCES "FinancialAccounts" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CashMovements_GymBranches_BranchId" FOREIGN KEY ("BranchId") REFERENCES "GymBranches" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CashMovements_Shifts_ShiftId" FOREIGN KEY ("ShiftId") REFERENCES "Shifts" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "EmployeeAdvances" (
    "Id" uuid NOT NULL,
    "BranchId" uuid NOT NULL,
    "FinancialAccountId" uuid NOT NULL,
    "ShiftId" uuid NOT NULL,
    "BusinessDayId" uuid,
    "BusinessDate" date NOT NULL,
    "ReceivingEmployeeId" uuid NOT NULL,
    "RecordingEmployeeId" uuid NOT NULL,
    "OperationId" uuid NOT NULL,
    "RequestHash" character varying(64) NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    "Reason" text NOT NULL,
    "Reference" character varying(512),
    "OccurredAt" timestamp with time zone NOT NULL,
    "CreatedByUserId" uuid NOT NULL,
    CONSTRAINT "PK_EmployeeAdvances" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_EmployeeAdvances_Amount" CHECK ("Amount" > 0),
    CONSTRAINT "FK_EmployeeAdvances_AspNetUsers_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_EmployeeAdvances_BusinessDays_BusinessDayId" FOREIGN KEY ("BusinessDayId") REFERENCES "BusinessDays" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_EmployeeAdvances_Employees_ReceivingEmployeeId" FOREIGN KEY ("ReceivingEmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_EmployeeAdvances_Employees_RecordingEmployeeId" FOREIGN KEY ("RecordingEmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_EmployeeAdvances_FinancialAccounts_FinancialAccountId" FOREIGN KEY ("FinancialAccountId") REFERENCES "FinancialAccounts" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_EmployeeAdvances_GymBranches_BranchId" FOREIGN KEY ("BranchId") REFERENCES "GymBranches" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_EmployeeAdvances_Shifts_ShiftId" FOREIGN KEY ("ShiftId") REFERENCES "Shifts" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "ExpenseReversals" (
    "Id" uuid NOT NULL,
    "ExpenseId" uuid NOT NULL,
    "BranchId" uuid NOT NULL,
    "ShiftId" uuid NOT NULL,
    "BusinessDayId" uuid,
    "BusinessDate" date NOT NULL,
    "OperationId" uuid NOT NULL,
    "RequestHash" character varying(64) NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    "Reason" text NOT NULL,
    "ReturnReference" character varying(512),
    "OccurredAt" timestamp with time zone NOT NULL,
    "CreatedByUserId" uuid NOT NULL,
    CONSTRAINT "PK_ExpenseReversals" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_ExpenseReversals_Amount" CHECK ("Amount" > 0),
    CONSTRAINT "FK_ExpenseReversals_AspNetUsers_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ExpenseReversals_BusinessDays_BusinessDayId" FOREIGN KEY ("BusinessDayId") REFERENCES "BusinessDays" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ExpenseReversals_Expenses_ExpenseId" FOREIGN KEY ("ExpenseId") REFERENCES "Expenses" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ExpenseReversals_GymBranches_BranchId" FOREIGN KEY ("BranchId") REFERENCES "GymBranches" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ExpenseReversals_Shifts_ShiftId" FOREIGN KEY ("ShiftId") REFERENCES "Shifts" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_Shifts_BranchId_OpenedAt" ON "Shifts" ("BranchId", "OpenedAt");

CREATE UNIQUE INDEX "IX_Shifts_CloseOperationId" ON "Shifts" ("CloseOperationId");

CREATE UNIQUE INDEX "IX_Shifts_EmployeeId" ON "Shifts" ("EmployeeId") WHERE "Status" = 'Open' AND "EmployeeId" IS NOT NULL;

CREATE INDEX "IX_Shifts_FinancialAccountId" ON "Shifts" ("FinancialAccountId");

CREATE UNIQUE INDEX "IX_Shifts_OperationId" ON "Shifts" ("OperationId");

CREATE UNIQUE INDEX "IX_FinancialTransactions_BranchId_DrawerSequence" ON "FinancialTransactions" ("BranchId", "DrawerSequence") WHERE "DrawerSequence" IS NOT NULL;

CREATE INDEX "IX_FinancialTransactions_CashMovementId" ON "FinancialTransactions" ("CashMovementId");

CREATE INDEX "IX_FinancialTransactions_EmployeeAdvanceId" ON "FinancialTransactions" ("EmployeeAdvanceId");

CREATE INDEX "IX_FinancialTransactions_ShiftId" ON "FinancialTransactions" ("ShiftId");

ALTER TABLE "FinancialTransactions" ADD CONSTRAINT "CK_FinancialTransactions_Source" CHECK ((("SourceType" = 'Payment' AND "PaymentId" IS NOT NULL AND "RefundId" IS NULL AND "ExpenseId" IS NULL AND "PayrollId" IS NULL) OR ("SourceType" = 'Refund' AND "PaymentId" IS NULL AND "RefundId" IS NOT NULL AND "ExpenseId" IS NULL AND "PayrollId" IS NULL) OR ("SourceType" = 'Expense' AND "PaymentId" IS NULL AND "RefundId" IS NULL AND "ExpenseId" IS NOT NULL AND "PayrollId" IS NULL) OR ("SourceType" = 'Payroll' AND "PaymentId" IS NULL AND "RefundId" IS NULL AND "ExpenseId" IS NULL AND "PayrollId" IS NOT NULL) OR ("SourceType" = 'Adjustment' AND "PaymentId" IS NULL AND "RefundId" IS NULL AND "ExpenseId" IS NULL AND "PayrollId" IS NULL)) AND "EmployeeAdvanceId" IS NULL AND "CashMovementId" IS NULL OR ("SourceType" = 'EmployeeAdvance' AND "EmployeeAdvanceId" IS NOT NULL AND "CashMovementId" IS NULL AND "PaymentId" IS NULL AND "RefundId" IS NULL AND "ExpenseId" IS NULL AND "PayrollId" IS NULL) OR ("SourceType" = 'CashMovement' AND "CashMovementId" IS NOT NULL AND "EmployeeAdvanceId" IS NULL AND "PaymentId" IS NULL AND "RefundId" IS NULL AND "ExpenseId" IS NULL AND "PayrollId" IS NULL));

ALTER TABLE "FinancialTransactions" ADD CONSTRAINT "CK_FinancialTransactions_SourceType" CHECK ("SourceType" IN ('Payment', 'Refund', 'Expense', 'Payroll', 'Adjustment', 'EmployeeAdvance', 'CashMovement'));

CREATE INDEX "IX_Expenses_BranchId_ExpenseDate" ON "Expenses" ("BranchId", "ExpenseDate");

CREATE UNIQUE INDEX "IX_Expenses_OperationId" ON "Expenses" ("OperationId");

CREATE INDEX "IX_Expenses_RecordingEmployeeId" ON "Expenses" ("RecordingEmployeeId");

CREATE INDEX "IX_CashCounts_BranchId_MovementSequence" ON "CashCounts" ("BranchId", "MovementSequence");

CREATE INDEX "IX_CashCounts_BranchId_OccurredAt" ON "CashCounts" ("BranchId", "OccurredAt");

CREATE INDEX "IX_CashCounts_BusinessDayId" ON "CashCounts" ("BusinessDayId");

CREATE INDEX "IX_CashCounts_CreatedByUserId" ON "CashCounts" ("CreatedByUserId");

CREATE INDEX "IX_CashCounts_FinancialAccountId" ON "CashCounts" ("FinancialAccountId");

CREATE UNIQUE INDEX "IX_CashCounts_OperationId" ON "CashCounts" ("OperationId");

CREATE INDEX "IX_CashCounts_ShiftId" ON "CashCounts" ("ShiftId");

CREATE INDEX "IX_CashDrawers_CreatedByUserId" ON "CashDrawers" ("CreatedByUserId");

CREATE UNIQUE INDEX "IX_CashDrawers_FinancialAccountId" ON "CashDrawers" ("FinancialAccountId");

CREATE UNIQUE INDEX "IX_CashDrawers_OperationId" ON "CashDrawers" ("OperationId");

CREATE INDEX "IX_CashMovements_BranchId_OccurredAt" ON "CashMovements" ("BranchId", "OccurredAt");

CREATE INDEX "IX_CashMovements_BusinessDayId" ON "CashMovements" ("BusinessDayId");

CREATE INDEX "IX_CashMovements_CreatedByUserId" ON "CashMovements" ("CreatedByUserId");

CREATE INDEX "IX_CashMovements_FinancialAccountId" ON "CashMovements" ("FinancialAccountId");

CREATE UNIQUE INDEX "IX_CashMovements_OperationId" ON "CashMovements" ("OperationId");

CREATE INDEX "IX_CashMovements_ShiftId" ON "CashMovements" ("ShiftId");

CREATE INDEX "IX_EmployeeAdvances_BranchId_OccurredAt" ON "EmployeeAdvances" ("BranchId", "OccurredAt");

CREATE INDEX "IX_EmployeeAdvances_BusinessDayId" ON "EmployeeAdvances" ("BusinessDayId");

CREATE INDEX "IX_EmployeeAdvances_CreatedByUserId" ON "EmployeeAdvances" ("CreatedByUserId");

CREATE INDEX "IX_EmployeeAdvances_FinancialAccountId" ON "EmployeeAdvances" ("FinancialAccountId");

CREATE UNIQUE INDEX "IX_EmployeeAdvances_OperationId" ON "EmployeeAdvances" ("OperationId");

CREATE INDEX "IX_EmployeeAdvances_ReceivingEmployeeId" ON "EmployeeAdvances" ("ReceivingEmployeeId");

CREATE INDEX "IX_EmployeeAdvances_RecordingEmployeeId" ON "EmployeeAdvances" ("RecordingEmployeeId");

CREATE INDEX "IX_EmployeeAdvances_ShiftId" ON "EmployeeAdvances" ("ShiftId");

CREATE INDEX "IX_ExpenseReversals_BranchId_OccurredAt" ON "ExpenseReversals" ("BranchId", "OccurredAt");

CREATE INDEX "IX_ExpenseReversals_BusinessDayId" ON "ExpenseReversals" ("BusinessDayId");

CREATE INDEX "IX_ExpenseReversals_CreatedByUserId" ON "ExpenseReversals" ("CreatedByUserId");

CREATE INDEX "IX_ExpenseReversals_ExpenseId" ON "ExpenseReversals" ("ExpenseId");

CREATE UNIQUE INDEX "IX_ExpenseReversals_OperationId" ON "ExpenseReversals" ("OperationId");

CREATE INDEX "IX_ExpenseReversals_ShiftId" ON "ExpenseReversals" ("ShiftId");

ALTER TABLE "Expenses" ADD CONSTRAINT "FK_Expenses_Employees_RecordingEmployeeId" FOREIGN KEY ("RecordingEmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT;

ALTER TABLE "FinancialTransactions" ADD CONSTRAINT "FK_FinancialTransactions_CashMovements_CashMovementId" FOREIGN KEY ("CashMovementId") REFERENCES "CashMovements" ("Id") ON DELETE RESTRICT;

ALTER TABLE "FinancialTransactions" ADD CONSTRAINT "FK_FinancialTransactions_EmployeeAdvances_EmployeeAdvanceId" FOREIGN KEY ("EmployeeAdvanceId") REFERENCES "EmployeeAdvances" ("Id") ON DELETE RESTRICT;

ALTER TABLE "FinancialTransactions" ADD CONSTRAINT "FK_FinancialTransactions_Shifts_ShiftId" FOREIGN KEY ("ShiftId") REFERENCES "Shifts" ("Id") ON DELETE RESTRICT;

ALTER TABLE "Shifts" ADD CONSTRAINT "FK_Shifts_Employees_EmployeeId" FOREIGN KEY ("EmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT;

ALTER TABLE "Shifts" ADD CONSTRAINT "FK_Shifts_FinancialAccounts_FinancialAccountId" FOREIGN KEY ("FinancialAccountId") REFERENCES "FinancialAccounts" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261010225923_PrepareEmployeeShiftsAndSharedDrawer', '10.0.8');

COMMIT;

