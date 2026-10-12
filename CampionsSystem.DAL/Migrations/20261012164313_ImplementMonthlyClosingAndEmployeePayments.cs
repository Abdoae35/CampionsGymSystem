using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ImplementMonthlyClosingAndEmployeePayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
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
                """);
            migrationBuilder.DropIndex(
                name: "IX_EmployeeSalarySettings_EmployeeId",
                table: "EmployeeSalarySettings");

            migrationBuilder.AlterColumn<Guid>(
                name: "CommissionPeriodId",
                table: "Payrolls",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "CommissionSummaryId",
                table: "Payrolls",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FinancialMonthId",
                table: "Payrolls",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecognizedByUserId",
                table: "Payrolls",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalaryEvidenceJson",
                table: "Payrolls",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeAdvanceId",
                table: "PayrollDeductions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FinancialMonthId",
                table: "FinancialTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PostedAt",
                table: "FinancialTransactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OperationId",
                table: "EmployeeSalarySettings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestHash",
                table: "EmployeeSalarySettings",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BasicEntitlementSnapshot",
                table: "EmployeeAdvances",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FinancialMonthId",
                table: "EmployeeAdvances",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FinancialMonthId",
                table: "CoachEarnings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FinancialMonths",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    LastRevision = table.Column<int>(type: "integer", nullable: false),
                    FirstClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialMonths", x => x.Id);
                    table.CheckConstraint("CK_FinancialMonths_Bounds", "\"EndDate\" >= \"StartDate\" AND \"EndDate\" - \"StartDate\" < 62");
                    table.CheckConstraint("CK_FinancialMonths_Status", "\"Status\" IN ('Open','Closed','Reopened') AND \"Version\" >= 0 AND \"LastRevision\" >= 0 AND (\"Status\" = 'Open' AND \"FirstClosedAt\" IS NULL OR \"Status\" <> 'Open' AND \"FirstClosedAt\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_FinancialMonths_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialMonths_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinancialOccurrenceEvidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FinancialTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OriginalOccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialOccurrenceEvidence", x => x.Id);
                    table.CheckConstraint("CK_FinancialOccurrenceEvidence_Date", "\"OriginalOccurredAt\" <= \"RecordedAt\"");
                    table.ForeignKey(
                        name: "FK_FinancialOccurrenceEvidence_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialOccurrenceEvidence_FinancialTransactions_Financial~",
                        column: x => x.FinancialTransactionId,
                        principalTable: "FinancialTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaintenancePlanRevisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExpectedCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FrequencyMonths = table.Column<int>(type: "integer", nullable: false),
                    FirstDueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenancePlanRevisions", x => x.Id);
                    table.CheckConstraint("CK_MaintenancePlanRevisions_Values", "\"ExpectedCost\" > 0 AND \"FrequencyMonths\" BETWEEN 1 AND 120 AND \"Revision\" > 0");
                    table.ForeignKey(
                        name: "FK_MaintenancePlanRevisions_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenancePlanRevisions_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinancialClosingRevisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FinancialMonthId = table.Column<Guid>(type: "uuid", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    SchemaVersion = table.Column<int>(type: "integer", nullable: false),
                    SnapshotAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ReportJson = table.Column<string>(type: "jsonb", nullable: false),
                    Workbook = table.Column<byte[]>(type: "bytea", nullable: false),
                    WorkbookSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialClosingRevisions", x => x.Id);
                    table.CheckConstraint("CK_FinancialClosingRevisions_Version", "\"Revision\" > 0 AND \"SchemaVersion\" > 0");
                    table.ForeignKey(
                        name: "FK_FinancialClosingRevisions_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialClosingRevisions_FinancialMonths_FinancialMonthId",
                        column: x => x.FinancialMonthId,
                        principalTable: "FinancialMonths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinancialMonthEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FinancialMonthId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Kind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ResultVersion = table.Column<int>(type: "integer", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialMonthEvents", x => x.Id);
                    table.CheckConstraint("CK_FinancialMonthEvents_Kind", "\"Kind\" IN ('Opened','Closed','Reopened') AND \"ResultVersion\" >= 0");
                    table.ForeignKey(
                        name: "FK_FinancialMonthEvents_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialMonthEvents_FinancialMonths_FinancialMonthId",
                        column: x => x.FinancialMonthId,
                        principalTable: "FinancialMonths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollPaymentRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinancialMonthId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Reference = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollPaymentRecords", x => x.Id);
                    table.CheckConstraint("CK_PayrollPaymentRecords_Balance", "\"Amount\" > 0 AND \"BalanceBefore\" >= \"Amount\" AND \"BalanceAfter\" = \"BalanceBefore\" - \"Amount\" AND \"PaidAt\" <= \"RecordedAt\"");
                    table.ForeignKey(
                        name: "FK_PayrollPaymentRecords_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollPaymentRecords_FinancialMonths_FinancialMonthId",
                        column: x => x.FinancialMonthId,
                        principalTable: "FinancialMonths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollPaymentRecords_Payrolls_PayrollId",
                        column: x => x.PayrollId,
                        principalTable: "Payrolls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payrolls_CommissionSummaryId",
                table: "Payrolls",
                column: "CommissionSummaryId",
                unique: true,
                filter: "\"CommissionSummaryId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Payrolls_FinancialMonthId_EmployeeId",
                table: "Payrolls",
                columns: new[] { "FinancialMonthId", "EmployeeId" },
                unique: true,
                filter: "\"FinancialMonthId\" IS NOT NULL AND \"CommissionSummaryId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Payrolls_RecognizedByUserId",
                table: "Payrolls",
                column: "RecognizedByUserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Payrolls_Recognized",
                table: "Payrolls",
                sql: "\"FinancialMonthId\" IS NULL OR (\"RecognizedByUserId\" IS NOT NULL AND \"BonusAmount\" = 0 AND \"NetAmount\" = \"BasicSalary\" + \"CommissionAmount\" - \"DeductionAmount\" AND (\"CommissionSummaryId\" IS NULL AND \"CommissionAmount\" = 0 AND \"SalaryEvidenceJson\" IS NOT NULL OR \"CommissionSummaryId\" IS NOT NULL AND \"CommissionPeriodId\" IS NOT NULL AND \"BasicSalary\" = 0 AND \"DeductionAmount\" = 0))");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollDeductions_EmployeeAdvanceId",
                table: "PayrollDeductions",
                column: "EmployeeAdvanceId",
                unique: true,
                filter: "\"EmployeeAdvanceId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_FinancialMonthId_OccurredAt_Id",
                table: "FinancialTransactions",
                columns: new[] { "FinancialMonthId", "OccurredAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalarySettings_EmployeeId_EffectiveFrom",
                table: "EmployeeSalarySettings",
                columns: new[] { "EmployeeId", "EffectiveFrom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalarySettings_OperationId",
                table: "EmployeeSalarySettings",
                column: "OperationId",
                unique: true,
                filter: "\"OperationId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAdvances_FinancialMonthId_ReceivingEmployeeId",
                table: "EmployeeAdvances",
                columns: new[] { "FinancialMonthId", "ReceivingEmployeeId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_EmployeeAdvances_Entitlement",
                table: "EmployeeAdvances",
                sql: "\"FinancialMonthId\" IS NULL AND \"BasicEntitlementSnapshot\" IS NULL OR \"FinancialMonthId\" IS NOT NULL AND \"BasicEntitlementSnapshot\" IS NOT NULL AND \"BasicEntitlementSnapshot\" >= \"Amount\"");

            migrationBuilder.CreateIndex(
                name: "IX_CoachEarnings_FinancialMonthId",
                table: "CoachEarnings",
                column: "FinancialMonthId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialClosingRevisions_ActorUserId",
                table: "FinancialClosingRevisions",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialClosingRevisions_FinancialMonthId_Revision",
                table: "FinancialClosingRevisions",
                columns: new[] { "FinancialMonthId", "Revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialMonthEvents_ActorUserId",
                table: "FinancialMonthEvents",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialMonthEvents_FinancialMonthId",
                table: "FinancialMonthEvents",
                column: "FinancialMonthId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialMonthEvents_OperationId",
                table: "FinancialMonthEvents",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialMonths_BranchId",
                table: "FinancialMonths",
                column: "BranchId",
                unique: true,
                filter: "\"Status\" = 'Open'");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialMonths_BranchId_StartDate",
                table: "FinancialMonths",
                columns: new[] { "BranchId", "StartDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialMonths_CreatedByUserId",
                table: "FinancialMonths",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialOccurrenceEvidence_ActorUserId",
                table: "FinancialOccurrenceEvidence",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialOccurrenceEvidence_FinancialTransactionId",
                table: "FinancialOccurrenceEvidence",
                column: "FinancialTransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialOccurrenceEvidence_OperationId",
                table: "FinancialOccurrenceEvidence",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenancePlanRevisions_ActorUserId",
                table: "MaintenancePlanRevisions",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenancePlanRevisions_BranchId",
                table: "MaintenancePlanRevisions",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenancePlanRevisions_OperationId",
                table: "MaintenancePlanRevisions",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenancePlanRevisions_PlanId_Revision",
                table: "MaintenancePlanRevisions",
                columns: new[] { "PlanId", "Revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPaymentRecords_ActorUserId",
                table: "PayrollPaymentRecords",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPaymentRecords_FinancialMonthId",
                table: "PayrollPaymentRecords",
                column: "FinancialMonthId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPaymentRecords_OperationId",
                table: "PayrollPaymentRecords",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPaymentRecords_PayrollId_RecordedAt",
                table: "PayrollPaymentRecords",
                columns: new[] { "PayrollId", "RecordedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_CoachEarnings_FinancialMonths_FinancialMonthId",
                table: "CoachEarnings",
                column: "FinancialMonthId",
                principalTable: "FinancialMonths",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAdvances_FinancialMonths_FinancialMonthId",
                table: "EmployeeAdvances",
                column: "FinancialMonthId",
                principalTable: "FinancialMonths",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_FinancialMonths_FinancialMonthId",
                table: "FinancialTransactions",
                column: "FinancialMonthId",
                principalTable: "FinancialMonths",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollDeductions_EmployeeAdvances_EmployeeAdvanceId",
                table: "PayrollDeductions",
                column: "EmployeeAdvanceId",
                principalTable: "EmployeeAdvances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payrolls_AspNetUsers_RecognizedByUserId",
                table: "Payrolls",
                column: "RecognizedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payrolls_EmployeeCommissionSummaries_CommissionSummaryId",
                table: "Payrolls",
                column: "CommissionSummaryId",
                principalTable: "EmployeeCommissionSummaries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payrolls_FinancialMonths_FinancialMonthId",
                table: "Payrolls",
                column: "FinancialMonthId",
                principalTable: "FinancialMonths",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoachEarnings_FinancialMonths_FinancialMonthId",
                table: "CoachEarnings");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAdvances_FinancialMonths_FinancialMonthId",
                table: "EmployeeAdvances");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_FinancialMonths_FinancialMonthId",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PayrollDeductions_EmployeeAdvances_EmployeeAdvanceId",
                table: "PayrollDeductions");

            migrationBuilder.DropForeignKey(
                name: "FK_Payrolls_AspNetUsers_RecognizedByUserId",
                table: "Payrolls");

            migrationBuilder.DropForeignKey(
                name: "FK_Payrolls_EmployeeCommissionSummaries_CommissionSummaryId",
                table: "Payrolls");

            migrationBuilder.DropForeignKey(
                name: "FK_Payrolls_FinancialMonths_FinancialMonthId",
                table: "Payrolls");

            migrationBuilder.DropTable(
                name: "FinancialClosingRevisions");

            migrationBuilder.DropTable(
                name: "FinancialMonthEvents");

            migrationBuilder.DropTable(
                name: "FinancialOccurrenceEvidence");

            migrationBuilder.DropTable(
                name: "MaintenancePlanRevisions");

            migrationBuilder.DropTable(
                name: "PayrollPaymentRecords");

            migrationBuilder.DropTable(
                name: "FinancialMonths");

            migrationBuilder.DropIndex(
                name: "IX_Payrolls_CommissionSummaryId",
                table: "Payrolls");

            migrationBuilder.DropIndex(
                name: "IX_Payrolls_FinancialMonthId_EmployeeId",
                table: "Payrolls");

            migrationBuilder.DropIndex(
                name: "IX_Payrolls_RecognizedByUserId",
                table: "Payrolls");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Payrolls_Recognized",
                table: "Payrolls");

            migrationBuilder.DropIndex(
                name: "IX_PayrollDeductions_EmployeeAdvanceId",
                table: "PayrollDeductions");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactions_FinancialMonthId_OccurredAt_Id",
                table: "FinancialTransactions");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeSalarySettings_EmployeeId_EffectiveFrom",
                table: "EmployeeSalarySettings");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeSalarySettings_OperationId",
                table: "EmployeeSalarySettings");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeAdvances_FinancialMonthId_ReceivingEmployeeId",
                table: "EmployeeAdvances");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EmployeeAdvances_Entitlement",
                table: "EmployeeAdvances");

            migrationBuilder.DropIndex(
                name: "IX_CoachEarnings_FinancialMonthId",
                table: "CoachEarnings");

            migrationBuilder.DropColumn(
                name: "CommissionSummaryId",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "FinancialMonthId",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "RecognizedByUserId",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "SalaryEvidenceJson",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "EmployeeAdvanceId",
                table: "PayrollDeductions");

            migrationBuilder.DropColumn(
                name: "FinancialMonthId",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "PostedAt",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "OperationId",
                table: "EmployeeSalarySettings");

            migrationBuilder.DropColumn(
                name: "RequestHash",
                table: "EmployeeSalarySettings");

            migrationBuilder.DropColumn(
                name: "BasicEntitlementSnapshot",
                table: "EmployeeAdvances");

            migrationBuilder.DropColumn(
                name: "FinancialMonthId",
                table: "EmployeeAdvances");

            migrationBuilder.DropColumn(
                name: "FinancialMonthId",
                table: "CoachEarnings");

            migrationBuilder.AlterColumn<Guid>(
                name: "CommissionPeriodId",
                table: "Payrolls",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalarySettings_EmployeeId",
                table: "EmployeeSalarySettings",
                column: "EmployeeId");
        }
    }
}
