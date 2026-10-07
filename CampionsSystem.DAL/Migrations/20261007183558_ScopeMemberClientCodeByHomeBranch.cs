using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ScopeMemberClientCodeByHomeBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Members_ClientCode",
                table: "Members");

            migrationBuilder.AlterColumn<long>(
                name: "ClientCode",
                table: "Members",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateIndex(
                name: "IX_Members_HomeBranchId_ClientCode",
                table: "Members",
                columns: new[] { "HomeBranchId", "ClientCode" },
                unique: true,
                filter: "\"ClientCode\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Members_HomeBranchId_ClientCode",
                table: "Members");

            migrationBuilder.AlterColumn<long>(
                name: "ClientCode",
                table: "Members",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_ClientCode",
                table: "Members",
                column: "ClientCode",
                unique: true);
        }
    }
}
