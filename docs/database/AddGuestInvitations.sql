START TRANSACTION;
ALTER TABLE "CheckIns" DROP CONSTRAINT "CK_CheckIns_CheckInType";

ALTER TABLE "CheckIns" DROP CONSTRAINT "CK_CheckIns_SubscriptionType";

ALTER TABLE "CheckIns" ALTER COLUMN "MemberId" DROP NOT NULL;

CREATE TABLE "GuestInvitations" (
    "Id" uuid NOT NULL,
    "InviteeMemberId" uuid,
    "InviteeName" character varying(200) NOT NULL,
    "InviteePhone" character varying(32) NOT NULL,
    "InviteePhoneNormalized" character varying(32) NOT NULL,
    "Source" character varying(40) NOT NULL,
    "InvitedByMemberId" uuid,
    "SourceSubscriptionBenefitId" uuid,
    "BranchId" uuid NOT NULL,
    "Status" character varying(40) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "RedeemedAt" timestamp with time zone,
    "CancelledAt" timestamp with time zone,
    "RedeemedCheckInId" uuid,
    "CreatedByUserId" uuid,
    "Notes" text,
    CONSTRAINT "PK_GuestInvitations" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_GuestInvitations_Cancellation" CHECK (("Status" = 'Cancelled' AND "CancelledAt" IS NOT NULL) OR ("Status" <> 'Cancelled' AND "CancelledAt" IS NULL)),
    CONSTRAINT "CK_GuestInvitations_Identity" CHECK (btrim("InviteeName") <> '' AND btrim("InviteePhone") <> '' AND btrim("InviteePhoneNormalized") <> '' AND "InviteePhoneNormalized" = btrim("InviteePhoneNormalized")),
    CONSTRAINT "CK_GuestInvitations_Inviter" CHECK (("Source" = 'Gym' AND "InvitedByMemberId" IS NULL AND "SourceSubscriptionBenefitId" IS NULL) OR ("Source" = 'Member' AND "InvitedByMemberId" IS NOT NULL)),
    CONSTRAINT "CK_GuestInvitations_Redemption" CHECK (("Status" = 'Redeemed' AND "RedeemedAt" IS NOT NULL) OR ("Status" <> 'Redeemed' AND "RedeemedAt" IS NULL AND "RedeemedCheckInId" IS NULL)),
    CONSTRAINT "CK_GuestInvitations_Source" CHECK ("Source" IN ('Gym', 'Member')),
    CONSTRAINT "CK_GuestInvitations_Status" CHECK ("Status" IN ('Available', 'Redeemed', 'Cancelled', 'Expired')),
    CONSTRAINT "CK_GuestInvitations_Timestamps" CHECK (("RedeemedAt" IS NULL OR "RedeemedAt" >= "CreatedAt") AND ("CancelledAt" IS NULL OR "CancelledAt" >= "CreatedAt")),
    CONSTRAINT "FK_GuestInvitations_AspNetUsers_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_GuestInvitations_CheckIns_RedeemedCheckInId" FOREIGN KEY ("RedeemedCheckInId") REFERENCES "CheckIns" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_GuestInvitations_GymBranches_BranchId" FOREIGN KEY ("BranchId") REFERENCES "GymBranches" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_GuestInvitations_MemberSubscriptionBenefits_SourceSubscript~" FOREIGN KEY ("SourceSubscriptionBenefitId") REFERENCES "MemberSubscriptionBenefits" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_GuestInvitations_Members_InvitedByMemberId" FOREIGN KEY ("InvitedByMemberId") REFERENCES "Members" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_GuestInvitations_Members_InviteeMemberId" FOREIGN KEY ("InviteeMemberId") REFERENCES "Members" ("Id") ON DELETE RESTRICT
);

ALTER TABLE "CheckIns" ADD CONSTRAINT "CK_CheckIns_CheckInType" CHECK ("CheckInType" IN ('Gym', 'BorrowedGym', 'Activity', 'Class', 'Trial'));

ALTER TABLE "CheckIns" ADD CONSTRAINT "CK_CheckIns_Member" CHECK ("CheckInType" = 'Trial' OR "MemberId" IS NOT NULL);

ALTER TABLE "CheckIns" ADD CONSTRAINT "CK_CheckIns_SubscriptionType" CHECK (("CheckInType" IN ('Gym', 'BorrowedGym') AND "MemberSubscriptionId" IS NOT NULL AND "ActivitySubscriptionId" IS NULL AND "GroupClassId" IS NULL) OR ("CheckInType" = 'Activity' AND "ActivitySubscriptionId" IS NOT NULL AND "MemberSubscriptionId" IS NULL AND "GroupClassId" IS NULL) OR ("CheckInType" = 'Class' AND "GroupClassId" IS NOT NULL AND "MemberSubscriptionId" IS NULL AND "ActivitySubscriptionId" IS NULL) OR ("CheckInType" = 'Trial' AND "MemberSubscriptionId" IS NULL AND "ActivitySubscriptionId" IS NULL AND "GroupClassId" IS NULL));

CREATE INDEX "IX_GuestInvitations_BranchId_Status_CreatedAt" ON "GuestInvitations" ("BranchId", "Status", "CreatedAt");

CREATE INDEX "IX_GuestInvitations_CreatedByUserId" ON "GuestInvitations" ("CreatedByUserId");

CREATE INDEX "IX_GuestInvitations_InvitedByMemberId" ON "GuestInvitations" ("InvitedByMemberId");

CREATE UNIQUE INDEX "IX_GuestInvitations_InviteeMemberId" ON "GuestInvitations" ("InviteeMemberId") WHERE "Status" = 'Redeemed' AND "InviteeMemberId" IS NOT NULL;

CREATE UNIQUE INDEX "IX_GuestInvitations_InviteePhoneNormalized" ON "GuestInvitations" ("InviteePhoneNormalized") WHERE "Status" = 'Redeemed';

CREATE UNIQUE INDEX "IX_GuestInvitations_RedeemedCheckInId" ON "GuestInvitations" ("RedeemedCheckInId") WHERE "RedeemedCheckInId" IS NOT NULL;

CREATE INDEX "IX_GuestInvitations_SourceSubscriptionBenefitId" ON "GuestInvitations" ("SourceSubscriptionBenefitId");

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

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261008012831_AddGuestInvitations', '10.0.8');

COMMIT;
