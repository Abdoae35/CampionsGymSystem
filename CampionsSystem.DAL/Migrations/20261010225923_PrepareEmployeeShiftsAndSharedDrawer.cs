using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class PrepareEmployeeShiftsAndSharedDrawer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shifts_BranchId",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactions_BranchId",
                table: "FinancialTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinancialTransactions_Source",
                table: "FinancialTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinancialTransactions_SourceType",
                table: "FinancialTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_BranchId",
                table: "Expenses");

            migrationBuilder.AddColumn<Guid>(
                name: "CloseOperationId",
                table: "Shifts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CloseRequestHash",
                table: "Shifts",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ClosingBusinessDate",
                table: "Shifts",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClosingReason",
                table: "Shifts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeId",
                table: "Shifts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FinancialAccountId",
                table: "Shifts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "OpeningBusinessDate",
                table: "Shifts",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OperationId",
                table: "Shifts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestHash",
                table: "Shifts",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "Shifts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "CashMovementId",
                table: "FinancialTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DrawerSequence",
                table: "FinancialTransactions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeAdvanceId",
                table: "FinancialTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShiftId",
                table: "FinancialTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "BusinessDate",
                table: "Expenses",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OperationId",
                table: "Expenses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PaidAt",
                table: "Expenses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptReference",
                table: "Expenses",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecordingEmployeeId",
                table: "Expenses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestHash",
                table: "Expenses",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CashCounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinancialAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uuid", nullable: true),
                    BusinessDayId = table.Column<Guid>(type: "uuid", nullable: true),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Kind = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    MovementSequence = table.Column<long>(type: "bigint", nullable: false),
                    ExpectedCash = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CountedCash = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BookCash = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Difference = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashCounts", x => x.Id);
                    table.CheckConstraint("CK_CashCounts_Count", "\"CountedCash\" >= 0");
                    table.CheckConstraint("CK_CashCounts_Difference", "\"Difference\" = \"CountedCash\" - \"ExpectedCash\"");
                    table.CheckConstraint("CK_CashCounts_Kind", "\"Kind\" IN ('Initial', 'Opening', 'Closing', 'Recount')");
                    table.ForeignKey(
                        name: "FK_CashCounts_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashCounts_BusinessDays_BusinessDayId",
                        column: x => x.BusinessDayId,
                        principalTable: "BusinessDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashCounts_FinancialAccounts_FinancialAccountId",
                        column: x => x.FinancialAccountId,
                        principalTable: "FinancialAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashCounts_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashCounts_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashDrawers",
                columns: table => new
                {
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinancialAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    MovementSequence = table.Column<long>(type: "bigint", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashDrawers", x => x.BranchId);
                    table.CheckConstraint("CK_CashDrawers_Sequence", "\"MovementSequence\" >= 0");
                    table.ForeignKey(
                        name: "FK_CashDrawers_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashDrawers_FinancialAccounts_FinancialAccountId",
                        column: x => x.FinancialAccountId,
                        principalTable: "FinancialAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashDrawers_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinancialAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uuid", nullable: true),
                    BusinessDayId = table.Column<Guid>(type: "uuid", nullable: true),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Kind = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashMovements", x => x.Id);
                    table.CheckConstraint("CK_CashMovements_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_CashMovements_Kind", "\"Kind\" IN ('InitialPosition', 'Withdrawal')");
                    table.ForeignKey(
                        name: "FK_CashMovements_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashMovements_BusinessDays_BusinessDayId",
                        column: x => x.BusinessDayId,
                        principalTable: "BusinessDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashMovements_FinancialAccounts_FinancialAccountId",
                        column: x => x.FinancialAccountId,
                        principalTable: "FinancialAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashMovements_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashMovements_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeAdvances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinancialAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessDayId = table.Column<Guid>(type: "uuid", nullable: true),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ReceivingEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordingEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Reference = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeAdvances", x => x.Id);
                    table.CheckConstraint("CK_EmployeeAdvances_Amount", "\"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_EmployeeAdvances_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeAdvances_BusinessDays_BusinessDayId",
                        column: x => x.BusinessDayId,
                        principalTable: "BusinessDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeAdvances_Employees_ReceivingEmployeeId",
                        column: x => x.ReceivingEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeAdvances_Employees_RecordingEmployeeId",
                        column: x => x.RecordingEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeAdvances_FinancialAccounts_FinancialAccountId",
                        column: x => x.FinancialAccountId,
                        principalTable: "FinancialAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeAdvances_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeAdvances_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseReversals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpenseId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessDayId = table.Column<Guid>(type: "uuid", nullable: true),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    ReturnReference = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseReversals", x => x.Id);
                    table.CheckConstraint("CK_ExpenseReversals_Amount", "\"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_ExpenseReversals_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpenseReversals_BusinessDays_BusinessDayId",
                        column: x => x.BusinessDayId,
                        principalTable: "BusinessDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpenseReversals_Expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "Expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpenseReversals_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpenseReversals_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_BranchId_OpenedAt",
                table: "Shifts",
                columns: new[] { "BranchId", "OpenedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_CloseOperationId",
                table: "Shifts",
                column: "CloseOperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_EmployeeId",
                table: "Shifts",
                column: "EmployeeId",
                unique: true,
                filter: "\"Status\" = 'Open' AND \"EmployeeId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_FinancialAccountId",
                table: "Shifts",
                column: "FinancialAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_OperationId",
                table: "Shifts",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_BranchId_DrawerSequence",
                table: "FinancialTransactions",
                columns: new[] { "BranchId", "DrawerSequence" },
                unique: true,
                filter: "\"DrawerSequence\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_CashMovementId",
                table: "FinancialTransactions",
                column: "CashMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_EmployeeAdvanceId",
                table: "FinancialTransactions",
                column: "EmployeeAdvanceId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_ShiftId",
                table: "FinancialTransactions",
                column: "ShiftId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinancialTransactions_Source",
                table: "FinancialTransactions",
                sql: "((\"SourceType\" = 'Payment' AND \"PaymentId\" IS NOT NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NULL) OR (\"SourceType\" = 'Refund' AND \"PaymentId\" IS NULL AND \"RefundId\" IS NOT NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NULL) OR (\"SourceType\" = 'Expense' AND \"PaymentId\" IS NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NOT NULL AND \"PayrollId\" IS NULL) OR (\"SourceType\" = 'Payroll' AND \"PaymentId\" IS NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NOT NULL) OR (\"SourceType\" = 'Adjustment' AND \"PaymentId\" IS NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NULL)) AND \"EmployeeAdvanceId\" IS NULL AND \"CashMovementId\" IS NULL OR (\"SourceType\" = 'EmployeeAdvance' AND \"EmployeeAdvanceId\" IS NOT NULL AND \"CashMovementId\" IS NULL AND \"PaymentId\" IS NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NULL) OR (\"SourceType\" = 'CashMovement' AND \"CashMovementId\" IS NOT NULL AND \"EmployeeAdvanceId\" IS NULL AND \"PaymentId\" IS NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinancialTransactions_SourceType",
                table: "FinancialTransactions",
                sql: "\"SourceType\" IN ('Payment', 'Refund', 'Expense', 'Payroll', 'Adjustment', 'EmployeeAdvance', 'CashMovement')");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_BranchId_ExpenseDate",
                table: "Expenses",
                columns: new[] { "BranchId", "ExpenseDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_OperationId",
                table: "Expenses",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_RecordingEmployeeId",
                table: "Expenses",
                column: "RecordingEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_CashCounts_BranchId_MovementSequence",
                table: "CashCounts",
                columns: new[] { "BranchId", "MovementSequence" });

            migrationBuilder.CreateIndex(
                name: "IX_CashCounts_BranchId_OccurredAt",
                table: "CashCounts",
                columns: new[] { "BranchId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CashCounts_BusinessDayId",
                table: "CashCounts",
                column: "BusinessDayId");

            migrationBuilder.CreateIndex(
                name: "IX_CashCounts_CreatedByUserId",
                table: "CashCounts",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CashCounts_FinancialAccountId",
                table: "CashCounts",
                column: "FinancialAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CashCounts_OperationId",
                table: "CashCounts",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashCounts_ShiftId",
                table: "CashCounts",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_CashDrawers_CreatedByUserId",
                table: "CashDrawers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CashDrawers_FinancialAccountId",
                table: "CashDrawers",
                column: "FinancialAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashDrawers_OperationId",
                table: "CashDrawers",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashMovements_BranchId_OccurredAt",
                table: "CashMovements",
                columns: new[] { "BranchId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CashMovements_BusinessDayId",
                table: "CashMovements",
                column: "BusinessDayId");

            migrationBuilder.CreateIndex(
                name: "IX_CashMovements_CreatedByUserId",
                table: "CashMovements",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CashMovements_FinancialAccountId",
                table: "CashMovements",
                column: "FinancialAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CashMovements_OperationId",
                table: "CashMovements",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashMovements_ShiftId",
                table: "CashMovements",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAdvances_BranchId_OccurredAt",
                table: "EmployeeAdvances",
                columns: new[] { "BranchId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAdvances_BusinessDayId",
                table: "EmployeeAdvances",
                column: "BusinessDayId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAdvances_CreatedByUserId",
                table: "EmployeeAdvances",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAdvances_FinancialAccountId",
                table: "EmployeeAdvances",
                column: "FinancialAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAdvances_OperationId",
                table: "EmployeeAdvances",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAdvances_ReceivingEmployeeId",
                table: "EmployeeAdvances",
                column: "ReceivingEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAdvances_RecordingEmployeeId",
                table: "EmployeeAdvances",
                column: "RecordingEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAdvances_ShiftId",
                table: "EmployeeAdvances",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseReversals_BranchId_OccurredAt",
                table: "ExpenseReversals",
                columns: new[] { "BranchId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseReversals_BusinessDayId",
                table: "ExpenseReversals",
                column: "BusinessDayId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseReversals_CreatedByUserId",
                table: "ExpenseReversals",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseReversals_ExpenseId",
                table: "ExpenseReversals",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseReversals_OperationId",
                table: "ExpenseReversals",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseReversals_ShiftId",
                table: "ExpenseReversals",
                column: "ShiftId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Employees_RecordingEmployeeId",
                table: "Expenses",
                column: "RecordingEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_CashMovements_CashMovementId",
                table: "FinancialTransactions",
                column: "CashMovementId",
                principalTable: "CashMovements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_EmployeeAdvances_EmployeeAdvanceId",
                table: "FinancialTransactions",
                column: "EmployeeAdvanceId",
                principalTable: "EmployeeAdvances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_Shifts_ShiftId",
                table: "FinancialTransactions",
                column: "ShiftId",
                principalTable: "Shifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Employees_EmployeeId",
                table: "Shifts",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_FinancialAccounts_FinancialAccountId",
                table: "Shifts",
                column: "FinancialAccountId",
                principalTable: "FinancialAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Employees_RecordingEmployeeId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_CashMovements_CashMovementId",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_EmployeeAdvances_EmployeeAdvanceId",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_Shifts_ShiftId",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Employees_EmployeeId",
                table: "Shifts");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_FinancialAccounts_FinancialAccountId",
                table: "Shifts");

            migrationBuilder.DropTable(
                name: "CashCounts");

            migrationBuilder.DropTable(
                name: "CashDrawers");

            migrationBuilder.DropTable(
                name: "CashMovements");

            migrationBuilder.DropTable(
                name: "EmployeeAdvances");

            migrationBuilder.DropTable(
                name: "ExpenseReversals");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_BranchId_OpenedAt",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_CloseOperationId",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_EmployeeId",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_FinancialAccountId",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_OperationId",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactions_BranchId_DrawerSequence",
                table: "FinancialTransactions");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactions_CashMovementId",
                table: "FinancialTransactions");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactions_EmployeeAdvanceId",
                table: "FinancialTransactions");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactions_ShiftId",
                table: "FinancialTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinancialTransactions_Source",
                table: "FinancialTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinancialTransactions_SourceType",
                table: "FinancialTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_BranchId_ExpenseDate",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_OperationId",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_RecordingEmployeeId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "CloseOperationId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "CloseRequestHash",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "ClosingBusinessDate",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "ClosingReason",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "FinancialAccountId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "OpeningBusinessDate",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "OperationId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "RequestHash",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "CashMovementId",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "DrawerSequence",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "EmployeeAdvanceId",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "ShiftId",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "BusinessDate",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "OperationId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "ReceiptReference",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "RecordingEmployeeId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "RequestHash",
                table: "Expenses");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_BranchId",
                table: "Shifts",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_BranchId",
                table: "FinancialTransactions",
                column: "BranchId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinancialTransactions_Source",
                table: "FinancialTransactions",
                sql: "(\"SourceType\" = 'Payment' AND \"PaymentId\" IS NOT NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NULL) OR (\"SourceType\" = 'Refund' AND \"PaymentId\" IS NULL AND \"RefundId\" IS NOT NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NULL) OR (\"SourceType\" = 'Expense' AND \"PaymentId\" IS NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NOT NULL AND \"PayrollId\" IS NULL) OR (\"SourceType\" = 'Payroll' AND \"PaymentId\" IS NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NOT NULL) OR (\"SourceType\" = 'Adjustment' AND \"PaymentId\" IS NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinancialTransactions_SourceType",
                table: "FinancialTransactions",
                sql: "\"SourceType\" IN ('Payment', 'Refund', 'Expense', 'Payroll', 'Adjustment')");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_BranchId",
                table: "Expenses",
                column: "BranchId");
        }
    }
}
