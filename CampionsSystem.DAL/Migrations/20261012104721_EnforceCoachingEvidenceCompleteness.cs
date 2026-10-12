using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class EnforceCoachingEvidenceCompleteness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PrivateSessions_Operational",
                table: "PrivateSessions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CoachEarnings_Rate",
                table: "CoachEarnings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CheckIns_ClassesSubscription",
                table: "CheckIns");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PrivateSessions_Operational",
                table: "PrivateSessions",
                sql: "\"OperationId\" IS NULL OR (\"BusinessDate\" IS NOT NULL AND \"RequestHash\" IS NOT NULL AND \"PrivateCoachAssignmentId\" IS NOT NULL AND \"SessionValue\" IS NOT NULL AND \"SessionValue\" >= 0 AND \"SessionNumber\" IS NOT NULL AND \"SessionNumber\" > 0 AND \"TransferredPackage\" IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PrivateExpirySettlements_Operational",
                table: "PrivateExpirySettlements",
                sql: "\"OperationId\" IS NULL OR (\"RequestHash\" IS NOT NULL AND \"BusinessDate\" IS NOT NULL AND \"CreatedByUserId\" IS NOT NULL AND \"PrivateCoachAssignmentId\" IS NOT NULL AND \"TransferredPackage\" IS NOT NULL AND \"Reason\" IS NOT NULL AND \"RemainderValue\" >= 0)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CoachEarnings_Rate",
                table: "CoachEarnings",
                sql: "(\"TransferredPackage\" AND \"Rate\" = 50) OR (NOT \"TransferredPackage\" AND \"CoachTargetRuleId\" IS NOT NULL AND \"Target\" IS NOT NULL AND \"Target\" > 0 AND ((\"TargetProgress\" >= \"Target\" AND \"Rate\" = 60) OR (\"TargetProgress\" < \"Target\" AND \"Rate\" = 50)))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ClassSessions_Operational",
                table: "ClassSessions",
                sql: "\"OperationId\" IS NULL OR (\"RequestHash\" IS NOT NULL AND \"BusinessDate\" IS NOT NULL AND \"CreatedByUserId\" IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CheckIns_ClassesSubscription",
                table: "CheckIns",
                sql: "\"ClassesSubscriptionId\" IS NULL OR (\"CheckInType\" = 'Class' AND \"ClassSessionId\" IS NOT NULL AND \"SessionConsumed\" AND \"OperationId\" IS NOT NULL AND \"RequestHash\" IS NOT NULL AND \"CoachEmployeeId\" IS NOT NULL AND \"SessionsBefore\" IS NOT NULL AND \"SessionsAfter\" IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PrivateSessions_Operational",
                table: "PrivateSessions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PrivateExpirySettlements_Operational",
                table: "PrivateExpirySettlements");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CoachEarnings_Rate",
                table: "CoachEarnings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ClassSessions_Operational",
                table: "ClassSessions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CheckIns_ClassesSubscription",
                table: "CheckIns");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PrivateSessions_Operational",
                table: "PrivateSessions",
                sql: "\"OperationId\" IS NULL OR (\"BusinessDate\" IS NOT NULL AND \"RequestHash\" IS NOT NULL AND \"PrivateCoachAssignmentId\" IS NOT NULL AND \"SessionValue\" >= 0 AND \"SessionNumber\" > 0 AND \"TransferredPackage\" IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CoachEarnings_Rate",
                table: "CoachEarnings",
                sql: "(\"TransferredPackage\" AND \"Rate\" = 50) OR (NOT \"TransferredPackage\" AND \"CoachTargetRuleId\" IS NOT NULL AND \"Target\" > 0 AND ((\"TargetProgress\" >= \"Target\" AND \"Rate\" = 60) OR (\"TargetProgress\" < \"Target\" AND \"Rate\" = 50)))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CheckIns_ClassesSubscription",
                table: "CheckIns",
                sql: "\"ClassesSubscriptionId\" IS NULL OR (\"CheckInType\" = 'Class' AND \"ClassSessionId\" IS NOT NULL AND \"SessionConsumed\")");
        }
    }
}
