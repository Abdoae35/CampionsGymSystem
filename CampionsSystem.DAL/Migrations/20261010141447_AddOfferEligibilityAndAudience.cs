using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddOfferEligibilityAndAudience : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MemberOfferEntitlements_OfferId",
                table: "MemberOfferEntitlements");

            migrationBuilder.AddColumn<Guid>(
                name: "OfferAudienceCategoryIdSnapshot",
                table: "SaleTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfferEligibilityModeSnapshot",
                table: "SaleTransactions",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AudienceCategoryId",
                table: "SaleMembershipSnapshots",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AudienceCategoryId",
                table: "Offers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EligibilityMode",
                table: "Offers",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RevokedAt",
                table: "MemberOfferEntitlements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RevokedByUserId",
                table: "MemberOfferEntitlements",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MemberAudienceCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    MembershipPlanCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberAudienceCategories", x => x.Id);
                    table.CheckConstraint("CK_MemberAudienceCategories_Period", "\"EndedAt\" >= \"AssignedAt\"");
                    table.ForeignKey(
                        name: "FK_MemberAudienceCategories_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberAudienceCategories_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberAudienceCategories_MembershipPlanCategories_Membershi~",
                        column: x => x.MembershipPlanCategoryId,
                        principalTable: "MembershipPlanCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaleTransactions_OfferAudienceCategoryIdSnapshot",
                table: "SaleTransactions",
                column: "OfferAudienceCategoryIdSnapshot");

            migrationBuilder.CreateIndex(
                name: "IX_SaleMembershipSnapshots_AudienceCategoryId",
                table: "SaleMembershipSnapshots",
                column: "AudienceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_AudienceCategoryId",
                table: "Offers",
                column: "AudienceCategoryId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Offers_EligibilityMode",
                table: "Offers",
                sql: "\"EligibilityMode\" IS NULL OR \"EligibilityMode\" IN ('General', 'MemberOnly')");

            migrationBuilder.CreateIndex(
                name: "IX_MemberOfferEntitlements_OfferId_MemberId_RevokedAt",
                table: "MemberOfferEntitlements",
                columns: new[] { "OfferId", "MemberId", "RevokedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MemberOfferEntitlements_RevokedByUserId",
                table: "MemberOfferEntitlements",
                column: "RevokedByUserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MemberOfferEntitlements_Revocation",
                table: "MemberOfferEntitlements",
                sql: "(\"RevokedAt\" IS NULL AND \"RevokedByUserId\" IS NULL) OR (\"RevokedAt\" IS NOT NULL AND \"RevokedAt\" >= \"CreatedAt\" AND \"RevokedByUserId\" IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_MemberAudienceCategories_AssignedByUserId",
                table: "MemberAudienceCategories",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberAudienceCategories_MemberId_MembershipPlanCategoryId",
                table: "MemberAudienceCategories",
                columns: new[] { "MemberId", "MembershipPlanCategoryId" },
                unique: true,
                filter: "\"EndedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MemberAudienceCategories_MembershipPlanCategoryId",
                table: "MemberAudienceCategories",
                column: "MembershipPlanCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberOfferEntitlements_AspNetUsers_RevokedByUserId",
                table: "MemberOfferEntitlements",
                column: "RevokedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_MembershipPlanCategories_AudienceCategoryId",
                table: "Offers",
                column: "AudienceCategoryId",
                principalTable: "MembershipPlanCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SaleMembershipSnapshots_MembershipPlanCategories_AudienceCa~",
                table: "SaleMembershipSnapshots",
                column: "AudienceCategoryId",
                principalTable: "MembershipPlanCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SaleTransactions_MembershipPlanCategories_OfferAudienceCate~",
                table: "SaleTransactions",
                column: "OfferAudienceCategoryIdSnapshot",
                principalTable: "MembershipPlanCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MemberOfferEntitlements_AspNetUsers_RevokedByUserId",
                table: "MemberOfferEntitlements");

            migrationBuilder.DropForeignKey(
                name: "FK_Offers_MembershipPlanCategories_AudienceCategoryId",
                table: "Offers");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleMembershipSnapshots_MembershipPlanCategories_AudienceCa~",
                table: "SaleMembershipSnapshots");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleTransactions_MembershipPlanCategories_OfferAudienceCate~",
                table: "SaleTransactions");

            migrationBuilder.DropTable(
                name: "MemberAudienceCategories");

            migrationBuilder.DropIndex(
                name: "IX_SaleTransactions_OfferAudienceCategoryIdSnapshot",
                table: "SaleTransactions");

            migrationBuilder.DropIndex(
                name: "IX_SaleMembershipSnapshots_AudienceCategoryId",
                table: "SaleMembershipSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_Offers_AudienceCategoryId",
                table: "Offers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Offers_EligibilityMode",
                table: "Offers");

            migrationBuilder.DropIndex(
                name: "IX_MemberOfferEntitlements_OfferId_MemberId_RevokedAt",
                table: "MemberOfferEntitlements");

            migrationBuilder.DropIndex(
                name: "IX_MemberOfferEntitlements_RevokedByUserId",
                table: "MemberOfferEntitlements");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MemberOfferEntitlements_Revocation",
                table: "MemberOfferEntitlements");

            migrationBuilder.DropColumn(
                name: "OfferAudienceCategoryIdSnapshot",
                table: "SaleTransactions");

            migrationBuilder.DropColumn(
                name: "OfferEligibilityModeSnapshot",
                table: "SaleTransactions");

            migrationBuilder.DropColumn(
                name: "AudienceCategoryId",
                table: "SaleMembershipSnapshots");

            migrationBuilder.DropColumn(
                name: "AudienceCategoryId",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "EligibilityMode",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "RevokedAt",
                table: "MemberOfferEntitlements");

            migrationBuilder.DropColumn(
                name: "RevokedByUserId",
                table: "MemberOfferEntitlements");

            migrationBuilder.CreateIndex(
                name: "IX_MemberOfferEntitlements_OfferId",
                table: "MemberOfferEntitlements",
                column: "OfferId");
        }
    }
}
