using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateActivitiesClassesAndFreelanceRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ActivityCoachAssignments_SportsActivityId",
                table: "ActivityCoachAssignments");

            migrationBuilder.AddColumn<string>(
                name: "SalesAttributionSnapshot",
                table: "SaleTransactions",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClassSessionId",
                table: "CheckIns",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ActivityCoachIdSnapshot",
                table: "ActivitySubscriptionTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CoachPercentageSnapshot",
                table: "ActivitySubscriptionTransactions",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ActivityCoachId",
                table: "ActivitySubscriptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CoachEmployeeId",
                table: "ActivityCoachAssignments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "ActivityCoachId",
                table: "ActivityCoachAssignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ActivityCoaches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExternalName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ExternalPhone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityCoaches", x => x.Id);
                    table.CheckConstraint("CK_ActivityCoaches_Percentage", "\"Percentage\" BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_ActivityCoaches_Type", "(\"CoachType\" = 'Internal' AND \"EmployeeId\" IS NOT NULL AND \"ExternalName\" IS NULL AND \"ExternalPhone\" IS NULL) OR (\"CoachType\" = 'External' AND \"EmployeeId\" IS NULL AND length(trim(\"ExternalName\")) > 0 AND \"ExternalName\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_ActivityCoaches_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassScheduleId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassSessions", x => x.Id);
                    table.CheckConstraint("CK_ClassSessions_Status", "\"Status\" IN ('Scheduled', 'Completed', 'Cancelled')");
                    table.CheckConstraint("CK_ClassSessions_Times", "(\"EndedAt\" IS NULL OR \"EndedAt\" >= \"StartedAt\") AND (\"Status\" <> 'Completed' OR \"EndedAt\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_ClassSessions_ClassSchedules_ClassScheduleId",
                        column: x => x.ClassScheduleId,
                        principalTable: "ClassSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassSessions_Employees_CoachEmployeeId",
                        column: x => x.CoachEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassSessions_GroupClasses_GroupClassId",
                        column: x => x.GroupClassId,
                        principalTable: "GroupClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassSessions_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MembershipPlanEligibleMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MembershipPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipPlanEligibleMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembershipPlanEligibleMembers_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MembershipPlanEligibleMembers_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MembershipPlanEligibleMembers_MembershipPlans_MembershipPla~",
                        column: x => x.MembershipPlanId,
                        principalTable: "MembershipPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActivityCoachSettlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommissionPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityCoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    SportsActivityId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    CoachTypeSnapshot = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CoachNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ActivityNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SubscribersCount = table.Column<int>(type: "integer", nullable: false),
                    EligibleRevenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Entitlement = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalizedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityCoachSettlements", x => x.Id);
                    table.CheckConstraint("CK_ActivityCoachSettlements_Amounts", "\"EligibleRevenue\" >= 0 AND \"Entitlement\" >= 0 AND \"SubscribersCount\" >= 0 AND \"Percentage\" BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_ActivityCoachSettlements_Employee", "(\"CoachTypeSnapshot\" = 'Internal' AND \"EmployeeId\" IS NOT NULL) OR (\"CoachTypeSnapshot\" = 'External' AND \"EmployeeId\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_ActivityCoachSettlements_ActivityCoaches_ActivityCoachId",
                        column: x => x.ActivityCoachId,
                        principalTable: "ActivityCoaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityCoachSettlements_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityCoachSettlements_CommissionPeriods_CommissionPeriod~",
                        column: x => x.CommissionPeriodId,
                        principalTable: "CommissionPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityCoachSettlements_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityCoachSettlements_SportsActivities_SportsActivityId",
                        column: x => x.SportsActivityId,
                        principalTable: "SportsActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_SaleTransactions_Attribution",
                table: "SaleTransactions",
                sql: "\"SalesAttributionSnapshot\" IS NULL OR (\"SalesAttributionSnapshot\" = 'House' AND \"SalesEmployeeId\" IS NULL AND \"SaleCategory\" IS NULL) OR (\"SalesAttributionSnapshot\" = 'Sales' AND \"SalesEmployeeId\" IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_ClassSessionId_MemberId",
                table: "CheckIns",
                columns: new[] { "ClassSessionId", "MemberId" },
                unique: true,
                filter: "\"ClassSessionId\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CheckIns_ClassSession",
                table: "CheckIns",
                sql: "\"ClassSessionId\" IS NULL OR \"CheckInType\" = 'Class'");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySubscriptionTransactions_ActivityCoachIdSnapshot",
                table: "ActivitySubscriptionTransactions",
                column: "ActivityCoachIdSnapshot");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ActivitySubscriptionTransactions_CoachSnapshot",
                table: "ActivitySubscriptionTransactions",
                sql: "(\"ActivityCoachIdSnapshot\" IS NULL AND \"CoachPercentageSnapshot\" IS NULL) OR (\"ActivityCoachIdSnapshot\" IS NOT NULL AND \"CoachPercentageSnapshot\" IS NOT NULL AND \"CoachPercentageSnapshot\" BETWEEN 0 AND 100)");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySubscriptions_ActivityCoachId_CreatedAt",
                table: "ActivitySubscriptions",
                columns: new[] { "ActivityCoachId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCoachAssignments_ActivityCoachId",
                table: "ActivityCoachAssignments",
                column: "ActivityCoachId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCoachAssignments_SportsActivityId_ActivityCoachId_S~",
                table: "ActivityCoachAssignments",
                columns: new[] { "SportsActivityId", "ActivityCoachId", "StartDate" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ActivityCoachAssignments_Coach",
                table: "ActivityCoachAssignments",
                sql: "(\"ActivityCoachId\" IS NOT NULL AND \"CoachEmployeeId\" IS NULL) OR (\"ActivityCoachId\" IS NULL AND \"CoachEmployeeId\" IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCoaches_EmployeeId",
                table: "ActivityCoaches",
                column: "EmployeeId",
                unique: true,
                filter: "\"EmployeeId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCoachSettlements_ActivityCoachId",
                table: "ActivityCoachSettlements",
                column: "ActivityCoachId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCoachSettlements_CommissionPeriodId_ActivityCoachId~",
                table: "ActivityCoachSettlements",
                columns: new[] { "CommissionPeriodId", "ActivityCoachId", "SportsActivityId", "Percentage" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCoachSettlements_CreatedByUserId",
                table: "ActivityCoachSettlements",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCoachSettlements_EmployeeId",
                table: "ActivityCoachSettlements",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCoachSettlements_SportsActivityId",
                table: "ActivityCoachSettlements",
                column: "SportsActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessions_BranchId_StartedAt",
                table: "ClassSessions",
                columns: new[] { "BranchId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessions_ClassScheduleId",
                table: "ClassSessions",
                column: "ClassScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessions_CoachEmployeeId_StartedAt",
                table: "ClassSessions",
                columns: new[] { "CoachEmployeeId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessions_GroupClassId_StartedAt",
                table: "ClassSessions",
                columns: new[] { "GroupClassId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MembershipPlanEligibleMembers_CreatedByUserId",
                table: "MembershipPlanEligibleMembers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipPlanEligibleMembers_MemberId",
                table: "MembershipPlanEligibleMembers",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipPlanEligibleMembers_MembershipPlanId_MemberId",
                table: "MembershipPlanEligibleMembers",
                columns: new[] { "MembershipPlanId", "MemberId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityCoachAssignments_ActivityCoaches_ActivityCoachId",
                table: "ActivityCoachAssignments",
                column: "ActivityCoachId",
                principalTable: "ActivityCoaches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ActivitySubscriptions_ActivityCoaches_ActivityCoachId",
                table: "ActivitySubscriptions",
                column: "ActivityCoachId",
                principalTable: "ActivityCoaches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ActivitySubscriptionTransactions_ActivityCoaches_ActivityCo~",
                table: "ActivitySubscriptionTransactions",
                column: "ActivityCoachIdSnapshot",
                principalTable: "ActivityCoaches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CheckIns_ClassSessions_ClassSessionId",
                table: "CheckIns",
                column: "ClassSessionId",
                principalTable: "ClassSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            // Add feature permissions and Owner grants without creating users or changing other grants.
            migrationBuilder.Sql("""
                INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('fd77a297-006f-5f22-81d4-7d8b8716ca4b', 'Activities.ManageCoaches', 'Activities.ManageCoaches', 'Activities', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
                INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('ac5ac767-448b-5edf-bfa5-4ae7beafa5fa', 'Activities.ViewCoachRevenue', 'Activities.ViewCoachRevenue', 'Activities', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
                INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('5152272e-0e95-53b8-a799-bbb9848df3f6', 'Classes.View', 'Classes.View', 'Classes', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
                INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('d6218055-793c-5802-96df-30823241a8bb', 'Classes.Manage', 'Classes.Manage', 'Classes', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
                INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('b43efd0e-04a7-5ede-8d89-465057b0913e', 'Classes.ViewAttendanceReport', 'Classes.ViewAttendanceReport', 'Classes', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
                INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('82f51aa3-84dd-57c8-bad2-1816f227cfd7', 'FreelancePlans.View', 'FreelancePlans.View', 'FreelancePlans', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
                INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('047a26aa-9a56-54db-a40c-8968563fc2dd', 'FreelancePlans.Manage', 'FreelancePlans.Manage', 'FreelancePlans', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
                INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('0bdc7160-c718-57cd-809d-f38f26e24e4e', 'FreelancePlans.ManageEligibleMembers', 'FreelancePlans.ManageEligibleMembers', 'FreelancePlans', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
                INSERT INTO "RolePermissions" ("RoleId", "PermissionId") SELECT r."Id", p."Id" FROM "AspNetRoles" r CROSS JOIN "Permissions" p WHERE r."NormalizedName" = 'OWNER' AND p."Code" IN ('Activities.ManageCoaches', 'Activities.ViewCoachRevenue', 'Classes.View', 'Classes.Manage', 'Classes.ViewAttendanceReport', 'FreelancePlans.View', 'FreelancePlans.Manage', 'FreelancePlans.ManageEligibleMembers') ON CONFLICT DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityCoachAssignments_ActivityCoaches_ActivityCoachId",
                table: "ActivityCoachAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ActivitySubscriptions_ActivityCoaches_ActivityCoachId",
                table: "ActivitySubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_ActivitySubscriptionTransactions_ActivityCoaches_ActivityCo~",
                table: "ActivitySubscriptionTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_CheckIns_ClassSessions_ClassSessionId",
                table: "CheckIns");

            migrationBuilder.DropTable(
                name: "ActivityCoachSettlements");

            migrationBuilder.DropTable(
                name: "ClassSessions");

            migrationBuilder.DropTable(
                name: "MembershipPlanEligibleMembers");

            migrationBuilder.DropTable(
                name: "ActivityCoaches");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SaleTransactions_Attribution",
                table: "SaleTransactions");

            migrationBuilder.DropIndex(
                name: "IX_CheckIns_ClassSessionId_MemberId",
                table: "CheckIns");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CheckIns_ClassSession",
                table: "CheckIns");

            migrationBuilder.DropIndex(
                name: "IX_ActivitySubscriptionTransactions_ActivityCoachIdSnapshot",
                table: "ActivitySubscriptionTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ActivitySubscriptionTransactions_CoachSnapshot",
                table: "ActivitySubscriptionTransactions");

            migrationBuilder.DropIndex(
                name: "IX_ActivitySubscriptions_ActivityCoachId_CreatedAt",
                table: "ActivitySubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_ActivityCoachAssignments_ActivityCoachId",
                table: "ActivityCoachAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ActivityCoachAssignments_SportsActivityId_ActivityCoachId_S~",
                table: "ActivityCoachAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ActivityCoachAssignments_Coach",
                table: "ActivityCoachAssignments");

            migrationBuilder.DropColumn(
                name: "SalesAttributionSnapshot",
                table: "SaleTransactions");

            migrationBuilder.DropColumn(
                name: "ClassSessionId",
                table: "CheckIns");

            migrationBuilder.DropColumn(
                name: "ActivityCoachIdSnapshot",
                table: "ActivitySubscriptionTransactions");

            migrationBuilder.DropColumn(
                name: "CoachPercentageSnapshot",
                table: "ActivitySubscriptionTransactions");

            migrationBuilder.DropColumn(
                name: "ActivityCoachId",
                table: "ActivitySubscriptions");

            migrationBuilder.DropColumn(
                name: "ActivityCoachId",
                table: "ActivityCoachAssignments");

            migrationBuilder.AlterColumn<Guid>(
                name: "CoachEmployeeId",
                table: "ActivityCoachAssignments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCoachAssignments_SportsActivityId",
                table: "ActivityCoachAssignments",
                column: "SportsActivityId");
        }
    }
}
