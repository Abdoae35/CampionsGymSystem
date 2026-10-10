using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class PrepareSalesPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MembershipSubscriptionTransactions_SaleTransactionId",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.AddColumn<string>(
                name: "ActivationMode",
                table: "SaleTransactions",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "BusinessDate",
                table: "SaleTransactions",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "SaleTransactions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledAt",
                table: "SaleTransactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CancelledByUserId",
                table: "SaleTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CommissionEligible",
                table: "SaleTransactions",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExtraDaysSnapshot",
                table: "SaleTransactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "InitialPaymentRequired",
                table: "SaleTransactions",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfferNameSnapshot",
                table: "SaleTransactions",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfferTypeSnapshot",
                table: "SaleTransactions",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OfferValueSnapshot",
                table: "SaleTransactions",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OperationId",
                table: "SaleTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchaseMode",
                table: "SaleTransactions",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestHash",
                table: "SaleTransactions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "RequestedStartDate",
                table: "SaleTransactions",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ReservationDueDate",
                table: "SaleTransactions",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TargetEligible",
                table: "SaleTransactions",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "SaleTransactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "BusinessDate",
                table: "Refunds",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OperationId",
                table: "Refunds",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestHash",
                table: "Refunds",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ShiftId",
                table: "Payments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "CommissionPeriodId",
                table: "Payments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<DateOnly>(
                name: "BusinessDate",
                table: "Payments",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OperationId",
                table: "Payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestHash",
                table: "Payments",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsStandaloneVisit",
                table: "MemberSubscriptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "BusinessDate",
                table: "FinancialTransactions",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InstallmentPaymentAllocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstallmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstallmentPaymentAllocations", x => x.Id);
                    table.CheckConstraint("CK_InstallmentPaymentAllocations_Amount", "\"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_InstallmentPaymentAllocations_Installments_InstallmentId",
                        column: x => x.InstallmentId,
                        principalTable: "Installments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InstallmentPaymentAllocations_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SaleMembershipSnapshots",
                columns: table => new
                {
                    SaleTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    MembershipPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PlanCategory = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CityId = table.Column<Guid>(type: "uuid", nullable: false),
                    FreelanceCoachEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    AccessMode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    DurationDays = table.Column<int>(type: "integer", nullable: false),
                    SessionCount = table.Column<int>(type: "integer", nullable: true),
                    FreezeDays = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleMembershipSnapshots", x => x.SaleTransactionId);
                    table.CheckConstraint("CK_SaleMembershipSnapshots_Duration", "\"DurationDays\" > 0 AND \"FreezeDays\" >= 0");
                    table.CheckConstraint("CK_SaleMembershipSnapshots_Mode", "(\"AccessMode\" = 'Unlimited' AND \"SessionCount\" IS NULL) OR (\"AccessMode\" = 'SessionBased' AND \"SessionCount\" IS NOT NULL AND \"SessionCount\" > 0)");
                    table.CheckConstraint("CK_SaleMembershipSnapshots_Type", "(\"PlanCategory\" = 'Normal' AND \"FreelanceCoachEmployeeId\" IS NULL) OR (\"PlanCategory\" = 'Freelance' AND \"FreelanceCoachEmployeeId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_SaleMembershipSnapshots_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleMembershipSnapshots_Employees_FreelanceCoachEmployeeId",
                        column: x => x.FreelanceCoachEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleMembershipSnapshots_MembershipPlans_MembershipPlanId",
                        column: x => x.MembershipPlanId,
                        principalTable: "MembershipPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleMembershipSnapshots_SaleTransactions_SaleTransactionId",
                        column: x => x.SaleTransactionId,
                        principalTable: "SaleTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SaleBenefitSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SaleTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    MembershipBenefitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    GrantType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleBenefitSnapshots", x => x.Id);
                    table.CheckConstraint("CK_SaleBenefitSnapshots_Quantity", "(\"GrantType\" = 'Access' AND \"Quantity\" IS NULL) OR (\"GrantType\" = 'Quantity' AND \"Quantity\" IS NOT NULL AND \"Quantity\" > 0)");
                    table.ForeignKey(
                        name: "FK_SaleBenefitSnapshots_MembershipBenefits_MembershipBenefitId",
                        column: x => x.MembershipBenefitId,
                        principalTable: "MembershipBenefits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleBenefitSnapshots_SaleMembershipSnapshots_SaleTransactio~",
                        column: x => x.SaleTransactionId,
                        principalTable: "SaleMembershipSnapshots",
                        principalColumn: "SaleTransactionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaleTransactions_CancelledByUserId",
                table: "SaleTransactions",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleTransactions_OperationId",
                table: "SaleTransactions",
                column: "OperationId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_SaleTransactions_Activation",
                table: "SaleTransactions",
                sql: "\"OperationId\" IS NULL OR ((\"ActivationMode\" = 'SpecificDate') = (\"RequestedStartDate\" IS NOT NULL))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SaleTransactions_Cancellation",
                table: "SaleTransactions",
                sql: "(\"CancelledAt\" IS NULL AND \"CancelledByUserId\" IS NULL AND \"CancellationReason\" IS NULL) OR (\"CancelledAt\" IS NOT NULL AND \"CancelledByUserId\" IS NOT NULL AND \"CancellationReason\" IS NOT NULL AND length(btrim(\"CancellationReason\")) > 0)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SaleTransactions_ExtraDays",
                table: "SaleTransactions",
                sql: "\"ExtraDaysSnapshot\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SaleTransactions_Workflow",
                table: "SaleTransactions",
                sql: "\"OperationId\" IS NULL OR (\"OperationId\" <> '00000000-0000-0000-0000-000000000000'::uuid AND \"RequestHash\" IS NOT NULL AND \"BusinessDate\" IS NOT NULL AND \"PurchaseMode\" IS NOT NULL AND \"PurchaseMode\" IN ('FullPayment', 'Reservation', 'Installment', 'StandaloneSingleSession') AND \"ActivationMode\" IS NOT NULL AND \"ActivationMode\" IN ('Now', 'FirstCheckIn', 'SpecificDate') AND \"CommissionEligible\" IS NOT NULL AND \"TargetEligible\" IS NOT NULL AND \"NetAmount\" > 0)");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_OperationId",
                table: "Refunds",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OperationId",
                table: "Payments",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipSubscriptionTransactions_SaleTransactionId",
                table: "MembershipSubscriptionTransactions",
                column: "SaleTransactionId",
                unique: true,
                filter: "\"OperationId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InstallmentPaymentAllocations_InstallmentId",
                table: "InstallmentPaymentAllocations",
                column: "InstallmentId");

            migrationBuilder.CreateIndex(
                name: "IX_InstallmentPaymentAllocations_PaymentId_InstallmentId",
                table: "InstallmentPaymentAllocations",
                columns: new[] { "PaymentId", "InstallmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleBenefitSnapshots_MembershipBenefitId",
                table: "SaleBenefitSnapshots",
                column: "MembershipBenefitId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleBenefitSnapshots_SaleTransactionId_MembershipBenefitId",
                table: "SaleBenefitSnapshots",
                columns: new[] { "SaleTransactionId", "MembershipBenefitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleMembershipSnapshots_CityId",
                table: "SaleMembershipSnapshots",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleMembershipSnapshots_FreelanceCoachEmployeeId",
                table: "SaleMembershipSnapshots",
                column: "FreelanceCoachEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleMembershipSnapshots_MembershipPlanId",
                table: "SaleMembershipSnapshots",
                column: "MembershipPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleTransactions_AspNetUsers_CancelledByUserId",
                table: "SaleTransactions",
                column: "CancelledByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleTransactions_AspNetUsers_CancelledByUserId",
                table: "SaleTransactions");

            migrationBuilder.DropTable(
                name: "InstallmentPaymentAllocations");

            migrationBuilder.DropTable(
                name: "SaleBenefitSnapshots");

            migrationBuilder.DropTable(
                name: "SaleMembershipSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_SaleTransactions_CancelledByUserId",
                table: "SaleTransactions");

            migrationBuilder.DropIndex(
                name: "IX_SaleTransactions_OperationId",
                table: "SaleTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SaleTransactions_Activation",
                table: "SaleTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SaleTransactions_Cancellation",
                table: "SaleTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SaleTransactions_ExtraDays",
                table: "SaleTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SaleTransactions_Workflow",
                table: "SaleTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Refunds_OperationId",
                table: "Refunds");

            migrationBuilder.DropIndex(
                name: "IX_Payments_OperationId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_MembershipSubscriptionTransactions_SaleTransactionId",
                table: "MembershipSubscriptionTransactions");

            migrationBuilder.DropColumn(
                name: "ActivationMode",
                table: "SaleTransactions");

            migrationBuilder.DropColumn(
                name: "BusinessDate",
                table: "SaleTransactions");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "SaleTransactions");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "SaleTransactions");

            migrationBuilder.DropColumn(
                name: "CancelledByUserId",
                table: "SaleTransactions");

            migrationBuilder.DropColumn(
                name: "CommissionEligible",
                table: "SaleTransactions");

            migrationBuilder.DropColumn(
                name: "ExtraDaysSnapshot",
                table: "SaleTransactions");

            migrationBuilder.DropColumn(
                name: "InitialPaymentRequired",
                table: "SaleTransactions");

            migrationBuilder.DropColumn(
                name: "OfferNameSnapshot",
                table: "SaleTransactions");

            migrationBuilder.DropColumn(
                name: "OfferTypeSnapshot",
                table: "SaleTransactions");

            migrationBuilder.DropColumn(
                name: "OfferValueSnapshot",
                table: "SaleTransactions");

            migrationBuilder.DropColumn(
                name: "OperationId",
                table: "SaleTransactions");

            migrationBuilder.DropColumn(
                name: "PurchaseMode",
                table: "SaleTransactions");

            migrationBuilder.DropColumn(
                name: "RequestHash",
                table: "SaleTransactions");

            migrationBuilder.DropColumn(
                name: "RequestedStartDate",
                table: "SaleTransactions");

            migrationBuilder.DropColumn(
                name: "ReservationDueDate",
                table: "SaleTransactions");

            migrationBuilder.DropColumn(
                name: "TargetEligible",
                table: "SaleTransactions");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "SaleTransactions");

            migrationBuilder.DropColumn(
                name: "BusinessDate",
                table: "Refunds");

            migrationBuilder.DropColumn(
                name: "OperationId",
                table: "Refunds");

            migrationBuilder.DropColumn(
                name: "RequestHash",
                table: "Refunds");

            migrationBuilder.DropColumn(
                name: "BusinessDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "OperationId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RequestHash",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsStandaloneVisit",
                table: "MemberSubscriptions");

            migrationBuilder.DropColumn(
                name: "BusinessDate",
                table: "FinancialTransactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "ShiftId",
                table: "Payments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CommissionPeriodId",
                table: "Payments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipSubscriptionTransactions_SaleTransactionId",
                table: "MembershipSubscriptionTransactions",
                column: "SaleTransactionId");
        }
    }
}
