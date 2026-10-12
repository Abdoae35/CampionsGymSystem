using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
// Explicit test-only settings, never operational defaults.
internal static class AdvanceSalaryFixture
{
    public static async Task ConfigureAsync(ApplicationDbContext db,Guid branch,Guid actor,DateOnly date,params Guid[] employeeIds)
    {
        if(!await db.FinancialMonths.AnyAsync(x=>x.BranchId==branch))db.Add(new FinancialMonth{BranchId=branch,Name="Explicit advance test period",StartDate=date,EndDate=date.AddDays(29),CreatedByUserId=actor,CreatedAt=new DateTimeOffset(date.ToDateTime(new TimeOnly(8,0)),TimeSpan.Zero)});
        foreach(var id in employeeIds)
        {
            var e=await db.Employees.AsNoTracking().SingleAsync(x=>x.Id==id);
            if(!await db.EmployeeSalarySettings.AnyAsync(x=>x.EmployeeId==id))db.Add(new EmployeeSalarySetting{EmployeeId=id,BasicSalary=10000,EffectiveFrom=e.HireDate,CreatedByUserId=actor});
            if(!await db.EmployeeBranchAssignments.AnyAsync(x=>x.EmployeeId==id&&x.BranchId==branch&&x.StartDate<=date&&(x.EndDate==null||x.EndDate>=date)))db.Add(new EmployeeBranchAssignment{EmployeeId=id,BranchId=branch,StartDate=e.HireDate,AssignedByUserId=actor});
        }
        await db.SaveChangesAsync();
    }
}
