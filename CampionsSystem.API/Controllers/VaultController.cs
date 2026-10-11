using CampionsSystem.BLL.Shifts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CampionsSystem.API.Controllers;
[ApiController,Route("api/vault")]
public sealed class VaultController(VaultService vault,ShiftReportService reports):BranchControllerBase
{
    [HttpPost("setup"),Authorize(Policy="Permission:Vault.Manage")]
    public Task<IActionResult> Setup(SetupDrawerRequest r,CancellationToken ct)=>Execute(()=>vault.SetupAsync(r,ct));
    [HttpGet("position"),Authorize(Policy="Permission:Vault.View")]
    public Task<IActionResult> Position([FromQuery]Guid? branchId,CancellationToken ct)=>Execute(()=>vault.PositionAsync(branchId,ct));
    [HttpPost("counts"),Authorize(Policy="Permission:Vault.Manage")]
    public Task<IActionResult> Count(CountCashRequest r,CancellationToken ct)=>Execute(()=>vault.CountAsync(r,ct));
    [HttpGet("counts"),Authorize(Policy="Permission:Vault.View")]
    public Task<IActionResult> Counts([FromQuery]VaultHistoryRequest r,CancellationToken ct)=>Execute(()=>vault.CountsAsync(r,ct));
    [HttpGet("movements"),Authorize(Policy="Permission:Vault.View")]
    public Task<IActionResult> Movements([FromQuery]VaultHistoryRequest r,CancellationToken ct)=>Execute(()=>vault.MovementsAsync(r,ct));
    [HttpPost("withdrawals"),Authorize(Policy="Permission:Vault.Withdraw")]
    public Task<IActionResult> Withdraw(WithdrawalRequest r,CancellationToken ct)=>Execute(()=>vault.WithdrawAsync(r,ct));
    [HttpGet("summary"),Authorize(Policy="Permission:Vault.View")]
    public Task<IActionResult> Summary([FromQuery]Guid? branchId,[FromQuery]DateOnly? from,[FromQuery]DateOnly? to,CancellationToken ct)=>Execute(()=>reports.BranchAsync(branchId,from,to,ct));
    [HttpGet("audit"),Authorize(Policy="Permission:Vault.View")]
    public Task<IActionResult> Audit([FromQuery]Guid? branchId,[FromQuery]Guid? entityId,[FromQuery]int page=1,[FromQuery]int pageSize=25,CancellationToken ct=default)=>Execute(()=>reports.AuditAsync(branchId,entityId,page,pageSize,ct));
}
