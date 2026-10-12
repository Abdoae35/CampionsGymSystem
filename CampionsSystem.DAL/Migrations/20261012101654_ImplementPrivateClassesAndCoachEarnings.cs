using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ImplementPrivateClassesAndCoachEarnings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Catalog only: existing role grants and effective overrides are unchanged.
            migrationBuilder.Sql("""
                INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('5932dca8-2dcc-57b2-b0b3-aeac0c4dfdb3', 'Private.RegisterSession', 'Private.RegisterSession', 'Private', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
                INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('ccca7368-03c0-57c9-a7aa-ca853840140b', 'Private.Transfer', 'Private.Transfer', 'Private', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
                INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('374c9757-42d1-52dd-858b-23f1cb0b6f97', 'Private.SettleExpiry', 'Private.SettleExpiry', 'Private', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
                INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('94897b97-5f90-5e85-a4fd-345335a12d05', 'Classes.RegisterAttendance', 'Classes.RegisterAttendance', 'Classes', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
                INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('9926fc9e-28e4-59ae-9c05-4118819c392d', 'CoachEarnings.View', 'CoachEarnings.View', 'CoachEarnings', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
                INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('50cd7ee3-397b-5935-b068-421dc557a932', 'CoachEarnings.ManageTargets', 'CoachEarnings.ManageTargets', 'CoachEarnings', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
                INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('20d0bdd5-4c22-5fbe-b5ec-5d857735f34e', 'CoachEarnings.Recognize', 'CoachEarnings.Recognize', 'CoachEarnings', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
                INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "IsActive", "CreatedAt") VALUES ('614dfe9a-fc31-5121-927d-3cfdfec83150', 'CoachEarnings.Pay', 'CoachEarnings.Pay', 'CoachEarnings', TRUE, CURRENT_TIMESTAMP) ON CONFLICT ("Code") DO NOTHING;
                """);

            migrationBuilder.DropCheckConstraint(
                name: "CK_SaleTransactions_SaleType",
                table: "SaleTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PrivateSubscriptionTransactions_SaleTransactionId",
                table: "PrivateSubscriptionTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PrivateSessions_GymCheckInId",
                table: "PrivateSessions");

            migrationBuilder.DropIndex(
                name: "IX_PrivateSessions_PrivateSubscriptionTransactionId",
                table: "PrivateSessions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PrivateExpirySettlements_EligibleAmount",
                table: "PrivateExpirySettlements");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CheckIns_SessionSnapshot",
                table: "CheckIns");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalAssignmentId",
                table: "PrivateSubscriptionTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalCoachEmployeeId",
                table: "PrivateSubscriptionTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PackageNameSnapshot",
                table: "PrivateSubscriptionTransactions",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RemainderValue",
                table: "PrivateSubscriptionTransactions",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartDateSnapshot",
                table: "PrivateSubscriptionTransactions",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "PrivateSubscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "CommissionPeriodId",
                table: "PrivateSessions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<DateOnly>(
                name: "BusinessDate",
                table: "PrivateSessions",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OperationId",
                table: "PrivateSessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrivateCoachAssignmentId",
                table: "PrivateSessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestHash",
                table: "PrivateSessions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SessionNumber",
                table: "PrivateSessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SessionValue",
                table: "PrivateSessions",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TransferredPackage",
                table: "PrivateSessions",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "BusinessDate",
                table: "PrivateExpirySettlements",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OperationId",
                table: "PrivateExpirySettlements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrivateCoachAssignmentId",
                table: "PrivateExpirySettlements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "PrivateExpirySettlements",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RemainderValue",
                table: "PrivateExpirySettlements",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RequestHash",
                table: "PrivateExpirySettlements",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TransferredPackage",
                table: "PrivateExpirySettlements",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OperationId",
                table: "PrivateCoachAssignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestHash",
                table: "PrivateCoachAssignments",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "BusinessDate",
                table: "ClassSessions",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "ClassSessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OperationId",
                table: "ClassSessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestHash",
                table: "ClassSessions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "ClassSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ClassesSubscriptionId",
                table: "CheckIns",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestHash",
                table: "CheckIns",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClassesPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SessionCount = table.Column<int>(type: "integer", nullable: false),
                    DurationDays = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassesPackages", x => x.Id);
                    table.CheckConstraint("CK_ClassesPackages_Terms", "length(btrim(\"Name\")) > 0 AND \"SessionCount\" > 0 AND \"DurationDays\" > 0 AND \"Price\" > 0");
                    table.ForeignKey(
                        name: "FK_ClassesPackages_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoachPayouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpenseId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AuthorizedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachPayouts", x => x.Id);
                    table.CheckConstraint("CK_CoachPayouts_Balance", "\"Amount\" > 0 AND \"BalanceBefore\" >= \"Amount\" AND \"BalanceAfter\" = \"BalanceBefore\" - \"Amount\"");
                    table.ForeignKey(
                        name: "FK_CoachPayouts_AspNetUsers_AuthorizedByUserId",
                        column: x => x.AuthorizedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachPayouts_Employees_CoachEmployeeId",
                        column: x => x.CoachEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachPayouts_Expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "Expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachPayouts_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoachTargetRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommissionPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrivateTarget = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ClassesAverageTarget = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachTargetRules", x => x.Id);
                    table.CheckConstraint("CK_CoachTargetRules_Targets", "(\"PrivateTarget\" IS NULL OR \"PrivateTarget\" > 0) AND (\"ClassesAverageTarget\" IS NULL OR \"ClassesAverageTarget\" > 0) AND (\"PrivateTarget\" IS NOT NULL OR \"ClassesAverageTarget\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_CoachTargetRules_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachTargetRules_CommissionPeriods_CommissionPeriodId",
                        column: x => x.CommissionPeriodId,
                        principalTable: "CommissionPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachTargetRules_Employees_CoachEmployeeId",
                        column: x => x.CoachEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassesSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassesPackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    SaleTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PurchasedSessions = table.Column<int>(type: "integer", nullable: false),
                    RemainingSessions = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitSessionValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RemainderValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassesSubscriptions", x => x.Id);
                    table.CheckConstraint("CK_ClassesSubscriptions_Balance", "\"PurchasedSessions\" > 0 AND \"RemainingSessions\" >= 0 AND \"RemainingSessions\" <= \"PurchasedSessions\" AND \"EndDate\" >= \"StartDate\"");
                    table.CheckConstraint("CK_ClassesSubscriptions_Value", "\"Price\" > 0 AND \"UnitSessionValue\" >= 0 AND \"RemainderValue\" >= 0 AND \"RemainderValue\" < \"PurchasedSessions\" * 0.01 AND \"Price\" = \"PurchasedSessions\" * \"UnitSessionValue\" + \"RemainderValue\"");
                    table.ForeignKey(
                        name: "FK_ClassesSubscriptions_ClassesPackages_ClassesPackageId",
                        column: x => x.ClassesPackageId,
                        principalTable: "ClassesPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassesSubscriptions_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassesSubscriptions_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassesSubscriptions_SaleTransactions_SaleTransactionId",
                        column: x => x.SaleTransactionId,
                        principalTable: "SaleTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoachEarnings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommissionPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachTargetRuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    SaleTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrivateSessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrivateExpirySettlementId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecognitionOperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EligibleValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TransferredPackage = table.Column<bool>(type: "boolean", nullable: false),
                    TargetProgress = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Target = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachEarnings", x => x.Id);
                    table.CheckConstraint("CK_CoachEarnings_Amounts", "\"EligibleValue\" > 0 AND \"Amount\" >= 0 AND \"Amount\" <= \"EligibleValue\" AND \"TargetProgress\" >= 0");
                    table.CheckConstraint("CK_CoachEarnings_Rate", "(\"TransferredPackage\" AND \"Rate\" = 50) OR (NOT \"TransferredPackage\" AND \"CoachTargetRuleId\" IS NOT NULL AND \"Target\" > 0 AND ((\"TargetProgress\" >= \"Target\" AND \"Rate\" = 60) OR (\"TargetProgress\" < \"Target\" AND \"Rate\" = 50)))");
                    table.CheckConstraint("CK_CoachEarnings_Source", "(\"PrivateSessionId\" IS NOT NULL) <> (\"PrivateExpirySettlementId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_CoachEarnings_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachEarnings_CoachTargetRules_CoachTargetRuleId",
                        column: x => x.CoachTargetRuleId,
                        principalTable: "CoachTargetRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachEarnings_CommissionPeriods_CommissionPeriodId",
                        column: x => x.CommissionPeriodId,
                        principalTable: "CommissionPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachEarnings_Employees_CoachEmployeeId",
                        column: x => x.CoachEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachEarnings_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachEarnings_PrivateExpirySettlements_PrivateExpirySettlem~",
                        column: x => x.PrivateExpirySettlementId,
                        principalTable: "PrivateExpirySettlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachEarnings_PrivateSessions_PrivateSessionId",
                        column: x => x.PrivateSessionId,
                        principalTable: "PrivateSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachEarnings_SaleTransactions_SaleTransactionId",
                        column: x => x.SaleTransactionId,
                        principalTable: "SaleTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoachPayoutAllocations",
                columns: table => new
                {
                    CoachPayoutId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachEarningId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachPayoutAllocations", x => new { x.CoachPayoutId, x.CoachEarningId });
                    table.CheckConstraint("CK_CoachPayoutAllocations_Amount", "\"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_CoachPayoutAllocations_CoachEarnings_CoachEarningId",
                        column: x => x.CoachEarningId,
                        principalTable: "CoachEarnings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachPayoutAllocations_CoachPayouts_CoachPayoutId",
                        column: x => x.CoachPayoutId,
                        principalTable: "CoachPayouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_SaleTransactions_SaleType",
                table: "SaleTransactions",
                sql: "\"SaleType\" IN ('GymMembership', 'Private', 'Activity', 'Other', 'Classes')");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSubscriptionTransactions_OriginalAssignmentId",
                table: "PrivateSubscriptionTransactions",
                column: "OriginalAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSubscriptionTransactions_OriginalCoachEmployeeId",
                table: "PrivateSubscriptionTransactions",
                column: "OriginalCoachEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSubscriptionTransactions_SaleTransactionId",
                table: "PrivateSubscriptionTransactions",
                column: "SaleTransactionId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_PrivateSubscriptionTransactions_Snapshot",
                table: "PrivateSubscriptionTransactions",
                sql: "\"OriginalCoachEmployeeId\" IS NULL OR (\"OriginalAssignmentId\" IS NOT NULL AND \"StartDateSnapshot\" IS NOT NULL AND \"PackageNameSnapshot\" IS NOT NULL AND \"NetAmount\" = \"SessionsAdded\" * \"UnitSessionValue\" + \"RemainderValue\" AND \"RemainderValue\" >= 0 AND \"RemainderValue\" < \"SessionsAdded\" * 0.01)");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSessions_GymCheckInId",
                table: "PrivateSessions",
                column: "GymCheckInId",
                unique: true,
                filter: "\"OperationId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSessions_OperationId",
                table: "PrivateSessions",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSessions_PrivateCoachAssignmentId",
                table: "PrivateSessions",
                column: "PrivateCoachAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSessions_PrivateSubscriptionTransactionId_SessionNum~",
                table: "PrivateSessions",
                columns: new[] { "PrivateSubscriptionTransactionId", "SessionNumber" },
                unique: true,
                filter: "\"OperationId\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PrivateSessions_Operational",
                table: "PrivateSessions",
                sql: "\"OperationId\" IS NULL OR (\"BusinessDate\" IS NOT NULL AND \"RequestHash\" IS NOT NULL AND \"PrivateCoachAssignmentId\" IS NOT NULL AND \"SessionValue\" >= 0 AND \"SessionNumber\" > 0 AND \"TransferredPackage\" IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateExpirySettlements_OperationId",
                table: "PrivateExpirySettlements",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrivateExpirySettlements_PrivateCoachAssignmentId",
                table: "PrivateExpirySettlements",
                column: "PrivateCoachAssignmentId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PrivateExpirySettlements_EligibleAmount",
                table: "PrivateExpirySettlements",
                sql: "\"EligibleAmount\" = \"RemainingSessions\" * \"UnitSessionValue\" + \"RemainderValue\"");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateCoachAssignments_OperationId",
                table: "PrivateCoachAssignments",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessions_CreatedByUserId",
                table: "ClassSessions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessions_OperationId",
                table: "ClassSessions",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_ClassesSubscriptionId",
                table: "CheckIns",
                column: "ClassesSubscriptionId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CheckIns_ClassesSubscription",
                table: "CheckIns",
                sql: "\"ClassesSubscriptionId\" IS NULL OR (\"CheckInType\" = 'Class' AND \"ClassSessionId\" IS NOT NULL AND \"SessionConsumed\")");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CheckIns_SessionSnapshot",
                table: "CheckIns",
                sql: "(\"SessionsBefore\" IS NULL AND \"SessionsAfter\" IS NULL) OR (\"SessionsBefore\" IS NOT NULL AND \"SessionsAfter\" IS NOT NULL AND \"SessionsBefore\" > 0 AND \"SessionsAfter\" = \"SessionsBefore\" - 1 AND \"CheckInType\" IN ('Gym', 'Class') AND \"SessionConsumed\")");

            migrationBuilder.CreateIndex(
                name: "IX_ClassesPackages_BranchId_Name",
                table: "ClassesPackages",
                columns: new[] { "BranchId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassesSubscriptions_BranchId_MemberId_EndDate",
                table: "ClassesSubscriptions",
                columns: new[] { "BranchId", "MemberId", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ClassesSubscriptions_ClassesPackageId",
                table: "ClassesSubscriptions",
                column: "ClassesPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassesSubscriptions_MemberId",
                table: "ClassesSubscriptions",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassesSubscriptions_SaleTransactionId",
                table: "ClassesSubscriptions",
                column: "SaleTransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoachEarnings_BranchId_CoachEmployeeId_BusinessDate",
                table: "CoachEarnings",
                columns: new[] { "BranchId", "CoachEmployeeId", "BusinessDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CoachEarnings_CoachEmployeeId",
                table: "CoachEarnings",
                column: "CoachEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachEarnings_CoachTargetRuleId",
                table: "CoachEarnings",
                column: "CoachTargetRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachEarnings_CommissionPeriodId_PrivateExpirySettlementId",
                table: "CoachEarnings",
                columns: new[] { "CommissionPeriodId", "PrivateExpirySettlementId" },
                unique: true,
                filter: "\"PrivateExpirySettlementId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CoachEarnings_CommissionPeriodId_PrivateSessionId",
                table: "CoachEarnings",
                columns: new[] { "CommissionPeriodId", "PrivateSessionId" },
                unique: true,
                filter: "\"PrivateSessionId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CoachEarnings_CreatedByUserId",
                table: "CoachEarnings",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachEarnings_PrivateExpirySettlementId",
                table: "CoachEarnings",
                column: "PrivateExpirySettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachEarnings_PrivateSessionId",
                table: "CoachEarnings",
                column: "PrivateSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachEarnings_RecognitionOperationId",
                table: "CoachEarnings",
                column: "RecognitionOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachEarnings_SaleTransactionId",
                table: "CoachEarnings",
                column: "SaleTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachPayoutAllocations_CoachEarningId",
                table: "CoachPayoutAllocations",
                column: "CoachEarningId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachPayouts_AuthorizedByUserId",
                table: "CoachPayouts",
                column: "AuthorizedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachPayouts_BranchId_CoachEmployeeId_PaidAt",
                table: "CoachPayouts",
                columns: new[] { "BranchId", "CoachEmployeeId", "PaidAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CoachPayouts_CoachEmployeeId",
                table: "CoachPayouts",
                column: "CoachEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachPayouts_ExpenseId",
                table: "CoachPayouts",
                column: "ExpenseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoachPayouts_OperationId",
                table: "CoachPayouts",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoachTargetRules_CoachEmployeeId",
                table: "CoachTargetRules",
                column: "CoachEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachTargetRules_CommissionPeriodId_CoachEmployeeId",
                table: "CoachTargetRules",
                columns: new[] { "CommissionPeriodId", "CoachEmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoachTargetRules_CreatedByUserId",
                table: "CoachTargetRules",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachTargetRules_OperationId",
                table: "CoachTargetRules",
                column: "OperationId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CheckIns_ClassesSubscriptions_ClassesSubscriptionId",
                table: "CheckIns",
                column: "ClassesSubscriptionId",
                principalTable: "ClassesSubscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassSessions_AspNetUsers_CreatedByUserId",
                table: "ClassSessions",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrivateExpirySettlements_PrivateCoachAssignments_PrivateCoa~",
                table: "PrivateExpirySettlements",
                column: "PrivateCoachAssignmentId",
                principalTable: "PrivateCoachAssignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrivateSessions_PrivateCoachAssignments_PrivateCoachAssignm~",
                table: "PrivateSessions",
                column: "PrivateCoachAssignmentId",
                principalTable: "PrivateCoachAssignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrivateSubscriptionTransactions_Employees_OriginalCoachEmpl~",
                table: "PrivateSubscriptionTransactions",
                column: "OriginalCoachEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrivateSubscriptionTransactions_PrivateCoachAssignments_Ori~",
                table: "PrivateSubscriptionTransactions",
                column: "OriginalAssignmentId",
                principalTable: "PrivateCoachAssignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckIns_ClassesSubscriptions_ClassesSubscriptionId",
                table: "CheckIns");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassSessions_AspNetUsers_CreatedByUserId",
                table: "ClassSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_PrivateExpirySettlements_PrivateCoachAssignments_PrivateCoa~",
                table: "PrivateExpirySettlements");

            migrationBuilder.DropForeignKey(
                name: "FK_PrivateSessions_PrivateCoachAssignments_PrivateCoachAssignm~",
                table: "PrivateSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_PrivateSubscriptionTransactions_Employees_OriginalCoachEmpl~",
                table: "PrivateSubscriptionTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PrivateSubscriptionTransactions_PrivateCoachAssignments_Ori~",
                table: "PrivateSubscriptionTransactions");

            migrationBuilder.DropTable(
                name: "ClassesSubscriptions");

            migrationBuilder.DropTable(
                name: "CoachPayoutAllocations");

            migrationBuilder.DropTable(
                name: "ClassesPackages");

            migrationBuilder.DropTable(
                name: "CoachEarnings");

            migrationBuilder.DropTable(
                name: "CoachPayouts");

            migrationBuilder.DropTable(
                name: "CoachTargetRules");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SaleTransactions_SaleType",
                table: "SaleTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PrivateSubscriptionTransactions_OriginalAssignmentId",
                table: "PrivateSubscriptionTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PrivateSubscriptionTransactions_OriginalCoachEmployeeId",
                table: "PrivateSubscriptionTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PrivateSubscriptionTransactions_SaleTransactionId",
                table: "PrivateSubscriptionTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PrivateSubscriptionTransactions_Snapshot",
                table: "PrivateSubscriptionTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PrivateSessions_GymCheckInId",
                table: "PrivateSessions");

            migrationBuilder.DropIndex(
                name: "IX_PrivateSessions_OperationId",
                table: "PrivateSessions");

            migrationBuilder.DropIndex(
                name: "IX_PrivateSessions_PrivateCoachAssignmentId",
                table: "PrivateSessions");

            migrationBuilder.DropIndex(
                name: "IX_PrivateSessions_PrivateSubscriptionTransactionId_SessionNum~",
                table: "PrivateSessions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PrivateSessions_Operational",
                table: "PrivateSessions");

            migrationBuilder.DropIndex(
                name: "IX_PrivateExpirySettlements_OperationId",
                table: "PrivateExpirySettlements");

            migrationBuilder.DropIndex(
                name: "IX_PrivateExpirySettlements_PrivateCoachAssignmentId",
                table: "PrivateExpirySettlements");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PrivateExpirySettlements_EligibleAmount",
                table: "PrivateExpirySettlements");

            migrationBuilder.DropIndex(
                name: "IX_PrivateCoachAssignments_OperationId",
                table: "PrivateCoachAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ClassSessions_CreatedByUserId",
                table: "ClassSessions");

            migrationBuilder.DropIndex(
                name: "IX_ClassSessions_OperationId",
                table: "ClassSessions");

            migrationBuilder.DropIndex(
                name: "IX_CheckIns_ClassesSubscriptionId",
                table: "CheckIns");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CheckIns_ClassesSubscription",
                table: "CheckIns");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CheckIns_SessionSnapshot",
                table: "CheckIns");

            migrationBuilder.DropColumn(
                name: "OriginalAssignmentId",
                table: "PrivateSubscriptionTransactions");

            migrationBuilder.DropColumn(
                name: "OriginalCoachEmployeeId",
                table: "PrivateSubscriptionTransactions");

            migrationBuilder.DropColumn(
                name: "PackageNameSnapshot",
                table: "PrivateSubscriptionTransactions");

            migrationBuilder.DropColumn(
                name: "RemainderValue",
                table: "PrivateSubscriptionTransactions");

            migrationBuilder.DropColumn(
                name: "StartDateSnapshot",
                table: "PrivateSubscriptionTransactions");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "PrivateSubscriptions");

            migrationBuilder.DropColumn(
                name: "BusinessDate",
                table: "PrivateSessions");

            migrationBuilder.DropColumn(
                name: "OperationId",
                table: "PrivateSessions");

            migrationBuilder.DropColumn(
                name: "PrivateCoachAssignmentId",
                table: "PrivateSessions");

            migrationBuilder.DropColumn(
                name: "RequestHash",
                table: "PrivateSessions");

            migrationBuilder.DropColumn(
                name: "SessionNumber",
                table: "PrivateSessions");

            migrationBuilder.DropColumn(
                name: "SessionValue",
                table: "PrivateSessions");

            migrationBuilder.DropColumn(
                name: "TransferredPackage",
                table: "PrivateSessions");

            migrationBuilder.DropColumn(
                name: "BusinessDate",
                table: "PrivateExpirySettlements");

            migrationBuilder.DropColumn(
                name: "OperationId",
                table: "PrivateExpirySettlements");

            migrationBuilder.DropColumn(
                name: "PrivateCoachAssignmentId",
                table: "PrivateExpirySettlements");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "PrivateExpirySettlements");

            migrationBuilder.DropColumn(
                name: "RemainderValue",
                table: "PrivateExpirySettlements");

            migrationBuilder.DropColumn(
                name: "RequestHash",
                table: "PrivateExpirySettlements");

            migrationBuilder.DropColumn(
                name: "TransferredPackage",
                table: "PrivateExpirySettlements");

            migrationBuilder.DropColumn(
                name: "OperationId",
                table: "PrivateCoachAssignments");

            migrationBuilder.DropColumn(
                name: "RequestHash",
                table: "PrivateCoachAssignments");

            migrationBuilder.DropColumn(
                name: "BusinessDate",
                table: "ClassSessions");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ClassSessions");

            migrationBuilder.DropColumn(
                name: "OperationId",
                table: "ClassSessions");

            migrationBuilder.DropColumn(
                name: "RequestHash",
                table: "ClassSessions");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "ClassSessions");

            migrationBuilder.DropColumn(
                name: "ClassesSubscriptionId",
                table: "CheckIns");

            migrationBuilder.DropColumn(
                name: "RequestHash",
                table: "CheckIns");

            migrationBuilder.AlterColumn<Guid>(
                name: "CommissionPeriodId",
                table: "PrivateSessions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_SaleTransactions_SaleType",
                table: "SaleTransactions",
                sql: "\"SaleType\" IN ('GymMembership', 'Private', 'Activity', 'Other')");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSubscriptionTransactions_SaleTransactionId",
                table: "PrivateSubscriptionTransactions",
                column: "SaleTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSessions_GymCheckInId",
                table: "PrivateSessions",
                column: "GymCheckInId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSessions_PrivateSubscriptionTransactionId",
                table: "PrivateSessions",
                column: "PrivateSubscriptionTransactionId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PrivateExpirySettlements_EligibleAmount",
                table: "PrivateExpirySettlements",
                sql: "\"EligibleAmount\" = \"RemainingSessions\" * \"UnitSessionValue\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CheckIns_SessionSnapshot",
                table: "CheckIns",
                sql: "(\"SessionsBefore\" IS NULL AND \"SessionsAfter\" IS NULL) OR (\"SessionsBefore\" IS NOT NULL AND \"SessionsAfter\" IS NOT NULL AND \"SessionsBefore\" > 0 AND \"SessionsAfter\" = \"SessionsBefore\" - 1 AND \"CheckInType\" = 'Gym' AND \"SessionConsumed\")");
        }
    }
}
