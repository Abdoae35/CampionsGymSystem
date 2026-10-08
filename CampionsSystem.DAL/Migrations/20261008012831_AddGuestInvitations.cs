using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CheckIns_CheckInType",
                table: "CheckIns");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CheckIns_SubscriptionType",
                table: "CheckIns");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberId",
                table: "CheckIns",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateTable(
                name: "GuestInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InviteeMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    InviteeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InviteePhone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    InviteePhoneNormalized = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Source = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    InvitedByMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceSubscriptionBenefitId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RedeemedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RedeemedCheckInId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestInvitations", x => x.Id);
                    table.CheckConstraint("CK_GuestInvitations_Cancellation", "(\"Status\" = 'Cancelled' AND \"CancelledAt\" IS NOT NULL) OR (\"Status\" <> 'Cancelled' AND \"CancelledAt\" IS NULL)");
                    table.CheckConstraint("CK_GuestInvitations_Identity", "btrim(\"InviteeName\") <> '' AND btrim(\"InviteePhone\") <> '' AND btrim(\"InviteePhoneNormalized\") <> '' AND \"InviteePhoneNormalized\" = btrim(\"InviteePhoneNormalized\")");
                    table.CheckConstraint("CK_GuestInvitations_Inviter", "(\"Source\" = 'Gym' AND \"InvitedByMemberId\" IS NULL AND \"SourceSubscriptionBenefitId\" IS NULL) OR (\"Source\" = 'Member' AND \"InvitedByMemberId\" IS NOT NULL)");
                    table.CheckConstraint("CK_GuestInvitations_Redemption", "(\"Status\" = 'Redeemed' AND \"RedeemedAt\" IS NOT NULL) OR (\"Status\" <> 'Redeemed' AND \"RedeemedAt\" IS NULL AND \"RedeemedCheckInId\" IS NULL)");
                    table.CheckConstraint("CK_GuestInvitations_Source", "\"Source\" IN ('Gym', 'Member')");
                    table.CheckConstraint("CK_GuestInvitations_Status", "\"Status\" IN ('Available', 'Redeemed', 'Cancelled', 'Expired')");
                    table.CheckConstraint("CK_GuestInvitations_Timestamps", "(\"RedeemedAt\" IS NULL OR \"RedeemedAt\" >= \"CreatedAt\") AND (\"CancelledAt\" IS NULL OR \"CancelledAt\" >= \"CreatedAt\")");
                    table.ForeignKey(
                        name: "FK_GuestInvitations_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GuestInvitations_CheckIns_RedeemedCheckInId",
                        column: x => x.RedeemedCheckInId,
                        principalTable: "CheckIns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GuestInvitations_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GuestInvitations_MemberSubscriptionBenefits_SourceSubscript~",
                        column: x => x.SourceSubscriptionBenefitId,
                        principalTable: "MemberSubscriptionBenefits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GuestInvitations_Members_InvitedByMemberId",
                        column: x => x.InvitedByMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GuestInvitations_Members_InviteeMemberId",
                        column: x => x.InviteeMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_CheckIns_CheckInType",
                table: "CheckIns",
                sql: "\"CheckInType\" IN ('Gym', 'BorrowedGym', 'Activity', 'Class', 'Trial')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CheckIns_Member",
                table: "CheckIns",
                sql: "\"CheckInType\" = 'Trial' OR \"MemberId\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CheckIns_SubscriptionType",
                table: "CheckIns",
                sql: "(\"CheckInType\" IN ('Gym', 'BorrowedGym') AND \"MemberSubscriptionId\" IS NOT NULL AND \"ActivitySubscriptionId\" IS NULL AND \"GroupClassId\" IS NULL) OR (\"CheckInType\" = 'Activity' AND \"ActivitySubscriptionId\" IS NOT NULL AND \"MemberSubscriptionId\" IS NULL AND \"GroupClassId\" IS NULL) OR (\"CheckInType\" = 'Class' AND \"GroupClassId\" IS NOT NULL AND \"MemberSubscriptionId\" IS NULL AND \"ActivitySubscriptionId\" IS NULL) OR (\"CheckInType\" = 'Trial' AND \"MemberSubscriptionId\" IS NULL AND \"ActivitySubscriptionId\" IS NULL AND \"GroupClassId\" IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_GuestInvitations_BranchId_Status_CreatedAt",
                table: "GuestInvitations",
                columns: new[] { "BranchId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GuestInvitations_CreatedByUserId",
                table: "GuestInvitations",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestInvitations_InvitedByMemberId",
                table: "GuestInvitations",
                column: "InvitedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestInvitations_InviteeMemberId",
                table: "GuestInvitations",
                column: "InviteeMemberId",
                unique: true,
                filter: "\"Status\" = 'Redeemed' AND \"InviteeMemberId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GuestInvitations_InviteePhoneNormalized",
                table: "GuestInvitations",
                column: "InviteePhoneNormalized",
                unique: true,
                filter: "\"Status\" = 'Redeemed'");

            migrationBuilder.CreateIndex(
                name: "IX_GuestInvitations_RedeemedCheckInId",
                table: "GuestInvitations",
                column: "RedeemedCheckInId",
                unique: true,
                filter: "\"RedeemedCheckInId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GuestInvitations_SourceSubscriptionBenefitId",
                table: "GuestInvitations",
                column: "SourceSubscriptionBenefitId");
            // Keep redeemed identities reserved even for direct SQL/bulk operations.
            migrationBuilder.Sql("""
                CREATE FUNCTION "PreserveRedeemedGuestInvitation"() RETURNS trigger
                LANGUAGE plpgsql AS $$
                BEGIN
                    IF OLD."Status" = 'Redeemed' THEN
                        IF TG_OP = 'DELETE' THEN
                            RAISE EXCEPTION 'Redeemed invitations are historical records' USING ERRCODE = '23514';
                        END IF;
                        IF NEW."Status" IS DISTINCT FROM OLD."Status"
                           OR NEW."InviteePhoneNormalized" IS DISTINCT FROM OLD."InviteePhoneNormalized"
                           OR NEW."RedeemedAt" IS DISTINCT FROM OLD."RedeemedAt"
                           OR (OLD."InviteeMemberId" IS NOT NULL AND NEW."InviteeMemberId" IS DISTINCT FROM OLD."InviteeMemberId")
                           OR (OLD."RedeemedCheckInId" IS NOT NULL AND NEW."RedeemedCheckInId" IS DISTINCT FROM OLD."RedeemedCheckInId") THEN
                            RAISE EXCEPTION 'Redeemed invitation identity and redemption are immutable' USING ERRCODE = '23514';
                        END IF;
                    END IF;
                    IF TG_OP = 'DELETE' THEN RETURN OLD; END IF;
                    RETURN NEW;
                END;
                $$;
                CREATE TRIGGER "TR_GuestInvitations_PreserveRedemption"
                BEFORE UPDATE OR DELETE ON "GuestInvitations"
                FOR EACH ROW EXECUTE FUNCTION "PreserveRedeemedGuestInvitation"();
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuestInvitations");
            migrationBuilder.Sql("""DROP FUNCTION "PreserveRedeemedGuestInvitation"();""");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CheckIns_CheckInType",
                table: "CheckIns");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CheckIns_Member",
                table: "CheckIns");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CheckIns_SubscriptionType",
                table: "CheckIns");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberId",
                table: "CheckIns",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_CheckIns_CheckInType",
                table: "CheckIns",
                sql: "\"CheckInType\" IN ('Gym', 'BorrowedGym', 'Activity', 'Class')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CheckIns_SubscriptionType",
                table: "CheckIns",
                sql: "(\"CheckInType\" IN ('Gym', 'BorrowedGym') AND \"MemberSubscriptionId\" IS NOT NULL AND \"ActivitySubscriptionId\" IS NULL AND \"GroupClassId\" IS NULL) OR (\"CheckInType\" = 'Activity' AND \"ActivitySubscriptionId\" IS NOT NULL AND \"MemberSubscriptionId\" IS NULL AND \"GroupClassId\" IS NULL) OR (\"CheckInType\" = 'Class' AND \"GroupClassId\" IS NOT NULL AND \"MemberSubscriptionId\" IS NULL AND \"ActivitySubscriptionId\" IS NULL)");
        }
    }
}
