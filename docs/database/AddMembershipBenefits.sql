START TRANSACTION;
CREATE TABLE "MembershipBenefits" (
    "Id" uuid NOT NULL,
    "Code" character varying(128) NOT NULL,
    "Name" character varying(256) NOT NULL,
    "Description" text,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_MembershipBenefits" PRIMARY KEY ("Id")
);

CREATE TABLE "MembershipPlanBenefits" (
    "Id" uuid NOT NULL,
    "MembershipPlanId" uuid NOT NULL,
    "MembershipBenefitId" uuid NOT NULL,
    "GrantType" character varying(40) NOT NULL,
    "Quantity" integer,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_MembershipPlanBenefits" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_MembershipPlanBenefits_GrantQuantity" CHECK (("GrantType" = 'Quantity' AND "Quantity" IS NOT NULL AND "Quantity" > 0) OR ("GrantType" = 'Access' AND "Quantity" IS NULL)),
    CONSTRAINT "FK_MembershipPlanBenefits_MembershipBenefits_MembershipBenefit~" FOREIGN KEY ("MembershipBenefitId") REFERENCES "MembershipBenefits" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_MembershipPlanBenefits_MembershipPlans_MembershipPlanId" FOREIGN KEY ("MembershipPlanId") REFERENCES "MembershipPlans" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "MemberSubscriptionBenefits" (
    "Id" uuid NOT NULL,
    "MembershipSubscriptionTransactionId" uuid NOT NULL,
    "MembershipBenefitId" uuid NOT NULL,
    "BenefitCodeSnapshot" character varying(128) NOT NULL,
    "BenefitNameSnapshot" character varying(256) NOT NULL,
    "GrantTypeSnapshot" character varying(40) NOT NULL,
    "GrantedQuantity" integer,
    "GrantedAt" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_MemberSubscriptionBenefits" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_MemberSubscriptionBenefits_GrantQuantity" CHECK (("GrantTypeSnapshot" = 'Quantity' AND "GrantedQuantity" IS NOT NULL AND "GrantedQuantity" > 0) OR ("GrantTypeSnapshot" = 'Access' AND "GrantedQuantity" IS NULL)),
    CONSTRAINT "FK_MemberSubscriptionBenefits_MembershipBenefits_MembershipBen~" FOREIGN KEY ("MembershipBenefitId") REFERENCES "MembershipBenefits" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_MemberSubscriptionBenefits_MembershipSubscriptionTransactio~" FOREIGN KEY ("MembershipSubscriptionTransactionId") REFERENCES "MembershipSubscriptionTransactions" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "MemberBenefitUsages" (
    "Id" uuid NOT NULL,
    "MemberSubscriptionBenefitId" uuid NOT NULL,
    "QuantityUsed" integer NOT NULL,
    "UsedAt" timestamp with time zone NOT NULL,
    "BranchId" uuid NOT NULL,
    "CreatedByUserId" uuid,
    "Notes" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_MemberBenefitUsages" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_MemberBenefitUsages_QuantityUsed" CHECK ("QuantityUsed" > 0),
    CONSTRAINT "FK_MemberBenefitUsages_AspNetUsers_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_MemberBenefitUsages_GymBranches_BranchId" FOREIGN KEY ("BranchId") REFERENCES "GymBranches" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_MemberBenefitUsages_MemberSubscriptionBenefits_MemberSubscr~" FOREIGN KEY ("MemberSubscriptionBenefitId") REFERENCES "MemberSubscriptionBenefits" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_MemberBenefitUsages_BranchId_UsedAt" ON "MemberBenefitUsages" ("BranchId", "UsedAt");

CREATE INDEX "IX_MemberBenefitUsages_CreatedByUserId" ON "MemberBenefitUsages" ("CreatedByUserId");

CREATE INDEX "IX_MemberBenefitUsages_MemberSubscriptionBenefitId_UsedAt" ON "MemberBenefitUsages" ("MemberSubscriptionBenefitId", "UsedAt");

CREATE UNIQUE INDEX "IX_MembershipBenefits_Code" ON "MembershipBenefits" ("Code");

CREATE UNIQUE INDEX "IX_MembershipBenefits_Name" ON "MembershipBenefits" ("Name");

CREATE INDEX "IX_MembershipPlanBenefits_MembershipBenefitId" ON "MembershipPlanBenefits" ("MembershipBenefitId");

CREATE UNIQUE INDEX "IX_MembershipPlanBenefits_MembershipPlanId_MembershipBenefitId" ON "MembershipPlanBenefits" ("MembershipPlanId", "MembershipBenefitId");

CREATE INDEX "IX_MemberSubscriptionBenefits_MembershipBenefitId" ON "MemberSubscriptionBenefits" ("MembershipBenefitId");

CREATE UNIQUE INDEX "IX_MemberSubscriptionBenefits_MembershipSubscriptionTransactio~" ON "MemberSubscriptionBenefits" ("MembershipSubscriptionTransactionId", "MembershipBenefitId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261007153001_AddMembershipBenefits', '10.0.8');

COMMIT;

