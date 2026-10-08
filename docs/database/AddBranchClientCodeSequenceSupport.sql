START TRANSACTION;
DROP INDEX "IX_Members_PrimaryPhone";

ALTER TABLE "Members" ADD "PrimaryPhoneNormalized" character varying(16);

ALTER TABLE "GymBranches" ADD "CountryCallingCode" character varying(4);

ALTER TABLE "GymBranches" ADD "LastMemberClientCode" bigint NOT NULL DEFAULT 0;

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

CREATE INDEX "IX_Members_PrimaryPhone" ON "Members" ("PrimaryPhone");

CREATE UNIQUE INDEX "IX_Members_PrimaryPhoneNormalized" ON "Members" ("PrimaryPhoneNormalized") WHERE "PrimaryPhoneNormalized" IS NOT NULL;

ALTER TABLE "Members" ADD CONSTRAINT "CK_Members_PrimaryPhoneNormalized" CHECK ("PrimaryPhoneNormalized" IS NULL OR "PrimaryPhoneNormalized" ~ '^[+][1-9][0-9]*$' AND length("PrimaryPhoneNormalized") BETWEEN 8 AND 16);

ALTER TABLE "GymBranches" ADD CONSTRAINT "CK_GymBranches_CountryCallingCode" CHECK ("CountryCallingCode" IS NULL OR "CountryCallingCode" ~ '^[+][1-9][0-9]*$' AND length("CountryCallingCode") BETWEEN 2 AND 4);

ALTER TABLE "GymBranches" ADD CONSTRAINT "CK_GymBranches_LastMemberClientCode" CHECK ("LastMemberClientCode" >= 0);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261008110432_AddBranchClientCodeSequenceSupport', '10.0.8');

COMMIT;

