using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesCommissionEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BelowTargetRateSnapshot",
                table: "EmployeeCommissionSummaries",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CommissionRuleId",
                table: "EmployeeCommissionSummaries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReachedTargetRateSnapshot",
                table: "EmployeeCommissionSummaries",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OperationId",
                table: "CommissionRules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestHash",
                table: "CommissionRules",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Revision",
                table: "CommissionRules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CloseOperationId",
                table: "CommissionPeriods",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CloseRequestHash",
                table: "CommissionPeriods",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EndBusinessDate",
                table: "CommissionPeriods",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OperationId",
                table: "CommissionPeriods",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestHash",
                table: "CommissionPeriods",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartBusinessDate",
                table: "CommissionPeriods",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "CommissionPeriods",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CommissionCollections",
                columns: table => new
                {
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Category = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    EligibilityReason = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionCollections", x => x.PaymentId);
                    table.CheckConstraint("CK_CommissionCollections_Category", "\"Category\" IN ('NewMembership','Renewal')");
                    table.ForeignKey(
                        name: "FK_CommissionCollections_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommissionCollections_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommissionRefundAllocations",
                columns: table => new
                {
                    RefundId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionRefundAllocations", x => new { x.RefundId, x.PaymentId });
                    table.CheckConstraint("CK_CommissionRefundAllocations_Amount", "\"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_CommissionRefundAllocations_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommissionRefundAllocations_Refunds_RefundId",
                        column: x => x.RefundId,
                        principalTable: "Refunds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommissionStatementLines",
                columns: table => new
                {
                    CommissionPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Category = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    EligibilityReason = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    GrossAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RefundedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EligibleAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionStatementLines", x => new { x.CommissionPeriodId, x.PaymentId });
                    table.CheckConstraint("CK_CommissionStatementLines_Amounts", "\"GrossAmount\" >= 0 AND \"RefundedAmount\" >= 0 AND \"RefundedAmount\" <= \"GrossAmount\" AND \"EligibleAmount\" >= 0 AND \"EligibleAmount\" <= \"GrossAmount\" - \"RefundedAmount\"");
                    table.CheckConstraint("CK_CommissionStatementLines_Category", "\"Category\" IN ('NewMembership','Renewal')");
                    table.ForeignKey(
                        name: "FK_CommissionStatementLines_CommissionPeriods_CommissionPeriod~",
                        column: x => x.CommissionPeriodId,
                        principalTable: "CommissionPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommissionStatementLines_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommissionStatementLines_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCommissionSummaries_CommissionRuleId",
                table: "EmployeeCommissionSummaries",
                column: "CommissionRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionRules_OperationId_CommissionType",
                table: "CommissionRules",
                columns: new[] { "OperationId", "CommissionType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommissionPeriods_BranchId_StartBusinessDate",
                table: "CommissionPeriods",
                columns: new[] { "BranchId", "StartBusinessDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CommissionPeriods_CloseOperationId",
                table: "CommissionPeriods",
                column: "CloseOperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommissionPeriods_OperationId",
                table: "CommissionPeriods",
                column: "OperationId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_CommissionPeriods_BusinessDates",
                table: "CommissionPeriods",
                sql: "(\"StartBusinessDate\" IS NULL AND \"EndBusinessDate\" IS NULL) OR (\"StartBusinessDate\" IS NOT NULL AND \"EndBusinessDate\" IS NOT NULL AND \"EndBusinessDate\" >= \"StartBusinessDate\")");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionCollections_EmployeeId",
                table: "CommissionCollections",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionRefundAllocations_PaymentId",
                table: "CommissionRefundAllocations",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionStatementLines_EmployeeId",
                table: "CommissionStatementLines",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionStatementLines_PaymentId",
                table: "CommissionStatementLines",
                column: "PaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeCommissionSummaries_CommissionRules_CommissionRuleId",
                table: "EmployeeCommissionSummaries",
                column: "CommissionRuleId",
                principalTable: "CommissionRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeCommissionSummaries_CommissionRules_CommissionRuleId",
                table: "EmployeeCommissionSummaries");

            migrationBuilder.DropTable(
                name: "CommissionCollections");

            migrationBuilder.DropTable(
                name: "CommissionRefundAllocations");

            migrationBuilder.DropTable(
                name: "CommissionStatementLines");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeCommissionSummaries_CommissionRuleId",
                table: "EmployeeCommissionSummaries");

            migrationBuilder.DropIndex(
                name: "IX_CommissionRules_OperationId_CommissionType",
                table: "CommissionRules");

            migrationBuilder.DropIndex(
                name: "IX_CommissionPeriods_BranchId_StartBusinessDate",
                table: "CommissionPeriods");

            migrationBuilder.DropIndex(
                name: "IX_CommissionPeriods_CloseOperationId",
                table: "CommissionPeriods");

            migrationBuilder.DropIndex(
                name: "IX_CommissionPeriods_OperationId",
                table: "CommissionPeriods");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CommissionPeriods_BusinessDates",
                table: "CommissionPeriods");

            migrationBuilder.DropColumn(
                name: "BelowTargetRateSnapshot",
                table: "EmployeeCommissionSummaries");

            migrationBuilder.DropColumn(
                name: "CommissionRuleId",
                table: "EmployeeCommissionSummaries");

            migrationBuilder.DropColumn(
                name: "ReachedTargetRateSnapshot",
                table: "EmployeeCommissionSummaries");

            migrationBuilder.DropColumn(
                name: "OperationId",
                table: "CommissionRules");

            migrationBuilder.DropColumn(
                name: "RequestHash",
                table: "CommissionRules");

            migrationBuilder.DropColumn(
                name: "Revision",
                table: "CommissionRules");

            migrationBuilder.DropColumn(
                name: "CloseOperationId",
                table: "CommissionPeriods");

            migrationBuilder.DropColumn(
                name: "CloseRequestHash",
                table: "CommissionPeriods");

            migrationBuilder.DropColumn(
                name: "EndBusinessDate",
                table: "CommissionPeriods");

            migrationBuilder.DropColumn(
                name: "OperationId",
                table: "CommissionPeriods");

            migrationBuilder.DropColumn(
                name: "RequestHash",
                table: "CommissionPeriods");

            migrationBuilder.DropColumn(
                name: "StartBusinessDate",
                table: "CommissionPeriods");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "CommissionPeriods");
        }
    }
}
