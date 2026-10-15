using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddShiftOverrideClosureAccountSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO "Permissions" ("Id", "Code", "Name", "Module", "Description", "IsActive", "CreatedAt")
                VALUES ('5b2bd675-2d27-4a7a-a5b8-4175680b5f19', 'Shifts.CloseOthers', 'Close another employee shift', 'Shifts',
                        'Allows an authorized Branch Manager or Owner to close another employee shift within branch scope.', TRUE, CURRENT_TIMESTAMP)
                ON CONFLICT ("Code") DO NOTHING;
                """);
            migrationBuilder.AddColumn<string>(
                name: "ClosingAccountBalancesJson",
                table: "Shifts",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosingAccountBalancesJson",
                table: "Shifts");
        }
    }
}
