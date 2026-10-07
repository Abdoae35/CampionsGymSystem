using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMembershipBenefits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MembershipBenefits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipBenefits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MembershipPlanBenefits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MembershipPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    MembershipBenefitId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrantType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipPlanBenefits", x => x.Id);
                    table.CheckConstraint("CK_MembershipPlanBenefits_GrantQuantity", "(\"GrantType\" = 'Quantity' AND \"Quantity\" IS NOT NULL AND \"Quantity\" > 0) OR (\"GrantType\" = 'Access' AND \"Quantity\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_MembershipPlanBenefits_MembershipBenefits_MembershipBenefit~",
                        column: x => x.MembershipBenefitId,
                        principalTable: "MembershipBenefits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MembershipPlanBenefits_MembershipPlans_MembershipPlanId",
                        column: x => x.MembershipPlanId,
                        principalTable: "MembershipPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MemberSubscriptionBenefits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MembershipSubscriptionTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    MembershipBenefitId = table.Column<Guid>(type: "uuid", nullable: false),
                    BenefitCodeSnapshot = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    BenefitNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    GrantTypeSnapshot = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    GrantedQuantity = table.Column<int>(type: "integer", nullable: true),
                    GrantedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberSubscriptionBenefits", x => x.Id);
                    table.CheckConstraint("CK_MemberSubscriptionBenefits_GrantQuantity", "(\"GrantTypeSnapshot\" = 'Quantity' AND \"GrantedQuantity\" IS NOT NULL AND \"GrantedQuantity\" > 0) OR (\"GrantTypeSnapshot\" = 'Access' AND \"GrantedQuantity\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_MemberSubscriptionBenefits_MembershipBenefits_MembershipBen~",
                        column: x => x.MembershipBenefitId,
                        principalTable: "MembershipBenefits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberSubscriptionBenefits_MembershipSubscriptionTransactio~",
                        column: x => x.MembershipSubscriptionTransactionId,
                        principalTable: "MembershipSubscriptionTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MemberBenefitUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberSubscriptionBenefitId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityUsed = table.Column<int>(type: "integer", nullable: false),
                    UsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberBenefitUsages", x => x.Id);
                    table.CheckConstraint("CK_MemberBenefitUsages_QuantityUsed", "\"QuantityUsed\" > 0");
                    table.ForeignKey(
                        name: "FK_MemberBenefitUsages_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberBenefitUsages_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberBenefitUsages_MemberSubscriptionBenefits_MemberSubscr~",
                        column: x => x.MemberSubscriptionBenefitId,
                        principalTable: "MemberSubscriptionBenefits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberBenefitUsages_BranchId_UsedAt",
                table: "MemberBenefitUsages",
                columns: new[] { "BranchId", "UsedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MemberBenefitUsages_CreatedByUserId",
                table: "MemberBenefitUsages",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberBenefitUsages_MemberSubscriptionBenefitId_UsedAt",
                table: "MemberBenefitUsages",
                columns: new[] { "MemberSubscriptionBenefitId", "UsedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MembershipBenefits_Code",
                table: "MembershipBenefits",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipBenefits_Name",
                table: "MembershipBenefits",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipPlanBenefits_MembershipBenefitId",
                table: "MembershipPlanBenefits",
                column: "MembershipBenefitId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipPlanBenefits_MembershipPlanId_MembershipBenefitId",
                table: "MembershipPlanBenefits",
                columns: new[] { "MembershipPlanId", "MembershipBenefitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberSubscriptionBenefits_MembershipBenefitId",
                table: "MemberSubscriptionBenefits",
                column: "MembershipBenefitId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberSubscriptionBenefits_MembershipSubscriptionTransactio~",
                table: "MemberSubscriptionBenefits",
                columns: new[] { "MembershipSubscriptionTransactionId", "MembershipBenefitId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberBenefitUsages");

            migrationBuilder.DropTable(
                name: "MembershipPlanBenefits");

            migrationBuilder.DropTable(
                name: "MemberSubscriptionBenefits");

            migrationBuilder.DropTable(
                name: "MembershipBenefits");
        }
    }
}
