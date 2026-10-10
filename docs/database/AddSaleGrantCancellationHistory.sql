START TRANSACTION;
ALTER TABLE "RefundParts" ADD "ProviderReference" character varying(256);

CREATE TABLE "BorrowedDebtSettlementEvents" (
    "Id" uuid NOT NULL,
    "BorrowedGymSessionId" uuid NOT NULL,
    "Sequence" integer NOT NULL,
    "GrantId" uuid NOT NULL,
    "IsReversal" boolean NOT NULL,
    "RefundId" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    "ActorId" uuid NOT NULL,
    CONSTRAINT "PK_BorrowedDebtSettlementEvents" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_BorrowedDebtSettlementEvents_State" CHECK ("Sequence" > 0 AND "IsReversal" = ("RefundId" IS NOT NULL)),
    CONSTRAINT "FK_BorrowedDebtSettlementEvents_AspNetUsers_ActorId" FOREIGN KEY ("ActorId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_BorrowedDebtSettlementEvents_BorrowedGymSessions_BorrowedGy~" FOREIGN KEY ("BorrowedGymSessionId") REFERENCES "BorrowedGymSessions" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_BorrowedDebtSettlementEvents_MembershipSubscriptionTransact~" FOREIGN KEY ("GrantId") REFERENCES "MembershipSubscriptionTransactions" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_BorrowedDebtSettlementEvents_Refunds_RefundId" FOREIGN KEY ("RefundId") REFERENCES "Refunds" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "SubscriptionGrantCancellations" (
    "Id" uuid NOT NULL,
    "GrantId" uuid NOT NULL,
    "RefundId" uuid NOT NULL,
    "PreviousEndDate" date,
    "NewEndDate" date,
    "PreviousRemainingSessions" integer,
    "NewRemainingSessions" integer,
    "DaysRemoved" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_SubscriptionGrantCancellations" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_SubscriptionGrantCancellations_Sessions" CHECK ("DaysRemoved" >= 0 AND "PreviousRemainingSessions" >= 0 AND "NewRemainingSessions" >= 0),
    CONSTRAINT "FK_SubscriptionGrantCancellations_MembershipSubscriptionTransa~" FOREIGN KEY ("GrantId") REFERENCES "MembershipSubscriptionTransactions" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_SubscriptionGrantCancellations_Refunds_RefundId" FOREIGN KEY ("RefundId") REFERENCES "Refunds" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_BorrowedDebtSettlementEvents_ActorId" ON "BorrowedDebtSettlementEvents" ("ActorId");

CREATE UNIQUE INDEX "IX_BorrowedDebtSettlementEvents_BorrowedGymSessionId_Sequence" ON "BorrowedDebtSettlementEvents" ("BorrowedGymSessionId", "Sequence");

CREATE INDEX "IX_BorrowedDebtSettlementEvents_GrantId" ON "BorrowedDebtSettlementEvents" ("GrantId");

CREATE INDEX "IX_BorrowedDebtSettlementEvents_RefundId" ON "BorrowedDebtSettlementEvents" ("RefundId");

CREATE UNIQUE INDEX "IX_SubscriptionGrantCancellations_GrantId" ON "SubscriptionGrantCancellations" ("GrantId");

CREATE UNIQUE INDEX "IX_SubscriptionGrantCancellations_RefundId" ON "SubscriptionGrantCancellations" ("RefundId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261010161931_AddSaleGrantCancellationHistory', '10.0.8');

COMMIT;
