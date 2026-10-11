using CampionsSystem.BLL.Shifts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CampionsSystem.API.Controllers;
[ApiController,Route("api/expenses")]
public sealed class ExpensesController(ExpenseService expenses):BranchControllerBase
{
    [HttpPost,Authorize(Policy="Permission:Expenses.Create")]
    public Task<IActionResult> Paid(PaidExpenseRequest r,CancellationToken ct)=>Execute(()=>expenses.RecordPaidAsync(r,ct));
    [HttpGet,Authorize(Policy="Permission:Expenses.View")]
    public Task<IActionResult> History([FromQuery]VaultHistoryRequest r,CancellationToken ct)=>Execute(()=>expenses.HistoryAsync(r,ct));
    [HttpGet("{id:guid}"),Authorize(Policy="Permission:Expenses.View")]
    public Task<IActionResult> Get(Guid id,CancellationToken ct)=>Execute(()=>expenses.GetAsync(id,ct));
    [HttpPost("{id:guid}/returns"),Authorize(Policy="Permission:Expenses.Reverse")]
    public Task<IActionResult> Return(Guid id,ReverseExpenseRequest r,CancellationToken ct)=>Execute(()=>expenses.ReverseAsync(id,r,ct));
    [HttpGet("{id:guid}/returns"),Authorize(Policy="Permission:Expenses.View")]
    public Task<IActionResult> Returns(Guid id,[FromQuery]int page=1,[FromQuery]int pageSize=25,CancellationToken ct=default)=>Execute(()=>expenses.ReturnsAsync(id,page,pageSize,ct));
    [HttpGet("categories"),Authorize(Policy="Permission:Expenses.View")]
    public Task<IActionResult> Categories([FromQuery]int page=1,[FromQuery]int pageSize=25,CancellationToken ct=default)=>Execute(()=>expenses.CategoriesAsync(page,pageSize,ct));
    [HttpPost("categories"),Authorize(Policy="Permission:Expenses.Manage")]
    public Task<IActionResult> Category(ExpenseCategoryRequest r,CancellationToken ct)=>Execute(()=>expenses.SetCategoryAsync(null,r,ct));
    [HttpPut("categories/{id:guid}"),Authorize(Policy="Permission:Expenses.Manage")]
    public Task<IActionResult> Category(Guid id,ExpenseCategoryRequest r,CancellationToken ct)=>Execute(()=>expenses.SetCategoryAsync(id,r,ct));
}
[ApiController,Route("api/employee-advances")]
public sealed class EmployeeAdvancesController(ExpenseService expenses):BranchControllerBase
{
    [HttpPost,Authorize(Policy="Permission:Advances.Create")]
    public Task<IActionResult> Paid(AdvanceRequest r,CancellationToken ct)=>Execute(()=>expenses.RecordAdvanceAsync(r,ct));
    [HttpGet,Authorize(Policy="Permission:Advances.View")]
    public Task<IActionResult> History([FromQuery]VaultHistoryRequest r,CancellationToken ct)=>Execute(()=>expenses.AdvancesAsync(r,ct));
    [HttpGet("{id:guid}"),Authorize(Policy="Permission:Advances.View")]
    public Task<IActionResult> Get(Guid id,CancellationToken ct)=>Execute(()=>expenses.GetAdvanceAsync(id,ct));
}
