using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace CampionsSystem.DAL.Tests;

// Creates real owned operational state through the feature services. It never assigns
// old financial records, and uses explicit immutable fixture opening money, not sales.
internal static class OperationalShiftFixture
{
    public static async Task<ShiftDetails> OpenAsync(ApplicationDbContext db,Guid actor,Guid branch,TimeProvider? clock=null,Guid? accountId=null)
    {
        var user=new TestBranchUser(new(actor,"fixture",null,null,[],[..ShiftPermissions.Codes,"Branches.AccessAll"],new(branch,"Branch")));
        var access=new OperationAccess(user,db,clock??TimeProvider.System,Options.Create(new AuthContextOptions()));
        var employee=await db.Employees.SingleOrDefaultAsync(x=>x.UserId==actor);
        if(employee==null){employee=new(){UserId=actor,FullName="Operational employee",Phone="op-"+actor.ToString("N"),HireDate=access.Today.AddYears(-1)};db.Add(employee);await db.SaveChangesAsync();}
        if(!await db.EmployeeBranchAssignments.AnyAsync(x=>x.EmployeeId==employee.Id&&x.BranchId==branch&&x.StartDate<=access.Today&&(x.EndDate==null||x.EndDate>=access.Today)))
        {db.Add(new EmployeeBranchAssignment{EmployeeId=employee.Id,BranchId=branch,StartDate=access.Today.AddYears(-1),AssignedByUserId=actor});await db.SaveChangesAsync();}
        var drawer=await db.CashDrawers.AsNoTracking().SingleOrDefaultAsync(x=>x.BranchId==branch);
        FinancialAccount account;
        if(drawer==null)
        {
            account=accountId==null?new FinancialAccount{BranchId=branch,Name="__shared_drawer",OpeningBalance=100000}:await db.FinancialAccounts.SingleAsync(x=>x.Id==accountId);
            if(accountId==null)db.Add(account);await db.SaveChangesAsync();
            await new VaultService(db,user,access).SetupAsync(new(Guid.NewGuid(),account.Id,account.OpeningBalance,"Explicit fixture opening cash",branch));
        }
        else account=await db.FinancialAccounts.SingleAsync(x=>x.Id==drawer.FinancialAccountId);
        var existing=await new ShiftService(db,user,access).CurrentAsync();
        return existing??await new ShiftService(db,user,access).OpenAsync(new(Guid.NewGuid(),(await new VaultService(db,user,access).PositionAsync(branch)).ExpectedPhysicalCash,branch));
    }
}
