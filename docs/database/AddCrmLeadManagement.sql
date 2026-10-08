START TRANSACTION;
DROP INDEX "IX_Leads_BranchId";

DROP INDEX "IX_Leads_ConvertedMemberId";

DROP INDEX "IX_LeadAssignments_LeadId";

DROP INDEX "IX_LeadAssignments_SalesEmployeeId";

ALTER TABLE "Leads" ADD "ConvertedAt" timestamp with time zone;

ALTER TABLE "Leads" ADD "LostAt" timestamp with time zone;

ALTER TABLE "Leads" ADD "LostReason" character varying(2048);

ALTER TABLE "Leads" ADD "NextFollowUpAt" timestamp with time zone;

ALTER TABLE "Leads" ADD "Notes" character varying(4096);

ALTER TABLE "Leads" ADD "PhoneNormalized" character varying(16);

ALTER TABLE "Leads" ADD "SecondaryPhone" character varying(64);

ALTER TABLE "LeadInteractions" ADD "ContactedAt" timestamp with time zone;

ALTER TABLE "LeadInteractions" ADD "CreatedByUserId" uuid;

CREATE TABLE "MemberFollowUpTasks" (
    "Id" uuid NOT NULL,
    "MemberId" uuid NOT NULL,
    "Reason" character varying(40) NOT NULL,
    "DueAt" timestamp with time zone NOT NULL,
    "Notes" character varying(4096),
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedByUserId" uuid,
    "CompletedAt" timestamp with time zone,
    "CompletedByUserId" uuid,
    CONSTRAINT "PK_MemberFollowUpTasks" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_MemberFollowUpTasks_CompletedAt" CHECK ("CompletedAt" IS NULL OR "CompletedAt" >= "CreatedAt"),
    CONSTRAINT "CK_MemberFollowUpTasks_Reason" CHECK ("Reason" IN ('RenewalDue','ExpiredMembership','InactiveMember','Reactivation','ManagerAssigned','Other')),
    CONSTRAINT "FK_MemberFollowUpTasks_AspNetUsers_CompletedByUserId" FOREIGN KEY ("CompletedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_MemberFollowUpTasks_AspNetUsers_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_MemberFollowUpTasks_Members_MemberId" FOREIGN KEY ("MemberId") REFERENCES "Members" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_Leads_BranchId_Status_NextFollowUpAt" ON "Leads" ("BranchId", "Status", "NextFollowUpAt");

CREATE UNIQUE INDEX "IX_Leads_ConvertedMemberId" ON "Leads" ("ConvertedMemberId") WHERE "ConvertedMemberId" IS NOT NULL;

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

CREATE UNIQUE INDEX "IX_Leads_PhoneNormalized" ON "Leads" ("PhoneNormalized") WHERE "PhoneNormalized" IS NOT NULL;

CREATE INDEX "IX_LeadInteractions_CreatedByUserId" ON "LeadInteractions" ("CreatedByUserId");

CREATE UNIQUE INDEX "IX_LeadAssignments_LeadId" ON "LeadAssignments" ("LeadId") WHERE "EndedAt" IS NULL;

CREATE INDEX "IX_LeadAssignments_SalesEmployeeId_EndedAt" ON "LeadAssignments" ("SalesEmployeeId", "EndedAt");

CREATE INDEX "IX_MemberFollowUpTasks_CompletedAt_DueAt" ON "MemberFollowUpTasks" ("CompletedAt", "DueAt");

CREATE INDEX "IX_MemberFollowUpTasks_CompletedByUserId" ON "MemberFollowUpTasks" ("CompletedByUserId");

CREATE INDEX "IX_MemberFollowUpTasks_CreatedByUserId" ON "MemberFollowUpTasks" ("CreatedByUserId");

CREATE UNIQUE INDEX "IX_MemberFollowUpTasks_MemberId_Reason" ON "MemberFollowUpTasks" ("MemberId", "Reason") WHERE "CompletedAt" IS NULL;

ALTER TABLE "LeadInteractions" ADD CONSTRAINT "FK_LeadInteractions_AspNetUsers_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261008143406_AddCrmLeadManagement', '10.0.8');

COMMIT;

START TRANSACTION;
ALTER TABLE "Leads" ALTER COLUMN "FullName" DROP NOT NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261008162710_AllowPhoneOnlyCrmLeads', '10.0.8');

COMMIT;

