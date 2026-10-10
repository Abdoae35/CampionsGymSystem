START TRANSACTION;
DROP INDEX "IX_MembershipSubscriptionTransactions_SaleTransactionId";

ALTER TABLE "SaleTransactions" ADD "ActivationMode" character varying(40);

ALTER TABLE "SaleTransactions" ADD "BusinessDate" date;

ALTER TABLE "SaleTransactions" ADD "CancellationReason" character varying(2000);

ALTER TABLE "SaleTransactions" ADD "CancelledAt" timestamp with time zone;

ALTER TABLE "SaleTransactions" ADD "CancelledByUserId" uuid;

ALTER TABLE "SaleTransactions" ADD "CommissionEligible" boolean;

ALTER TABLE "SaleTransactions" ADD "ExtraDaysSnapshot" integer NOT NULL DEFAULT 0;

ALTER TABLE "SaleTransactions" ADD "InitialPaymentRequired" numeric(18,2);

ALTER TABLE "SaleTransactions" ADD "OfferNameSnapshot" character varying(256);

ALTER TABLE "SaleTransactions" ADD "OfferTypeSnapshot" character varying(40);

ALTER TABLE "SaleTransactions" ADD "OfferValueSnapshot" numeric(18,2);

ALTER TABLE "SaleTransactions" ADD "OperationId" uuid;

ALTER TABLE "SaleTransactions" ADD "PurchaseMode" character varying(40);

ALTER TABLE "SaleTransactions" ADD "RequestHash" character varying(64);

ALTER TABLE "SaleTransactions" ADD "RequestedStartDate" date;

ALTER TABLE "SaleTransactions" ADD "ReservationDueDate" date;

ALTER TABLE "SaleTransactions" ADD "TargetEligible" boolean;

ALTER TABLE "SaleTransactions" ADD "Version" integer NOT NULL DEFAULT 0;

ALTER TABLE "Refunds" ADD "BusinessDate" date;

ALTER TABLE "Refunds" ADD "OperationId" uuid;

ALTER TABLE "Refunds" ADD "RequestHash" character varying(64);

ALTER TABLE "Payments" ALTER COLUMN "ShiftId" DROP NOT NULL;

ALTER TABLE "Payments" ALTER COLUMN "CommissionPeriodId" DROP NOT NULL;

ALTER TABLE "Payments" ADD "BusinessDate" date;

ALTER TABLE "Payments" ADD "OperationId" uuid;

ALTER TABLE "Payments" ADD "RequestHash" character varying(64);

ALTER TABLE "MemberSubscriptions" ADD "IsStandaloneVisit" boolean NOT NULL DEFAULT FALSE;

ALTER TABLE "FinancialTransactions" ADD "BusinessDate" date;

CREATE TABLE "InstallmentPaymentAllocations" (
    "Id" uuid NOT NULL,
    "PaymentId" uuid NOT NULL,
    "InstallmentId" uuid NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    CONSTRAINT "PK_InstallmentPaymentAllocations" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_InstallmentPaymentAllocations_Amount" CHECK ("Amount" > 0),
    CONSTRAINT "FK_InstallmentPaymentAllocations_Installments_InstallmentId" FOREIGN KEY ("InstallmentId") REFERENCES "Installments" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_InstallmentPaymentAllocations_Payments_PaymentId" FOREIGN KEY ("PaymentId") REFERENCES "Payments" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "SaleMembershipSnapshots" (
    "SaleTransactionId" uuid NOT NULL,
    "MembershipPlanId" uuid NOT NULL,
    "PlanName" character varying(256) NOT NULL,
    "PlanCategory" character varying(40) NOT NULL,
    "CityId" uuid NOT NULL,
    "FreelanceCoachEmployeeId" uuid,
    "AccessMode" character varying(40) NOT NULL,
    "DurationDays" integer NOT NULL,
    "SessionCount" integer,
    "FreezeDays" integer NOT NULL,
    CONSTRAINT "PK_SaleMembershipSnapshots" PRIMARY KEY ("SaleTransactionId"),
    CONSTRAINT "CK_SaleMembershipSnapshots_Duration" CHECK ("DurationDays" > 0 AND "FreezeDays" >= 0),
    CONSTRAINT "CK_SaleMembershipSnapshots_Mode" CHECK (("AccessMode" = 'Unlimited' AND "SessionCount" IS NULL) OR ("AccessMode" = 'SessionBased' AND "SessionCount" IS NOT NULL AND "SessionCount" > 0)),
    CONSTRAINT "CK_SaleMembershipSnapshots_Type" CHECK (("PlanCategory" = 'Normal' AND "FreelanceCoachEmployeeId" IS NULL) OR ("PlanCategory" = 'Freelance' AND "FreelanceCoachEmployeeId" IS NOT NULL)),
    CONSTRAINT "FK_SaleMembershipSnapshots_Cities_CityId" FOREIGN KEY ("CityId") REFERENCES "Cities" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_SaleMembershipSnapshots_Employees_FreelanceCoachEmployeeId" FOREIGN KEY ("FreelanceCoachEmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_SaleMembershipSnapshots_MembershipPlans_MembershipPlanId" FOREIGN KEY ("MembershipPlanId") REFERENCES "MembershipPlans" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_SaleMembershipSnapshots_SaleTransactions_SaleTransactionId" FOREIGN KEY ("SaleTransactionId") REFERENCES "SaleTransactions" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "SaleBenefitSnapshots" (
    "Id" uuid NOT NULL,
    "SaleTransactionId" uuid NOT NULL,
    "MembershipBenefitId" uuid NOT NULL,
    "Code" character varying(80) NOT NULL,
    "Name" character varying(256) NOT NULL,
    "GrantType" character varying(40) NOT NULL,
    "Quantity" integer,
    CONSTRAINT "PK_SaleBenefitSnapshots" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_SaleBenefitSnapshots_Quantity" CHECK (("GrantType" = 'Access' AND "Quantity" IS NULL) OR ("GrantType" = 'Quantity' AND "Quantity" IS NOT NULL AND "Quantity" > 0)),
    CONSTRAINT "FK_SaleBenefitSnapshots_MembershipBenefits_MembershipBenefitId" FOREIGN KEY ("MembershipBenefitId") REFERENCES "MembershipBenefits" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_SaleBenefitSnapshots_SaleMembershipSnapshots_SaleTransactio~" FOREIGN KEY ("SaleTransactionId") REFERENCES "SaleMembershipSnapshots" ("SaleTransactionId") ON DELETE RESTRICT
);

CREATE INDEX "IX_SaleTransactions_CancelledByUserId" ON "SaleTransactions" ("CancelledByUserId");

CREATE UNIQUE INDEX "IX_SaleTransactions_OperationId" ON "SaleTransactions" ("OperationId");

ALTER TABLE "SaleTransactions" ADD CONSTRAINT "CK_SaleTransactions_Activation" CHECK ("OperationId" IS NULL OR (("ActivationMode" = 'SpecificDate') = ("RequestedStartDate" IS NOT NULL)));

ALTER TABLE "SaleTransactions" ADD CONSTRAINT "CK_SaleTransactions_Cancellation" CHECK (("CancelledAt" IS NULL AND "CancelledByUserId" IS NULL AND "CancellationReason" IS NULL) OR ("CancelledAt" IS NOT NULL AND "CancelledByUserId" IS NOT NULL AND "CancellationReason" IS NOT NULL AND length(btrim("CancellationReason")) > 0));

ALTER TABLE "SaleTransactions" ADD CONSTRAINT "CK_SaleTransactions_ExtraDays" CHECK ("ExtraDaysSnapshot" >= 0);

ALTER TABLE "SaleTransactions" ADD CONSTRAINT "CK_SaleTransactions_Workflow" CHECK ("OperationId" IS NULL OR ("OperationId" <> '00000000-0000-0000-0000-000000000000'::uuid AND "RequestHash" IS NOT NULL AND "BusinessDate" IS NOT NULL AND "PurchaseMode" IS NOT NULL AND "PurchaseMode" IN ('FullPayment', 'Reservation', 'Installment', 'StandaloneSingleSession') AND "ActivationMode" IS NOT NULL AND "ActivationMode" IN ('Now', 'FirstCheckIn', 'SpecificDate') AND "CommissionEligible" IS NOT NULL AND "TargetEligible" IS NOT NULL AND "NetAmount" > 0));

CREATE UNIQUE INDEX "IX_Refunds_OperationId" ON "Refunds" ("OperationId");

CREATE UNIQUE INDEX "IX_Payments_OperationId" ON "Payments" ("OperationId");

CREATE UNIQUE INDEX "IX_MembershipSubscriptionTransactions_SaleTransactionId" ON "MembershipSubscriptionTransactions" ("SaleTransactionId") WHERE "OperationId" IS NOT NULL;

CREATE INDEX "IX_InstallmentPaymentAllocations_InstallmentId" ON "InstallmentPaymentAllocations" ("InstallmentId");

CREATE UNIQUE INDEX "IX_InstallmentPaymentAllocations_PaymentId_InstallmentId" ON "InstallmentPaymentAllocations" ("PaymentId", "InstallmentId");

CREATE INDEX "IX_SaleBenefitSnapshots_MembershipBenefitId" ON "SaleBenefitSnapshots" ("MembershipBenefitId");

CREATE UNIQUE INDEX "IX_SaleBenefitSnapshots_SaleTransactionId_MembershipBenefitId" ON "SaleBenefitSnapshots" ("SaleTransactionId", "MembershipBenefitId");

CREATE INDEX "IX_SaleMembershipSnapshots_CityId" ON "SaleMembershipSnapshots" ("CityId");

CREATE INDEX "IX_SaleMembershipSnapshots_FreelanceCoachEmployeeId" ON "SaleMembershipSnapshots" ("FreelanceCoachEmployeeId");

CREATE INDEX "IX_SaleMembershipSnapshots_MembershipPlanId" ON "SaleMembershipSnapshots" ("MembershipPlanId");

ALTER TABLE "SaleTransactions" ADD CONSTRAINT "FK_SaleTransactions_AspNetUsers_CancelledByUserId" FOREIGN KEY ("CancelledByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261010134009_PrepareSalesPayments', '10.0.8');

COMMIT;
