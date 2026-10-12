using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Coaching;

public sealed partial class CoachingService
{
    public Task<CoachPayoutDetails> PayAsync(CoachPayoutRequest r, CancellationToken ct = default)
        => coordination.TransactionAsync(async () =>
        {
            FinancialOperations.Operation(r.OperationId); FinancialOperations.Money(r.Amount, true); FinancialOperations.Reason(r.Reason);
            if (r.ReceiptReference?.Length > 512) throw new BusinessRuleException("Receipt reference is too long.");
            var actor = await OwnerAsync(CoachingPermissions.Pay, r.BranchId, ct); await coordination.LockBranchAsync(r.BranchId, ct);
            var hash = FinancialOperations.Hash(r);
            var prior = await db.CoachPayouts.AsNoTracking().SingleOrDefaultAsync(p => p.OperationId == r.OperationId, ct);
            if (prior != null) { FinancialOperations.Retry(prior.RequestHash, hash, prior.AuthorizedByUserId, actor); return await PayoutDetailsAsync(prior, ct); }
            var balance = await BalanceCoreAsync(r.BranchId, r.CoachEmployeeId, ct);
            if (balance.Outstanding < r.Amount) throw new BusinessRuleException("Payout exceeds this coach's recognized outstanding branch earnings.");
            var shift = await coordination.RequireShiftAsync(r.BranchId, actor, ct);
            var account = await coordination.AccountAsync(r.BranchId, r.FinancialAccountId, ct);
            if (account.AccountType != FinancialAccountType.Cash && await coordination.BookAsync(account.Id, ct) < r.Amount) throw new BusinessRuleException("Insufficient financial account balance.");
            if (!await db.ExpenseCategories.AnyAsync(c => c.Id == r.ExpenseCategoryId && c.IsActive, ct)) throw new BusinessRuleException("Select an active expense category.");
            var (date, day) = await coordination.BusinessDateAsync(r.BranchId, ct);
            var expense = new Expense { BranchId = r.BranchId, ShiftId = shift.Id, RecordingEmployeeId = shift.EmployeeId,
                ExpenseCategoryId = r.ExpenseCategoryId, FinancialAccountId = account.Id, OperationId = r.OperationId, RequestHash = hash,
                Amount = r.Amount, Description = r.Reason.Trim(), ReceiptReference = r.ReceiptReference, ExpenseDate = access.Now, PaidAt = access.Now,
                CreatedAt = access.Now, CreatedByUserId = actor, ApprovedByUserId = actor, BusinessDate = date, BusinessDayId = day, Status = ExpenseStatus.Recorded };
            var payout = new CoachPayout { BranchId = r.BranchId, CoachEmployeeId = r.CoachEmployeeId, Expense = expense,
                OperationId = r.OperationId, RequestHash = hash, Amount = r.Amount, BalanceBefore = balance.Outstanding,
                BalanceAfter = balance.Outstanding - r.Amount, AuthorizedByUserId = actor, PaidAt = access.Now, BusinessDate = date };
            db.Add(payout);
            var earnings = await db.CoachEarnings.AsNoTracking().Where(e => e.BranchId == r.BranchId && e.CoachEmployeeId == r.CoachEmployeeId)
                .OrderBy(e => e.BusinessDate).ThenBy(e => e.CreatedAt).ThenBy(e => e.Id).ToListAsync(ct);
            var remaining = r.Amount;
            foreach (var earning in earnings)
            {
                var paid = await db.CoachPayoutAllocations.Where(a => a.CoachEarningId == earning.Id).SumAsync(a => a.Amount, ct);
                var take = Math.Min(remaining, earning.Amount - paid);
                if (take <= 0) continue;
                db.Add(new CoachPayoutAllocation { CoachPayout = payout, CoachEarningId = earning.Id, Amount = take }); remaining -= take;
                if (remaining == 0) break;
            }
            if (remaining != 0) throw new BusinessRuleException("Earning allocation history does not reconcile.");
            await coordination.PostAsync(new FinancialTransaction { BranchId = r.BranchId, FinancialAccountId = account.Id, ShiftId = shift.Id,
                Expense = expense, SourceType = FinancialSourceType.Expense, TransactionType = FinancialTransactionType.Debit, Amount = r.Amount,
                BusinessDate = date, BusinessDayId = day, OccurredAt = access.Now, CreatedByUserId = actor, Notes = r.Reason.Trim() }, ct);
            access.Audit(actor, r.BranchId, "CoachEarnings.Paid", nameof(CoachPayout), payout.Id, after: new { r.CoachEmployeeId, r.Amount, ExpenseId = expense.Id, payout.BalanceBefore, payout.BalanceAfter, AccountId = account.Id, account.AccountType, date });
            return await PayoutDetailsAsync(payout, ct);
        }, ct);
    private async Task<CoachPayoutDetails> PayoutDetailsAsync(CoachPayout p, CancellationToken ct)
    {
        var e = p.Expense ?? await db.Expenses.AsNoTracking().SingleAsync(e => e.Id == p.ExpenseId, ct);
        var method = await db.FinancialAccounts.Where(a => a.Id == e.FinancialAccountId).Select(a => a.AccountType).SingleAsync(ct);
        return new(p.Id, p.BranchId, p.CoachEmployeeId, p.ExpenseId, e.FinancialAccountId, method.ToString(), p.Amount, p.BalanceBefore, p.BalanceAfter, p.BusinessDate, p.PaidAt, p.AuthorizedByUserId);
    }
    public async Task<CoachingPage<CoachPayoutDetails>> PayoutHistoryAsync(Guid branch, Guid coach, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); await access.RequireAsync(CoachingPermissions.EarningsView, branch, ct);
        var q = db.CoachPayouts.AsNoTracking().Where(p => p.BranchId == branch && p.CoachEmployeeId == coach); var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(p => p.PaidAt).ThenBy(p => p.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        var details = new List<CoachPayoutDetails>(); foreach (var p in rows) details.Add(await PayoutDetailsAsync(p, ct)); return new(details, total, page, pageSize);
    }
}
