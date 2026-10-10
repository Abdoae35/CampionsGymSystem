using CampionsSystem.BLL.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CampionsSystem.API.Controllers;
[ApiController,Route("api/financial-accounts")]
public sealed class FinancialAccountsController(SalesService sales):BranchControllerBase
{
    [HttpGet,Authorize(Policy="Permission:Payments.View")]
    public Task<IActionResult> Accounts([FromQuery]Guid? branchId,CancellationToken ct)=>Execute(()=>sales.GetAccountsAsync(branchId,ct));
    [HttpPost,Authorize(Policy="Permission:Branches.Manage"),Authorize(Policy="Permission:Branches.AccessAll")]
    public Task<IActionResult> Create(CreateFinancialAccountRequest r,CancellationToken ct)=>Execute(()=>sales.CreateAccountAsync(r,ct));
}
