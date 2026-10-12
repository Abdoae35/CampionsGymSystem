using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.MonthlyFinance;

public static class FinancialMonthPosting
{
    // The caller owns the branch drawer lock. Legacy branches remain operational
    // until explicit rollout; once configured, an open destination is compulsory.
    public static async Task<FinancialMonth?> ResolveAsync(ApplicationDbContext db, Guid branch, DateOnly date, CancellationToken ct)
    {
        var month = await db.FinancialMonths.AsNoTracking().SingleOrDefaultAsync(x => x.BranchId == branch && x.Status == "Open", ct);
        if (month == null)
        {
            if (await db.FinancialMonths.AnyAsync(x => x.BranchId == branch, ct))
                throw new BusinessRuleException("Open the next branch financial month before starting shifts or recording new financial operations.");
            return null;
        }
        if (date > month.EndDate) throw new BusinessRuleException("BusinessDate exceeds the open financial month; close it and open its successor.");
        if (date < month.StartDate && !await db.FinancialMonths.AnyAsync(x => x.BranchId == branch && x.FirstClosedAt != null && x.EndDate >= date && x.EndDate < month.StartDate, ct))
            throw new BusinessRuleException("BusinessDate precedes the configured financial history.");
        return month;
    }
}
