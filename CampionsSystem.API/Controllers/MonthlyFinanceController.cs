using CampionsSystem.BLL.MonthlyFinance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CampionsSystem.API.Controllers;

[ApiController,Route("api/financial-months")]
public sealed class MonthlyFinanceController(MonthlyFinanceService finance,SalaryService salaries):BranchControllerBase
{
    [HttpPost,Authorize(Policy="Permission:FinancialMonths.Manage")]
    public Task<IActionResult> Open(OpenFinancialMonthRequest r,CancellationToken ct)=>Execute(()=>finance.OpenAsync(r,ct));
    [HttpGet,Authorize(Policy="Permission:FinancialMonths.View")]
    public Task<IActionResult> History([FromQuery]Guid? branchId,[FromQuery]int page=1,[FromQuery]int pageSize=25,CancellationToken ct=default)=>Execute(()=>finance.HistoryAsync(branchId,page,pageSize,ct));
    [HttpGet("{id:guid}"),Authorize(Policy="Permission:FinancialMonths.View")]
    public Task<IActionResult> Get(Guid id,CancellationToken ct)=>Execute(()=>finance.GetAsync(id,ct));
    [HttpPost("{id:guid}/close"),Authorize(Policy="Permission:FinancialMonths.Close")]
    public Task<IActionResult> Close(Guid id,CloseFinancialMonthRequest r,CancellationToken ct)=>Execute(()=>finance.CloseAsync(id,r,ct));
    [HttpPost("{id:guid}/reopen"),Authorize(Policy="Permission:FinancialMonths.Reopen")]
    public Task<IActionResult> Reopen(Guid id,ReopenFinancialMonthRequest r,CancellationToken ct)=>Execute(()=>finance.ReopenAsync(id,r,ct));
    [HttpGet("{id:guid}/report"),Authorize(Policy="Permission:FinancialMonths.View")]
    public Task<IActionResult> Report(Guid id,[FromQuery]int? revision,CancellationToken ct)=>Execute(()=>finance.ReportAsync(id,revision,ct));
    [HttpGet("{id:guid}/transactions"),Authorize(Policy="Permission:FinancialMonths.View")]
    public Task<IActionResult> Transactions(Guid id,[FromQuery]int? revision,[FromQuery]int page=1,[FromQuery]int pageSize=25,CancellationToken ct=default)=>Execute(()=>finance.TransactionsAsync(id,revision,page,pageSize,ct));
    [HttpGet("{id:guid}/revisions"),Authorize(Policy="Permission:FinancialMonths.View")]
    public Task<IActionResult> Revisions(Guid id,CancellationToken ct)=>Execute(()=>finance.RevisionsAsync(id,ct));
    [HttpGet("{id:guid}/events"),Authorize(Policy="Permission:FinancialMonths.View")]
    public Task<IActionResult> Events(Guid id,[FromQuery]int page=1,[FromQuery]int pageSize=25,CancellationToken ct=default)=>Execute(()=>finance.EventsAsync(id,page,pageSize,ct));
    [HttpGet("{id:guid}/export"),Authorize(Policy="Permission:FinancialMonths.Export")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    public async Task<IActionResult> Export(Guid id,[FromQuery]int? revision,CancellationToken ct)
    {
        var result=await Execute(()=>finance.ExportAsync(id,revision,ct));
        if(result is OkObjectResult{Value:WorkbookDownload w}){Response.Headers["X-Workbook-SHA256"]=w.Sha256;return File(w.Bytes,"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",w.FileName);}return result;
    }
    [HttpGet("{id:guid}/employees/{employeeId:guid}/salary"),Authorize(Policy="Permission:Salaries.View")]
    public Task<IActionResult> Salary(Guid id,Guid employeeId,CancellationToken ct)=>Execute(()=>salaries.PreviewAsync(id,employeeId,ct));
    [HttpGet("consolidated"),Authorize(Policy="Permission:FinancialMonths.ViewConsolidated")]
    public Task<IActionResult> Consolidated([FromQuery]Guid[] monthIds,CancellationToken ct)=>Execute(()=>finance.ConsolidatedAsync(monthIds,ct));
    [HttpPost("transactions/{ledgerId:guid}/original-occurrence"),Authorize(Policy="Permission:FinancialMonths.Manage")]
    public Task<IActionResult> Occurrence(Guid ledgerId,RecordOccurrenceRequest r,CancellationToken ct)=>Execute(()=>finance.RecordOccurrenceAsync(ledgerId,r,ct));
}
[ApiController,Route("api/employee-salaries")]
public sealed class EmployeeSalariesController(SalaryService salaries):BranchControllerBase
{
    [HttpPost("{employeeId:guid}/settings"),Authorize(Policy="Permission:Salaries.Manage")]
    public Task<IActionResult> Configure(Guid employeeId,SetSalaryRequest r,CancellationToken ct)=>Execute(()=>salaries.ConfigureAsync(employeeId,r,ct));
    [HttpGet("{employeeId:guid}/settings"),Authorize(Policy="Permission:Salaries.View")]
    public Task<IActionResult> History(Guid employeeId,[FromQuery]Guid? branchId,CancellationToken ct)=>Execute(()=>salaries.HistoryAsync(employeeId,branchId,ct));
}
[ApiController,Route("api/payroll")]
public sealed class PayrollController(PayrollService payroll):BranchControllerBase
{
    [HttpGet,Authorize(Policy="Permission:Payroll.View")]
    public Task<IActionResult> History([FromQuery]Guid? branchId,[FromQuery]Guid? employeeId,[FromQuery]Guid? monthId,[FromQuery]int page=1,[FromQuery]int pageSize=25,CancellationToken ct=default)=>Execute(()=>payroll.HistoryAsync(branchId,employeeId,monthId,page,pageSize,ct));
    [HttpPost("commission-recognitions"),Authorize(Policy="Permission:Payroll.Manage")]
    public Task<IActionResult> Recognize(RecognizePayrollCommissionsRequest r,CancellationToken ct)=>Execute(()=>payroll.RecognizeCommissionPeriodAsync(r,ct));
    [HttpPost("{id:guid}/payments"),Authorize(Policy="Permission:Payroll.RecordPayment")]
    public Task<IActionResult> Pay(Guid id,RecordPayrollPaymentRequest r,CancellationToken ct)=>Execute(()=>payroll.RecordPaymentAsync(id,r,ct));
    [HttpGet("{id:guid}/payments"),Authorize(Policy="Permission:Payroll.View")]
    public Task<IActionResult> Payments(Guid id,[FromQuery]int page=1,[FromQuery]int pageSize=25,CancellationToken ct=default)=>Execute(()=>payroll.PaymentsAsync(id,page,pageSize,ct));
}
[ApiController,Route("api/maintenance-plans")]
public sealed class MaintenancePlansController(MaintenanceService maintenance):BranchControllerBase
{
    [HttpPost,Authorize(Policy="Permission:Maintenance.Manage")]
    public Task<IActionResult> Configure(MaintenancePlanRequest r,CancellationToken ct)=>Execute(()=>maintenance.ConfigureAsync(r,ct));
    [HttpGet,Authorize(Policy="Permission:Maintenance.View")]
    public Task<IActionResult> Plans([FromQuery]Guid? branchId,[FromQuery]DateOnly? asOf,[FromQuery]int page=1,[FromQuery]int pageSize=25,CancellationToken ct=default)=>Execute(()=>maintenance.PlansAsync(branchId,asOf,page,pageSize,ct));
    [HttpGet("{id:guid}/history"),Authorize(Policy="Permission:Maintenance.View")]
    public Task<IActionResult> History(Guid id,[FromQuery]int page=1,[FromQuery]int pageSize=25,CancellationToken ct=default)=>Execute(()=>maintenance.HistoryAsync(id,page,pageSize,ct));
}
