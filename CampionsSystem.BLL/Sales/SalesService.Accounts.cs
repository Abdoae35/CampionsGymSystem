using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Sales;
public sealed partial class SalesService
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
        var account=new FinancialAccount{BranchId=branch,Name=name,AccountType=r.Method,OpeningBalance=0,CreatedAt=access.Now};db.Add(account);
        access.Audit(actor.UserId,branch,"FinancialAccount.Created",nameof(FinancialAccount),account.Id,after:new{name,Method=r.Method,OpeningBalance=0});
        await db.SaveChangesAsync(ct);return new FinancialAccountDetails(account.Id,branch,name,r.Method.ToString(),true);
    },ct);
}
