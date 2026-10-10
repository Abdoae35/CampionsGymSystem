START TRANSACTION;
DROP INDEX "IX_MemberOfferEntitlements_OfferId";

ALTER TABLE "SaleTransactions" ADD "OfferAudienceCategoryIdSnapshot" uuid;

ALTER TABLE "SaleTransactions" ADD "OfferEligibilityModeSnapshot" character varying(40);

ALTER TABLE "SaleMembershipSnapshots" ADD "AudienceCategoryId" uuid;

ALTER TABLE "Offers" ADD "AudienceCategoryId" uuid;

ALTER TABLE "Offers" ADD "EligibilityMode" character varying(40);

ALTER TABLE "MemberOfferEntitlements" ADD "RevokedAt" timestamp with time zone;

ALTER TABLE "MemberOfferEntitlements" ADD "RevokedByUserId" uuid;

CREATE TABLE "MemberAudienceCategories" (
    "Id" uuid NOT NULL,
    "MemberId" uuid NOT NULL,
    "MembershipPlanCategoryId" uuid NOT NULL,
    "AssignedByUserId" uuid NOT NULL,
    "AssignedAt" timestamp with time zone NOT NULL,
    "EndedAt" timestamp with time zone,
    CONSTRAINT "PK_MemberAudienceCategories" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_MemberAudienceCategories_Period" CHECK ("EndedAt" >= "AssignedAt"),
    CONSTRAINT "FK_MemberAudienceCategories_AspNetUsers_AssignedByUserId" FOREIGN KEY ("AssignedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_MemberAudienceCategories_Members_MemberId" FOREIGN KEY ("MemberId") REFERENCES "Members" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_MemberAudienceCategories_MembershipPlanCategories_Membershi~" FOREIGN KEY ("MembershipPlanCategoryId") REFERENCES "MembershipPlanCategories" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_SaleTransactions_OfferAudienceCategoryIdSnapshot" ON "SaleTransactions" ("OfferAudienceCategoryIdSnapshot");

CREATE INDEX "IX_SaleMembershipSnapshots_AudienceCategoryId" ON "SaleMembershipSnapshots" ("AudienceCategoryId");

CREATE INDEX "IX_Offers_AudienceCategoryId" ON "Offers" ("AudienceCategoryId");

ALTER TABLE "Offers" ADD CONSTRAINT "CK_Offers_EligibilityMode" CHECK ("EligibilityMode" IS NULL OR "EligibilityMode" IN ('General', 'MemberOnly'));

CREATE INDEX "IX_MemberOfferEntitlements_OfferId_MemberId_RevokedAt" ON "MemberOfferEntitlements" ("OfferId", "MemberId", "RevokedAt");

CREATE INDEX "IX_MemberOfferEntitlements_RevokedByUserId" ON "MemberOfferEntitlements" ("RevokedByUserId");

ALTER TABLE "MemberOfferEntitlements" ADD CONSTRAINT "CK_MemberOfferEntitlements_Revocation" CHECK (("RevokedAt" IS NULL AND "RevokedByUserId" IS NULL) OR ("RevokedAt" IS NOT NULL AND "RevokedAt" >= "CreatedAt" AND "RevokedByUserId" IS NOT NULL));

CREATE INDEX "IX_MemberAudienceCategories_AssignedByUserId" ON "MemberAudienceCategories" ("AssignedByUserId");

CREATE UNIQUE INDEX "IX_MemberAudienceCategories_MemberId_MembershipPlanCategoryId" ON "MemberAudienceCategories" ("MemberId", "MembershipPlanCategoryId") WHERE "EndedAt" IS NULL;

CREATE INDEX "IX_MemberAudienceCategories_MembershipPlanCategoryId" ON "MemberAudienceCategories" ("MembershipPlanCategoryId");

ALTER TABLE "MemberOfferEntitlements" ADD CONSTRAINT "FK_MemberOfferEntitlements_AspNetUsers_RevokedByUserId" FOREIGN KEY ("RevokedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT;

ALTER TABLE "Offers" ADD CONSTRAINT "FK_Offers_MembershipPlanCategories_AudienceCategoryId" FOREIGN KEY ("AudienceCategoryId") REFERENCES "MembershipPlanCategories" ("Id") ON DELETE RESTRICT;

ALTER TABLE "SaleMembershipSnapshots" ADD CONSTRAINT "FK_SaleMembershipSnapshots_MembershipPlanCategories_AudienceCa~" FOREIGN KEY ("AudienceCategoryId") REFERENCES "MembershipPlanCategories" ("Id") ON DELETE RESTRICT;

ALTER TABLE "SaleTransactions" ADD CONSTRAINT "FK_SaleTransactions_MembershipPlanCategories_OfferAudienceCate~" FOREIGN KEY ("OfferAudienceCategoryIdSnapshot") REFERENCES "MembershipPlanCategories" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261010141447_AddOfferEligibilityAndAudience', '10.0.8');

COMMIT;
