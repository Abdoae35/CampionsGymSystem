using System.Text.Json;
using CampionsSystem.BLL.MonthlyFinance;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed partial class EmployeeShiftTests
{
    [Fact] public async Task SalaryExcludesDatedSuspensionAndResignationGapsButApprovedLeaveRemainsPaid()
    {
        var month=await Month();
        // Historical status evidence uses the same existing EmployeeStatusChanged audit shape.
        void Status(int before,int after,int day)=>db.Add(new AuditLog{UserId=a.Id,BranchId=branch.Id,Action="EmployeeStatusChanged",EntityType=nameof(Employee),EntityId=ea.Id,
            OldValues=JsonSerializer.Serialize(new{EmploymentStatus=before,TerminationDate=(DateOnly?)null}),NewValues=JsonSerializer.Serialize(new{EmploymentStatus=after,TerminationDate=(DateOnly?)null}),CreatedAt=clock.Now.AddDays(day)});
        Status(0,2,5);Status(2,0,10);Status(0,1,15);Status(1,0,20);
        db.Add(new EmployeeLeaveRequest{EmployeeId=ea.Id,FromDate=today.AddDays(21),ToDate=today.AddDays(25),Status=EmployeeLeaveRequestStatus.Approved,Reason="Paid approved leave",ReviewedByUserId=a.Id,CreatedAt=clock.Now,ReviewedAt=clock.Now});
        await db.SaveChangesAsync();clock.Now=clock.Now.AddDays(29);var result=await Salaries().PreviewAsync(month.Id,ea.Id);
        Assert.Equal(20,result.EligibleDays);Assert.Equal(4000,result.BasicSalary);
    }
    [Fact] public async Task MissingSalaryHistoryFailsClosingAtomicallyAndExplicitZeroIsSupported()
    {
        FinanceAccess();var month=await Months().OpenAsync(new(Guid.NewGuid(),"Month",today,today,"Start"));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Months().CloseAsync(month.Id,new(Guid.NewGuid(),0,"Missing salaries")));
        Assert.Equal("Open",(await Months().GetAsync(month.Id)).Status);Assert.Empty(await db.Payrolls.ToListAsync());Assert.Empty(await db.FinancialClosingRevisions.ToListAsync());
        await Salaries().ConfigureAsync(ea.Id,new(Guid.NewGuid(),0,ea.HireDate,"No basic salary"));await Salaries().ConfigureAsync(eb.Id,new(Guid.NewGuid(),0,eb.HireDate,"No basic salary"));
        await Months().CloseAsync(month.Id,new(Guid.NewGuid(),0,"Zero explicitly configured"));var zero=await Months().ReportAsync(month.Id);Assert.Equal(0,zero.Summary.BasicSalaryEarned);Assert.All(zero.Payroll,x=>Assert.Equal("NoAmountDue",x.Status));
    }
    [Fact] public async Task TransferAndSalaryDecreaseCannotStrandAdvancesOrRewriteRecognizedSalary()
    {
        var month=await Month(salary:1000);user.Context=user.Context! with{Permissions=[..user.Context.Permissions,"Employees.Transfer"]};
        var shift=await Service().OpenAsync(new(Guid.NewGuid(),100000));await Expenses().RecordAdvanceAsync(new(Guid.NewGuid(),ea.Id,cash.Id,900,"Advance"));
        await Service().CloseAsync(shift.Id,new(Guid.NewGuid(),shift.Version,99100));
        clock.Now=clock.Now.AddDays(10);var transfers=new CampionsSystem.BLL.Branches.EmployeeBranchAssignmentService(db,user,access);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>transfers.TransferAsync(ea.Id,new(other.Id,today.AddDays(10))));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Salaries().ConfigureAsync(ea.Id,new(Guid.NewGuid(),0,today.AddDays(10),"Reduce")));
        clock.Now=clock.Now.AddDays(19);await Months().CloseAsync(month.Id,new(Guid.NewGuid(),0,"Close"));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Salaries().ConfigureAsync(ea.Id,new(Guid.NewGuid(),1,today.AddDays(29),"Rewrite")));
        Assert.Equal(1000,(await db.EmployeeSalarySettings.SingleAsync(x=>x.EmployeeId==ea.Id)).BasicSalary);
    }
    [Fact] public async Task AdvanceFromOneBranchCannotReduceAnotherBranchSalary()
    {
        var month=await Month(salary:6000);var assignment=await db.EmployeeBranchAssignments.SingleAsync(x=>x.EmployeeId==ea.Id);assignment.EndDate=today.AddDays(14);
        db.Add(new EmployeeBranchAssignment{EmployeeId=ea.Id,BranchId=other.Id,StartDate=today.AddDays(15),AssignedByUserId=a.Id});
        var second=new FinancialMonth{BranchId=other.Id,Name="Mahalla",StartDate=month.StartDate,EndDate=month.EndDate,CreatedByUserId=a.Id,CreatedAt=clock.Now};db.Add(second);await db.SaveChangesAsync();
        await Service().OpenAsync(new(Guid.NewGuid(),100000));await Expenses().RecordAdvanceAsync(new(Guid.NewGuid(),ea.Id,cash.Id,1000,"Tanta advance"));
        Assert.Equal(2000,(await Salaries().PreviewAsync(month.Id,ea.Id)).PayableBasicSalary);
        var otherBranch=await Salaries().PreviewAsync(second.Id,ea.Id);Assert.Equal(3000,otherBranch.PayableBasicSalary);Assert.Equal(0,otherBranch.Advances);
    }
    [Fact] public void SalaryRoundingConservesFullPeriodAcrossBranchTransferAndRateChanges()
    {
        var first=new FinancialMonth{BranchId=branch.Id,StartDate=today,EndDate=today.AddDays(29)};
        var second=new FinancialMonth{BranchId=other.Id,StartDate=first.StartDate,EndDate=first.EndDate};
        var assignments=new[]{new EmployeeBranchAssignment{BranchId=branch.Id,StartDate=today,EndDate=today.AddDays(12)},new EmployeeBranchAssignment{BranchId=other.Id,StartDate=today.AddDays(13)}};
        var settings=new[]{new EmployeeSalarySetting{BasicSalary=1000,EffectiveFrom=today}};
        var one=SalaryProjection.Calculate(ea,first,assignments,settings,0);var two=SalaryProjection.Calculate(ea,second,assignments,settings,0);
        Assert.Equal(433.33m,one.BasicSalary);Assert.Equal(566.67m,two.BasicSalary);Assert.Equal(1000,one.BasicSalary+two.BasicSalary);
    }
    [Fact] public async Task FinancialMonthVersionIdempotencyAndBoundsAreEnforced()
    {
        var month=await Month(today,0);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Months().OpenAsync(new(Guid.NewGuid(),"Duplicate",today.AddDays(1),today.AddDays(30),"Cannot overlap open")));
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(()=>Months().CloseAsync(month.Id,new(Guid.NewGuid(),1,"Stale")));
        var close=new CloseFinancialMonthRequest(Guid.NewGuid(),0,"Close");var closed=await Months().CloseAsync(month.Id,close);
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Months().CloseAsync(month.Id,close with{Reason="Changed"}));
        await Assert.ThrowsAsync<BusinessRuleException>(()=>Months().OpenAsync(new(Guid.NewGuid(),"Gap",today.AddDays(2),today.AddDays(30),"Gap")));
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(()=>Months().ReopenAsync(month.Id,new(Guid.NewGuid(),0,"Stale")));
        var r=new ReopenFinancialMonthRequest(Guid.NewGuid(),closed.Version,"Investigate");await Months().ReopenAsync(month.Id,r);await Months().ReopenAsync(month.Id,r);
        Assert.Equal(3,(await Months().EventsAsync(month.Id)).Total);
    }
    [Fact] public async Task ReclosingDoesNotCreateSalaryCostsForEmployeesAddedAfterOriginalCutoff()
    {
        var month=await Month(today);var closed=await Months().CloseAsync(month.Id,new(Guid.NewGuid(),0,"First close"));clock.Now=clock.Now.AddHours(1);
        var later=new Employee{FullName="Later employee",Phone="later-salary",HireDate=today};db.Add(later);
        db.Add(new EmployeeBranchAssignment{EmployeeId=later.Id,BranchId=branch.Id,StartDate=today,AssignedByUserId=a.Id});await db.SaveChangesAsync();
        await Salaries().ConfigureAsync(later.Id,new(Guid.NewGuid(),6000,today,"New employee"));
        var reopened=await Months().ReopenAsync(month.Id,new(Guid.NewGuid(),closed.Version,"Investigate"));
        await Months().CloseAsync(month.Id,new(Guid.NewGuid(),reopened.Version,"Reissue"));var report=await Months().ReportAsync(month.Id);
        Assert.Equal(12000,report.Summary.BasicSalaryEarned);Assert.Equal(12000,report.Summary.EmployeeClosingOutstanding);
        Assert.Equal(2,report.Salaries.Count);Assert.DoesNotContain(report.Salaries,x=>x.EmployeeId==later.Id);Assert.Equal(2,await db.Payrolls.CountAsync());
    }
}
