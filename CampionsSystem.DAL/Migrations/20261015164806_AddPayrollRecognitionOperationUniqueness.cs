using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollRecognitionOperationUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_PayrollRecognition_Operation",
                table: "AuditLogs",
                columns: new[] { "Action", "EntityId" },
                unique: true,
                filter: "\"Action\" = 'Payroll.RecognitionOperation'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_PayrollRecognition_Operation",
                table: "AuditLogs");
        }
    }
}
