using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Shifts;

public sealed class VaultService(ApplicationDbContext db, ICurrentUserService current, OperationAccess access)
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
        var initialCount = await money.CountAsync(branch,actor,r.OperationId,hash,r.CountedCash,"Initial",null,r.Reason,ct);
        access.Audit(actor,branch,"Vault.DrawerConfigured",nameof(CashDrawer),branch,after:new { account.Id,r.CountedCash,date });
        await db.SaveChangesAsync(ct); return SetupSnapshot(initialCount);
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
        await db.SaveChangesAsync(ct); return Details(count);
    },ct);
    public Task<DrawerPosition> PositionAsync(Guid? branchId=null,CancellationToken ct=default)=>money.TransactionAsync(async()=>
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
    public async Task<ShiftPage<FinancialMovementDetails>> MovementsAsync(VaultHistoryRequest r,CancellationToken ct=default)
    {
        Validate(r);var(branch,_)=await money.ResolveAsync(ShiftPermissions.VaultView,r.BranchId,ct);
        var q=db.FinancialTransactions.AsNoTracking().Where(x=>x.BranchId==branch);
        if(r.From!=null)q=q.Where(x=>x.BusinessDate>=r.From);if(r.To!=null)q=q.Where(x=>x.BusinessDate<=r.To);if(r.AccountId!=null)q=q.Where(x=>x.FinancialAccountId==r.AccountId);
        var total=await q.CountAsync(ct);var items=await q.OrderByDescending(x=>x.OccurredAt).ThenBy(x=>x.Id).Skip((r.Page-1)*r.PageSize).Take(r.PageSize)
            .Select(x=>new FinancialMovementDetails(x.Id,x.FinancialAccountId,x.BranchId,x.ShiftId,x.SourceType,x.TransactionType,x.Amount,x.OccurredAt,x.BusinessDate,x.CreatedByUserId)).ToListAsync(ct);
        return new(items,total,r.Page,r.PageSize);
    }
}
