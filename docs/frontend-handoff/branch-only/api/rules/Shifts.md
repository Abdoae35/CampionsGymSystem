# Shifts rule/validation evidence

Frozen working-tree source including pending Private drafts. Shared cross-cutting helpers: Authentication, Branches, Operations and Shifts.

## CampionsSystem.BLL/Shifts/ShiftPermissionSetup.cs

Lines 11–16

```csharp

    {
        await using var tx=db.Database.CurrentTransaction==null?await db.Database.BeginTransactionAsync(ct):null;
        if(globalRoleId.HasValue&&!await db.RolePermissions.AnyAsync(x=>x.RoleId==globalRoleId&&x.Permission.Code=="Branches.AccessAll"&&x.Permission.IsActive,ct))
            throw new InvalidOperationException("Select an existing role with active global branch capability.");
        var rows=await db.Permissions.Where(x=>ShiftPermissions.Codes.Contains(x.Code)).ToListAsync(ct);
        foreach(var code in ShiftPermissions.Codes.Where(code=>rows.All(x=>x.Code!=code)))

```

## CampionsSystem.BLL/Shifts/VaultService.cs

Lines 10–41

```csharp

{
    private readonly FinancialOperations money = new(db,current,access);
    private static DrawerPosition SetupSnapshot(CashCount c) => new(c.BranchId,c.FinancialAccountId,c.BookCash,c.CountedCash,c.CountedCash,c.MovementSequence,c.Id,c.Difference);
    private static CashCountDetails Details(CashCount c) => new(c.Id,c.BranchId,c.ShiftId,c.Kind,c.ExpectedCash,c.CountedCash,c.BookCash,c.Difference,c.OccurredAt,c.BusinessDate,c.CreatedByUserId,c.Reason,c.ObservationOrder);
    private static CashMovementDetails Details(CashMovement m) => new(m.Id,m.BranchId,m.ShiftId,m.Kind,m.Amount,m.OccurredAt,m.BusinessDate,m.Reason);
    public Task<DrawerPosition> SetupAsync(SetupDrawerRequest r, CancellationToken ct = default) => money.TransactionAsync(async () =>
    {
        FinancialOperations.Operation(r.OperationId); FinancialOperations.Money(r.CountedCash); FinancialOperations.Reason(r.Reason);
        var (branch,actor) = await money.ResolveAsync(ShiftPermissions.VaultManage,r.BranchId,ct);
        await access.RequireAsync("Branches.AccessAll",branch,ct);
        var branchRow = await db.GymBranches.FromSqlInterpolated($"SELECT * FROM \"GymBranches\" WHERE \"Id\"={branch} FOR UPDATE").AsNoTracking().SingleOrDefaultAsync(ct);
        if(branchRow==null || !branchRow.IsActive)throw new BusinessRuleException("Select an active branch.");
        var hash = FinancialOperations.Hash(r with { BranchId = branch });
        var prior = await db.CashDrawers.SingleOrDefaultAsync(x=>x.OperationId==r.OperationId,ct);
        if (prior != null) { FinancialOperations.Retry(prior.RequestHash,hash,prior.CreatedByUserId,actor); if(prior.BranchId!=branch) throw new UnauthorizedAccessException(); return SetupSnapshot(await db.CashCounts.AsNoTracking().SingleAsync(x=>x.OperationId==r.OperationId,ct)); }
        if (await db.CashDrawers.AnyAsync(x=>x.BranchId==branch,ct)) throw new BusinessRuleException("Branch drawer is already configured; record a new physical observation instead.");
        var account = await db.FinancialAccounts.FromSqlInterpolated($"SELECT * FROM \"FinancialAccounts\" WHERE \"Id\"={r.FinancialAccountId} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
        if(account==null || !account.IsActive || account.BranchId!=branch || account.AccountType!=FinancialAccountType.Cash) throw new BusinessRuleException("Select an active Cash account of this branch.");
        var drawer = new CashDrawer { BranchId=branch,FinancialAccountId=account.Id,OperationId=r.OperationId,RequestHash=hash,CreatedAt=access.Now,CreatedByUserId=actor };
        db.Add(drawer); await db.SaveChangesAsync(ct);
        var(date,day)=await money.BusinessDateAsync(branch,ct);
        // Only a virgin zero-opening ledger receives an explicit initial position.
        // Existing ledger balances remain intact: setup counts discrepancies, not new cash.
        if(r.CountedCash>0 && account.OpeningBalance==0 && !await db.FinancialTransactions.AnyAsync(x=>x.FinancialAccountId==account.Id,ct))
        {
            var initial = new CashMovement { BranchId=branch,FinancialAccountId=account.Id,OperationId=r.OperationId,RequestHash=hash,Kind="InitialPosition",Amount=r.CountedCash,
                BusinessDate=date,BusinessDayId=day,OccurredAt=access.Now,CreatedByUserId=actor,Reason=r.Reason };
            db.Add(initial);
            await money.PostAsync(new FinancialTransaction { BranchId=branch,FinancialAccountId=account.Id,CashMovement=initial,SourceType=FinancialSourceType.CashMovement,
                TransactionType=FinancialTransactionType.Credit,Amount=r.CountedCash,OccurredAt=access.Now,CreatedByUserId=actor,BusinessDate=date,BusinessDayId=day,Notes="Initial physical cash position; not sales revenue." },ct);
            access.Audit(actor,branch,"Vault.InitialPositionRecorded",nameof(CashMovement),initial.Id,after:new { initial.Amount,account.Id,date });
        }

```

Lines 45–54

```csharp

    },ct);
    public Task<CashCountDetails> CountAsync(CountCashRequest r,CancellationToken ct=default)=>money.TransactionAsync(async()=>
    {
        FinancialOperations.Operation(r.OperationId); FinancialOperations.Money(r.CountedCash); FinancialOperations.Reason(r.Reason);
        var(branch,actor)=await money.ResolveAsync(ShiftPermissions.VaultManage,r.BranchId,ct); await money.LockBranchAsync(branch,ct);
        var hash=FinancialOperations.Hash(r with { BranchId=branch });
        var prior=await db.CashCounts.AsNoTracking().SingleOrDefaultAsync(x=>x.OperationId==r.OperationId,ct);
        if(prior!=null) { FinancialOperations.Retry(prior.RequestHash,hash,prior.CreatedByUserId,actor); if(prior.BranchId!=branch)throw new UnauthorizedAccessException(); return Details(prior); }
        var shift=await money.RequireShiftAsync(branch,actor,ct);
        var count=await money.CountAsync(branch,actor,r.OperationId,hash,r.CountedCash,"Recount",shift.Id,r.Reason,ct);

```

Lines 58–82

```csharp

    {var(branch,_)=await money.ResolveAsync(ShiftPermissions.VaultView,branchId,ct);await money.LockBranchAsync(branch,ct);return await money.PositionAsync(branch,ct);},ct);
    public Task<CashMovementDetails> WithdrawAsync(WithdrawalRequest r,CancellationToken ct=default)=>money.TransactionAsync(async()=>
    {
        FinancialOperations.Operation(r.OperationId);FinancialOperations.Money(r.Amount,true);FinancialOperations.Reason(r.Reason);
        var(branch,actor)=await money.ResolveAsync(ShiftPermissions.Withdraw,r.BranchId,ct);await access.RequireAsync("Branches.AccessAll",branch,ct);await money.LockBranchAsync(branch,ct);
        var hash=FinancialOperations.Hash(r with { BranchId=branch });var prior=await db.CashMovements.AsNoTracking().SingleOrDefaultAsync(x=>x.OperationId==r.OperationId,ct);
        if(prior!=null){FinancialOperations.Retry(prior.RequestHash,hash,prior.CreatedByUserId,actor);if(prior.BranchId!=branch)throw new UnauthorizedAccessException();return Details(prior);}
        var shift=await money.RequireShiftAsync(branch,actor,ct);var drawer=await money.DrawerAsync(branch,ct);var(date,day)=await money.BusinessDateAsync(branch,ct);
        var movement=new CashMovement{BranchId=branch,FinancialAccountId=drawer.FinancialAccountId,ShiftId=shift.Id,OperationId=r.OperationId,RequestHash=hash,
            Kind="Withdrawal",Amount=r.Amount,Reason=r.Reason,OccurredAt=access.Now,CreatedByUserId=actor,BusinessDate=date,BusinessDayId=day};db.Add(movement);
        await money.PostAsync(new FinancialTransaction{BranchId=branch,FinancialAccountId=drawer.FinancialAccountId,ShiftId=shift.Id,CashMovement=movement,SourceType=FinancialSourceType.CashMovement,
            TransactionType=FinancialTransactionType.Debit,Amount=r.Amount,OccurredAt=access.Now,CreatedByUserId=actor,BusinessDate=date,BusinessDayId=day,Notes=r.Reason},ct);
        access.Audit(actor,branch,"Vault.Withdrawn",nameof(CashMovement),movement.Id,after:new{r.Amount,shift.Id,date});return Details(movement);
    },ct);
    private static void Validate(VaultHistoryRequest r)
    {if(r.Page<1||r.PageSize is <1 or >100||r.From>r.To)throw new BusinessRuleException("Invalid history range or pagination.");}
    public async Task<ShiftPage<CashCountDetails>> CountsAsync(VaultHistoryRequest r,CancellationToken ct=default)
    {
        Validate(r);var(branch,_)=await money.ResolveAsync(ShiftPermissions.VaultView,r.BranchId,ct);
        var q=db.CashCounts.AsNoTracking().Where(x=>x.BranchId==branch);
        if(r.From!=null)q=q.Where(x=>x.BusinessDate>=r.From);if(r.To!=null)q=q.Where(x=>x.BusinessDate<=r.To);
        if(r.AccountId!=null)q=q.Where(x=>x.FinancialAccountId==r.AccountId);
        var total=await q.CountAsync(ct);var rows=await q.OrderByDescending(x=>x.ObservationOrder).Skip((r.Page-1)*r.PageSize).Take(r.PageSize).ToListAsync(ct);
        return new(rows.Select(Details).ToList(),total,r.Page,r.PageSize);
    }

```

Lines 84–91

```csharp

    {
        Validate(r);var(branch,_)=await money.ResolveAsync(ShiftPermissions.VaultView,r.BranchId,ct);
        var q=db.FinancialTransactions.AsNoTracking().Where(x=>x.BranchId==branch);
        if(r.From!=null)q=q.Where(x=>x.BusinessDate>=r.From);if(r.To!=null)q=q.Where(x=>x.BusinessDate<=r.To);if(r.AccountId!=null)q=q.Where(x=>x.FinancialAccountId==r.AccountId);
        var total=await q.CountAsync(ct);var items=await q.OrderByDescending(x=>x.OccurredAt).ThenBy(x=>x.Id).Skip((r.Page-1)*r.PageSize).Take(r.PageSize)
            .Select(x=>new FinancialMovementDetails(x.Id,x.FinancialAccountId,x.BranchId,x.ShiftId,x.SourceType,x.TransactionType,x.Amount,x.OccurredAt,x.BusinessDate,x.CreatedByUserId)).ToListAsync(ct);
        return new(items,total,r.Page,r.PageSize);
    }

```

## CampionsSystem.BLL/Shifts/ExpenseService.Advances.cs

Lines 8–45

```csharp

{
    private async Task<AdvanceDetails> Details(EmployeeAdvance a,CancellationToken ct)=>new(a.Id,a.BranchId,a.ShiftId,a.ReceivingEmployeeId,
        await db.Employees.Where(x=>x.Id==a.ReceivingEmployeeId).Select(x=>x.FullName).SingleAsync(ct),a.RecordingEmployeeId,a.FinancialAccountId,
        await db.FinancialAccounts.Where(x=>x.Id==a.FinancialAccountId).Select(x=>x.AccountType).SingleAsync(ct),a.Amount,a.Reason,a.Reference,a.OccurredAt,a.BusinessDate);
    public Task<AdvanceDetails> RecordAdvanceAsync(AdvanceRequest r,CancellationToken ct=default)=>money.TransactionAsync(async()=>
    {
        FinancialOperations.Operation(r.OperationId);FinancialOperations.Money(r.Amount,true);FinancialOperations.Reason(r.Reason);
        if(r.Reference?.Length>512)throw new BusinessRuleException("Reference must be at most 512 characters.");
        var(branch,actor)=await money.ResolveAsync(ShiftPermissions.AdvanceCreate,r.BranchId,ct);await money.LockBranchAsync(branch,ct);
        var hash=FinancialOperations.Hash(r with{BranchId=branch});var prior=await db.EmployeeAdvances.AsNoTracking().SingleOrDefaultAsync(x=>x.OperationId==r.OperationId,ct);
        if(prior!=null){FinancialOperations.Retry(prior.RequestHash,hash,prior.CreatedByUserId,actor);if(prior.BranchId!=branch)throw new UnauthorizedAccessException();return await Details(prior,ct);}
        var shift=await money.RequireShiftAsync(branch,actor,ct);var account=await money.AccountAsync(branch,r.FinancialAccountId,ct);
        var(date,day)=await money.BusinessDateAsync(branch,ct);
        var recipient=await db.Employees.FromSqlInterpolated($"SELECT * FROM \"Employees\" WHERE \"Id\"={r.ReceivingEmployeeId} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
        if(recipient==null||recipient.EmploymentStatus!=EmploymentStatus.Active||recipient.HireDate>date||recipient.TerminationDate<=date)
            throw new BusinessRuleException("Select an active receiving employee.");
        var advance=new EmployeeAdvance{BranchId=branch,ShiftId=shift.Id,ReceivingEmployeeId=recipient.Id,RecordingEmployeeId=shift.EmployeeId!.Value,
            FinancialAccountId=account.Id,OperationId=r.OperationId,RequestHash=hash,Amount=r.Amount,Reason=r.Reason,Reference=r.Reference,
            OccurredAt=access.Now,CreatedByUserId=actor,BusinessDate=date,BusinessDayId=day};db.Add(advance);
        await money.PostAsync(new FinancialTransaction{BranchId=branch,FinancialAccountId=account.Id,ShiftId=shift.Id,EmployeeAdvance=advance,
            SourceType=FinancialSourceType.EmployeeAdvance,TransactionType=FinancialTransactionType.Debit,Amount=r.Amount,OccurredAt=access.Now,CreatedByUserId=actor,
            BusinessDate=date,BusinessDayId=day,Notes=r.Reason},ct);
        access.Audit(actor,branch,"EmployeeAdvance.Paid",nameof(EmployeeAdvance),advance.Id,after:new{r.ReceivingEmployeeId,advance.RecordingEmployeeId,r.Amount,account.AccountType,ShiftId=shift.Id,date});
        return await Details(advance,ct);
    },ct);
    public async Task<AdvanceDetails> GetAdvanceAsync(Guid id,CancellationToken ct=default)
    {
        var row=await db.EmployeeAdvances.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new BranchNotFoundException("Advance not found.");
        await access.RequireAsync(ShiftPermissions.AdvanceView,row.BranchId,ct);return await Details(row,ct);
    }
    public async Task<ShiftPage<AdvanceDetails>> AdvancesAsync(VaultHistoryRequest r,CancellationToken ct=default)
    {
        if(r.Page<1||r.PageSize is <1 or >100||r.From>r.To)throw new BusinessRuleException("Invalid history range or pagination.");
        var(branch,_)=await money.ResolveAsync(ShiftPermissions.AdvanceView,r.BranchId,ct);var q=db.EmployeeAdvances.AsNoTracking().Where(x=>x.BranchId==branch);
        if(r.From!=null)q=q.Where(x=>x.BusinessDate>=r.From);if(r.To!=null)q=q.Where(x=>x.BusinessDate<=r.To);if(r.AccountId!=null)q=q.Where(x=>x.FinancialAccountId==r.AccountId);
        var total=await q.CountAsync(ct);var rows=await q.OrderByDescending(x=>x.OccurredAt).ThenBy(x=>x.Id).Skip((r.Page-1)*r.PageSize).Take(r.PageSize).ToListAsync(ct);
        var items=new List<AdvanceDetails>();foreach(var row in rows)items.Add(await Details(row,ct));return new(items,total,r.Page,r.PageSize);
    }

```

## CampionsSystem.BLL/Shifts/ShiftService.cs

Lines 17–43

```csharp

        var row=await db.BusinessDays.AsNoTracking().SingleOrDefaultAsync(x=>x.BranchId==branch&&x.Status==BusinessDayStatus.Open,ct);
        return row==null?null:DayDetails(row);
    }
    public static BusinessDayDetails DayDetails(BusinessDay day)=>new(day.Id,day.BranchId,day.BusinessDate,day.Status,day.OpenedAt,day.ClosedAt,day.OpenedByUserId,day.ClosedByUserId);
    private async Task<ShiftDetails> Details(Shift row, CancellationToken ct) => new(row.Id, row.BranchId, row.EmployeeId,
        row.EmployeeId == null ? null : await db.Employees.Where(x => x.Id == row.EmployeeId).Select(x => x.FullName).SingleAsync(ct),
        row.Status, row.Version, row.OpenedAt, row.ClosedAt, row.OpeningBusinessDate, row.ClosingBusinessDate, row.OpeningCash,
        row.ExpectedClosingCash, row.ActualClosingCash, row.Difference);

    public Task<ShiftDetails> OpenAsync(OpenShiftRequest r, CancellationToken ct = default) => money.TransactionAsync(async () =>
    {
        FinancialOperations.Operation(r.OperationId); FinancialOperations.Money(r.OpeningCash);
        var (branch, actor) = await money.ResolveAsync(ShiftPermissions.Manage, r.BranchId, ct); var hash = FinancialOperations.Hash(r with { BranchId = branch });
        await money.LockBranchAsync(branch, ct);
        var prior = await db.Shifts.AsNoTracking().SingleOrDefaultAsync(x => x.OperationId == r.OperationId, ct);
        if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.OpenedByUserId, actor); if (prior.BranchId != branch) throw new UnauthorizedAccessException(); return await Details(prior, ct); }
        var (date, day) = await money.BusinessDateAsync(branch, ct); var employee = await money.EmployeeAsync(actor, branch, date, ct, true);
        if (await db.EmployeeLeaveRequests.AnyAsync(x => x.EmployeeId == employee.Id && x.Status == EmployeeLeaveRequestStatus.Approved && x.FromDate <= date && x.ToDate >= date, ct))
            throw new BusinessRuleException("Employee has approved leave for this business date.");
        if (await db.Shifts.AnyAsync(x => x.EmployeeId == employee.Id && x.Status == ShiftStatus.Open, ct))
            throw new BusinessRuleException("You already have an open shift; retrieve and close it instead of creating another.");
        var drawer = await money.DrawerAsync(branch, ct);
        var shift = new Shift { BranchId = branch, EmployeeId = employee.Id, FinancialAccountId = drawer.FinancialAccountId,
            OperationId = r.OperationId, RequestHash = hash, OpeningCash = r.OpeningCash, OpenedByUserId = actor, OpenedAt = access.Now,
            OpeningBusinessDate = date, BusinessDayId = day, Status = ShiftStatus.Open, Version = 1 };
        db.Add(shift); db.Add(new ShiftEmployee { Shift = shift, EmployeeId = employee.Id, JoinedAt = access.Now });
        await money.CountAsync(branch, actor, r.OperationId, hash, r.OpeningCash, "Opening", shift.Id, null, ct);

```

Lines 47–93

```csharp


    public Task<ShiftDetails> CloseAsync(Guid id, CloseShiftRequest r, CancellationToken ct = default) => money.TransactionAsync(async () =>
    {
        FinancialOperations.Operation(r.OperationId); FinancialOperations.Money(r.CountedCash);
        if(r.Reason?.Length>2000)throw new BusinessRuleException("Closing reason must be at most 2000 characters.");
        var info = await db.Shifts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new BranchNotFoundException("Shift not found.");
        var actor = await access.RequireAsync(ShiftPermissions.Manage, info.BranchId, ct);
        if (info.OpenedByUserId != actor || info.EmployeeId == null || info.OperationId == null) throw new UnauthorizedAccessException();
        await money.LockBranchAsync(info.BranchId, ct);
        var shift = await db.Shifts.FromSqlInterpolated($"SELECT * FROM \"Shifts\" WHERE \"Id\"={id} FOR UPDATE").SingleAsync(ct); await db.Entry(shift).ReloadAsync(ct);
        var hash = FinancialOperations.Hash(new { id, r });
        var prior = await db.Shifts.AsNoTracking().SingleOrDefaultAsync(x => x.CloseOperationId == r.OperationId, ct);
        if (prior != null) { FinancialOperations.Retry(prior.CloseRequestHash, hash, prior.ClosedByUserId!.Value, actor); return await Details(prior, ct); }
        if (shift.Status != ShiftStatus.Open) throw new BusinessRuleException("Shift already closed.");
        if (shift.Version != r.ExpectedVersion) throw new DbUpdateConcurrencyException();
        if (access.Now < shift.OpenedAt) throw new BusinessRuleException("Cannot backdate shift closure.");
        var count = await money.CountAsync(shift.BranchId, actor, r.OperationId, hash, r.CountedCash, "Closing", id, r.Reason, ct);
        shift.CloseOperationId = r.OperationId; shift.CloseRequestHash = hash; shift.ClosedAt = access.Now; shift.ClosedByUserId = actor;
        shift.ExpectedClosingCash = count.ExpectedCash; shift.ActualClosingCash = count.CountedCash; shift.Difference = count.Difference;
        shift.ClosingBusinessDate = count.BusinessDate; shift.ClosingReason = r.Reason; shift.Status = ShiftStatus.Closed; shift.Version++;
        var participant = await db.ShiftEmployees.SingleAsync(x => x.ShiftId == id && x.EmployeeId == shift.EmployeeId, ct); participant.LeftAt = access.Now;
        access.Audit(actor, shift.BranchId, "Shift.Closed", nameof(Shift), id, after: new { count.ExpectedCash, count.CountedCash, count.Difference, count.BusinessDate });
        await db.SaveChangesAsync(ct); return await Details(shift, ct);
    }, ct);

    public async Task<ShiftDetails?> CurrentAsync(CancellationToken ct = default)
    {
        var actor = await access.RequireAsync(ShiftPermissions.View, null, ct);
        var row = await db.Shifts.AsNoTracking().SingleOrDefaultAsync(x => x.OpenedByUserId == actor && x.EmployeeId != null && x.Status == ShiftStatus.Open, ct);
        if (row == null) return null; await access.RequireAsync(ShiftPermissions.View, row.BranchId, ct); return await Details(row, ct);
    }
    public async Task<ShiftDetails> GetAsync(Guid id, CancellationToken ct = default)
    {
        var row = await db.Shifts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new BranchNotFoundException("Shift not found.");
        await access.RequireAsync(ShiftPermissions.View, row.BranchId, ct); return await Details(row, ct);
    }
    public async Task<ShiftPage<ShiftDetails>> HistoryAsync(ShiftHistoryRequest r, CancellationToken ct = default)
    {
        var (branch, _) = await money.ResolveAsync(ShiftPermissions.View, r.BranchId, ct);
        if (r.Page < 1 || r.PageSize is < 1 or > 100 || r.From > r.To) throw new BusinessRuleException("Invalid history range or pagination.");
        var query = db.Shifts.AsNoTracking().Where(x => x.BranchId == branch);
        if (r.EmployeeId != null) query = query.Where(x => x.EmployeeId == r.EmployeeId);
        if (r.From != null) query = query.Where(x => x.OpeningBusinessDate >= r.From);
        if (r.To != null) query = query.Where(x => x.OpeningBusinessDate <= r.To);
        var total = await query.CountAsync(ct); var rows = await query.OrderByDescending(x => x.OpenedAt).ThenBy(x => x.Id).Skip((r.Page - 1) * r.PageSize).Take(r.PageSize).ToListAsync(ct);
        var items = new List<ShiftDetails>(); foreach (var row in rows) items.Add(await Details(row, ct));
        return new(items, total, r.Page, r.PageSize);

```

Lines 95–115

```csharp


    public Task<BusinessDay> OpenBusinessDayAsync(OpenBusinessDayRequest r, CancellationToken ct = default) => money.TransactionAsync(async () =>
    {
        FinancialOperations.Operation(r.OperationId);
        var (branch, actor) = await money.ResolveAsync(ShiftPermissions.Manage, r.BranchId, ct); await money.LockBranchAsync(branch, ct);
        var prior = await db.BusinessDays.AsNoTracking().SingleOrDefaultAsync(x => x.Id == r.OperationId, ct);
        if (prior != null) { if (prior.BranchId != branch || prior.BusinessDate != r.BusinessDate || prior.OpenedByUserId != actor) throw new BusinessRuleException("Operation identity belongs to another BusinessDay."); return prior; }
        if (r.BusinessDate > access.Today || await db.BusinessDays.AnyAsync(x => x.BranchId == branch && (x.Status == BusinessDayStatus.Open || x.BusinessDate >= r.BusinessDate), ct))
            throw new BusinessRuleException("Close the prior BusinessDay first; business dates must advance and cannot be future dates.");
        var day = new BusinessDay { Id = r.OperationId, BranchId = branch, BusinessDate = r.BusinessDate, OpenedByUserId = actor, OpenedAt = access.Now, CreatedAt = access.Now, Status = BusinessDayStatus.Open };
        db.Add(day); access.Audit(actor, branch, "BusinessDay.Opened", nameof(BusinessDay), day.Id, after: new { day.BusinessDate }); return day;
    }, ct);
    public Task<BusinessDay> CloseBusinessDayAsync(Guid id, CancellationToken ct = default) => money.TransactionAsync(async () =>
    {
        var info = await db.BusinessDays.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new BranchNotFoundException("BusinessDay not found.");
        var actor = await access.RequireAsync(ShiftPermissions.Manage, info.BranchId, ct); await money.LockBranchAsync(info.BranchId, ct);
        var day = await db.BusinessDays.SingleAsync(x => x.Id == id, ct); await db.Entry(day).ReloadAsync(ct);
        if (day.Status == BusinessDayStatus.Closed) return day;
        if(access.Now<day.OpenedAt)throw new BusinessRuleException("Cannot backdate BusinessDay closure.");
        day.ClosedAt = access.Now; day.ClosedByUserId = actor; day.Status = BusinessDayStatus.Closed;
        access.Audit(actor, day.BranchId, "BusinessDay.Closed", nameof(BusinessDay), id); return day;

```

## CampionsSystem.BLL/Shifts/FinancialOperations.cs

Lines 16–26

```csharp

public sealed class FinancialOperations(ApplicationDbContext db, ICurrentUserService current, OperationAccess access)
{
    public static string Hash<T>(T value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value))));
    public static void Operation(Guid id) { if (id == Guid.Empty) throw new BusinessRuleException("A nonempty operation identity is required."); }
    public static void Money(decimal value, bool positive = false)
    { OperationAccess.Money(value); if (value > 9999999999999999.99m || positive && value <= 0) throw new BusinessRuleException("Invalid money amount."); }
    public static void Reason(string? reason) { if (string.IsNullOrWhiteSpace(reason) || reason.Length > 2000) throw new BusinessRuleException("A reason of up to 2000 characters is required."); }
    public static void Retry(string? storedHash, string hash, Guid originalActor, Guid actor)
    { if (storedHash != hash || originalActor != actor) throw new BusinessRuleException("Operation identity belongs to a different request or actor."); }
    public async Task<T> TransactionAsync<T>(Func<Task<T>> action, CancellationToken ct)
    {

```

Lines 32–80

```csharp

    }
    public async Task<(Guid Branch, Guid Actor)> ResolveAsync(string permission, Guid? selected, CancellationToken ct)
    {
        var user = await current.GetAsync(ct) ?? throw new UnauthorizedAccessException();
        var branch = selected ?? user.CurrentBranchId ?? throw new BusinessRuleException("Select an operational branch.");
        var actor = await access.RequireAsync(permission, branch, ct); return (branch, actor);
    }
    public async Task LockBranchAsync(Guid branch, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction == null) throw new InvalidOperationException("Drawer coordination requires a transaction.");
        // Lock the dedicated shared drawer, not the branch exclusively: existing Plan
        // administration takes branch SHARE after Plan locks and must not deadlock here.
        var drawer = await db.CashDrawers.FromSqlInterpolated($"SELECT * FROM \"CashDrawers\" WHERE \"BranchId\"={branch} FOR UPDATE").AsNoTracking().SingleOrDefaultAsync(ct);
        if (drawer == null) throw new BusinessRuleException("Configure the branch shared drawer first.");
        var row = await db.GymBranches.FromSqlInterpolated($"SELECT * FROM \"GymBranches\" WHERE \"Id\"={branch} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
        if (row is null) throw new BranchNotFoundException("Branch not found.");
        if (!row.IsActive) throw new BusinessRuleException("Branch is inactive.");
    }
    public async Task<(DateOnly Date, Guid? Day)> BusinessDateAsync(Guid branch, CancellationToken ct)
    {
        var day = await db.BusinessDays.FromSqlInterpolated($"SELECT * FROM \"BusinessDays\" WHERE \"BranchId\"={branch} AND \"Status\"='Open' FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
        if (day != null && (day.OpenedAt > access.Now || day.BusinessDate > access.Today)) throw new BusinessRuleException("Invalid open BusinessDay.");
        return (day?.BusinessDate ?? access.Today, day?.Id);
    }
    public async Task<Employee> EmployeeAsync(Guid actor, Guid branch, DateOnly date, CancellationToken ct, bool lockEmployee = false)
    {
        var employee = lockEmployee
            ? await db.Employees.FromSqlInterpolated($"SELECT * FROM \"Employees\" WHERE \"UserId\"={actor} FOR UPDATE").AsNoTracking().SingleOrDefaultAsync(ct)
            : await db.Employees.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == actor, ct);
        if (employee == null || employee.EmploymentStatus != EmploymentStatus.Active || employee.HireDate > date || employee.TerminationDate <= date)
            throw new BusinessRuleException("An active linked employee is required.");
        if (!await db.EmployeeBranchAssignments.AnyAsync(x => x.EmployeeId == employee.Id && x.BranchId == branch && x.StartDate <= date && (x.EndDate == null || x.EndDate >= date), ct))
            throw new BusinessRuleException("Employee is not assigned to the operational branch.");
        return employee;
    }
    public async Task<Shift> RequireShiftAsync(Guid branch, Guid actor, CancellationToken ct)
    {
        var (date, _) = await BusinessDateAsync(branch, ct); var employee = await EmployeeAsync(actor, branch, date, ct);
        var shift = await db.Shifts.AsNoTracking().SingleOrDefaultAsync(x => x.EmployeeId == employee.Id && x.Status == ShiftStatus.Open, ct);
        if (shift == null || shift.BranchId != branch || shift.OpenedByUserId != actor || shift.OperationId == null || shift.OpenedAt > access.Now)
            throw new BusinessRuleException("Open your own employee shift at this branch before performing this operation.");
        return shift;
    }
    public async Task<CashDrawer> DrawerAsync(Guid branch, CancellationToken ct)
    {
        var drawer = await db.CashDrawers.SingleOrDefaultAsync(x => x.BranchId == branch, ct)
            ?? throw new BusinessRuleException("Configure the branch shared drawer first.");
        await db.Entry(drawer).ReloadAsync(ct); return drawer;
    }

```

Lines 87–92

```csharp

    public async Task<DrawerPosition> PositionAsync(Guid branch, CancellationToken ct)
    {
        var drawer = await DrawerAsync(branch, ct);
        var last = await db.CashCounts.AsNoTracking().Where(x => x.BranchId == branch).OrderByDescending(x => x.ObservationOrder).FirstOrDefaultAsync(ct);
        var book = await BookAsync(drawer.FinancialAccountId, ct);
        var physical = last == null ? book : last.CountedCash + await db.FinancialTransactions.Where(x => x.BranchId == branch && x.FinancialAccountId == drawer.FinancialAccountId && x.DrawerSequence > last.MovementSequence)

```

Lines 96–103

```csharp

    }
    public async Task<CashCount> CountAsync(Guid branch, Guid actor, Guid operation, string hash, decimal counted, string kind, Guid? shift, string? reason, CancellationToken ct)
    {
        Money(counted); var pos = await PositionAsync(branch, ct); var (date, day) = await BusinessDateAsync(branch, ct);
        var count = new CashCount { BranchId = branch, FinancialAccountId = pos.AccountId, ShiftId = shift, OperationId = operation, RequestHash = hash,
            BusinessDate = date, BusinessDayId = day, Kind = kind, MovementSequence = pos.MovementSequence, ExpectedCash = pos.ExpectedPhysicalCash,
            CountedCash = counted, BookCash = pos.BookCash, Difference = counted - pos.ExpectedPhysicalCash, OccurredAt = access.Now, CreatedByUserId = actor, Reason = reason };
        db.Add(count); access.Audit(actor, branch, "Vault.CashCounted", nameof(CashCount), count.Id, after: new { kind, shift, count.ExpectedCash, counted, count.Difference, count.BookCash, date });

```

Lines 107–121

```csharp

    {
        var account = await db.FinancialAccounts.FromSqlInterpolated($"SELECT * FROM \"FinancialAccounts\" WHERE \"Id\"={id} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
        if (account == null || account.BranchId != branch || !historicalReversal && !account.IsActive || account.AccountType is not (FinancialAccountType.Cash or FinancialAccountType.InstaPay))
            throw new BusinessRuleException("Select an active Cash or InstaPay account of this branch.");
        if (account.AccountType == FinancialAccountType.Cash && (await DrawerAsync(branch, ct)).FinancialAccountId != id)
            throw new BusinessRuleException("Cash operations must use the configured shared drawer account.");
        return account;
    }
    public async Task CheckCashPayoutAsync(Guid branch, decimal amount, CancellationToken ct)
    { Money(amount); if ((await PositionAsync(branch, ct)).ExpectedPhysicalCash < amount) throw new BusinessRuleException("Insufficient tracked physical Cash."); }
    public async Task PostAsync(FinancialTransaction row, CancellationToken ct, bool historicalReversal = false)
    {
        if (db.Database.CurrentTransaction == null) throw new InvalidOperationException("Financial posting requires a transaction.");
        var account = await AccountAsync(row.BranchId, row.FinancialAccountId, ct, historicalReversal);
        if (account.AccountType == FinancialAccountType.Cash)

```

## CampionsSystem.BLL/Shifts/ShiftContracts.cs

Lines 4–9

```csharp


public static class ShiftPermissions
{
    public const string View = "Shifts.View", Manage = "Shifts.Manage", VaultView = "Vault.View", VaultManage = "Vault.Manage", Withdraw = "Vault.Withdraw",
        ExpenseView = "Expenses.View", ExpenseCreate = "Expenses.Create", ExpenseManage = "Expenses.Manage", ExpenseReverse = "Expenses.Reverse",
        AdvanceView = "Advances.View", AdvanceCreate = "Advances.Create";

```

Lines 12–19

```csharp

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record OpenShiftRequest(Guid OperationId, decimal OpeningCash, Guid? BranchId = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CloseShiftRequest(Guid OperationId, long ExpectedVersion, decimal CountedCash, string? Reason = null);
public sealed record ShiftDetails(Guid Id, Guid BranchId, Guid? EmployeeId, string? EmployeeName, ShiftStatus Status, long Version,
    DateTimeOffset OpenedAt, DateTimeOffset? ClosedAt, DateOnly? OpeningBusinessDate, DateOnly? ClosingBusinessDate,
    decimal OpeningCash, decimal? ExpectedClosingCash, decimal? ActualClosingCash, decimal? Difference);
public sealed record DrawerPosition(Guid BranchId, Guid AccountId, decimal BookCash, decimal ExpectedPhysicalCash, decimal LastCountedCash,

```

Lines 25–30

```csharp

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record WithdrawalRequest(Guid OperationId, decimal Amount, string Reason, Guid? BranchId = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record OpenBusinessDayRequest(Guid OperationId, DateOnly BusinessDate, Guid? BranchId = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record PaidExpenseRequest(Guid OperationId, Guid ExpenseCategoryId, Guid FinancialAccountId, decimal Amount, string Description, string? ReceiptReference = null, Guid? BranchId = null);

```

Lines 32–56

```csharp

public sealed record ReverseExpenseRequest(Guid OperationId, decimal ReturnedAmount, bool FundsActuallyReturned, string Reason, string? ReturnReference = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record AdvanceRequest(Guid OperationId, Guid ReceivingEmployeeId, Guid FinancialAccountId, decimal Amount, string Reason, string? Reference = null, Guid? BranchId = null);
public sealed record ShiftHistoryRequest(Guid? BranchId = null, Guid? EmployeeId = null, DateOnly? From = null, DateOnly? To = null, int Page = 1, int PageSize = 25);
public sealed record ShiftPage<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
public sealed record CashCountDetails(Guid Id, Guid BranchId, Guid? ShiftId, string Kind, decimal ExpectedCash, decimal CountedCash, decimal BookCash,
    decimal Difference, DateTimeOffset OccurredAt, DateOnly BusinessDate, Guid ActorId, string? Reason, long ObservationOrder);
public sealed record CashMovementDetails(Guid Id, Guid BranchId, Guid? ShiftId, string Kind, decimal Amount, DateTimeOffset OccurredAt, DateOnly BusinessDate, string Reason);
public sealed record VaultHistoryRequest(Guid? BranchId = null, Guid? AccountId = null, DateOnly? From = null, DateOnly? To = null, int Page = 1, int PageSize = 25);
public sealed record FinancialMovementDetails(Guid Id, Guid AccountId, Guid BranchId, Guid? ShiftId, FinancialSourceType SourceType,
    FinancialTransactionType Direction, decimal Amount, DateTimeOffset OccurredAt, DateOnly? BusinessDate, Guid ActorId);
public sealed record ExpenseDetails(Guid Id, Guid BranchId, Guid? ShiftId, Guid? RecordingEmployeeId, Guid CategoryId, string CategoryName,
    Guid AccountId, FinancialAccountType Method, decimal Amount, decimal ReturnedAmount, string Description, string? ReceiptReference,
    DateTimeOffset OccurredAt, DateOnly? BusinessDate, DateTimeOffset? PaidAt, ExpenseStatus OriginalStatus);
public sealed record ExpenseReversalDetails(Guid Id, Guid ExpenseId, Guid BranchId, Guid ShiftId, decimal Amount, string Reason,
    string? ReturnReference, DateTimeOffset OccurredAt, DateOnly BusinessDate);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ExpenseCategoryRequest(string Name, bool IsActive = true);
public sealed record ExpenseCategoryDetails(Guid Id, string Name, bool IsActive);
public sealed record AdvanceDetails(Guid Id, Guid BranchId, Guid ShiftId, Guid ReceivingEmployeeId, string ReceivingEmployeeName,
    Guid RecordingEmployeeId, Guid AccountId, FinancialAccountType Method, decimal Amount, string Reason, string? Reference,
    DateTimeOffset OccurredAt, DateOnly BusinessDate);
public sealed record BusinessDayDetails(Guid Id, Guid BranchId, DateOnly BusinessDate, BusinessDayStatus Status,
    DateTimeOffset OpenedAt, DateTimeOffset? ClosedAt, Guid? OpenedByUserId, Guid? ClosedByUserId);
public sealed record MethodFinancialTotals(FinancialAccountType Method, decimal Collected, decimal Refunded, decimal ExpensesPaid,

```

Lines 58–63

```csharp

    decimal OtherDebits, decimal NetMovement, decimal LegacyUnshiftedNetMovement);
public sealed record ShiftFinancialSummary(ShiftDetails Shift, IReadOnlyList<MethodFinancialTotals> Activity,
    DrawerPosition CurrentSharedDrawer);
public sealed record BranchFinancialSummary(Guid BranchId, DateOnly? From, DateOnly? To,
    IReadOnlyList<MethodFinancialTotals> Activity, DrawerPosition CurrentSharedDrawer);
public sealed record FinancialAuditDetails(Guid Id, string Event, string? EntityType, Guid? EntityId,

```

## CampionsSystem.BLL/Shifts/ShiftReportService.cs

Lines 31–58

```csharp

    }
    public Task<ShiftFinancialSummary> ShiftAsync(Guid id,CancellationToken ct=default)=>money.TransactionAsync(async()=>
    {
        var branch=await db.Shifts.Where(x=>x.Id==id).Select(x=>(Guid?)x.BranchId).SingleOrDefaultAsync(ct)??throw new BranchNotFoundException("Shift not found.");
        await access.RequireAsync(ShiftPermissions.View,branch,ct); await access.RequireAsync(ShiftPermissions.VaultView,branch,ct);
        await money.LockBranchAsync(branch,ct);var shift=await new ShiftService(db,current,access).GetAsync(id,ct);
        // Only explicit associations count as employee activity; overlapping time windows
        // and legacy null links never assign another employee's money to this shift.
        return new ShiftFinancialSummary(shift,await Totals(db.FinancialTransactions.AsNoTracking().Where(x=>x.BranchId==shift.BranchId&&x.ShiftId==id),ct),await money.PositionAsync(shift.BranchId,ct));
    },ct);
    public Task<BranchFinancialSummary> BranchAsync(Guid? branchId=null,DateOnly? from=null,DateOnly? to=null,CancellationToken ct=default)=>money.TransactionAsync(async()=>
    {
        if(from>to)throw new BusinessRuleException("Invalid business date range.");
        var(branch,_)=await money.ResolveAsync(ShiftPermissions.VaultView,branchId,ct); await money.LockBranchAsync(branch,ct);
        var q=db.FinancialTransactions.AsNoTracking().Where(x=>x.BranchId==branch);
        if(from!=null)q=q.Where(x=>x.BusinessDate>=from); if(to!=null)q=q.Where(x=>x.BusinessDate<=to);
        return new BranchFinancialSummary(branch,from,to,await Totals(q,ct),await money.PositionAsync(branch,ct));
    },ct);
    public async Task<ShiftPage<FinancialAuditDetails>> AuditAsync(Guid? branchId=null,Guid? entityId=null,int page=1,int pageSize=25,CancellationToken ct=default)
    {
        if(page<1||pageSize is <1 or >100)throw new BusinessRuleException("Invalid audit query.");
        var(branch,_)=await money.ResolveAsync(ShiftPermissions.VaultView,branchId,ct);
        var q=db.AuditLogs.AsNoTracking().Where(x=>x.BranchId==branch&&(x.Action.StartsWith("Shift.")||x.Action.StartsWith("Vault.")||x.Action.StartsWith("Expense.")||x.Action.StartsWith("EmployeeAdvance.")||x.Action.StartsWith("BusinessDay.")));
        if(entityId!=null)q=q.Where(x=>x.EntityId==entityId);
        var total=await q.CountAsync(ct);
        var items=await q.OrderByDescending(x=>x.CreatedAt).ThenBy(x=>x.Id).Skip((page-1)*pageSize).Take(pageSize)
            .Select(x=>new FinancialAuditDetails(x.Id,x.Action,x.EntityType,x.EntityId,x.UserId,x.CreatedAt,x.OldValues,x.NewValues)).ToListAsync(ct);
        return new(items,total,page,pageSize);

```

## CampionsSystem.BLL/Shifts/ExpenseService.cs

Lines 15–58

```csharp

        var category=await db.ExpenseCategories.Where(x=>x.Id==e.ExpenseCategoryId).Select(x=>x.Name).SingleAsync(ct);
        var method=await db.FinancialAccounts.Where(x=>x.Id==e.FinancialAccountId).Select(x=>x.AccountType).SingleAsync(ct);
        var returned=await db.ExpenseReversals.Where(x=>x.ExpenseId==e.Id).SumAsync(x=>x.Amount,ct);
        return new(e.Id,e.BranchId,e.ShiftId,e.RecordingEmployeeId,e.ExpenseCategoryId,category,e.FinancialAccountId,method,e.Amount,returned,e.Description,e.ReceiptReference,e.ExpenseDate,e.BusinessDate,e.PaidAt,e.Status);
    }
    public Task<ExpenseDetails> RecordPaidAsync(PaidExpenseRequest r,CancellationToken ct=default)=>money.TransactionAsync(async()=>
    {
        FinancialOperations.Operation(r.OperationId);FinancialOperations.Money(r.Amount,true);FinancialOperations.Reason(r.Description);
        if(r.ReceiptReference?.Length>512)throw new BusinessRuleException("Receipt reference must be at most 512 characters.");
        var(branch,actor)=await money.ResolveAsync(ShiftPermissions.ExpenseCreate,r.BranchId,ct);await money.LockBranchAsync(branch,ct);
        var hash=FinancialOperations.Hash(r with{BranchId=branch});var prior=await db.Expenses.AsNoTracking().SingleOrDefaultAsync(x=>x.OperationId==r.OperationId,ct);
        if(prior!=null){FinancialOperations.Retry(prior.RequestHash,hash,prior.CreatedByUserId,actor);if(prior.BranchId!=branch)throw new UnauthorizedAccessException();return await Details(prior,ct);}
        var shift=await money.RequireShiftAsync(branch,actor,ct);var account=await money.AccountAsync(branch,r.FinancialAccountId,ct);
        var category=await db.ExpenseCategories.FromSqlInterpolated($"SELECT * FROM \"ExpenseCategories\" WHERE \"Id\"={r.ExpenseCategoryId} FOR SHARE").AsNoTracking().SingleOrDefaultAsync(ct);
        if(category==null||!category.IsActive)throw new BusinessRuleException("Select an active expense category.");
        var(date,day)=await money.BusinessDateAsync(branch,ct);
        var expense=new Expense{BranchId=branch,ShiftId=shift.Id,RecordingEmployeeId=shift.EmployeeId,FinancialAccountId=account.Id,ExpenseCategoryId=category.Id,
            OperationId=r.OperationId,RequestHash=hash,Amount=r.Amount,Description=r.Description,ReceiptReference=r.ReceiptReference,ExpenseDate=access.Now,PaidAt=access.Now,
            CreatedAt=access.Now,CreatedByUserId=actor,BusinessDate=date,BusinessDayId=day,Status=ExpenseStatus.Recorded};db.Add(expense);
        await money.PostAsync(new FinancialTransaction{BranchId=branch,FinancialAccountId=account.Id,ShiftId=shift.Id,Expense=expense,SourceType=FinancialSourceType.Expense,
            TransactionType=FinancialTransactionType.Debit,Amount=r.Amount,OccurredAt=access.Now,CreatedByUserId=actor,BusinessDate=date,BusinessDayId=day,Notes=r.Description},ct);
        access.Audit(actor,branch,"Expense.Paid",nameof(Expense),expense.Id,after:new{r.Amount,AccountId=account.Id,account.AccountType,ShiftId=shift.Id,date,r.Description});
        return await Details(expense,ct);
    },ct);
    public Task<ExpenseReversalDetails> ReverseAsync(Guid id,ReverseExpenseRequest r,CancellationToken ct=default)=>money.TransactionAsync<ExpenseReversalDetails>(async()=>
    {
        FinancialOperations.Operation(r.OperationId);FinancialOperations.Money(r.ReturnedAmount,true);FinancialOperations.Reason(r.Reason);
        if(!r.FundsActuallyReturned||r.ReturnReference?.Length>512)throw new BusinessRuleException("Confirm actual returned funds; a reversal cannot invent returned money.");
        var info=await db.Expenses.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new BranchNotFoundException("Expense not found.");
        var actor=await access.RequireAsync(ShiftPermissions.ExpenseReverse,info.BranchId,ct);await money.LockBranchAsync(info.BranchId,ct);
        var hash=FinancialOperations.Hash(new{id,r});var prior=await db.ExpenseReversals.AsNoTracking().SingleOrDefaultAsync(x=>x.OperationId==r.OperationId,ct);
        if(prior!=null){FinancialOperations.Retry(prior.RequestHash,hash,prior.CreatedByUserId,actor);return new(prior.Id,prior.ExpenseId,prior.BranchId,prior.ShiftId,prior.Amount,prior.Reason,prior.ReturnReference,prior.OccurredAt,prior.BusinessDate);}
        var shift=await money.RequireShiftAsync(info.BranchId,actor,ct);
        if(info.OperationId==null||info.PaidAt==null)throw new BusinessRuleException("Legacy expense payment must be explicitly reconciled before managed reversal.");
        if(await db.CoachPayouts.AnyAsync(p=>p.ExpenseId==id,ct))throw new BusinessRuleException("Coach payout returns require an explicit Owner earnings adjustment policy.");
        var returned=await db.ExpenseReversals.Where(x=>x.ExpenseId==id).SumAsync(x=>x.Amount,ct);
        if(r.ReturnedAmount>info.Amount-returned)throw new BusinessRuleException("Return exceeds the paid, not-yet-returned expense amount.");
        var account=await money.AccountAsync(info.BranchId,info.FinancialAccountId,ct);var(date,day)=await money.BusinessDateAsync(info.BranchId,ct);
        var reversal=new ExpenseReversal{ExpenseId=id,BranchId=info.BranchId,ShiftId=shift.Id,OperationId=r.OperationId,RequestHash=hash,Amount=r.ReturnedAmount,
            Reason=r.Reason,ReturnReference=r.ReturnReference,OccurredAt=access.Now,CreatedByUserId=actor,BusinessDate=date,BusinessDayId=day};db.Add(reversal);
        var ledger=new FinancialTransaction{BranchId=info.BranchId,FinancialAccountId=account.Id,ShiftId=shift.Id,ExpenseId=id,SourceType=FinancialSourceType.Expense,
            TransactionType=FinancialTransactionType.Credit,Amount=r.ReturnedAmount,OccurredAt=access.Now,CreatedByUserId=actor,BusinessDate=date,BusinessDayId=day,Notes=r.Reason};
        await money.PostAsync(ledger,ct);
        access.Audit(actor,info.BranchId,"Expense.ReturnRecorded",nameof(ExpenseReversal),reversal.Id,after:new{ExpenseId=id,LedgerId=ledger.Id,r.ReturnedAmount,r.Reason,ShiftId=shift.Id,date});

```

Lines 60–98

```csharp

    },ct);
    public async Task<ExpenseDetails> GetAsync(Guid id,CancellationToken ct=default)
    {
        var row=await db.Expenses.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new BranchNotFoundException("Expense not found.");
        await access.RequireAsync(ShiftPermissions.ExpenseView,row.BranchId,ct);return await Details(row,ct);
    }
    public async Task<ShiftPage<ExpenseReversalDetails>> ReturnsAsync(Guid id,int page=1,int pageSize=25,CancellationToken ct=default)
    {
        if(page<1||pageSize is <1 or >100)throw new BusinessRuleException("Invalid pagination.");
        var branch=await db.Expenses.Where(x=>x.Id==id).Select(x=>(Guid?)x.BranchId).SingleOrDefaultAsync(ct)??throw new BranchNotFoundException("Expense not found.");
        await access.RequireAsync(ShiftPermissions.ExpenseView,branch,ct);
        var q=db.ExpenseReversals.AsNoTracking().Where(x=>x.ExpenseId==id);
        var total=await q.CountAsync(ct);var items=await q.OrderByDescending(x=>x.OccurredAt).ThenBy(x=>x.Id).Skip((page-1)*pageSize).Take(pageSize)
            .Select(x=>new ExpenseReversalDetails(x.Id,x.ExpenseId,x.BranchId,x.ShiftId,x.Amount,x.Reason,x.ReturnReference,x.OccurredAt,x.BusinessDate)).ToListAsync(ct);
        return new(items,total,page,pageSize);
    }
    public async Task<ShiftPage<ExpenseDetails>> HistoryAsync(VaultHistoryRequest r,CancellationToken ct=default)
    {
        if(r.Page<1||r.PageSize is <1 or >100||r.From>r.To)throw new BusinessRuleException("Invalid history range or pagination.");
        var(branch,_)=await money.ResolveAsync(ShiftPermissions.ExpenseView,r.BranchId,ct);var q=db.Expenses.AsNoTracking().Where(x=>x.BranchId==branch);
        if(r.From!=null)q=q.Where(x=>x.BusinessDate>=r.From);if(r.To!=null)q=q.Where(x=>x.BusinessDate<=r.To);if(r.AccountId!=null)q=q.Where(x=>x.FinancialAccountId==r.AccountId);
        var total=await q.CountAsync(ct);var rows=await q.OrderByDescending(x=>x.ExpenseDate).ThenBy(x=>x.Id).Skip((r.Page-1)*r.PageSize).Take(r.PageSize).ToListAsync(ct);
        var items=new List<ExpenseDetails>();foreach(var row in rows)items.Add(await Details(row,ct));return new(items,total,r.Page,r.PageSize);
    }
    public async Task<ShiftPage<ExpenseCategoryDetails>> CategoriesAsync(int page=1,int pageSize=25,CancellationToken ct=default)
    {
        await access.RequireAsync(ShiftPermissions.ExpenseView,null,ct);
        if(page<1||pageSize is <1 or >100)throw new BusinessRuleException("Invalid category pagination.");
        var q=db.ExpenseCategories.AsNoTracking();var total=await q.CountAsync(ct);
        var items=await q.OrderBy(x=>x.Name).ThenBy(x=>x.Id).Skip((page-1)*pageSize).Take(pageSize).Select(x=>new ExpenseCategoryDetails(x.Id,x.Name,x.IsActive)).ToListAsync(ct);
        return new(items,total,page,pageSize);
    }
    public Task<ExpenseCategoryDetails> SetCategoryAsync(Guid? id,ExpenseCategoryRequest r,CancellationToken ct=default)=>money.TransactionAsync<ExpenseCategoryDetails>(async()=>
    {
        await access.RequireAsync(ShiftPermissions.ExpenseManage,null,ct);var actor=await access.RequireAsync("Branches.AccessAll",null,ct);
        if(string.IsNullOrWhiteSpace(r.Name)||r.Name.Trim().Length>256)throw new BusinessRuleException("A category name of up to 256 characters is required.");
        var row=id==null?new ExpenseCategory():await db.ExpenseCategories.SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new BranchNotFoundException("Category not found.");
        var before=new{row.Name,row.IsActive};row.Name=r.Name.Trim();row.IsActive=r.IsActive;if(id==null)db.Add(row);
        access.Audit(actor,null,id==null?"ExpenseCategory.Created":"ExpenseCategory.Updated",nameof(ExpenseCategory),row.Id,before,new{row.Name,row.IsActive});

```
