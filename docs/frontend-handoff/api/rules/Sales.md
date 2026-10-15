# Sales rule/validation evidence

Completed source; deployment/configuration prerequisites are separate. Shared cross-cutting helpers: Authentication, Branches, Operations and Shifts.

## CampionsSystem.BLL/Sales/OfferService.cs

Lines 11–29

```csharp

public sealed partial class OfferService(ApplicationDbContext db, ICurrentUserService current,
    OperationAccess access, SubscriptionService subscriptions)
{
    private async Task<CurrentUserContext> RequireAsync(string permission, Guid? branch, CancellationToken ct)
    {
        var actor = await current.GetAsync(ct);
        if (actor is null || !actor.Permissions.Contains(permission)) throw new UnauthorizedAccessException();
        BranchAccessService.EnsureScope(actor, branch);
        return actor;
    }
    private async Task<Guid> SellingBranchAsync(Guid? selected, CancellationToken ct)
    {
        var actor = await current.GetAsync(ct) ?? throw new UnauthorizedAccessException();
        var id = selected ?? actor.CurrentBranchId ?? throw new BusinessRuleException("Select an operational branch.");
        await RequireAsync(SalesPermissions.Create, id, ct);
        if (!await db.GymBranches.AnyAsync(x => x.Id == id && x.IsActive && x.City.IsActive, ct))
            throw new BusinessRuleException("Selling branch must be active.");
        return id;
    }

```

Lines 38–81

```csharp

    private async Task<Offer> LockedOfferAsync(Guid id, CancellationToken ct)
    {
        var offer = await db.Offers.FromSqlInterpolated($"SELECT * FROM \"Offers\" WHERE \"Id\"={id} FOR UPDATE")
            .SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Offer not found.");
        await db.Entry(offer).ReloadAsync(ct); return offer;
    }
    private async Task<OfferDetails> DetailsAsync(Offer o, CancellationToken ct) => new(o.Id, o.BranchId, o.Name,
        o.OfferType.ToString(), o.Value, o.StartDate, o.EndDate, o.IsPermanent, o.IsActive,
        await db.OfferMembershipPlans.Where(x => x.OfferId == o.Id).OrderBy(x => x.MembershipPlanId).Select(x => x.MembershipPlanId).ToListAsync(ct),
        o.EligibilityMode?.ToString(), o.AudienceCategoryId);
    public async Task<OfferDetails> GetAsync(Guid id, CancellationToken ct = default)
    {
        var o = await db.Offers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new BranchNotFoundException("Offer not found.");
        await RequireAsync(SalesPermissions.Offers, o.BranchId, ct); return await DetailsAsync(o, ct);
    }
    public Task<OfferDetails> CreateAsync(OfferRequest r, CancellationToken ct = default) => SaveAsync(null, r, ct);
    public Task<OfferDetails> UpdateAsync(Guid id, OfferRequest r, CancellationToken ct = default) => SaveAsync(id, r, ct);
    private async Task<OfferDetails> SaveAsync(Guid? id, OfferRequest r, CancellationToken ct)
    {
        var actor = await current.GetAsync(ct) ?? throw new UnauthorizedAccessException();
        var branch = r.BranchId ?? (id.HasValue ? await db.Offers.Where(x => x.Id == id).Select(x => x.BranchId).SingleOrDefaultAsync(ct) : actor.CurrentBranchId);
        actor = await RequireAsync(SalesPermissions.Offers, branch, ct);
        if (string.IsNullOrWhiteSpace(r.Name) || r.Name.Trim().Length > 256 || r.Description?.Length > 2000
            || !Enum.IsDefined(r.Type) || r.EligibilityMode is null || !Enum.IsDefined(r.EligibilityMode.Value)
            || r.StartDate > r.EndDate || r.MembershipPlanIds is null || r.MembershipPlanIds.Count is < 1 or > 100
            || r.MembershipPlanIds.Any(x => x == Guid.Empty) || r.MembershipPlanIds.Distinct().Count() != r.MembershipPlanIds.Count)
            throw new BusinessRuleException("Invalid Offer configuration; explicit eligibility mode and linked plans are required.");
        OperationAccess.Money(r.Value);
        if (r.Value <= 0 || r.Value > 9999999999999999.99m || r.Type == OfferType.PercentageDiscount && r.Value > 100
            || r.Type == OfferType.ExtraDays && (r.Value != decimal.Truncate(r.Value) || r.Value > 36500))
            throw new BusinessRuleException("Invalid discount or extra-day value.");
        return await TransactionAsync(async () =>
        {
            var offer = id.HasValue ? await LockedOfferAsync(id.Value, ct) : new Offer { CreatedByUserId = actor.UserId, CreatedAt = access.Now };
            if (id.HasValue) await RequireAsync(SalesPermissions.Offers, offer.BranchId, ct);
            if (id.HasValue && offer.BranchId != branch) throw new BusinessRuleException("Offer branch ownership cannot be reassigned.");
            if (branch.HasValue && !await db.GymBranches.AnyAsync(x => x.Id == branch && x.IsActive, ct)) throw new BusinessRuleException("Select an active branch.");
            if (r.AudienceCategoryId.HasValue && !await db.MembershipPlanCategories.AnyAsync(x => x.Id == r.AudienceCategoryId && x.IsActive, ct))
                throw new BusinessRuleException("Select an active audience category.");
            var plans = await db.MembershipPlans.Where(x => r.MembershipPlanIds.Contains(x.Id)).ToListAsync(ct);
            if (plans.Count != r.MembershipPlanIds.Count || plans.Any(x => x.PlanCategory == PlanCategory.Employee || x.BranchId == null || branch != null && x.BranchId != branch))
                throw new BusinessRuleException("Offer links must be customer plans in its owning branch.");
            var old = id.HasValue ? new { offer.Name, offer.Value, offer.EligibilityMode, offer.AudienceCategoryId, offer.StartDate, offer.EndDate, offer.IsActive } : null;
            offer.Name = r.Name.Trim(); offer.Description = r.Description?.Trim(); offer.BranchId = branch; offer.OfferType = r.Type;

```

Lines 94–126

```csharp

    public async Task<PricingResult> QuoteAsync(Guid memberId, Guid planId, Guid? offerId, Guid? branchId = null, CancellationToken ct = default)
    {
        var branch = await SellingBranchAsync(branchId, ct);
        var plan = await db.MembershipPlans.AsNoTracking().SingleOrDefaultAsync(x => x.Id == planId, ct) ?? throw new BranchNotFoundException("Plan not found.");
        var member = await db.Members.AsNoTracking().SingleOrDefaultAsync(x => x.Id == memberId, ct) ?? throw new BranchNotFoundException("Member not found.");
        return await PriceAsync(member, plan, offerId, branch, await subscriptions.EffectiveBusinessDateAsync(branch, ct), ct);
    }
    internal async Task<PricingResult> PriceAsync(Member member, MembershipPlan plan, Guid? offerId, Guid branch, DateOnly date, CancellationToken ct)
    {
        if (!plan.IsActive || plan.BranchId != branch || !await db.Cities.AnyAsync(x => x.Id == plan.CityId && x.IsActive, ct))
            throw new BusinessRuleException("Select an active customer plan sold by the operational branch.");
        await subscriptions.ValidateSaleCustomerAsync(member, plan, ct);
        if (!offerId.HasValue) return new(plan.Price, 0, plan.Price, 0, null, null, null, null);
        var offer = await db.Offers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == offerId, ct);
        if (offer is null || !offer.IsActive || offer.EligibilityMode is null || offer.BranchId != null && offer.BranchId != branch
            || offer.StartDate > date || offer.EndDate < date || !await db.OfferMembershipPlans.AnyAsync(x => x.OfferId == offerId && x.MembershipPlanId == plan.Id, ct))
            throw new BusinessRuleException("Offer is not applicable to this plan, selling branch or business date.");
        if (!await subscriptions.MatchesAudienceAsync(member.Id, offer.AudienceCategoryId, ct)) throw new BusinessRuleException("Member does not match the Offer audience.");
        if (offer.EligibilityMode == OfferEligibilityMode.MemberOnly && !await db.MemberOfferEntitlements.AnyAsync(x => x.OfferId == offer.Id
            && x.MemberId == member.Id && x.RevokedAt == null && x.CreatedAt <= access.Now && (x.StartDate == null || x.StartDate <= date) && (x.EndDate == null || x.EndDate >= date), ct))
            throw new BusinessRuleException("MemberOnly Offer requires a current MemberOfferEntitlement.");
        return Calculate(plan.Price, offer);
    }
    private static PricingResult Calculate(decimal price, Offer offer)
    {
        var discount = offer.OfferType switch { OfferType.FixedDiscount => offer.Value,
            OfferType.PercentageDiscount => decimal.Round(price * offer.Value / 100m, 2, MidpointRounding.AwayFromZero), OfferType.ExtraDays => 0m,
            _ => throw new BusinessRuleException("Unsupported Offer type.") };
        if (discount < 0 || discount >= price || offer.Value <= 0 || offer.OfferType == OfferType.PercentageDiscount && offer.Value > 100
            || offer.OfferType == OfferType.ExtraDays && (offer.Value != decimal.Truncate(offer.Value) || offer.Value > 36500))
            throw new BusinessRuleException("Offer must preserve a positive valid paid price and valid extra-day quantity.");
        return new(price, discount, price - discount, offer.OfferType == OfferType.ExtraDays ? (int)offer.Value : 0,
            offer.Id, offer.Name, offer.OfferType, offer.Value, offer.EligibilityMode, offer.AudienceCategoryId);

```

Lines 130–137

```csharp

        var branch = await SellingBranchAsync(branchId, ct);
        // Validate plan eligibility even when there are no Offers, then evaluate the bounded catalog.
        await QuoteAsync(memberId, planId, null, branch, ct);
        var date = await subscriptions.EffectiveBusinessDateAsync(branch, ct);
        var ids = await db.Offers.Where(x => x.IsActive && x.EligibilityMode != null && (x.StartDate == null || x.StartDate <= date) && (x.EndDate == null || x.EndDate >= date) && (x.BranchId == null || x.BranchId == branch)
            && db.OfferMembershipPlans.Any(l => l.OfferId == x.Id && l.MembershipPlanId == planId)).OrderBy(x => x.Name).ThenBy(x => x.Id).Select(x => x.Id).Take(100).ToListAsync(ct);
        var results = new List<OfferDetails>();
        foreach (var id in ids)

```

## CampionsSystem.BLL/Sales/SalesService.cs

Lines 17–32

```csharp

{
    private readonly FinancialOperations shiftMoney = new(db, current, access);
    private static string Hash<T>(T request) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request))));
    private static void Operation(Guid id) { if (id == Guid.Empty) throw new BusinessRuleException("A nonempty operation identity is required."); }
    private static void Amount(decimal value, bool positive = true)
    {
        OperationAccess.Money(value);
        if (value > 9999999999999999.99m || positive && value <= 0) throw new BusinessRuleException("A valid positive money amount is required.");
    }
    private async Task<CurrentUserContext> RequireAsync(string permission, Guid? branch, CancellationToken ct)
    {
        var actor = await current.GetAsync(ct);
        if (actor is null || !actor.Permissions.Contains(permission)) throw new UnauthorizedAccessException();
        BranchAccessService.EnsureScope(actor, branch); return actor;
    }
    private async Task<T> TransactionAsync<T>(Func<Task<T>> change, CancellationToken ct)

```

Lines 40–75

```csharp

    private async Task<Member> LockMemberAsync(Guid id, CancellationToken ct)
    {
        var row = await db.Members.FromSqlInterpolated($"SELECT * FROM \"Members\" WHERE \"Id\"={id} FOR UPDATE").SingleOrDefaultAsync(ct)
            ?? throw new BranchNotFoundException("Member not found."); await db.Entry(row).ReloadAsync(ct); return row;
    }
    private async Task<SaleTransaction> LockSaleAsync(Guid id, string permission, CancellationToken ct)
    {
        var info = await db.SaleTransactions.AsNoTracking().Where(x => x.Id == id).Select(x => new { x.MemberId, x.BranchId }).SingleOrDefaultAsync(ct)
            ?? throw new BranchNotFoundException("Sale not found.");
        await RequireAsync(permission, info.BranchId, ct); await shiftMoney.LockBranchAsync(info.BranchId, ct); await LockMemberAsync(info.MemberId, ct);
        var sale = await db.SaleTransactions.FromSqlInterpolated($"SELECT * FROM \"SaleTransactions\" WHERE \"Id\"={id} FOR UPDATE").SingleAsync(ct);
        await db.Entry(sale).ReloadAsync(ct);
        if (sale.OperationId == null) throw new BusinessRuleException("Legacy Sale requires reconciliation before managed Finance operations.");
        return sale;
    }
    private async Task<(DateOnly Date, Guid? Day)> BusinessDateAsync(Guid branch, CancellationToken ct)
    {
        var day = await db.BusinessDays.FromSqlInterpolated($"SELECT * FROM \"BusinessDays\" WHERE \"BranchId\"={branch} AND \"Status\"='Open' FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
        if (day != null && (day.OpenedAt > access.Now || day.BusinessDate > access.Today)) throw new BusinessRuleException("Invalid open BusinessDay.");
        return (day?.BusinessDate ?? access.Today, day?.Id);
    }
    private async Task<IReadOnlyList<FinancialAccount>> AccountsAsync(Guid branch, IReadOnlyList<PaymentPartInput> parts, decimal amount, CancellationToken ct, bool historicalReversal=false)
    {
        Amount(amount);
        if (parts is null || parts.Count<1 || !historicalReversal && parts.Count>20 || parts.Any(x => x is null || x.FinancialAccountId == Guid.Empty || x.ProviderReference?.Length > 256)
            ) throw new BusinessRuleException("PaymentParts must sum exactly to the payment amount.");
        foreach (var part in parts) Amount(part.Amount);
        if (parts.Sum(x=>x.Amount) != amount) throw new BusinessRuleException("PaymentParts must sum exactly to the payment amount.");
        var accounts = new List<FinancialAccount>();
        foreach (var id in parts.Select(x => x.FinancialAccountId).Distinct().Order())
        {
            var account = await db.FinancialAccounts.FromSqlInterpolated($"SELECT * FROM \"FinancialAccounts\" WHERE \"Id\"={id} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
            if (account is null || !historicalReversal && !account.IsActive || account.BranchId != branch || account.AccountType is not (FinancialAccountType.Cash or FinancialAccountType.InstaPay))
                throw new BusinessRuleException("Select an active Cash or InstaPay account of the operational branch.");
            await shiftMoney.AccountAsync(branch, id, ct, historicalReversal);
            accounts.Add(account);

```

Lines 81–88

```csharp

    {
        var activeShift = await shiftMoney.RequireShiftAsync(sale.BranchId, actor, ct);
        await AccountsAsync(sale.BranchId, parts, amount, ct);
        var (date, day) = await BusinessDateAsync(sale.BranchId, ct);
        var payment = new Payment { SaleTransaction = sale, BranchId = sale.BranchId, OperationId = operation, RequestHash = hash,
            ShiftId = activeShift.Id, Amount = amount, Status = PaymentStatus.Completed, PaidAt = access.Now, ReceivedByUserId = actor, BusinessDate = date, BusinessDayId = day };
        await new Commissions.CommissionSources(db, access).RecordCollectionAsync(payment, sale, ct);
        db.Add(payment);

```

Lines 94–99

```csharp

        }
        foreach (var group in parts.GroupBy(x => x.FinancialAccountId)) await shiftMoney.PostAsync(new FinancialTransaction {
            FinancialAccountId = group.Key, BranchId = sale.BranchId, ShiftId = activeShift.Id, Payment = payment, SourceType = FinancialSourceType.Payment,
            TransactionType = FinancialTransactionType.Credit, Amount = group.Sum(x => x.Amount), BusinessDate = date,
            BusinessDayId = day, OccurredAt = access.Now, CreatedByUserId = actor }, ct);
        access.Audit(actor, sale.BranchId, "Payment.Recorded", nameof(Payment), payment.Id, after: new { sale.Id, amount, date });

```

Lines 103–121

```csharp

    internal Task<SaleReceipt> RecordSaleAsync(CreateSaleRequest r, CancellationToken ct = default) => TransactionAsync(async () =>
    {
        Operation(r.OperationId);
        var actor = await current.GetAsync(ct) ?? throw new UnauthorizedAccessException();
        var branch = r.BranchId ?? actor.CurrentBranchId ?? throw new BusinessRuleException("Select an operational branch.");
        actor = await RequireAsync(SalesPermissions.Create, branch, ct);
        await shiftMoney.LockBranchAsync(branch, ct);
        var branchRow = await db.GymBranches.FromSqlInterpolated($"SELECT * FROM \"GymBranches\" WHERE \"Id\"={branch} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
        if (branchRow is null || !branchRow.IsActive || !await db.Cities.AnyAsync(x => x.Id == branchRow.CityId && x.IsActive, ct)) throw new BusinessRuleException("Selling branch must be active.");
        // Match the existing plan-edit lock before member locks; delayed activation uses the immutable sold snapshot.
        var plan = await db.MembershipPlans.FromSqlInterpolated($"SELECT * FROM \"MembershipPlans\" WHERE \"Id\"={r.MembershipPlanId} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct)
            ?? throw new BranchNotFoundException("Plan not found.");
        var member = await LockMemberAsync(r.MemberId, ct); var hash = Hash(r with { BranchId = branch });
        var prior = await db.SaleTransactions.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId, ct);
        if (prior != null)
        {
            if (prior.RequestHash != hash || prior.CreatedByUserId != actor.UserId || prior.BranchId != branch) throw new BusinessRuleException("Operation identity belongs to another Sale request.");
            return new SaleReceipt(prior.Id, await db.Payments.Where(x => x.SaleTransactionId == prior.Id).Select(x => x.Id).ToListAsync(ct));
        }

```

Lines 124–136

```csharp

            || (r.ActivationMode == SubscriptionActivationMode.SpecificDate) != r.StartDate.HasValue
            || r.PaymentParts.Count > 20 || r.PurchaseMode != PurchaseMode.Installment && r.PaymentParts.Count == 0 || r.PaymentParts.Any(x => x is null)
            || r.PurchaseMode != PurchaseMode.Reservation && r.ReservationDueDate != null
            || r.PurchaseMode != PurchaseMode.Installment && (r.Installments is { Count: > 0 } || r.InitialPaymentRequired != null)) throw new BusinessRuleException("Invalid Sale configuration.");
        foreach (var part in r.PaymentParts) Amount(part.Amount);
        var (date, day) = await BusinessDateAsync(branch, ct);
        if (r.ReservationDueDate < date) throw new BusinessRuleException("Reservation due date cannot precede the effective BusinessDate.");
        if (r.StartDate < date) throw new BusinessRuleException("Specific activation date must not precede the effective BusinessDate.");
        if (r.OfferId.HasValue) await db.Offers.FromSqlInterpolated($"SELECT * FROM \"Offers\" WHERE \"Id\"={r.OfferId} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
        if (r.PurchaseMode == PurchaseMode.StandaloneSingleSession && (plan.AccessMode != AccessMode.SessionBased || plan.SessionCount != 1))
            throw new BusinessRuleException("Standalone visits require an eligible one-session SessionBased plan.");
        var price = await offers.PriceAsync(member, plan, r.OfferId, branch, date, ct); Amount(price.FinalPrice);
        if(r.PurchaseMode!=PurchaseMode.StandaloneSingleSession)await subscriptions.ValidatePaidDebtCapacityAsync(member.Id,plan,ct);

```

Lines 138–147

```csharp

        var initialRequired = r.PurchaseMode == PurchaseMode.Installment ? r.InitialPaymentRequired ?? tender : (decimal?)null;
        if (r.PurchaseMode == PurchaseMode.Installment)
        {
            await RequireAsync(SalesPermissions.Installments, branch, ct);
            ValidateInstallmentSchedule(r.Installments, price.FinalPrice - tender, initialRequired!.Value, price.FinalPrice, date);
        }
        if (tender > price.FinalPrice || r.PurchaseMode is PurchaseMode.FullPayment or PurchaseMode.StandaloneSingleSession && tender != price.FinalPrice)
            throw new BusinessRuleException("Payment must not exceed the Sale price; full-payment purchases require the exact final price.");
        var owner = await db.MemberSalesAssignments.AsNoTracking().SingleOrDefaultAsync(x => x.MemberId == member.Id && x.EndedAt == null && x.AssignedAt <= access.Now, ct);
        var owned = owner?.AssignmentType == AssignmentType.Sales;

```

Lines 151–156

```csharp

            && x.PurchaseMode != PurchaseMode.StandaloneSingleSession, ct)
            || await db.MemberSubscriptions.AnyAsync(x => x.MemberId == member.Id && x.EmployeeId == null && !x.IsStandaloneVisit, ct);
        var sale = new SaleTransaction { MemberId = member.Id, BranchId = branch, SaleType = SaleType.GymMembership,
            OperationId = r.OperationId, RequestHash = hash, CreatedShiftId = activeShift.Id, BusinessDate = date, BusinessDayId = day,
            PurchaseMode = r.PurchaseMode, ActivationMode = r.ActivationMode, RequestedStartDate = r.StartDate, ReservationDueDate = r.ReservationDueDate, InitialPaymentRequired = initialRequired,
            GrossAmount = price.OriginalPrice, DiscountAmount = price.Discount, NetAmount = price.FinalPrice, Status = tender == price.FinalPrice ? SaleStatus.Completed : SaleStatus.Open,

```

Lines 170–175

```csharp

        foreach (var config in configs)
        {
            var benefit = await db.MembershipBenefits.FromSqlInterpolated($"SELECT * FROM \"MembershipBenefits\" WHERE \"Id\"={config.MembershipBenefitId} FOR SHARE").AsNoTracking().SingleAsync(ct);
            if (!benefit.IsActive) throw new BusinessRuleException("Plan contains an inactive benefit.");
            db.Add(new SaleBenefitSnapshot { SaleMembershipSnapshot = snapshot, MembershipBenefitId = benefit.Id, Code = benefit.Code, Name = benefit.Name,
                GrantType = config.GrantType, Quantity = config.Quantity });

```

## CampionsSystem.BLL/Sales/SalesService.Coaching.cs

Lines 17–66

```csharp

            var packageBranch = classes
                ? await db.ClassesPackages.Where(x => x.Id == r.PackageId).Select(x => (Guid?)x.BranchId).SingleOrDefaultAsync(ct)
                : await db.PrivatePackages.Where(x => x.Id == r.PackageId).Select(x => (Guid?)x.BranchId).SingleOrDefaultAsync(ct);
            var branch = packageBranch ?? throw new BranchNotFoundException("Package not found.");
            var actor = await RequireAsync(SalesPermissions.Create, branch, ct);
            await RequireAsync(classes ? CoachingPermissions.ClassesManage : CoachingPermissions.PrivateManage, branch, ct);
            await shiftMoney.LockBranchAsync(branch, ct);
            if (!await db.GymBranches.AnyAsync(b => b.Id == branch && b.IsActive && b.City.IsActive, ct)) throw new BusinessRuleException("Active selling branch and city required.");
            var member = await LockMemberAsync(r.MemberId, ct); var hash = Hash(new { r, classes, branch });
            var prior = await db.SaleTransactions.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId, ct);
            if (prior != null)
            {
                if (prior.RequestHash != hash || prior.CreatedByUserId != actor.UserId || prior.BranchId != branch) throw new BusinessRuleException("Operation belongs to another coaching purchase.");
                var subId = classes ? await db.ClassesSubscriptions.Where(s => s.SaleTransactionId == prior.Id).Select(s => s.Id).SingleAsync(ct)
                    : await db.PrivateSubscriptionTransactions.Where(t => t.SaleTransactionId == prior.Id).Select(t => t.PrivateSubscriptionId).SingleAsync(ct);
                return new CoachingPurchaseReceipt(subId, prior.Id, await db.Payments.Where(p => p.SaleTransactionId == prior.Id).Select(p => p.Id).ToListAsync(ct));
            }
            var shift = await shiftMoney.RequireShiftAsync(branch, actor.UserId, ct);
            var (date, day) = await BusinessDateAsync(branch, ct);
            string name; int count, days; decimal price;
            if (classes)
            {
                if (r.CoachEmployeeId != null) throw new BusinessRuleException("Classes packages share sessions across types and are not assigned to a Private coach.");
                var p = await db.ClassesPackages.FromSqlInterpolated($"SELECT * FROM \"ClassesPackages\" WHERE \"Id\"={r.PackageId} FOR SHARE").AsNoTracking().SingleAsync(ct);
                if (!p.IsActive) throw new BusinessRuleException("Package is inactive.");
                (name, count, days, price) = (p.Name, p.SessionCount, p.DurationDays, p.Price);
            }
            else
            {
                var p = await db.PrivatePackages.FromSqlInterpolated($"SELECT * FROM \"PrivatePackages\" WHERE \"Id\"={r.PackageId} FOR SHARE").AsNoTracking().SingleAsync(ct);
                if (!p.IsActive || r.CoachEmployeeId == null) throw new BusinessRuleException("Active Private package and assigned coach required.");
                await new CoachingService(db, current, access, subscriptions).ValidateCoachAsync(r.CoachEmployeeId.Value, branch, date, ct);
                (name, count, days, price) = (p.Name, p.SessionCount, p.DurationDays, p.Price);
            }
            if (!member.IsActive || count <= 0 || days <= 0 || days > DateOnly.MaxValue.DayNumber - date.DayNumber) throw new BusinessRuleException("Invalid package duration/count or inactive member.");
            Amount(price);
            if (r.PurchaseMode is not (PurchaseMode.FullPayment or PurchaseMode.Installment) || r.PaymentParts == null || r.PaymentParts.Count is < 1 or > 20 || r.PaymentParts.Any(p => p is null))
                throw new BusinessRuleException("Coaching packages support full payment or an explicitly scheduled installment purchase with an initial collection.");
            foreach (var p in r.PaymentParts) Amount(p.Amount);
            var tender = r.PaymentParts.Sum(p => p.Amount); Amount(tender);
            if (tender > price || r.PurchaseMode == PurchaseMode.FullPayment && (tender != price || r.Installments is { Count: > 0 })) throw new BusinessRuleException("Invalid package collection total.");
            if (r.PurchaseMode == PurchaseMode.Installment)
            {
                await RequireAsync(SalesPermissions.Installments, branch, ct);
                ValidateInstallmentSchedule(r.Installments, price - tender, tender, price, date);
            }
            var sale = new SaleTransaction { MemberId = member.Id, BranchId = branch, SaleType = classes ? SaleType.Classes : SaleType.Private,
                OperationId = r.OperationId, RequestHash = hash, CreatedShiftId = shift.Id, BusinessDate = date, BusinessDayId = day,
                PurchaseMode = r.PurchaseMode, ActivationMode = SubscriptionActivationMode.Now, InitialPaymentRequired = r.PurchaseMode == PurchaseMode.Installment ? tender : null,
                GrossAmount = price, NetAmount = price, Status = tender == price ? SaleStatus.Completed : SaleStatus.Open,

```

Lines 98–103

```csharp

    {
        if (sale.SaleType != SaleType.Private) return;
        var protectedValue = await db.CoachEarnings.Where(e => e.SaleTransactionId == sale.Id).SumAsync(e => e.EligibleValue, ct);
        if (newNetCollection < protectedValue) throw new BusinessRuleException("Refund affects finalized Private earnings. An Owner adjustment policy is required; nothing was changed.");
    }
    private async Task CancelCoachingGrantAsync(SaleTransaction sale, Guid actor, CancellationToken ct)

```

## CampionsSystem.BLL/Sales/SalesContracts.cs

Lines 3–26

```csharp

namespace CampionsSystem.BLL.Sales;
public static class SalesPermissions
{
    public const string View = "Payments.View";
    public const string Create = "Payments.Create";
    public const string Refund = "Payments.Refund";
    public const string Offers = "Offers.Manage";
    public const string Installments = "Installments.Manage";
    public const string QuickCancel = "Sales.QuickCancel";
    public static readonly string[] Codes = [View, Create, Refund, Offers, Installments, QuickCancel];
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record PaymentPartInput(Guid FinancialAccountId, decimal Amount, string? ProviderReference = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record InstallmentLineInput(decimal Amount, DateOnly DueDate);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record InstallmentAllocationInput(Guid InstallmentId, decimal Amount);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CreateSaleRequest(Guid OperationId, Guid MemberId, Guid MembershipPlanId,
    PurchaseMode PurchaseMode, SubscriptionActivationMode ActivationMode,
    IReadOnlyList<PaymentPartInput> PaymentParts, Guid? OfferId = null, DateOnly? StartDate = null,
    DateOnly? ReservationDueDate = null, IReadOnlyList<InstallmentLineInput>? Installments = null, Guid? BranchId = null, decimal? InitialPaymentRequired = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RecordPaymentRequest(Guid OperationId, decimal Amount, IReadOnlyList<PaymentPartInput> Parts,

```

Lines 33–58

```csharp

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record OfferRequest(string Name, OfferType Type, decimal Value, IReadOnlyList<Guid> MembershipPlanIds,
    OfferEligibilityMode? EligibilityMode, Guid? AudienceCategoryId = null,
    DateOnly? StartDate = null, DateOnly? EndDate = null, bool IsPermanent = false,
    bool IsActive = true, string? Description = null, Guid? BranchId = null);
public sealed record OfferDetails(Guid Id, Guid? BranchId, string Name, string Type, decimal Value,
    DateOnly? StartDate, DateOnly? EndDate, bool IsPermanent, bool IsActive, IReadOnlyList<Guid> MembershipPlanIds, string? EligibilityMode, Guid? AudienceCategoryId);
public sealed record PricingResult(decimal OriginalPrice, decimal Discount, decimal FinalPrice, int ExtraDays,
    Guid? OfferId, string? OfferName, OfferType? OfferType, decimal? OfferValue,
    OfferEligibilityMode? EligibilityMode = null, Guid? AudienceCategoryId = null);
public sealed record FinancialPartDetails(Guid AccountId, string AccountName, string Method, decimal Amount, string? Reference);
public sealed record PaymentDetails(Guid Id, Guid? OperationId, Guid BranchId, DateOnly? BusinessDate,
    Guid? BusinessDayId, DateTimeOffset PaidAt, decimal Amount, IReadOnlyList<FinancialPartDetails> Parts,
    IReadOnlyList<InstallmentAllocationInput> Allocations);
public sealed record RefundDetails(Guid Id, Guid? OperationId, Guid BranchId, DateOnly? BusinessDate,
    DateTimeOffset CreatedAt, decimal Amount, string Type, string Reason, IReadOnlyList<FinancialPartDetails> Parts);
public sealed record InstallmentDetails(Guid Id, int Number, DateOnly DueDate, decimal Amount, decimal Paid, string Status);
public sealed record SaleSummary(Guid Id, Guid MemberId, Guid BranchId, DateOnly? BusinessDate, DateTimeOffset CreatedAt,
    decimal OriginalPrice, decimal Discount, decimal FinalPrice, decimal Collected, decimal Refunded, decimal Outstanding,
    string Status, string? PurchaseMode, string? ActivationMode, bool Cancelled, int Version);
public sealed record SaleDetails(SaleSummary Sale, string MemberName, string? PlanName, string? PlanType,
    Guid? PlanId, Guid? AccessCityId, Guid? SalesEmployeeId, string? SalesAttribution,
    bool? CommissionEligible, bool? TargetEligible, Guid? SubscriptionId, Guid? GrantId,
    string ActivationStatus, DateOnly? RequestedStartDate, DateOnly? ReservationDueDate,
    string? OfferName, int ExtraDays, decimal RetainedCancellationRevenue,
    IReadOnlyList<PaymentDetails> Payments, IReadOnlyList<RefundDetails> Refunds, IReadOnlyList<InstallmentDetails> Installments,

```

Lines 64–70

```csharp

    public Guid? BranchId { get; set; }
    public Guid? MemberId { get; set; }
    public PurchaseMode? PurchaseMode { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;

```

Lines 74–80

```csharp

    decimal CashRefunded, decimal InstaPayRefunded, decimal GrossCollected, decimal Refunds, decimal NetCollected);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record GrantMemberOfferRequest(Guid MemberId, DateOnly? StartDate = null, DateOnly? EndDate = null);
public sealed record MemberOfferGrantDetails(Guid Id, Guid MemberId, Guid OfferId, DateOnly? StartDate, DateOnly? EndDate, DateTimeOffset? RevokedAt);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record SetMemberAudienceRequest(IReadOnlyList<Guid> AudienceCategoryIds);

```

## CampionsSystem.BLL/Sales/OfferService.MemberEligibility.cs

Lines 9–24

```csharp

    public async Task<MemberOfferGrantDetails> GrantMemberAsync(Guid offerId, GrantMemberOfferRequest r, CancellationToken ct = default)
    {
        var actor = await current.GetAsync(ct);
        if (actor is null || !actor.Permissions.Contains(SalesPermissions.Offers)) throw new UnauthorizedAccessException();
        if (r.MemberId == Guid.Empty || r.StartDate > r.EndDate) throw new BusinessRuleException("Invalid member grant dates.");
        return await TransactionAsync(async () =>
        {
            var member = await db.Members.FromSqlInterpolated($"SELECT * FROM \"Members\" WHERE \"Id\"={r.MemberId} FOR UPDATE")
                .SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Member not found.");
            await db.Entry(member).ReloadAsync(ct);
            var offer = await LockedOfferAsync(offerId, ct); await RequireAsync(SalesPermissions.Offers, offer.BranchId, ct);
            if (!member.IsActive || !offer.IsActive || offer.EligibilityMode != OfferEligibilityMode.MemberOnly)
                throw new BusinessRuleException("Select an active Member and MemberOnly Offer.");
            var existing = await db.MemberOfferEntitlements.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).FirstOrDefaultAsync(x => x.MemberId == member.Id && x.OfferId == offerId
                && x.RevokedAt == null && x.StartDate == r.StartDate && x.EndDate == r.EndDate, ct);
            if (existing != null) return GrantDetails(existing);

```

Lines 32–43

```csharp

    public async Task<MemberOfferGrantDetails> RevokeMemberAsync(Guid offerId, Guid grantId, CancellationToken ct = default)
    {
        var actor = await current.GetAsync(ct);
        if (actor is null || !actor.Permissions.Contains(SalesPermissions.Offers)) throw new UnauthorizedAccessException();
        var memberId = await db.MemberOfferEntitlements.Where(x => x.Id == grantId && x.OfferId == offerId).Select(x => (Guid?)x.MemberId).SingleOrDefaultAsync(ct)
            ?? throw new BranchNotFoundException("Offer grant not found.");
        return await TransactionAsync(async () =>
        {
            await db.Members.FromSqlInterpolated($"SELECT * FROM \"Members\" WHERE \"Id\"={memberId} FOR UPDATE").SingleAsync(ct);
            var offer = await LockedOfferAsync(offerId, ct); await RequireAsync(SalesPermissions.Offers, offer.BranchId, ct);
            var grant = await db.MemberOfferEntitlements.SingleAsync(x => x.Id == grantId && x.OfferId == offerId, ct); await db.Entry(grant).ReloadAsync(ct);
            if (grant.RevokedAt != null) return GrantDetails(grant);

```

Lines 51–65

```csharp

    public async Task<IReadOnlyList<MemberAudienceDetails>> SetMemberAudiencesAsync(Guid memberId, SetMemberAudienceRequest r, CancellationToken ct = default)
    {
        var actor = await current.GetAsync(ct);
        if (actor is null || !actor.Permissions.Contains("Members.Manage")) throw new UnauthorizedAccessException();
        if (r.AudienceCategoryIds is null || r.AudienceCategoryIds.Count > 100 || r.AudienceCategoryIds.Any(x => x == Guid.Empty)
            || r.AudienceCategoryIds.Distinct().Count() != r.AudienceCategoryIds.Count) throw new BusinessRuleException("Invalid audience assignments.");
        return await TransactionAsync(async () =>
        {
            var member = await db.Members.FromSqlInterpolated($"SELECT * FROM \"Members\" WHERE \"Id\"={memberId} FOR UPDATE")
                .SingleOrDefaultAsync(ct) ?? throw new BranchNotFoundException("Member not found.");
            await db.Entry(member).ReloadAsync(ct); BranchAccessService.EnsureScope(actor, member.HomeBranchId);
            var categories = await db.MembershipPlanCategories.Where(x => r.AudienceCategoryIds.Contains(x.Id) && x.IsActive).ToListAsync(ct);
            if (categories.Count != r.AudienceCategoryIds.Count) throw new BusinessRuleException("Assign only active configured audience categories.");
            var existing = await db.MemberAudienceCategories.Where(x => x.MemberId == memberId && x.EndedAt == null).ToListAsync(ct);
            foreach (var row in existing.Where(x => !r.AudienceCategoryIds.Contains(x.MembershipPlanCategoryId))) row.EndedAt = access.Now;

```

Lines 68–73

```csharp

            access.Audit(actor.UserId, member.HomeBranchId, "Member.AudiencesChanged", nameof(Member), member.Id,
                new { Categories = existing.Select(x => x.MembershipPlanCategoryId).ToArray() }, new { Categories = r.AudienceCategoryIds });
            await db.SaveChangesAsync(ct);
            return await db.MemberAudienceCategories.Where(x => x.MemberId == memberId && x.EndedAt == null).OrderBy(x => x.MembershipPlanCategory.Name)
                .Select(x => new MemberAudienceDetails(x.Id, x.MembershipPlanCategoryId, x.MembershipPlanCategory.Name, x.AssignedAt, x.EndedAt)).ToListAsync(ct);
        }, ct);

```

## CampionsSystem.BLL/Sales/SalesService.Refunds.cs

Lines 7–12

```csharp

{
    public Task<RefundDetails> RefundAsync(Guid saleId,RefundRequest r,CancellationToken ct=default)=>TransactionAsync(async()=>
    {
        Operation(r.OperationId);var sale=await LockSaleAsync(saleId,SalesPermissions.Refund,ct);var actor=await RequireAsync(SalesPermissions.Refund,sale.BranchId,ct);
        await RequireRefundRoleAsync(actor.UserId,quick:false,ct);
        var hash=Hash(new{SaleId=saleId,Request=r,Type=RefundType.ManagerRefund});
        var retry=await RefundRetryAsync(sale,r.OperationId,hash,actor.UserId,ct);if(retry!=null)return retry;

```

Lines 15–20

```csharp

    private async Task<RefundDetails?> RefundRetryAsync(SaleTransaction sale,Guid op,string hash,Guid actor,CancellationToken ct)
    {
        var retry=await db.Refunds.AsNoTracking().SingleOrDefaultAsync(x=>x.OperationId==op,ct);if(retry==null)return null;
        if(retry.SaleTransactionId!=sale.Id || retry.RequestHash!=hash || retry.ProcessedByUserId!=actor)throw new BusinessRuleException("Operation identity belongs to another refund request.");
        return await RefundDetailsAsync(retry.Id,ct);
    }

```

Lines 22–41

```csharp

        string reason,RefundType type,Guid actor,CancellationToken ct)
    {
        var owner=type==RefundType.ManagerRefund && await new OwnerAdministrationAccess(db,current).IsOwnerAsync(actor,ct);
        var activeShift=owner?null:await shiftMoney.RequireShiftAsync(sale.BranchId,actor,ct);
        Amount(amount,false);if(string.IsNullOrWhiteSpace(reason)||reason.Length>2000)throw new BusinessRuleException("A cancellation/refund reason is required (maximum 2000 characters).");
        if(parts is null)throw new BusinessRuleException("Refund parts are required (empty for a zero refund).");
        var collected=await db.Payments.Where(p=>p.SaleTransactionId==sale.Id && p.Status==PaymentStatus.Completed).SumAsync(p=>p.Amount,ct);
        var refunded=await db.Refunds.Where(r=>r.SaleTransactionId==sale.Id).SumAsync(r=>r.Amount,ct);
        if(amount>collected-refunded)throw new BusinessRuleException("Refund exceeds the collected, unrefunded amount.");
        await GuardCoachingRefundAsync(sale,collected-refunded-amount,ct);
        var historical=amount>0 && type==RefundType.QuickRefund && await HistoricalQuickRefundAsync(sale,parts,ct);
        if(amount>0)await AccountsAsync(sale.BranchId,parts,amount,ct,historical);
        else if(parts.Count!=0)throw new BusinessRuleException("A zero refund/cancellation must have no financial parts.");
        var (date,day)=await BusinessDateAsync(sale.BranchId,ct);
        var refund=new Refund{ShiftId=activeShift?.Id,SaleTransactionId=sale.Id,BranchId=sale.BranchId,OperationId=op,RequestHash=hash,Amount=amount,RefundType=type,
            Reason=reason.Trim(),ProcessedByUserId=actor,ApprovedByUserId=type==RefundType.QuickRefund?null:actor,CreatedAt=access.Now,BusinessDate=date,BusinessDayId=day};
        await new Commissions.CommissionSources(db,access).AllocateRefundAsync(refund,ct);
        db.Add(refund);
        foreach(var part in parts)db.Add(new RefundPart{Refund=refund,FinancialAccountId=part.FinancialAccountId,Amount=part.Amount,ProviderReference=part.ProviderReference});
        foreach(var group in parts.GroupBy(p=>p.FinancialAccountId))await shiftMoney.PostAsync(new FinancialTransaction{ShiftId=activeShift?.Id,Refund=refund,FinancialAccountId=group.Key,BranchId=sale.BranchId,
            TransactionType=FinancialTransactionType.Debit,SourceType=FinancialSourceType.Refund,Amount=group.Sum(p=>p.Amount),BusinessDate=date,BusinessDayId=day,OccurredAt=access.Now,CreatedByUserId=actor},ct,historical);
        if(sale.CancelledAt==null)
        {

```

Lines 55–61

```csharp

    private async Task<RefundDetails> RefundDetailsAsync(Guid id,CancellationToken ct)
    {
        var r=await db.Refunds.AsNoTracking().SingleAsync(x=>x.Id==id,ct);
        return new(r.Id,r.OperationId,r.BranchId,r.BusinessDate,r.CreatedAt,r.Amount,r.RefundType.ToString(),r.Reason,
            await db.RefundParts.AsNoTracking().Where(p=>p.RefundId==id).OrderBy(p=>p.Id).Select(p=>new FinancialPartDetails(p.FinancialAccountId,p.FinancialAccount.Name,p.FinancialAccount.AccountType.ToString(),p.Amount,p.ProviderReference)).ToListAsync(ct));
    }
}

```

## CampionsSystem.BLL/Sales/SalesService.Queries.cs

Lines 6–21

```csharp

namespace CampionsSystem.BLL.Sales;
public sealed partial class SalesService
{
    private static void Page(int page,int size){if(page is <1 or >1000000 || size is <1 or >100)throw new BusinessRuleException("Invalid pagination: page >= 1 and page size 1..100.");}
    private async Task<Guid?> ReadScopeAsync(Guid? selected,CancellationToken ct)
    {
        var actor=await current.GetAsync(ct)??throw new UnauthorizedAccessException();
        var branch=selected??(actor.Permissions.Contains("Branches.AccessAll")?null:actor.CurrentBranchId);
        await RequireAsync(SalesPermissions.View,branch,ct);return branch;
    }
    private async Task<SaleTransaction> ReadSaleAsync(Guid id,CancellationToken ct)
    {
        var sale=await db.SaleTransactions.AsNoTracking().SingleOrDefaultAsync(s=>s.Id==id,ct)??throw new BranchNotFoundException("Sale not found.");
        await RequireAsync(SalesPermissions.View,sale.BranchId,ct);return sale;
    }
    private sealed class MoneyFacts

```

Lines 33–53

```csharp

        var status=s.OperationId==null?s.Status.ToString():s.CancelledAt!=null?f.Refunded==f.Collected && f.Collected>0?"Refunded":f.Refunded>0?"PartiallyRefunded":"Cancelled"
            :f.Collected>=s.NetAmount?"Paid":s.PurchaseMode==PurchaseMode.Reservation?"Reserved":s.PurchaseMode==PurchaseMode.Installment
                && f.Collected>0 && f.Collected>=s.InitialPaymentRequired?"InstallmentActive":f.Collected>0?"PartiallyPaid":"PendingPayment";
        return new(s.Id,s.MemberId,s.BranchId,s.BusinessDate,s.CreatedAt,s.GrossAmount,s.DiscountAmount,s.NetAmount,f.Collected,f.Refunded,
            s.CancelledAt!=null?0:Math.Max(0,s.NetAmount-f.Collected),status,s.PurchaseMode?.ToString(),s.ActivationMode?.ToString(),s.CancelledAt!=null,s.Version);
    }
    public async Task<SaleHistory> GetHistoryAsync(SaleHistoryRequest r,CancellationToken ct=default)
    {
        Page(r.Page,r.PageSize);if(r.From>r.To || r.PurchaseMode.HasValue && !Enum.IsDefined(r.PurchaseMode.Value))throw new BusinessRuleException("Invalid Sale history filter.");
        var branch=await ReadScopeAsync(r.BranchId,ct);var q=db.SaleTransactions.AsNoTracking();
        if(branch!=null)q=q.Where(s=>s.BranchId==branch);if(r.MemberId!=null)q=q.Where(s=>s.MemberId==r.MemberId);
        if(r.PurchaseMode!=null)q=q.Where(s=>s.PurchaseMode==r.PurchaseMode);if(r.From!=null)q=q.Where(s=>s.BusinessDate>=r.From);if(r.To!=null)q=q.Where(s=>s.BusinessDate<=r.To);
        var count=await q.CountAsync(ct);var rows=await Facts(q.OrderByDescending(s=>s.CreatedAt).ThenBy(s=>s.Id).Skip((r.Page-1)*r.PageSize).Take(r.PageSize)).ToListAsync(ct);
        return new(rows.Select(Summary).ToArray(),count,r.Page,r.PageSize);
    }
    public async Task<IReadOnlyList<InstallmentDetails>> GetInstallmentsAsync(Guid saleId,CancellationToken ct=default)
    {
        var sale=await ReadSaleAsync(saleId,ct);var date=(await BusinessDateAsync(sale.BranchId,ct)).Date;
        var rows=await db.Installments.AsNoTracking().Where(i=>i.InstallmentAgreement.SaleTransactionId==saleId).OrderBy(i=>i.InstallmentNumber).Take(100)
            .Select(i=>new{i.Id,i.InstallmentNumber,i.DueDate,i.AmountDue,Paid=db.InstallmentPaymentAllocations.Where(a=>a.InstallmentId==i.Id && a.Payment.Status==PaymentStatus.Completed).Sum(a=>a.Amount)}).ToListAsync(ct);
        return rows.Select(i=>new InstallmentDetails(i.Id,i.InstallmentNumber,i.DueDate,i.AmountDue,i.Paid,

```

Lines 56–67

```csharp

    public async Task<PaymentHistory> GetPaymentsAsync(Guid saleId,int page=1,int pageSize=25,CancellationToken ct=default)
    {
        Page(page,pageSize);await ReadSaleAsync(saleId,ct);var q=db.Payments.AsNoTracking().Where(p=>p.SaleTransactionId==saleId);var count=await q.CountAsync(ct);
        var ids=await q.OrderByDescending(p=>p.PaidAt).ThenBy(p=>p.Id).Skip((page-1)*pageSize).Take(pageSize).Select(p=>p.Id).ToListAsync(ct);
        var rows=new List<PaymentDetails>();foreach(var id in ids)rows.Add(await PaymentDetailsAsync(id,ct));return new(rows,count,page,pageSize);
    }
    public async Task<RefundHistory> GetRefundsAsync(Guid saleId,int page=1,int pageSize=25,CancellationToken ct=default)
    {
        Page(page,pageSize);await ReadSaleAsync(saleId,ct);var q=db.Refunds.AsNoTracking().Where(p=>p.SaleTransactionId==saleId);var count=await q.CountAsync(ct);
        var ids=await q.OrderByDescending(p=>p.CreatedAt).ThenBy(p=>p.Id).Skip((page-1)*pageSize).Take(pageSize).Select(p=>p.Id).ToListAsync(ct);
        var rows=new List<RefundDetails>();foreach(var id in ids)rows.Add(await RefundDetailsAsync(id,ct));return new(rows,count,page,pageSize);
    }

```

Lines 81–86

```csharp

            var evidence=await db.CommissionCollections.Where(c=>payments.Select(p=>p.Id).Contains(c.PaymentId)).ToDictionaryAsync(c=>c.PaymentId,ct);
            foreach(var p in payments.Where(p=>p.Amount>refunded.GetValueOrDefault(p.Id)))
            {
                var reason=evidence.TryGetValue(p.Id,out var fact)?fact.EligibilityReason:(await new Commissions.CommissionSources(db,access).AttributionAsync(sale,p.BusinessDate,p.PaidAt,ct)).Reason;
                if(reason=="Eligible"){retainedEligible=true;break;}
            }

```

Lines 92–102

```csharp

            await GetInstallmentsAsync(id,ct),sale.OfferId,sale.OfferTypeSnapshot?.ToString(),sale.OfferValueSnapshot,sale.OfferEligibilityModeSnapshot?.ToString(),sale.OfferAudienceCategoryIdSnapshot,
            RetainedCommissionEligible:retainedEligible,RetainedTargetEligible:retainedEligible);
    }
    public async Task<BranchFinancialTotals> GetBranchTotalsAsync(Guid? branchId=null,DateOnly? from=null,DateOnly? to=null,CancellationToken ct=default)
    {
        if(from>to)throw new BusinessRuleException("Invalid financial date range.");var actor=await current.GetAsync(ct)??throw new UnauthorizedAccessException();
        var branch=branchId??actor.CurrentBranchId??throw new BusinessRuleException("Select a branch for financial totals.");await RequireAsync(SalesPermissions.View,branch,ct);
        var p=db.PaymentParts.Where(p=>p.Payment.BranchId==branch && p.Payment.Status==PaymentStatus.Completed && (from==null || p.Payment.BusinessDate>=from) && (to==null || p.Payment.BusinessDate<=to));
        var r=db.RefundParts.Where(r=>r.Refund.BranchId==branch && (from==null || r.Refund.BusinessDate>=from) && (to==null || r.Refund.BusinessDate<=to));
        var pc=await p.GroupBy(p=>p.FinancialAccount.AccountType).Select(g=>new{Method=g.Key,Amount=g.Sum(p=>p.Amount)}).ToListAsync(ct);
        var rc=await r.GroupBy(p=>p.FinancialAccount.AccountType).Select(g=>new{Method=g.Key,Amount=g.Sum(p=>p.Amount)}).ToListAsync(ct);

```

## CampionsSystem.BLL/Sales/OfferService.Queries.cs

Lines 5–13

```csharp

{
    public async Task<OfferHistory> GetManagementHistoryAsync(Guid? branchId=null,int page=1,int pageSize=25,CancellationToken ct=default)
    {
        if(page is <1 or >1000000 || pageSize is <1 or >100)throw new BusinessRuleException("Invalid Offer pagination.");
        var actor=await current.GetAsync(ct)??throw new UnauthorizedAccessException();var branch=branchId??(actor.Permissions.Contains("Branches.AccessAll")?null:actor.CurrentBranchId);
        await RequireAsync(SalesPermissions.Offers,branch,ct);var q=db.Offers.AsNoTracking();if(branch!=null)q=q.Where(o=>o.BranchId==branch);
        var count=await q.CountAsync(ct);var offers=await q.OrderByDescending(o=>o.CreatedAt).ThenBy(o=>o.Id).Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct);
        var rows=new List<OfferDetails>();foreach(var o in offers)rows.Add(await DetailsAsync(o,ct));return new(rows,count,page,pageSize);
    }

```

## CampionsSystem.BLL/Sales/SalesService.QuickCancel.cs

Current Fix #3 source excerpt

```csharp

{
    public Task<RefundDetails> QuickCancelAsync(Guid saleId,QuickCancelRequest r,CancellationToken ct=default)=>TransactionAsync(async()=>
    {
        Operation(r.OperationId);var sale=await LockSaleAsync(saleId,SalesPermissions.QuickCancel,ct);var actor=await RequireAsync(SalesPermissions.QuickCancel,sale.BranchId,ct);
        await RequireRefundRoleAsync(actor.UserId,quick:true,ct);
        var hash=Hash(new{SaleId=saleId,Request=r,Type=RefundType.QuickRefund});var retry=await RefundRetryAsync(sale,r.OperationId,hash,actor.UserId,ct);if(retry!=null)return retry;
        if(sale.CancelledAt!=null || sale.CreatedAt>access.Now || access.Now-sale.CreatedAt>TimeSpan.FromHours(24))
            throw new BusinessRuleException("Quick Cancel requires an uncancelled Sale created within the last 24 actual hours.");
        if(r.Amount is null)throw new BusinessRuleException("Choose an explicit refund amount, including zero for cancellation without payment.");
        var refund=await RefundCoreAsync(sale,r.OperationId,hash,r.Amount.Value,r.Parts??[],r.Reason,RefundType.QuickRefund,actor.UserId,ct);
        access.Audit(actor.UserId,sale.BranchId,"Sale.QuickCancelled",nameof(SaleTransaction),sale.Id,
            after:new{RefundId=refund.Id,refund.Amount,Retained=await RetainedAsync(sale.Id,ct)});
        await db.SaveChangesAsync(ct);return refund;
    },ct);

```

## CampionsSystem.BLL/Sales/SalesPermissionSetup.cs

Lines 9–14

```csharp

    {
        await using var tx=db.Database.CurrentTransaction is null?await db.Database.BeginTransactionAsync(ct):null;
        if(managementRoleId.HasValue && !await db.RolePermissions.AnyAsync(p=>p.RoleId==managementRoleId && p.Permission.Code=="Branches.AccessAll" && p.Permission.IsActive,ct))
            throw new InvalidOperationException("Select an existing role with active global branch capability.");
        var permissions=await db.Permissions.Where(p=>SalesPermissions.Codes.Contains(p.Code)).ToListAsync(ct);
        foreach(var code in SalesPermissions.Codes.Where(code=>permissions.All(p=>p.Code!=code)))

```

## CampionsSystem.BLL/Sales/SalesService.Accounts.cs

Lines 7–28

```csharp

{
    public async Task<IReadOnlyList<FinancialAccountDetails>> GetAccountsAsync(Guid? branchId=null,CancellationToken ct=default)
    {
        var actor=await current.GetAsync(ct)??throw new UnauthorizedAccessException();var branch=branchId??actor.CurrentBranchId??throw new BusinessRuleException("Select an operational branch.");
        await RequireAsync(SalesPermissions.View,branch,ct);
        return await db.FinancialAccounts.AsNoTracking().Where(a=>a.BranchId==branch && a.IsActive && (a.AccountType==FinancialAccountType.Cash || a.AccountType==FinancialAccountType.InstaPay))
            .OrderBy(a=>a.Name).Take(100).Select(a=>new FinancialAccountDetails(a.Id,a.BranchId,a.Name,a.AccountType.ToString(),a.IsActive)).ToListAsync(ct);
    }
    public Task<FinancialAccountDetails> CreateAccountAsync(CreateFinancialAccountRequest r,CancellationToken ct=default)=>TransactionAsync(async()=>
    {
        var actor=await current.GetAsync(ct)??throw new UnauthorizedAccessException();
        if(!actor.Permissions.Contains("Branches.AccessAll"))throw new UnauthorizedAccessException();
        var branch=r.BranchId??actor.CurrentBranchId??throw new BusinessRuleException("Select a branch.");await RequireAsync("Branches.Manage",branch,ct);
        if(string.IsNullOrWhiteSpace(r.Name)||r.Name.Length>256 || r.Method is not (FinancialAccountType.Cash or FinancialAccountType.InstaPay))throw new BusinessRuleException("A named branch Cash/InstaPay account is required.");
        var row=await db.GymBranches.FromSqlInterpolated($"SELECT * FROM \"GymBranches\" WHERE \"Id\"={branch} FOR UPDATE").AsNoTracking().SingleOrDefaultAsync(ct);
        if(row==null || !row.IsActive)throw new BusinessRuleException("Account branch must be active.");
        var name=r.Name.Trim();var existing=await db.FinancialAccounts.AsNoTracking().SingleOrDefaultAsync(a=>a.BranchId==branch && a.Name==name,ct);
        if(existing!=null)
        {
            if(existing.AccountType!=r.Method || !existing.IsActive)throw new BusinessRuleException("This account name already identifies another/inactive account.");
            return new FinancialAccountDetails(existing.Id,branch,existing.Name,existing.AccountType.ToString(),existing.IsActive);
        }

```

## CampionsSystem.BLL/Sales/SalesService.Installments.cs

Lines 6–19

```csharp

public sealed partial class SalesService
{
    private static void ValidateInstallmentSchedule(IReadOnlyList<InstallmentLineInput>? lines,decimal remaining,
        decimal initial,decimal total,DateOnly date)
    {
        Amount(initial);
        if(initial>total || remaining<=0 || lines is null || lines.Count is <1 or >100 || lines.Any(x=>x is null || x.DueDate<date))
            throw new BusinessRuleException("Installments require a positive agreed initial payment and valid remaining obligations.");
        foreach(var line in lines)Amount(line.Amount);
        if(lines.Sum(x=>x.Amount)!=remaining)throw new BusinessRuleException("Installment schedule must equal the Sale balance after the actual initial payment.");
    }
    private void CreateInstallmentSchedule(SaleTransaction sale,IReadOnlyList<InstallmentLineInput> lines,DateOnly date,Guid actor)
    {
        var agreement=new InstallmentAgreement{SaleTransaction=sale,TotalAmount=lines.Sum(x=>x.Amount),NumberOfInstallments=lines.Count,

```

Lines 26–40

```csharp

    {
        if(allocations is null || allocations.Count is <1 or >100 || allocations.Any(x=>x is null || x.InstallmentId==Guid.Empty)
            || allocations.Select(x=>x.InstallmentId).Distinct().Count()!=allocations.Count)
            throw new BusinessRuleException("Installment payments require explicit unique obligation allocations.");
        foreach(var allocation in allocations)Amount(allocation.Amount);
        if(allocations.Sum(x=>x.Amount)!=payment.Amount)throw new BusinessRuleException("Installment allocations must equal the Payment amount.");
        var wasOverdue=await subscriptions.IsPaidSaleOverdueAsync(sale.Id,payment.BusinessDate!.Value,ct);
        foreach(var allocation in allocations.OrderBy(x=>x.InstallmentId))
        {
            var line=await db.Installments.Include(x=>x.InstallmentAgreement).SingleOrDefaultAsync(x=>x.Id==allocation.InstallmentId && x.InstallmentAgreement.SaleTransactionId==sale.Id,ct)
                ?? throw new BusinessRuleException("Installment does not belong to this Sale.");
            var paid=await db.InstallmentPaymentAllocations.Where(x=>x.InstallmentId==line.Id && x.Payment.Status==PaymentStatus.Completed).SumAsync(x=>x.Amount,ct);
            if(allocation.Amount>line.AmountDue-paid)throw new BusinessRuleException("Allocation exceeds the outstanding installment obligation.");
            db.Add(new InstallmentPaymentAllocation{Payment=payment,InstallmentId=line.Id,Amount=allocation.Amount});
            line.AmountPaid=paid+allocation.Amount;line.Status=line.AmountPaid==line.AmountDue?InstallmentStatus.Paid:InstallmentStatus.PartiallyPaid;

```

Lines 45–50

```csharp

        var agreement=await db.InstallmentAgreements.SingleAsync(x=>x.SaleTransactionId==sale.Id,ct);
        if(!await db.Installments.AnyAsync(x=>x.InstallmentAgreementId==agreement.Id && x.AmountDue>
            db.InstallmentPaymentAllocations.Where(a=>a.InstallmentId==x.Id && a.Payment.Status==PaymentStatus.Completed).Sum(a=>a.Amount),ct))agreement.Status=InstallmentAgreementStatus.Completed;
        if(wasOverdue && !await subscriptions.IsPaidSaleOverdueAsync(sale.Id,payment.BusinessDate!.Value,ct))
            access.Audit(actor,sale.BranchId,"Installment.OverdueResolved",nameof(SaleTransaction),sale.Id,after:new{sale.MemberId});
    }

```

## CampionsSystem.BLL/Sales/SalesService.Payments.cs

Lines 8–25

```csharp

    public Task<PaymentDetails> RecordPaymentAsync(Guid saleId, RecordPaymentRequest r, CancellationToken ct=default) => TransactionAsync(async () =>
    {
        Operation(r.OperationId); var sale = await LockSaleAsync(saleId,SalesPermissions.Create,ct);
        var actor = await RequireAsync(SalesPermissions.Create,sale.BranchId,ct); var hash=Hash(new{SaleId=saleId,Request=r});
        var retry=await db.Payments.AsNoTracking().SingleOrDefaultAsync(x=>x.OperationId==r.OperationId,ct);
        if(retry!=null)
        {
            if(retry.SaleTransactionId!=saleId || retry.RequestHash!=hash || retry.ReceivedByUserId!=actor.UserId)
                throw new BusinessRuleException("Operation identity belongs to another payment request.");
            return await PaymentDetailsAsync(retry.Id,ct);
        }
        if(sale.CancelledAt!=null)throw new BusinessRuleException("Cancelled Sales cannot receive additional payments.");
        if(sale.PurchaseMode!=PurchaseMode.Installment && r.Allocations is {Count:>0})throw new BusinessRuleException("This Sale has no installment obligations.");
        Amount(r.Amount);
        var collected=await db.Payments.Where(x=>x.SaleTransactionId==sale.Id && x.Status==PaymentStatus.Completed).SumAsync(x=>x.Amount,ct);
        if(r.Amount>sale.NetAmount-collected)throw new BusinessRuleException("Payment exceeds the outstanding Sale amount.");
        var payment=await PaymentAsync(sale,r.OperationId,hash,r.Amount,r.Parts,actor.UserId,ct);
        if(sale.PurchaseMode==PurchaseMode.Installment)await AllocateInstallmentsAsync(sale,payment,r.Allocations,actor.UserId,ct);

```

Lines 33–40

```csharp

    private async Task<PaymentDetails> PaymentDetailsAsync(Guid id,CancellationToken ct)
    {
        var p=await db.Payments.AsNoTracking().SingleAsync(x=>x.Id==id,ct);
        return new(p.Id,p.OperationId,p.BranchId,p.BusinessDate,p.BusinessDayId,p.PaidAt,p.Amount,
            await db.PaymentParts.AsNoTracking().Where(x=>x.PaymentId==id).OrderBy(x=>x.Id).Select(x=>new FinancialPartDetails(x.FinancialAccountId,x.FinancialAccount.Name,x.FinancialAccount.AccountType.ToString(),x.Amount,x.ProviderReference)).ToListAsync(ct),
            await db.InstallmentPaymentAllocations.AsNoTracking().Where(x=>x.PaymentId==id).OrderBy(x=>x.InstallmentId).Select(x=>new InstallmentAllocationInput(x.InstallmentId,x.Amount)).ToListAsync(ct));
    }
}

```

## CampionsSystem.BLL/Sales/SalesService.Activation.cs

Lines 7–12

```csharp

    {
        var result = await RecordSaleAsync(r,ct);
        var sale = await db.SaleTransactions.SingleAsync(x=>x.Id==result.SaleId,ct);
        var actor = await RequireAsync(SalesPermissions.Create,sale.BranchId,ct);
        if (!await db.MembershipSubscriptionTransactions.AnyAsync(x=>x.SaleTransactionId==sale.Id && x.OperationId!=null,ct))
            await subscriptions.ActivatePaidSaleAsync(sale.Id,actor.UserId,false,null,ct);

```
