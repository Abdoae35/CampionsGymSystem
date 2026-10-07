using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FinalBusinessSchemaAlignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BusinessDayId",
                table: "Shifts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BusinessDayId",
                table: "SaleTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BusinessDayId",
                table: "Refunds",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BusinessDayId",
                table: "Payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MembershipPlanCategoryId",
                table: "MembershipPlans",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BusinessDayId",
                table: "FinancialTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BusinessDayId",
                table: "Expenses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BusinessDayId",
                table: "ClassSessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BusinessDayId",
                table: "CheckIns",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BusinessDays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OpenedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    OpenedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessDays", x => x.Id);
                    table.CheckConstraint("CK_BusinessDays_Lifecycle", "(\"Status\" = 'Open' AND \"ClosedAt\" IS NULL AND \"ClosedByUserId\" IS NULL) OR (\"Status\" = 'Closed' AND \"ClosedAt\" IS NOT NULL AND \"ClosedAt\" >= \"OpenedAt\")");
                    table.CheckConstraint("CK_BusinessDays_Status", "\"Status\" IN ('Open', 'Closed')");
                    table.ForeignKey(
                        name: "FK_BusinessDays_AspNetUsers_ClosedByUserId",
                        column: x => x.ClosedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BusinessDays_AspNetUsers_OpenedByUserId",
                        column: x => x.OpenedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BusinessDays_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeLeaveRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ToDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ManagerNotes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeLeaveRequests", x => x.Id);
                    table.CheckConstraint("CK_EmployeeLeaveRequests_Dates", "\"ToDate\" >= \"FromDate\"");
                    table.CheckConstraint("CK_EmployeeLeaveRequests_Review", "(\"Status\" = 'Pending' AND \"ReviewedAt\" IS NULL AND \"ReviewedByUserId\" IS NULL) OR (\"Status\" IN ('Approved', 'Rejected') AND \"ReviewedAt\" IS NOT NULL AND \"ReviewedAt\" >= \"CreatedAt\")");
                    table.CheckConstraint("CK_EmployeeLeaveRequests_Status", "\"Status\" IN ('Pending', 'Approved', 'Rejected')");
                    table.ForeignKey(
                        name: "FK_EmployeeLeaveRequests_AspNetUsers_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeLeaveRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MembershipPlanCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipPlanCategories", x => x.Id);
                    table.CheckConstraint("CK_MembershipPlanCategories_Name", "length(trim(\"Name\")) > 0");
                });

            migrationBuilder.CreateTable(
                name: "ShiftEmployeeChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReplacementEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftEmployeeChanges", x => x.Id);
                    table.CheckConstraint("CK_ShiftEmployeeChanges_DifferentEmployees", "\"OriginalEmployeeId\" <> \"ReplacementEmployeeId\"");
                    table.ForeignKey(
                        name: "FK_ShiftEmployeeChanges_AspNetUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShiftEmployeeChanges_Employees_OriginalEmployeeId",
                        column: x => x.OriginalEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShiftEmployeeChanges_Employees_ReplacementEmployeeId",
                        column: x => x.ReplacementEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShiftEmployeeChanges_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPermissionOverrides",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Effect = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissionOverrides", x => new { x.UserId, x.PermissionId });
                    table.CheckConstraint("CK_UserPermissionOverrides_Effect", "\"Effect\" IN ('Allow', 'Deny')");
                    table.ForeignKey(
                        name: "FK_UserPermissionOverrides_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserPermissionOverrides_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserPermissionOverrides_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_BusinessDayId",
                table: "Shifts",
                column: "BusinessDayId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleTransactions_BusinessDayId",
                table: "SaleTransactions",
                column: "BusinessDayId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_BusinessDayId",
                table: "Refunds",
                column: "BusinessDayId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BusinessDayId",
                table: "Payments",
                column: "BusinessDayId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipPlans_MembershipPlanCategoryId",
                table: "MembershipPlans",
                column: "MembershipPlanCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_BusinessDayId",
                table: "FinancialTransactions",
                column: "BusinessDayId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_BusinessDayId",
                table: "Expenses",
                column: "BusinessDayId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessions_BusinessDayId",
                table: "ClassSessions",
                column: "BusinessDayId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_BusinessDayId",
                table: "CheckIns",
                column: "BusinessDayId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessDays_BranchId",
                table: "BusinessDays",
                column: "BranchId",
                unique: true,
                filter: "\"Status\" = 'Open'");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessDays_BranchId_BusinessDate",
                table: "BusinessDays",
                columns: new[] { "BranchId", "BusinessDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessDays_ClosedByUserId",
                table: "BusinessDays",
                column: "ClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessDays_OpenedByUserId",
                table: "BusinessDays",
                column: "OpenedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveRequests_EmployeeId_FromDate_ToDate",
                table: "EmployeeLeaveRequests",
                columns: new[] { "EmployeeId", "FromDate", "ToDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveRequests_ReviewedByUserId",
                table: "EmployeeLeaveRequests",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveRequests_Status",
                table: "EmployeeLeaveRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipPlanCategories_Name",
                table: "MembershipPlanCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShiftEmployeeChanges_ChangedByUserId",
                table: "ShiftEmployeeChanges",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftEmployeeChanges_OriginalEmployeeId",
                table: "ShiftEmployeeChanges",
                column: "OriginalEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftEmployeeChanges_ReplacementEmployeeId",
                table: "ShiftEmployeeChanges",
                column: "ReplacementEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftEmployeeChanges_ShiftId_ChangedAt",
                table: "ShiftEmployeeChanges",
                columns: new[] { "ShiftId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissionOverrides_CreatedByUserId",
                table: "UserPermissionOverrides",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissionOverrides_PermissionId",
                table: "UserPermissionOverrides",
                column: "PermissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckIns_BusinessDays_BusinessDayId",
                table: "CheckIns",
                column: "BusinessDayId",
                principalTable: "BusinessDays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassSessions_BusinessDays_BusinessDayId",
                table: "ClassSessions",
                column: "BusinessDayId",
                principalTable: "BusinessDays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_BusinessDays_BusinessDayId",
                table: "Expenses",
                column: "BusinessDayId",
                principalTable: "BusinessDays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_BusinessDays_BusinessDayId",
                table: "FinancialTransactions",
                column: "BusinessDayId",
                principalTable: "BusinessDays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipPlans_MembershipPlanCategories_MembershipPlanCate~",
                table: "MembershipPlans",
                column: "MembershipPlanCategoryId",
                principalTable: "MembershipPlanCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_BusinessDays_BusinessDayId",
                table: "Payments",
                column: "BusinessDayId",
                principalTable: "BusinessDays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Refunds_BusinessDays_BusinessDayId",
                table: "Refunds",
                column: "BusinessDayId",
                principalTable: "BusinessDays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SaleTransactions_BusinessDays_BusinessDayId",
                table: "SaleTransactions",
                column: "BusinessDayId",
                principalTable: "BusinessDays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_BusinessDays_BusinessDayId",
                table: "Shifts",
                column: "BusinessDayId",
                principalTable: "BusinessDays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckIns_BusinessDays_BusinessDayId",
                table: "CheckIns");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassSessions_BusinessDays_BusinessDayId",
                table: "ClassSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_BusinessDays_BusinessDayId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_BusinessDays_BusinessDayId",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipPlans_MembershipPlanCategories_MembershipPlanCate~",
                table: "MembershipPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_BusinessDays_BusinessDayId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Refunds_BusinessDays_BusinessDayId",
                table: "Refunds");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleTransactions_BusinessDays_BusinessDayId",
                table: "SaleTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_BusinessDays_BusinessDayId",
                table: "Shifts");

            migrationBuilder.DropTable(
                name: "BusinessDays");

            migrationBuilder.DropTable(
                name: "EmployeeLeaveRequests");

            migrationBuilder.DropTable(
                name: "MembershipPlanCategories");

            migrationBuilder.DropTable(
                name: "ShiftEmployeeChanges");

            migrationBuilder.DropTable(
                name: "UserPermissionOverrides");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_BusinessDayId",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_SaleTransactions_BusinessDayId",
                table: "SaleTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Refunds_BusinessDayId",
                table: "Refunds");

            migrationBuilder.DropIndex(
                name: "IX_Payments_BusinessDayId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_MembershipPlans_MembershipPlanCategoryId",
                table: "MembershipPlans");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactions_BusinessDayId",
                table: "FinancialTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_BusinessDayId",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_ClassSessions_BusinessDayId",
                table: "ClassSessions");

            migrationBuilder.DropIndex(
                name: "IX_CheckIns_BusinessDayId",
                table: "CheckIns");

            migrationBuilder.DropColumn(
                name: "BusinessDayId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "BusinessDayId",
                table: "SaleTransactions");

            migrationBuilder.DropColumn(
                name: "BusinessDayId",
                table: "Refunds");

            migrationBuilder.DropColumn(
                name: "BusinessDayId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "MembershipPlanCategoryId",
                table: "MembershipPlans");

            migrationBuilder.DropColumn(
                name: "BusinessDayId",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "BusinessDayId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "BusinessDayId",
                table: "ClassSessions");

            migrationBuilder.DropColumn(
                name: "BusinessDayId",
                table: "CheckIns");
        }
    }
}
