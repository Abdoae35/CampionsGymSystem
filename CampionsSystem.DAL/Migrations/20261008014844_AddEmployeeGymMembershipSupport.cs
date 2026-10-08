using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeGymMembershipSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_MembershipPlans_DurationDays",
                table: "MembershipPlans");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MembershipPlans_PlanCategoryCoach",
                table: "MembershipPlans");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "EndDate",
                table: "MemberSubscriptions",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeId",
                table: "MemberSubscriptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MemberId",
                table: "Employees",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberSubscriptions_EmployeeId",
                table: "MemberSubscriptions",
                column: "EmployeeId",
                unique: true,
                filter: "\"EmployeeId\" IS NOT NULL AND \"EndDate\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MemberSubscriptions_EmployeeId_StartDate",
                table: "MemberSubscriptions",
                columns: new[] { "EmployeeId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MemberSubscriptions_OpenEmployeeMember",
                table: "MemberSubscriptions",
                column: "MemberId",
                unique: true,
                filter: "\"EmployeeId\" IS NOT NULL AND \"EndDate\" IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MemberSubscriptions_EmploymentPeriod",
                table: "MemberSubscriptions",
                sql: "(\"EmployeeId\" IS NULL AND \"EndDate\" IS NOT NULL) OR (\"EmployeeId\" IS NOT NULL AND \"AccessMode\" = 'Unlimited' AND ((\"EndDate\" IS NULL AND \"Status\" IN ('Scheduled', 'Active')) OR (\"EndDate\" IS NOT NULL AND \"Status\" IN ('Expired', 'Cancelled'))))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MembershipPlans_DurationDays",
                table: "MembershipPlans",
                sql: "(\"PlanCategory\" = 'Employee' AND \"DurationDays\" = 0) OR (\"PlanCategory\" <> 'Employee' AND \"DurationDays\" > 0)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MembershipPlans_Employee",
                table: "MembershipPlans",
                sql: "\"PlanCategory\" <> 'Employee' OR (\"Price\" = 0 AND \"AccessMode\" = 'Unlimited' AND \"SessionCount\" IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MembershipPlans_PlanCategoryCoach",
                table: "MembershipPlans",
                sql: "(\"PlanCategory\" IN ('Normal', 'Employee') AND \"FreelanceCoachEmployeeId\" IS NULL) OR (\"PlanCategory\" = 'Freelance' AND \"FreelanceCoachEmployeeId\" IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_MemberId",
                table: "Employees",
                column: "MemberId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Members_MemberId",
                table: "Employees",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSubscriptions_Employees_EmployeeId",
                table: "MemberSubscriptions",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Members_MemberId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberSubscriptions_Employees_EmployeeId",
                table: "MemberSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_MemberSubscriptions_EmployeeId",
                table: "MemberSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_MemberSubscriptions_EmployeeId_StartDate",
                table: "MemberSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_MemberSubscriptions_OpenEmployeeMember",
                table: "MemberSubscriptions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MemberSubscriptions_EmploymentPeriod",
                table: "MemberSubscriptions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MembershipPlans_DurationDays",
                table: "MembershipPlans");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MembershipPlans_Employee",
                table: "MembershipPlans");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MembershipPlans_PlanCategoryCoach",
                table: "MembershipPlans");

            migrationBuilder.DropIndex(
                name: "IX_Employees_MemberId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "MemberSubscriptions");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "Employees");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "EndDate",
                table: "MemberSubscriptions",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_MembershipPlans_DurationDays",
                table: "MembershipPlans",
                sql: "\"DurationDays\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MembershipPlans_PlanCategoryCoach",
                table: "MembershipPlans",
                sql: "(\"PlanCategory\" = 'Normal' AND \"FreelanceCoachEmployeeId\" IS NULL) OR (\"PlanCategory\" = 'Freelance' AND \"FreelanceCoachEmployeeId\" IS NOT NULL)");
        }
    }
}
