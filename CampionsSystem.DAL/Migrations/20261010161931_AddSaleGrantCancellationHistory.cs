using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleGrantCancellationHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProviderReference",
                table: "RefundParts",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BorrowedDebtSettlementEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BorrowedGymSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    GrantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsReversal = table.Column<bool>(type: "boolean", nullable: false),
                    RefundId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowedDebtSettlementEvents", x => x.Id);
                    table.CheckConstraint("CK_BorrowedDebtSettlementEvents_State", "\"Sequence\" > 0 AND \"IsReversal\" = (\"RefundId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_BorrowedDebtSettlementEvents_AspNetUsers_ActorId",
                        column: x => x.ActorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BorrowedDebtSettlementEvents_BorrowedGymSessions_BorrowedGy~",
                        column: x => x.BorrowedGymSessionId,
                        principalTable: "BorrowedGymSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BorrowedDebtSettlementEvents_MembershipSubscriptionTransact~",
                        column: x => x.GrantId,
                        principalTable: "MembershipSubscriptionTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BorrowedDebtSettlementEvents_Refunds_RefundId",
                        column: x => x.RefundId,
                        principalTable: "Refunds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionGrantCancellations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GrantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RefundId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NewEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PreviousRemainingSessions = table.Column<int>(type: "integer", nullable: true),
                    NewRemainingSessions = table.Column<int>(type: "integer", nullable: true),
                    DaysRemoved = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionGrantCancellations", x => x.Id);
                    table.CheckConstraint("CK_SubscriptionGrantCancellations_Sessions", "\"DaysRemoved\" >= 0 AND \"PreviousRemainingSessions\" >= 0 AND \"NewRemainingSessions\" >= 0");
                    table.ForeignKey(
                        name: "FK_SubscriptionGrantCancellations_MembershipSubscriptionTransa~",
                        column: x => x.GrantId,
                        principalTable: "MembershipSubscriptionTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubscriptionGrantCancellations_Refunds_RefundId",
                        column: x => x.RefundId,
                        principalTable: "Refunds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BorrowedDebtSettlementEvents_ActorId",
                table: "BorrowedDebtSettlementEvents",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowedDebtSettlementEvents_BorrowedGymSessionId_Sequence",
                table: "BorrowedDebtSettlementEvents",
                columns: new[] { "BorrowedGymSessionId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BorrowedDebtSettlementEvents_GrantId",
                table: "BorrowedDebtSettlementEvents",
                column: "GrantId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowedDebtSettlementEvents_RefundId",
                table: "BorrowedDebtSettlementEvents",
                column: "RefundId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionGrantCancellations_GrantId",
                table: "SubscriptionGrantCancellations",
                column: "GrantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionGrantCancellations_RefundId",
                table: "SubscriptionGrantCancellations",
                column: "RefundId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BorrowedDebtSettlementEvents");

            migrationBuilder.DropTable(
                name: "SubscriptionGrantCancellations");

            migrationBuilder.DropColumn(
                name: "ProviderReference",
                table: "RefundParts");
        }
    }
}
