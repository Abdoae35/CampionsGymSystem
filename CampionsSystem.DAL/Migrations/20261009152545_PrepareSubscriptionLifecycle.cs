using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class PrepareSubscriptionLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubscriptionFreezes_MemberSubscriptionId",
                table: "SubscriptionFreezes");

            migrationBuilder.DropIndex(
                name: "IX_MembershipSubscriptionTransactions_MemberSubscriptionId",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MembershipSubscriptionTransactions_PlanCategoryCoachSnapshot",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MembershipSubscriptionTransactions_PurchasedDurationDays",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedAt",
                table: "SubscriptionFreezes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompletedByUserId",
                table: "SubscriptionFreezes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "NewSubscriptionEndDate",
                table: "SubscriptionFreezes",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OperationId",
                table: "SubscriptionFreezes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PreviousSubscriptionEndDate",
                table: "SubscriptionFreezes",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FollowsSubscriptionId",
                table: "MemberSubscriptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "MemberSubscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "SaleTransactionId",
                table: "MembershipSubscriptionTransactions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "NewEndDate",
                table: "MembershipSubscriptionTransactions",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<string>(
                name: "AccessModeSnapshot",
                table: "MembershipSubscriptionTransactions",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BusinessDayId",
                table: "MembershipSubscriptionTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CityIdSnapshot",
                table: "MembershipSubscriptionTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "MembershipSubscriptionTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FreezeDaysSnapshot",
                table: "MembershipSubscriptionTransactions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OperationId",
                table: "MembershipSubscriptionTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PlanBranchIdSnapshot",
                table: "MembershipSubscriptionTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlanNameSnapshot",
                table: "MembershipSubscriptionTransactions",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousRemainingSessions",
                table: "MembershipSubscriptionTransactions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RenewedFromSubscriptionId",
                table: "MembershipSubscriptionTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartDateSnapshot",
                table: "MembershipSubscriptionTransactions",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SubscriptionHolidayExtensions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberSubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchClosureId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProtectedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DaysApplied = table.Column<int>(type: "integer", nullable: false),
                    PreviousEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    NewEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionHolidayExtensions", x => x.Id);
                    table.CheckConstraint("CK_SubscriptionHolidayExtensions_Days", "\"DaysApplied\" IN (0, 1) AND \"NewEndDate\" = \"PreviousEndDate\" + \"DaysApplied\"");
                    table.ForeignKey(
                        name: "FK_SubscriptionHolidayExtensions_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubscriptionHolidayExtensions_BranchClosures_BranchClosureId",
                        column: x => x.BranchClosureId,
                        principalTable: "BranchClosures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubscriptionHolidayExtensions_MemberSubscriptions_MemberSub~",
                        column: x => x.MemberSubscriptionId,
                        principalTable: "MemberSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionFreezes_CompletedByUserId",
                table: "SubscriptionFreezes",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionFreezes_MemberSubscriptionId_StartDate",
                table: "SubscriptionFreezes",
                columns: new[] { "MemberSubscriptionId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionFreezes_OperationId",
                table: "SubscriptionFreezes",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberSubscriptions_FollowsSubscriptionId",
                table: "MemberSubscriptions",
                column: "FollowsSubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipSubscriptionTransactions_BusinessDayId",
                table: "MembershipSubscriptionTransactions",
                column: "BusinessDayId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipSubscriptionTransactions_CityIdSnapshot",
                table: "MembershipSubscriptionTransactions",
                column: "CityIdSnapshot");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipSubscriptionTransactions_CreatedByUserId",
                table: "MembershipSubscriptionTransactions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipSubscriptionTransactions_MemberSubscriptionId_Cre~",
                table: "MembershipSubscriptionTransactions",
                columns: new[] { "MemberSubscriptionId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MembershipSubscriptionTransactions_OperationId",
                table: "MembershipSubscriptionTransactions",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipSubscriptionTransactions_PlanBranchIdSnapshot",
                table: "MembershipSubscriptionTransactions",
                column: "PlanBranchIdSnapshot");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipSubscriptionTransactions_RenewedFromSubscriptionId",
                table: "MembershipSubscriptionTransactions",
                column: "RenewedFromSubscriptionId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MembershipSubscriptionTransactions_FreezeSnapshot",
                table: "MembershipSubscriptionTransactions",
                sql: "\"FreezeDaysSnapshot\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MembershipSubscriptionTransactions_PlanCategoryCoachSnapshot",
                table: "MembershipSubscriptionTransactions",
                sql: "(\"PlanCategorySnapshot\" IN ('Normal', 'Employee') AND \"FreelanceCoachEmployeeIdSnapshot\" IS NULL) OR (\"PlanCategorySnapshot\" = 'Freelance' AND \"FreelanceCoachEmployeeIdSnapshot\" IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MembershipSubscriptionTransactions_PurchasedDurationDays",
                table: "MembershipSubscriptionTransactions",
                sql: "(\"PlanCategorySnapshot\" = 'Employee' AND \"PurchasedDurationDays\" = 0 AND \"NewEndDate\" IS NULL AND \"SaleTransactionId\" IS NULL AND \"PlanPriceSnapshot\" = 0 AND \"NetPriceSnapshot\" = 0 AND \"PurchasedSessions\" IS NULL) OR (\"PlanCategorySnapshot\" <> 'Employee' AND \"PurchasedDurationDays\" > 0 AND \"NewEndDate\" IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionHolidayExtensions_BranchClosureId",
                table: "SubscriptionHolidayExtensions",
                column: "BranchClosureId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionHolidayExtensions_CreatedByUserId",
                table: "SubscriptionHolidayExtensions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionHolidayExtensions_MemberSubscriptionId_Protecte~",
                table: "SubscriptionHolidayExtensions",
                columns: new[] { "MemberSubscriptionId", "ProtectedDate" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipSubscriptionTransactions_AspNetUsers_CreatedByUse~",
                table: "MembershipSubscriptionTransactions",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipSubscriptionTransactions_BusinessDays_BusinessDay~",
                table: "MembershipSubscriptionTransactions",
                column: "BusinessDayId",
                principalTable: "BusinessDays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipSubscriptionTransactions_Cities_CityIdSnapshot",
                table: "MembershipSubscriptionTransactions",
                column: "CityIdSnapshot",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipSubscriptionTransactions_GymBranches_PlanBranchId~",
                table: "MembershipSubscriptionTransactions",
                column: "PlanBranchIdSnapshot",
                principalTable: "GymBranches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipSubscriptionTransactions_MemberSubscriptions_Rene~",
                table: "MembershipSubscriptionTransactions",
                column: "RenewedFromSubscriptionId",
                principalTable: "MemberSubscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSubscriptions_MemberSubscriptions_FollowsSubscription~",
                table: "MemberSubscriptions",
                column: "FollowsSubscriptionId",
                principalTable: "MemberSubscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionFreezes_AspNetUsers_CompletedByUserId",
                table: "SubscriptionFreezes",
                column: "CompletedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MembershipSubscriptionTransactions_AspNetUsers_CreatedByUse~",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipSubscriptionTransactions_BusinessDays_BusinessDay~",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipSubscriptionTransactions_Cities_CityIdSnapshot",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipSubscriptionTransactions_GymBranches_PlanBranchId~",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipSubscriptionTransactions_MemberSubscriptions_Rene~",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberSubscriptions_MemberSubscriptions_FollowsSubscription~",
                table: "MemberSubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionFreezes_AspNetUsers_CompletedByUserId",
                table: "SubscriptionFreezes");

            migrationBuilder.DropTable(
                name: "SubscriptionHolidayExtensions");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionFreezes_CompletedByUserId",
                table: "SubscriptionFreezes");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionFreezes_MemberSubscriptionId_StartDate",
                table: "SubscriptionFreezes");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionFreezes_OperationId",
                table: "SubscriptionFreezes");

            migrationBuilder.DropIndex(
                name: "IX_MemberSubscriptions_FollowsSubscriptionId",
                table: "MemberSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_MembershipSubscriptionTransactions_BusinessDayId",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropIndex(
                name: "IX_MembershipSubscriptionTransactions_CityIdSnapshot",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropIndex(
                name: "IX_MembershipSubscriptionTransactions_CreatedByUserId",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropIndex(
                name: "IX_MembershipSubscriptionTransactions_MemberSubscriptionId_Cre~",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropIndex(
                name: "IX_MembershipSubscriptionTransactions_OperationId",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropIndex(
                name: "IX_MembershipSubscriptionTransactions_PlanBranchIdSnapshot",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropIndex(
                name: "IX_MembershipSubscriptionTransactions_RenewedFromSubscriptionId",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MembershipSubscriptionTransactions_FreezeSnapshot",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MembershipSubscriptionTransactions_PlanCategoryCoachSnapshot",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MembershipSubscriptionTransactions_PurchasedDurationDays",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "SubscriptionFreezes");

            migrationBuilder.DropColumn(
                name: "CompletedByUserId",
                table: "SubscriptionFreezes");

            migrationBuilder.DropColumn(
                name: "NewSubscriptionEndDate",
                table: "SubscriptionFreezes");

            migrationBuilder.DropColumn(
                name: "OperationId",
                table: "SubscriptionFreezes");

            migrationBuilder.DropColumn(
                name: "PreviousSubscriptionEndDate",
                table: "SubscriptionFreezes");

            migrationBuilder.DropColumn(
                name: "FollowsSubscriptionId",
                table: "MemberSubscriptions");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "MemberSubscriptions");

            migrationBuilder.DropColumn(
                name: "AccessModeSnapshot",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropColumn(
                name: "BusinessDayId",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropColumn(
                name: "CityIdSnapshot",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropColumn(
                name: "FreezeDaysSnapshot",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropColumn(
                name: "OperationId",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropColumn(
                name: "PlanBranchIdSnapshot",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropColumn(
                name: "PlanNameSnapshot",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropColumn(
                name: "PreviousRemainingSessions",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropColumn(
                name: "RenewedFromSubscriptionId",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropColumn(
                name: "StartDateSnapshot",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "SaleTransactionId",
                table: "MembershipSubscriptionTransactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "NewEndDate",
                table: "MembershipSubscriptionTransactions",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionFreezes_MemberSubscriptionId",
                table: "SubscriptionFreezes",
                column: "MemberSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipSubscriptionTransactions_MemberSubscriptionId",
                table: "MembershipSubscriptionTransactions",
                column: "MemberSubscriptionId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MembershipSubscriptionTransactions_PlanCategoryCoachSnapshot",
                table: "MembershipSubscriptionTransactions",
                sql: "(\"PlanCategorySnapshot\" = 'Normal' AND \"FreelanceCoachEmployeeIdSnapshot\" IS NULL) OR (\"PlanCategorySnapshot\" = 'Freelance' AND \"FreelanceCoachEmployeeIdSnapshot\" IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MembershipSubscriptionTransactions_PurchasedDurationDays",
                table: "MembershipSubscriptionTransactions",
                sql: "\"PurchasedDurationDays\" > 0");
        }
    }
}
