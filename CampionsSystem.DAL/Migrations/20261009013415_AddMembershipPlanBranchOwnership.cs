using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMembershipPlanBranchOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "MembershipPlans",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipPlans_BranchId",
                table: "MembershipPlans",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipPlans_GymBranches_BranchId",
                table: "MembershipPlans",
                column: "BranchId",
                principalTable: "GymBranches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MembershipPlans_GymBranches_BranchId",
                table: "MembershipPlans");

            migrationBuilder.DropIndex(
                name: "IX_MembershipPlans_BranchId",
                table: "MembershipPlans");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "MembershipPlans");
        }
    }
}
