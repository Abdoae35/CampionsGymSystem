using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCrmLeadManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Leads_BranchId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_ConvertedMemberId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_LeadAssignments_LeadId",
                table: "LeadAssignments");

            migrationBuilder.DropIndex(
                name: "IX_LeadAssignments_SalesEmployeeId",
                table: "LeadAssignments");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ConvertedAt",
                table: "Leads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LostAt",
                table: "Leads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LostReason",
                table: "Leads",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NextFollowUpAt",
                table: "Leads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Leads",
                type: "character varying(4096)",
                maxLength: 4096,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNormalized",
                table: "Leads",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryPhone",
                table: "Leads",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ContactedAt",
                table: "LeadInteractions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "LeadInteractions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MemberFollowUpTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberFollowUpTasks", x => x.Id);
                    table.CheckConstraint("CK_MemberFollowUpTasks_CompletedAt", "\"CompletedAt\" IS NULL OR \"CompletedAt\" >= \"CreatedAt\"");
                    table.CheckConstraint("CK_MemberFollowUpTasks_Reason", "\"Reason\" IN ('RenewalDue','ExpiredMembership','InactiveMember','Reactivation','ManagerAssigned','Other')");
                    table.ForeignKey(
                        name: "FK_MemberFollowUpTasks_AspNetUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberFollowUpTasks_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberFollowUpTasks_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Leads_BranchId_Status_NextFollowUpAt",
                table: "Leads",
                columns: new[] { "BranchId", "Status", "NextFollowUpAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Leads_ConvertedMemberId",
                table: "Leads",
                column: "ConvertedMemberId",
                unique: true,
                filter: "\"ConvertedMemberId\" IS NOT NULL");

            // Only safely derivable, unique phones are backfilled. Never guess a country.
            migrationBuilder.Sql("""
                WITH candidates AS (
                    SELECT l."Id", CASE
                        WHEN regexp_replace(btrim(l."Phone"), '[[:space:]().-]', '', 'g') ~ '^[+][1-9][0-9]*$'
                            THEN regexp_replace(btrim(l."Phone"), '[[:space:]().-]', '', 'g')
                        WHEN b."CountryCallingCode" IS NOT NULL AND regexp_replace(btrim(l."Phone"), '[[:space:]().-]', '', 'g') ~ '^[0-9]+$'
                            THEN b."CountryCallingCode" || regexp_replace(regexp_replace(btrim(l."Phone"), '[[:space:]().-]', '', 'g'), '^0', '')
                        ELSE NULL END AS phone
                    FROM "Leads" l JOIN "GymBranches" b ON b."Id" = l."BranchId"
                ), unique_phones AS (
                    SELECT phone FROM candidates WHERE length(phone) BETWEEN 8 AND 16
                    GROUP BY phone HAVING count(*) = 1
                )
                UPDATE "Leads" l SET "PhoneNormalized" = c.phone FROM candidates c
                JOIN unique_phones u ON u.phone = c.phone WHERE l."Id" = c."Id";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Leads_PhoneNormalized",
                table: "Leads",
                column: "PhoneNormalized",
                unique: true,
                filter: "\"PhoneNormalized\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LeadInteractions_CreatedByUserId",
                table: "LeadInteractions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadAssignments_LeadId",
                table: "LeadAssignments",
                column: "LeadId",
                unique: true,
                filter: "\"EndedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LeadAssignments_SalesEmployeeId_EndedAt",
                table: "LeadAssignments",
                columns: new[] { "SalesEmployeeId", "EndedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MemberFollowUpTasks_CompletedAt_DueAt",
                table: "MemberFollowUpTasks",
                columns: new[] { "CompletedAt", "DueAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MemberFollowUpTasks_CompletedByUserId",
                table: "MemberFollowUpTasks",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberFollowUpTasks_CreatedByUserId",
                table: "MemberFollowUpTasks",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberFollowUpTasks_MemberId_Reason",
                table: "MemberFollowUpTasks",
                columns: new[] { "MemberId", "Reason" },
                unique: true,
                filter: "\"CompletedAt\" IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_LeadInteractions_AspNetUsers_CreatedByUserId",
                table: "LeadInteractions",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeadInteractions_AspNetUsers_CreatedByUserId",
                table: "LeadInteractions");

            migrationBuilder.DropTable(
                name: "MemberFollowUpTasks");

            migrationBuilder.DropIndex(
                name: "IX_Leads_BranchId_Status_NextFollowUpAt",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_ConvertedMemberId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_PhoneNormalized",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_LeadInteractions_CreatedByUserId",
                table: "LeadInteractions");

            migrationBuilder.DropIndex(
                name: "IX_LeadAssignments_LeadId",
                table: "LeadAssignments");

            migrationBuilder.DropIndex(
                name: "IX_LeadAssignments_SalesEmployeeId_EndedAt",
                table: "LeadAssignments");

            migrationBuilder.DropColumn(
                name: "ConvertedAt",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LostAt",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LostReason",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "NextFollowUpAt",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "PhoneNormalized",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "SecondaryPhone",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ContactedAt",
                table: "LeadInteractions");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "LeadInteractions");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_BranchId",
                table: "Leads",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_ConvertedMemberId",
                table: "Leads",
                column: "ConvertedMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadAssignments_LeadId",
                table: "LeadAssignments",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadAssignments_SalesEmployeeId",
                table: "LeadAssignments",
                column: "SalesEmployeeId");
        }
    }
}
