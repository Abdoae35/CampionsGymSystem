using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class PreventDuplicateFreezeHolidayCompensation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DaysAlreadyCompensated",
                table: "SubscriptionFreezes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_SubscriptionFreezes_CompensatedDays",
                table: "SubscriptionFreezes",
                sql: "\"DaysAlreadyCompensated\" >= 0 AND \"DaysAlreadyCompensated\" <= \"DaysApplied\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_SubscriptionFreezes_CompensatedDays",
                table: "SubscriptionFreezes");

            migrationBuilder.DropColumn(
                name: "DaysAlreadyCompensated",
                table: "SubscriptionFreezes");
        }
    }
}
