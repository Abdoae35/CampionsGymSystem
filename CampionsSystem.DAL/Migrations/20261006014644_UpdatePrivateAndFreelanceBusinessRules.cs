using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePrivateAndFreelanceBusinessRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Legacy delivered sessions lack batch/period links. Never invent financial history.
            migrationBuilder.Sql("""
                DO $rules$
                BEGIN
                    IF EXISTS (SELECT 1 FROM "PrivateSessions") OR EXISTS (
                        SELECT 1 FROM "PrivateSubscriptions" s
                        WHERE s."RemainingSessions" <> COALESCE((
                            SELECT SUM(t."SessionsAdded") FROM "PrivateSubscriptionTransactions" t
                            WHERE t."PrivateSubscriptionId" = s."Id"), 0)
                           OR s."TotalSessions" <> COALESCE((
                            SELECT SUM(t."SessionsAdded") FROM "PrivateSubscriptionTransactions" t
                            WHERE t."PrivateSubscriptionId" = s."Id"), 0)
                    ) THEN
                        RAISE EXCEPTION 'Private history requires explicit batch/period/counter reconciliation before UpdatePrivateAndFreelanceBusinessRules. No history was changed.';
                    END IF;
                END $rules$;
                """);

            migrationBuilder.DropIndex(
                name: "IX_PrivateSubscriptionTransactions_PrivateSubscriptionId",
                table: "PrivateSubscriptionTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PrivateSessions_CoachEmployeeId",
                table: "PrivateSessions");

            migrationBuilder.AddColumn<int>(
                name: "RemainingSessions",
                table: "PrivateSubscriptionTransactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CommissionPeriodId",
                table: "PrivateSessions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PrivateSubscriptionTransactionId",
                table: "PrivateSessions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<int>(
                name: "SessionsAllocated",
                table: "PrivateCoachAssignments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "CommissionBasisAmount",
                table: "PrivateCoachAssignments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<Guid>(
                name: "FreelanceCoachEmployeeIdSnapshot",
                table: "MembershipSubscriptionTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlanCategorySnapshot",
                table: "MembershipSubscriptionTransactions",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Normal");

            migrationBuilder.AddColumn<Guid>(
                name: "FreelanceCoachEmployeeId",
                table: "MembershipPlans",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlanCategory",
                table: "MembershipPlans",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Normal");

            // Only pristine unused legacy batches pass the preflight guard.
            migrationBuilder.Sql("""
                UPDATE "PrivateSubscriptionTransactions" SET "RemainingSessions" = "SessionsAdded";
                ALTER TABLE "PrivateSubscriptionTransactions" ALTER COLUMN "RemainingSessions" DROP DEFAULT;
                ALTER TABLE "PrivateSessions" ALTER COLUMN "CommissionPeriodId" DROP DEFAULT;
                ALTER TABLE "PrivateSessions" ALTER COLUMN "PrivateSubscriptionTransactionId" DROP DEFAULT;
                ALTER TABLE "MembershipPlans" ALTER COLUMN "PlanCategory" DROP DEFAULT;
                ALTER TABLE "MembershipSubscriptionTransactions" ALTER COLUMN "PlanCategorySnapshot" DROP DEFAULT;
                """);

            migrationBuilder.CreateTable(
                name: "PrivateExpirySettlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrivateSubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrivateSubscriptionTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommissionPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    RemainingSessions = table.Column<int>(type: "integer", nullable: false),
                    UnitSessionValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EligibleAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SettledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateExpirySettlements", x => x.Id);
                    table.CheckConstraint("CK_PrivateExpirySettlements_EligibleAmount", "\"EligibleAmount\" = \"RemainingSessions\" * \"UnitSessionValue\"");
                    table.CheckConstraint("CK_PrivateExpirySettlements_RemainingSessions", "\"RemainingSessions\" > 0");
                    table.CheckConstraint("CK_PrivateExpirySettlements_UnitSessionValue", "\"UnitSessionValue\" >= 0");
                    table.ForeignKey(
                        name: "FK_PrivateExpirySettlements_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrivateExpirySettlements_CommissionPeriods_CommissionPeriod~",
                        column: x => x.CommissionPeriodId,
                        principalTable: "CommissionPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrivateExpirySettlements_Employees_CoachEmployeeId",
                        column: x => x.CoachEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrivateExpirySettlements_PrivateSubscriptionTransactions_Pr~",
                        column: x => x.PrivateSubscriptionTransactionId,
                        principalTable: "PrivateSubscriptionTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrivateExpirySettlements_PrivateSubscriptions_PrivateSubscr~",
                        column: x => x.PrivateSubscriptionId,
                        principalTable: "PrivateSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSubscriptionTransactions_PrivateSubscriptionId_Creat~",
                table: "PrivateSubscriptionTransactions",
                columns: new[] { "PrivateSubscriptionId", "CreatedAt", "Id" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_PrivateSubscriptionTransactions_RemainingSessions",
                table: "PrivateSubscriptionTransactions",
                sql: "\"RemainingSessions\" >= 0 AND \"RemainingSessions\" <= \"SessionsAdded\"");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSessions_CoachEmployeeId_SessionAt",
                table: "PrivateSessions",
                columns: new[] { "CoachEmployeeId", "SessionAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSessions_CommissionPeriodId",
                table: "PrivateSessions",
                column: "CommissionPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSessions_PrivateSubscriptionTransactionId",
                table: "PrivateSessions",
                column: "PrivateSubscriptionTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateCoachAssignments_PrivateSubscriptionId_EndedAt",
                table: "PrivateCoachAssignments",
                columns: new[] { "PrivateSubscriptionId", "EndedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MembershipSubscriptionTransactions_FreelanceCoachEmployeeId~",
                table: "MembershipSubscriptionTransactions",
                column: "FreelanceCoachEmployeeIdSnapshot");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MembershipSubscriptionTransactions_PlanCategoryCoachSnapshot",
                table: "MembershipSubscriptionTransactions",
                sql: "(\"PlanCategorySnapshot\" = 'Normal' AND \"FreelanceCoachEmployeeIdSnapshot\" IS NULL) OR (\"PlanCategorySnapshot\" = 'Freelance' AND \"FreelanceCoachEmployeeIdSnapshot\" IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipPlans_FreelanceCoachEmployeeId",
                table: "MembershipPlans",
                column: "FreelanceCoachEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipPlans_PlanCategory_FreelanceCoachEmployeeId_IsAct~",
                table: "MembershipPlans",
                columns: new[] { "PlanCategory", "FreelanceCoachEmployeeId", "IsActive" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_MembershipPlans_PlanCategoryCoach",
                table: "MembershipPlans",
                sql: "(\"PlanCategory\" = 'Normal' AND \"FreelanceCoachEmployeeId\" IS NULL) OR (\"PlanCategory\" = 'Freelance' AND \"FreelanceCoachEmployeeId\" IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateExpirySettlements_CoachEmployeeId",
                table: "PrivateExpirySettlements",
                column: "CoachEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateExpirySettlements_CommissionPeriodId",
                table: "PrivateExpirySettlements",
                column: "CommissionPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateExpirySettlements_CreatedByUserId",
                table: "PrivateExpirySettlements",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateExpirySettlements_PrivateSubscriptionId",
                table: "PrivateExpirySettlements",
                column: "PrivateSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateExpirySettlements_PrivateSubscriptionTransactionId",
                table: "PrivateExpirySettlements",
                column: "PrivateSubscriptionTransactionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipPlans_Employees_FreelanceCoachEmployeeId",
                table: "MembershipPlans",
                column: "FreelanceCoachEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipSubscriptionTransactions_Employees_FreelanceCoach~",
                table: "MembershipSubscriptionTransactions",
                column: "FreelanceCoachEmployeeIdSnapshot",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrivateSessions_CommissionPeriods_CommissionPeriodId",
                table: "PrivateSessions",
                column: "CommissionPeriodId",
                principalTable: "CommissionPeriods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrivateSessions_PrivateSubscriptionTransactions_PrivateSubs~",
                table: "PrivateSessions",
                column: "PrivateSubscriptionTransactionId",
                principalTable: "PrivateSubscriptionTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MembershipPlans_Employees_FreelanceCoachEmployeeId",
                table: "MembershipPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipSubscriptionTransactions_Employees_FreelanceCoach~",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PrivateSessions_CommissionPeriods_CommissionPeriodId",
                table: "PrivateSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_PrivateSessions_PrivateSubscriptionTransactions_PrivateSubs~",
                table: "PrivateSessions");

            migrationBuilder.DropTable(
                name: "PrivateExpirySettlements");

            migrationBuilder.DropIndex(
                name: "IX_PrivateSubscriptionTransactions_PrivateSubscriptionId_Creat~",
                table: "PrivateSubscriptionTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PrivateSubscriptionTransactions_RemainingSessions",
                table: "PrivateSubscriptionTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PrivateSessions_CoachEmployeeId_SessionAt",
                table: "PrivateSessions");

            migrationBuilder.DropIndex(
                name: "IX_PrivateSessions_CommissionPeriodId",
                table: "PrivateSessions");

            migrationBuilder.DropIndex(
                name: "IX_PrivateSessions_PrivateSubscriptionTransactionId",
                table: "PrivateSessions");

            migrationBuilder.DropIndex(
                name: "IX_PrivateCoachAssignments_PrivateSubscriptionId_EndedAt",
                table: "PrivateCoachAssignments");

            migrationBuilder.DropIndex(
                name: "IX_MembershipSubscriptionTransactions_FreelanceCoachEmployeeId~",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MembershipSubscriptionTransactions_PlanCategoryCoachSnapshot",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropIndex(
                name: "IX_MembershipPlans_FreelanceCoachEmployeeId",
                table: "MembershipPlans");

            migrationBuilder.DropIndex(
                name: "IX_MembershipPlans_PlanCategory_FreelanceCoachEmployeeId_IsAct~",
                table: "MembershipPlans");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MembershipPlans_PlanCategoryCoach",
                table: "MembershipPlans");

            migrationBuilder.DropColumn(
                name: "RemainingSessions",
                table: "PrivateSubscriptionTransactions");

            migrationBuilder.DropColumn(
                name: "CommissionPeriodId",
                table: "PrivateSessions");

            migrationBuilder.DropColumn(
                name: "PrivateSubscriptionTransactionId",
                table: "PrivateSessions");

            migrationBuilder.DropColumn(
                name: "FreelanceCoachEmployeeIdSnapshot",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropColumn(
                name: "PlanCategorySnapshot",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropColumn(
                name: "FreelanceCoachEmployeeId",
                table: "MembershipPlans");

            migrationBuilder.DropColumn(
                name: "PlanCategory",
                table: "MembershipPlans");

            migrationBuilder.AlterColumn<int>(
                name: "SessionsAllocated",
                table: "PrivateCoachAssignments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "CommissionBasisAmount",
                table: "PrivateCoachAssignments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSubscriptionTransactions_PrivateSubscriptionId",
                table: "PrivateSubscriptionTransactions",
                column: "PrivateSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSessions_CoachEmployeeId",
                table: "PrivateSessions",
                column: "CoachEmployeeId");
        }
    }
}
