using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class PrepareOperationalCheckIns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BorrowedGymSessions_CheckInId",
                table: "BorrowedGymSessions");

            migrationBuilder.DropIndex(
                name: "IX_BorrowedGymSessions_ExpiredSubscriptionId",
                table: "BorrowedGymSessions");

            migrationBuilder.DropIndex(
                name: "IX_BorrowedGymSessions_MemberId",
                table: "BorrowedGymSessions");

            migrationBuilder.AddColumn<DateOnly>(
                name: "BusinessDate",
                table: "CheckIns",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OperationId",
                table: "CheckIns",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SessionsAfter",
                table: "CheckIns",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SessionsBefore",
                table: "CheckIns",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SettledByTransactionId",
                table: "BorrowedGymSessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_BranchId_BusinessDate_MemberId",
                table: "CheckIns",
                columns: new[] { "BranchId", "BusinessDate", "MemberId" });

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_OperationId",
                table: "CheckIns",
                column: "OperationId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_CheckIns_OperationalDate",
                table: "CheckIns",
                sql: "\"OperationId\" IS NULL OR \"BusinessDate\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CheckIns_SessionSnapshot",
                table: "CheckIns",
                sql: "(\"SessionsBefore\" IS NULL AND \"SessionsAfter\" IS NULL) OR (\"SessionsBefore\" IS NOT NULL AND \"SessionsAfter\" IS NOT NULL AND \"SessionsBefore\" > 0 AND \"SessionsAfter\" = \"SessionsBefore\" - 1 AND \"CheckInType\" = 'Gym' AND \"SessionConsumed\")");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowedGymSessions_CheckInId",
                table: "BorrowedGymSessions",
                column: "CheckInId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BorrowedGymSessions_ExpiredSubscriptionId_Status",
                table: "BorrowedGymSessions",
                columns: new[] { "ExpiredSubscriptionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BorrowedGymSessions_MemberId_Status",
                table: "BorrowedGymSessions",
                columns: new[] { "MemberId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BorrowedGymSessions_SettledByTransactionId",
                table: "BorrowedGymSessions",
                column: "SettledByTransactionId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BorrowedGymSessions_GrantSettlement",
                table: "BorrowedGymSessions",
                sql: "\"SettledByTransactionId\" IS NULL OR (\"Status\" = 'Settled' AND \"SettledAt\" IS NOT NULL AND \"SettledBySubscriptionId\" IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowedGymSessions_MembershipSubscriptionTransactions_Sett~",
                table: "BorrowedGymSessions",
                column: "SettledByTransactionId",
                principalTable: "MembershipSubscriptionTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowedGymSessions_MembershipSubscriptionTransactions_Sett~",
                table: "BorrowedGymSessions");

            migrationBuilder.DropIndex(
                name: "IX_CheckIns_BranchId_BusinessDate_MemberId",
                table: "CheckIns");

            migrationBuilder.DropIndex(
                name: "IX_CheckIns_OperationId",
                table: "CheckIns");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CheckIns_OperationalDate",
                table: "CheckIns");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CheckIns_SessionSnapshot",
                table: "CheckIns");

            migrationBuilder.DropIndex(
                name: "IX_BorrowedGymSessions_CheckInId",
                table: "BorrowedGymSessions");

            migrationBuilder.DropIndex(
                name: "IX_BorrowedGymSessions_ExpiredSubscriptionId_Status",
                table: "BorrowedGymSessions");

            migrationBuilder.DropIndex(
                name: "IX_BorrowedGymSessions_MemberId_Status",
                table: "BorrowedGymSessions");

            migrationBuilder.DropIndex(
                name: "IX_BorrowedGymSessions_SettledByTransactionId",
                table: "BorrowedGymSessions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BorrowedGymSessions_GrantSettlement",
                table: "BorrowedGymSessions");

            migrationBuilder.DropColumn(
                name: "BusinessDate",
                table: "CheckIns");

            migrationBuilder.DropColumn(
                name: "OperationId",
                table: "CheckIns");

            migrationBuilder.DropColumn(
                name: "SessionsAfter",
                table: "CheckIns");

            migrationBuilder.DropColumn(
                name: "SessionsBefore",
                table: "CheckIns");

            migrationBuilder.DropColumn(
                name: "SettledByTransactionId",
                table: "BorrowedGymSessions");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowedGymSessions_CheckInId",
                table: "BorrowedGymSessions",
                column: "CheckInId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowedGymSessions_ExpiredSubscriptionId",
                table: "BorrowedGymSessions",
                column: "ExpiredSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowedGymSessions_MemberId",
                table: "BorrowedGymSessions",
                column: "MemberId");
        }
    }
}
