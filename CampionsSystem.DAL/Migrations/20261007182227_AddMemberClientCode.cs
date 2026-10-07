using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberClientCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ClientCode",
                table: "Members",
                type: "bigint",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_Members_ClientCode",
                table: "Members",
                column: "ClientCode",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Members_ClientCode_Positive",
                table: "Members",
                sql: "\"ClientCode\" > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Members_ClientCode",
                table: "Members");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Members_ClientCode_Positive",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "ClientCode",
                table: "Members");
        }
    }
}
