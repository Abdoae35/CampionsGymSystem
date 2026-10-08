using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchClientCodeSequenceSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Members_PrimaryPhone",
                table: "Members");

            migrationBuilder.AddColumn<string>(
                name: "PrimaryPhoneNormalized",
                table: "Members",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCallingCode",
                table: "GymBranches",
                type: "character varying(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastMemberClientCode",
                table: "GymBranches",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            // Existing branch-local codes are preserved; allocation starts beyond their maximum.
            migrationBuilder.Sql("""
                UPDATE "GymBranches" b SET "LastMemberClientCode" = COALESCE(
                    (SELECT MAX(m."ClientCode") FROM "Members" m WHERE m."HomeBranchId" = b."Id"), 0);
                WITH candidates AS (
                    SELECT "Id", regexp_replace(btrim("PrimaryPhone"), '[[:space:]().-]', '', 'g') AS phone
                    FROM "Members"
                ), safe AS (
                    SELECT *, count(*) OVER (PARTITION BY phone) AS occurrences FROM candidates
                    WHERE phone ~ '^[+][1-9][0-9]*$' AND length(phone) BETWEEN 8 AND 16
                )
                UPDATE "Members" m SET "PrimaryPhoneNormalized" = s.phone FROM safe s
                WHERE m."Id" = s."Id" AND s.occurrences = 1;
                DO $$ BEGIN
                    RAISE NOTICE 'Member phones requiring explicit reconciliation: %',
                        (SELECT count(*) FROM "Members" WHERE "PrimaryPhoneNormalized" IS NULL);
                END $$;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Members_PrimaryPhone",
                table: "Members",
                column: "PrimaryPhone");

            migrationBuilder.CreateIndex(
                name: "IX_Members_PrimaryPhoneNormalized",
                table: "Members",
                column: "PrimaryPhoneNormalized",
                unique: true,
                filter: "\"PrimaryPhoneNormalized\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Members_PrimaryPhoneNormalized",
                table: "Members",
                sql: "\"PrimaryPhoneNormalized\" IS NULL OR \"PrimaryPhoneNormalized\" ~ '^[+][1-9][0-9]*$' AND length(\"PrimaryPhoneNormalized\") BETWEEN 8 AND 16");

            migrationBuilder.AddCheckConstraint(
                name: "CK_GymBranches_CountryCallingCode",
                table: "GymBranches",
                sql: "\"CountryCallingCode\" IS NULL OR \"CountryCallingCode\" ~ '^[+][1-9][0-9]*$' AND length(\"CountryCallingCode\") BETWEEN 2 AND 4");

            migrationBuilder.AddCheckConstraint(
                name: "CK_GymBranches_LastMemberClientCode",
                table: "GymBranches",
                sql: "\"LastMemberClientCode\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Members_PrimaryPhone",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_PrimaryPhoneNormalized",
                table: "Members");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Members_PrimaryPhoneNormalized",
                table: "Members");

            migrationBuilder.DropCheckConstraint(
                name: "CK_GymBranches_CountryCallingCode",
                table: "GymBranches");

            migrationBuilder.DropCheckConstraint(
                name: "CK_GymBranches_LastMemberClientCode",
                table: "GymBranches");

            migrationBuilder.DropColumn(
                name: "PrimaryPhoneNormalized",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "CountryCallingCode",
                table: "GymBranches");

            migrationBuilder.DropColumn(
                name: "LastMemberClientCode",
                table: "GymBranches");

            migrationBuilder.CreateIndex(
                name: "IX_Members_PrimaryPhone",
                table: "Members",
                column: "PrimaryPhone",
                unique: true);
        }
    }
}
