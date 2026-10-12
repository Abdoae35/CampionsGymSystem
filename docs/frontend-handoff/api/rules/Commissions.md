# Commissions rule/validation evidence

Completed source; deployment/configuration prerequisites are separate. Shared cross-cutting helpers: Authentication, Branches, Operations and Shifts.

## CampionsSystem.BLL/Commissions/CommissionContracts.cs

Lines 2–20

```csharp

namespace CampionsSystem.BLL.Commissions;
public static class CommissionPermissions
{
    public const string View = "Commission.View", Manage = "Commission.ManageRules", Close = "Commission.ClosePeriod";
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CategoryRuleInput(decimal Target, decimal BelowRate, decimal ReachedRate);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ConfigureCommissionRequest(Guid OperationId, Guid? EmployeeId, DateOnly EffectiveFrom,
    CategoryRuleInput NewClients, CategoryRuleInput Renewals, Guid? BranchId = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record OpenCommissionPeriodRequest(Guid OperationId, string Name, DateOnly StartDate, DateOnly EndDate, Guid? BranchId = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record FinalizeCommissionPeriodRequest(Guid OperationId, int ExpectedVersion);
public sealed record CommissionPeriodDetails(Guid Id, Guid BranchId, string Name, DateOnly? StartDate, DateOnly? EndDate,
    string Status, int Version, DateTimeOffset? FinalizedAt);
public sealed record CommissionRuleDetails(Guid Id, Guid? EmployeeId, Guid BranchId, string Category, DateOnly EffectiveFrom,
    decimal? Target, decimal? BelowRate, decimal? ReachedRate, int Revision);
public sealed record CommissionCategoryProgress(string Category, decimal GrossCollections, decimal Refunds, decimal EligibleCollections,

```

Lines 29–36

```csharp

    decimal NewClientEligibleCollections, decimal RenewalEligibleCollections, decimal TotalCommission, int EmployeeCount,
    IReadOnlyList<EmployeeCommissionStatement> Employees, int Page, int PageSize);
public sealed record CommissionSourceLine(Guid PaymentId, Guid SaleId, Guid MemberId, Guid BranchId, Guid? OriginalSalesEmployeeId,
    Guid? EmployeeId, string? Category, string EligibilityReason, DateOnly? BusinessDate, DateTimeOffset PaidAt,
    decimal GrossCollection, decimal Refunds, decimal RetainedCollection, decimal EligibleCollection);
public sealed record CommissionPage<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
public sealed record CommissionRefundSource(Guid RefundId, Guid PaymentId, decimal Amount, DateOnly? CollectionBusinessDate,
    DateOnly? RefundBusinessDate, DateTimeOffset RefundedAt);

```

## CampionsSystem.BLL/Commissions/CommissionSources.cs

Lines 11–25

```csharp

{
    internal async Task RecordCollectionAsync(Payment payment, SaleTransaction sale, CancellationToken ct)
    {
        var date = payment.BusinessDate!.Value;
        var period = await db.CommissionPeriods.AsNoTracking().SingleOrDefaultAsync(p => p.BranchId == sale.BranchId
            && p.StartBusinessDate <= date && p.EndBusinessDate >= date, ct);
        if (period?.Status == CommissionPeriodStatus.Closed) throw new BusinessRuleException("Collection BusinessDate belongs to a finalized commission period.");
        payment.CommissionPeriodId = period?.Id;
        var (employee, reason) = await AttributionAsync(sale, date, payment.PaidAt, ct);
        db.Add(new CommissionCollection { Payment = payment, EmployeeId = employee, Category = sale.SaleCategory,
            EligibilityReason = reason, CreatedAt = access.Now });
    }
    internal async Task<(Guid? Employee, string Reason)> AttributionAsync(SaleTransaction sale, DateOnly? date, DateTimeOffset paidAt, CancellationToken ct)
    {
        if (sale.SaleType != SaleType.GymMembership) return (null, "ExcludedProduct");

```

Lines 33–44

```csharp

        var employmentDate = access.LocalDate(paidAt);
        // Historical source audit preserves ended contracts after rehire clears the current termination field.
        var changes = await db.AuditLogs.AsNoTracking().Where(a => a.EntityId == e.Id && a.Action == "EmployeeStatusChanged")
            .OrderBy(a => a.CreatedAt).ThenBy(a => a.Id).Select(a => new { a.CreatedAt, a.OldValues, a.NewValues }).ToListAsync(ct);
        var ended = changes.Where(a => a.CreatedAt >= sale.CreatedAt && a.CreatedAt <= paidAt).Select(a => a.NewValues);
        foreach (var json in ended.Where(x => x != null))
        {
            using var doc = JsonDocument.Parse(json!);
            if (doc.RootElement.TryGetProperty("TerminationDate", out var term) && term.ValueKind == JsonValueKind.String
                && DateOnly.TryParse(term.GetString(), out var end) && employmentDate > end) return (null, "EmploymentEnded");
        }
        var status = e.EmploymentStatus; var termination = e.TerminationDate;

```

Lines 50–55

```csharp

            if (doc.RootElement.TryGetProperty("EmploymentStatus", out var value))
                status = value.ValueKind == JsonValueKind.Number ? (EmploymentStatus)value.GetInt32() : Enum.Parse<EmploymentStatus>(value.GetString()!);
            if (doc.RootElement.TryGetProperty("TerminationDate", out var term))
                termination = term.ValueKind == JsonValueKind.String ? DateOnly.Parse(term.GetString()!) : null;
        }
        if (e.HireDate > employmentDate || termination < employmentDate || status != EmploymentStatus.Active

```

Lines 60–65

```csharp

        Dictionary<Guid, decimal> already, DateTimeOffset refundAt)
    {
        var result = new Dictionary<Guid, decimal>();
        foreach (var p in payments.Where(p => p.PaidAt <= refundAt).OrderByDescending(p => p.PaidAt).ThenByDescending(p => p.Id))
        {
            var take = Math.Min(amount, p.Amount - already.GetValueOrDefault(p.Id));

```

Lines 67–99

```csharp

            result[p.Id] = take; already[p.Id] = already.GetValueOrDefault(p.Id) + take; amount -= take;
            if (amount == 0) break;
        }
        if (amount != 0) throw new BusinessRuleException("Refund cannot be reconciled to actual original collections.");
        return result;
    }
    internal async Task<Dictionary<Guid, decimal>> RefundedAsync(IReadOnlyList<Payment> payments, CancellationToken ct)
    {
        var sales = payments.Select(p => p.SaleTransactionId).Distinct().ToArray();
        var refunds = await db.Refunds.AsNoTracking().Where(r => sales.Contains(r.SaleTransactionId)).OrderBy(r => r.CreatedAt).ThenBy(r => r.Id).ToListAsync(ct);
        var ids = refunds.Select(r => r.Id).ToArray();
        var allocations = await db.CommissionRefundAllocations.AsNoTracking().Where(a => ids.Contains(a.RefundId)).ToListAsync(ct);
        var already = new Dictionary<Guid, decimal>();
        foreach (var refund in refunds.OrderBy(r => r.CreatedAt).ThenBy(r => r.Id))
        {
            var explicitRows = allocations.Where(a => a.RefundId == refund.Id).ToList();
            if (explicitRows.Count > 0)
            {
                if (explicitRows.Sum(a => a.Amount) != refund.Amount) throw new BusinessRuleException("Refund allocation history does not reconcile.");
                foreach (var a in explicitRows) already[a.PaymentId] = already.GetValueOrDefault(a.PaymentId) + a.Amount;
            }
            else if (refund.OriginalPaymentId is Guid original)
            {
                var p = payments.SingleOrDefault(p => p.Id == original && p.SaleTransactionId == refund.SaleTransactionId)
                    ?? throw new BusinessRuleException("Legacy refund has an invalid original collection.");
                if (p.Amount - already.GetValueOrDefault(p.Id) < refund.Amount) throw new BusinessRuleException("Legacy refund exceeds its original collection.");
                already[p.Id] = already.GetValueOrDefault(p.Id) + refund.Amount;
            }
            else AllocateNewest(payments.Where(p => p.SaleTransactionId == refund.SaleTransactionId).ToArray(), refund.Amount, already, refund.CreatedAt);
        }
        if (payments.Any(p => already.GetValueOrDefault(p.Id) > p.Amount)) throw new BusinessRuleException("Refund allocation exceeds a collection.");
        return already;
    }

```

Lines 106–112

```csharp

        {
            var p = payments.Single(p => p.Id == paymentId);
            if (await db.CommissionPeriods.AnyAsync(period => period.Status == CommissionPeriodStatus.Closed && period.BranchId == p.BranchId
                && (period.Id == p.CommissionPeriodId || period.StartBusinessDate <= p.BusinessDate && period.EndBusinessDate >= p.BusinessDate), ct))
                throw new BusinessRuleException("Refund would change a finalized commission period; no financial or entitlement change was made.");
            db.Add(new CommissionRefundAllocation { Refund = refund, PaymentId = paymentId, Amount = amount });
        }

```

## CampionsSystem.BLL/Commissions/CommissionService.Calculation.cs

Lines 6–11

```csharp

namespace CampionsSystem.BLL.Commissions;
public sealed partial class CommissionService
{
    internal static decimal Earned(decimal eligible, decimal rate) => decimal.Round(eligible * rate / 100m, 2, MidpointRounding.AwayFromZero);
    private async Task<List<CommissionSourceLine>> LinesAsync(CommissionPeriod period, CancellationToken ct)
    {

```

Lines 13–20

```csharp

            return await db.CommissionStatementLines.AsNoTracking().Where(l => l.CommissionPeriodId == period.Id).Select(l =>
                new CommissionSourceLine(l.PaymentId, l.Payment.SaleTransactionId, l.Payment.SaleTransaction.MemberId, period.BranchId,
                    l.Payment.SaleTransaction.SalesEmployeeId, l.EmployeeId, l.Category == null ? null : l.Category.ToString(), l.EligibilityReason,
                    l.Payment.BusinessDate, l.Payment.PaidAt, l.GrossAmount, l.RefundedAmount, l.GrossAmount - l.RefundedAmount, l.EligibleAmount)).ToListAsync(ct);
        var periodPayments = await db.Payments.AsNoTracking().Where(p => p.BranchId == period.BranchId && p.Status == PaymentStatus.Completed
            && p.BusinessDate >= period.StartBusinessDate && p.BusinessDate <= period.EndBusinessDate).ToListAsync(ct);
        var saleIds = periodPayments.Select(p => p.SaleTransactionId).Distinct().ToArray();
        var allPayments = await db.Payments.AsNoTracking().Where(p => saleIds.Contains(p.SaleTransactionId) && p.Status == PaymentStatus.Completed).ToListAsync(ct);

```

Lines 25–33

```csharp

        foreach (var p in periodPayments)
        {
            var sale = sales[p.SaleTransactionId]; evidence.TryGetValue(p.Id, out var fact);
            var (employee, reason) = fact == null ? await sources.AttributionAsync(sale, p.BusinessDate, p.PaidAt, ct) : (fact.EmployeeId, fact.EligibilityReason);
            var refunded = refunds.GetValueOrDefault(p.Id); var net = p.Amount - refunded;
            result.Add(new(p.Id, sale.Id, sale.MemberId, p.BranchId, sale.SalesEmployeeId, employee, (fact?.Category ?? sale.SaleCategory)?.ToString(), reason,
                p.BusinessDate, p.PaidAt, p.Amount, refunded, net, reason == "Eligible" ? net : 0));
        }
        return result;

```

Lines 61–69

```csharp

                var n = Closed(CommissionType.SalesNew); var r = Closed(CommissionType.SalesRenewal);
                result.Add(new(g.Key, g.First().Employee.FullName, n, r, n.Commission + r.Commission, period.Id, true, House(g.Key), Excluded(g.Key)));
            }
            return result.OrderBy(x => x.EmployeeId).ToList();
        }
        var rules = await db.CommissionRules.AsNoTracking().Where(r => r.BranchId == period.BranchId && r.IsActive && r.EffectiveFrom <= period.StartBusinessDate
            && (r.EffectiveTo == null || r.EffectiveTo >= period.StartBusinessDate) && (r.CommissionType == CommissionType.SalesNew || r.CommissionType == CommissionType.SalesRenewal)).ToListAsync(ct);
        var employees = lines.Where(x => x.EmployeeId != null).Select(x => x.EmployeeId!.Value)
            .Concat(rules.Where(x => x.EmployeeId != null).Select(x => x.EmployeeId!.Value)).Distinct().ToArray();

```

Lines 71–76

```csharp

        foreach (var employee in employees.Order())
        {
            CommissionRule? Rule(CommissionType type) => rules.Where(r => r.CommissionType == type && (r.EmployeeId == employee || r.EmployeeId == null))
                .OrderByDescending(r => r.EmployeeId != null).ThenByDescending(r => r.EffectiveFrom).ThenByDescending(r => r.Revision).ThenByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id).FirstOrDefault();
            var n = Calculate("NewMembership", lines.Where(x => x.EmployeeId == employee && x.Category == "NewMembership").ToArray(), Rule(CommissionType.SalesNew));
            var r = Calculate("Renewal", lines.Where(x => x.EmployeeId == employee && x.Category == "Renewal").ToArray(), Rule(CommissionType.SalesRenewal));

```

Lines 97–102

```csharp

            lines.Where(l => l.EmployeeId == null && l.Category == "Renewal" && l.EligibilityReason is "House" or "EmploymentEnded" or "EmploymentInactive").Sum(l => l.RetainedCollection),
            retained - eligible - house, lines.Where(l => l.Category == "NewMembership").Sum(l => l.EligibleCollection),
            lines.Where(l => l.Category == "Renewal").Sum(l => l.EligibleCollection), statements.Sum(s => s.TotalCommission), statements.Count,
            statements.Skip((page - 1) * pageSize).Take(pageSize).ToArray(), page, pageSize);
    }
    public Task<EmployeeCommissionStatement> EmployeeStatementAsync(Guid periodId, Guid employeeId, CancellationToken ct = default)

```

Lines 104–109

```csharp

    private async Task<EmployeeCommissionStatement> EmployeeStatementCoreAsync(Guid periodId, Guid employeeId, CancellationToken ct)
    {
        var p = await ReadAsync(periodId, ct); var statements = await StatementsAsync(p, await LinesAsync(p, ct), ct);
        return statements.SingleOrDefault(s => s.EmployeeId == employeeId) ?? throw new Branches.BranchNotFoundException("Employee has no configuration or attributed collection in this period.");
    }
    public Task<CommissionPage<CommissionSourceLine>> SourcesAsync(Guid periodId, Guid? employeeId = null, int page = 1, int pageSize = 25, CancellationToken ct = default)

```

Lines 112–118

```csharp

    {
        Page(page, pageSize); var p = await ReadAsync(periodId, ct); IEnumerable<CommissionSourceLine> lines = await LinesAsync(p, ct);
        if (employeeId.HasValue) lines = lines.Where(l => l.EmployeeId == employeeId || l.OriginalSalesEmployeeId == employeeId);
        var rows = lines.OrderByDescending(l => l.PaidAt).ThenByDescending(l => l.PaymentId).ToArray();
        return new(rows.Skip((page - 1) * pageSize).Take(pageSize).ToArray(), rows.Length, page, pageSize);
    }
    public Task<CommissionPage<CommissionRefundSource>> RefundSourcesAsync(Guid periodId, int page = 1, int pageSize = 25, CancellationToken ct = default)

```

Lines 120–146

```csharp

        {
            Page(page, pageSize); var p = await ReadAsync(periodId, ct);
            var q = db.CommissionRefundAllocations.AsNoTracking().Where(a => a.Payment.BranchId == p.BranchId
                && a.Payment.BusinessDate >= p.StartBusinessDate && a.Payment.BusinessDate <= p.EndBusinessDate);
            var total = await q.CountAsync(ct);
            var rows = await q.OrderByDescending(a => a.Refund.CreatedAt).ThenBy(a => a.RefundId).ThenBy(a => a.PaymentId).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(a => new CommissionRefundSource(a.RefundId, a.PaymentId, a.Amount, a.Payment.BusinessDate, a.Refund.BusinessDate, a.Refund.CreatedAt)).ToListAsync(ct);
            return new CommissionPage<CommissionRefundSource>(rows, total, page, pageSize);
        }, ct);
    public Task<CommissionPeriodDetails> FinalizeAsync(Guid id, FinalizeCommissionPeriodRequest r, CancellationToken ct = default) => coordination.TransactionAsync(async () =>
    {
        FinancialOperations.Operation(r.OperationId);
        var initial = await db.CommissionPeriods.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, ct) ?? throw new Branches.BranchNotFoundException("Commission period not found.");
        var actor = await AdminAsync(CommissionPermissions.Close, initial.BranchId, ct);
        if (initial.StartBusinessDate == null || initial.EndBusinessDate == null) throw new BusinessRuleException("Legacy commission period needs explicit reconciliation.");
        await LockAsync(initial.BranchId, ct);
        var p = await db.CommissionPeriods.SingleAsync(x => x.Id == id, ct); await db.Entry(p).ReloadAsync(ct);
        var hash = FinancialOperations.Hash(new { Id = id, Request = r, Actor = actor });
        if (p.CloseOperationId == r.OperationId) { if (p.CloseRequestHash != hash) throw new BusinessRuleException("Operation belongs to another finalization request."); return Details(p); }
        if (p.Version != r.ExpectedVersion) throw new DbUpdateConcurrencyException("Period version is stale.");
        if (p.Status != CommissionPeriodStatus.Open) throw new BusinessRuleException("Period is already finalized.");
        var (date, _) = await coordination.BusinessDateAsync(p.BranchId, ct);
        if (date <= p.EndBusinessDate) throw new BusinessRuleException("Finalize only after the period's last effective BusinessDate has ended.");
        var lines = await LinesAsync(p, ct); var statements = await StatementsAsync(p, lines, ct);
        if (statements.Any(s => !s.NewClients.Configured || !s.Renewals.Configured)) throw new BusinessRuleException("Both independent rules must be configured for every attributed employee before finalization.");
        foreach (var line in lines) db.Add(new CommissionStatementLine { CommissionPeriodId = p.Id, PaymentId = line.PaymentId, EmployeeId = line.EmployeeId,
            Category = line.Category == null ? null : Enum.Parse<SaleCategory>(line.Category), EligibilityReason = line.EligibilityReason,

```

## CampionsSystem.BLL/Commissions/CommissionService.cs

Lines 13–19

```csharp

    private readonly FinancialOperations coordination = new(db, current, access);
    private async Task<Guid> AdminAsync(string permission, Guid branch, CancellationToken ct)
    {
        var actor = await access.RequireAsync(permission, branch, ct);
        await access.RequireAsync("Branches.AccessAll", branch, ct); return actor;
    }
    private async Task LockAsync(Guid branch, CancellationToken ct)

```

Lines 22–77

```csharp

        else
        {
            var row = await db.GymBranches.FromSqlInterpolated($"SELECT * FROM \"GymBranches\" WHERE \"Id\"={branch} FOR UPDATE").AsNoTracking().SingleOrDefaultAsync(ct);
            if (row is null || !row.IsActive) throw new BusinessRuleException("Select an active branch.");
        }
    }
    private static void Page(int page, int size)
    { if (page < 1 || page > 1000000 || size is < 1 or > 100) throw new BusinessRuleException("Page must be positive; pageSize must be 1..100."); }
    internal static CommissionPeriodDetails Details(CommissionPeriod p) => new(p.Id, p.BranchId, p.Name, p.StartBusinessDate, p.EndBusinessDate, p.Status.ToString(), p.Version, p.ClosedAt);
    private static CommissionRuleDetails RuleDetails(CommissionRule r) => new(r.Id, r.EmployeeId, r.BranchId, r.CommissionType.ToString(), r.EffectiveFrom, r.TargetAmount, r.BelowTargetRate, r.ReachedTargetRate, r.Revision);
    private async Task<CommissionPeriod> ReadAsync(Guid id, CancellationToken ct)
    {
        var p = await db.CommissionPeriods.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new BranchNotFoundException("Commission period not found.");
        await access.RequireAsync(CommissionPermissions.View, p.BranchId, ct);
        if (p.StartBusinessDate == null || p.EndBusinessDate == null) throw new BusinessRuleException("Legacy commission period needs explicit reconciliation; no BusinessDate bounds were invented.");
        return p;
    }
    public Task<CommissionPeriodDetails> OpenAsync(OpenCommissionPeriodRequest r, CancellationToken ct = default) => coordination.TransactionAsync(async () =>
    {
        FinancialOperations.Operation(r.OperationId);
        var (branch, _) = await coordination.ResolveAsync(CommissionPermissions.Close, r.BranchId, ct);
        var actor = await AdminAsync(CommissionPermissions.Close, branch, ct); await LockAsync(branch, ct);
        var hash = FinancialOperations.Hash(new { Request = r with { BranchId = branch }, Actor = actor });
        var retry = await db.CommissionPeriods.AsNoTracking().SingleOrDefaultAsync(p => p.OperationId == r.OperationId, ct);
        if (retry != null) { if (retry.RequestHash != hash || retry.BranchId != branch) throw new BusinessRuleException("Operation belongs to another period request."); return Details(retry); }
        if (string.IsNullOrWhiteSpace(r.Name) || r.Name.Length > 256 || r.StartDate == default || r.EndDate < r.StartDate || r.EndDate == DateOnly.MaxValue)
            throw new BusinessRuleException("A name and valid inclusive BusinessDate bounds are required.");
        var last = await db.CommissionPeriods.AsNoTracking().Where(p => p.BranchId == branch).OrderByDescending(p => p.StartAt).FirstOrDefaultAsync(ct);
        if (last != null && (last.Status != CommissionPeriodStatus.Closed || last.EndBusinessDate == null || r.StartDate != last.EndBusinessDate.Value.AddDays(1)))
            throw new BusinessRuleException("Close the preceding managed period; the next period must start immediately after its EndDate.");
        var period = new CommissionPeriod { BranchId = branch, Name = r.Name.Trim(), StartBusinessDate = r.StartDate, EndBusinessDate = r.EndDate,
            StartAt = access.BusinessDateBounds(r.StartDate).Start, EndAt = access.BusinessDateBounds(r.EndDate).End,
            OperationId = r.OperationId, RequestHash = hash, CreatedAt = access.Now, Status = CommissionPeriodStatus.Open };
        db.Add(period); access.Audit(actor, branch, "Commission.PeriodOpened", nameof(CommissionPeriod), period.Id, after: r); return Details(period);
    }, ct);
    public Task<IReadOnlyList<CommissionRuleDetails>> ConfigureAsync(ConfigureCommissionRequest r, CancellationToken ct = default) => coordination.TransactionAsync(async () =>
    {
        FinancialOperations.Operation(r.OperationId); var (branch, _) = await coordination.ResolveAsync(CommissionPermissions.Manage, r.BranchId, ct);
        var actor = await AdminAsync(CommissionPermissions.Manage, branch, ct); await LockAsync(branch, ct);
        var hash = FinancialOperations.Hash(r with { BranchId = branch });
        var prior = await db.CommissionRules.AsNoTracking().Where(x => x.OperationId == r.OperationId).ToListAsync(ct);
        if (prior.Count > 0) { if (prior.Count != 2 || prior.Any(x => x.RequestHash != hash || x.CreatedByUserId != actor)) throw new BusinessRuleException("Operation belongs to another configuration request."); return (IReadOnlyList<CommissionRuleDetails>)prior.Select(RuleDetails).ToArray(); }
        if (r.EmployeeId.HasValue && !await db.Employees.AnyAsync(e => e.Id == r.EmployeeId, ct)) throw new BranchNotFoundException("Employee not found.");
        if (r.NewClients == null || r.Renewals == null) throw new BusinessRuleException("Both independent category rules are required.");
        var period = await db.CommissionPeriods.AsNoTracking().Where(p => p.BranchId == branch).OrderByDescending(p => p.StartAt).FirstOrDefaultAsync(ct);
        if (period != null && (period.EndBusinessDate == null || r.EffectiveFrom != period.EndBusinessDate.Value.AddDays(1)))
            throw new BusinessRuleException("Configuration changes must start on the next applicable period's first BusinessDate.");
        var (today, _) = await coordination.BusinessDateAsync(branch, ct);
        if (r.EffectiveFrom == default || period == null && r.EffectiveFrom < today) throw new BusinessRuleException("Initial rule configuration cannot be backdated.");
        var result = new List<CommissionRuleDetails>();
        foreach (var (type, input) in new[] { (CommissionType.SalesNew, r.NewClients), (CommissionType.SalesRenewal, r.Renewals) })
        {
            FinancialOperations.Money(input.Target); OperationAccess.Percentage(input.BelowRate); OperationAccess.Percentage(input.ReachedRate);
            if (input.Target <= 0 || input.ReachedRate < input.BelowRate) throw new BusinessRuleException("Target must be positive and reached rate cannot be below the base rate.");
            var revision = await db.CommissionRules.Where(x => x.BranchId == branch && x.EmployeeId == r.EmployeeId && x.CommissionType == type).MaxAsync(x => (int?)x.Revision, ct) ?? 0;
            var rule = new CommissionRule { BranchId = branch, EmployeeId = r.EmployeeId, CommissionType = type, CalculationMethod = CommissionCalculationMethod.TargetPercentage,

```

Lines 85–90

```csharp

    {
        Page(page, pageSize); var (branch, _) = await coordination.ResolveAsync(CommissionPermissions.View, branchId, ct);
        var q = db.CommissionPeriods.AsNoTracking().Where(x => x.BranchId == branch);
        var count = await q.CountAsync(ct); var rows = await q.OrderByDescending(x => x.StartAt).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new(rows.Select(Details).ToArray(), count, page, pageSize);
    }

```

Lines 92–97

```csharp

    {
        Page(page, pageSize); var (branch, _) = await coordination.ResolveAsync(CommissionPermissions.View, branchId, ct);
        var q = db.CommissionRules.AsNoTracking().Where(x => x.BranchId == branch && x.EmployeeId == employeeId && (x.CommissionType == CommissionType.SalesNew || x.CommissionType == CommissionType.SalesRenewal));
        var count = await q.CountAsync(ct); var rows = await q.OrderByDescending(x => x.EffectiveFrom).ThenByDescending(x => x.Revision).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new(rows.Select(RuleDetails).ToArray(), count, page, pageSize);
    }

```
