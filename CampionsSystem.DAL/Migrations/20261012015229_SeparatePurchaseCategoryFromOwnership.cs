using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SeparatePurchaseCategoryFromOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_SaleTransactions_Attribution",
                table: "SaleTransactions");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SaleTransactions_Attribution",
                table: "SaleTransactions",
                sql: "\"SalesAttributionSnapshot\" IS NULL OR (\"SalesAttributionSnapshot\" = 'House' AND \"SalesEmployeeId\" IS NULL) OR (\"SalesAttributionSnapshot\" = 'Sales' AND \"SalesEmployeeId\" IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_SaleTransactions_Attribution",
                table: "SaleTransactions");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SaleTransactions_Attribution",
                table: "SaleTransactions",
                sql: "\"SalesAttributionSnapshot\" IS NULL OR (\"SalesAttributionSnapshot\" = 'House' AND \"SalesEmployeeId\" IS NULL AND \"SaleCategory\" IS NULL) OR (\"SalesAttributionSnapshot\" = 'Sales' AND \"SalesEmployeeId\" IS NOT NULL)");
        }
    }
}
