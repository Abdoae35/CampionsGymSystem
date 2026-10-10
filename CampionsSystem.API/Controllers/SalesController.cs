using CampionsSystem.BLL.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CampionsSystem.API.Controllers;
[ApiController,Route("api/sales")]
public sealed class SalesController(SalesService sales):BranchControllerBase
{
    [HttpPost,Authorize(Policy="Permission:Payments.Create")]
    public Task<IActionResult> Create(CreateSaleRequest r,CancellationToken ct)=>Execute(()=>sales.CreateAsync(r,ct));
    [HttpGet,Authorize(Policy="Permission:Payments.View")]
    public Task<IActionResult> History([FromQuery]SaleHistoryRequest r,CancellationToken ct)=>Execute(()=>sales.GetHistoryAsync(r,ct));
    [HttpGet("{id:guid}"),Authorize(Policy="Permission:Payments.View")]
    public Task<IActionResult> Details(Guid id,CancellationToken ct)=>Execute(()=>sales.GetDetailsAsync(id,ct));
    [HttpPost("{id:guid}/payments"),Authorize(Policy="Permission:Payments.Create")]
    public Task<IActionResult> Pay(Guid id,RecordPaymentRequest r,CancellationToken ct)=>Execute(()=>sales.RecordPaymentAsync(id,r,ct));
    [HttpGet("{id:guid}/payments"),Authorize(Policy="Permission:Payments.View")]
    public Task<IActionResult> Payments(Guid id,[FromQuery]int page=1,[FromQuery]int pageSize=25,CancellationToken ct=default)=>Execute(()=>sales.GetPaymentsAsync(id,page,pageSize,ct));
    [HttpGet("{id:guid}/installments"),Authorize(Policy="Permission:Payments.View")]
    public Task<IActionResult> Installments(Guid id,CancellationToken ct)=>Execute(()=>sales.GetInstallmentsAsync(id,ct));
    [HttpPost("{id:guid}/refunds"),Authorize(Policy="Permission:Payments.Refund")]
    public Task<IActionResult> Refund(Guid id,RefundRequest r,CancellationToken ct)=>Execute(()=>sales.RefundAsync(id,r,ct));
    [HttpGet("{id:guid}/refunds"),Authorize(Policy="Permission:Payments.View")]
    public Task<IActionResult> Refunds(Guid id,[FromQuery]int page=1,[FromQuery]int pageSize=25,CancellationToken ct=default)=>Execute(()=>sales.GetRefundsAsync(id,page,pageSize,ct));
    [HttpPost("{id:guid}/quick-cancel"),Authorize(Policy="Permission:Sales.QuickCancel")]
    public Task<IActionResult> QuickCancel(Guid id,QuickCancelRequest r,CancellationToken ct)=>Execute(()=>sales.QuickCancelAsync(id,r,ct));
    [HttpGet("branch-totals"),Authorize(Policy="Permission:Payments.View")]
    public Task<IActionResult> Totals([FromQuery]Guid? branchId,[FromQuery]DateOnly? from,[FromQuery]DateOnly? to,CancellationToken ct)=>Execute(()=>sales.GetBranchTotalsAsync(branchId,from,to,ct));
}
