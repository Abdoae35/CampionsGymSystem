using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ImplementOwnerAccessAdministration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_AccessAdministration_Operation",
                table: "AuditLogs",
                column: "EntityId",
                unique: true,
                filter: "\"Action\" = 'AccessAdministration.Changed'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_AccessAdministration_Operation",
                table: "AuditLogs");
        }
    }
}
