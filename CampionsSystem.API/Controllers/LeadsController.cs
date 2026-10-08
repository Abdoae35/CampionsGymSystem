using System.Text.Json;
using CampionsSystem.BLL.Crm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CampionsSystem.API.Controllers;
[ApiController,Route("api/leads")]
public sealed class LeadsController(LeadManagementService leads) : BranchControllerBase
{
    [HttpGet,Authorize(Policy="Permission:Leads.View")]
    public Task<IActionResult> List([FromQuery] LeadListRequest r,CancellationToken ct)=>Execute(()=>leads.GetLeadsAsync(r,ct));
    [HttpGet("my"),Authorize(Policy="Permission:Leads.View")]
    public Task<IActionResult> Mine([FromQuery] LeadListRequest r,CancellationToken ct)=>Execute(()=>leads.GetMyLeadsAsync(r,ct));
    [HttpGet("lookup"),Authorize(Policy="Permission:Leads.View")]
    public Task<IActionResult> Lookup([FromQuery] Guid branchId,[FromQuery] string phone,CancellationToken ct)=>Execute(()=>leads.LookupPhoneAsync(branchId,phone,ct));
    [HttpGet("{id:guid}"),Authorize(Policy="Permission:Leads.View")]
    public Task<IActionResult> Get(Guid id,CancellationToken ct)=>Execute(()=>leads.GetLeadByIdAsync(id,ct));
    [HttpPost,Authorize(Policy="Permission:Leads.Manage")]
    public Task<IActionResult> Create(CreateLeadRequest r,CancellationToken ct)=>Execute(()=>leads.CreateLeadAsync(r,ct));
    [HttpPost("bulk"),Authorize(Policy="Permission:Leads.Assign")]
    public Task<IActionResult> Bulk([FromBody] JsonElement[] rows,CancellationToken ct)=>Execute(()=>leads.CreateLeadsBulkAsync(rows,ct));
    [HttpPut("{id:guid}"),Authorize(Policy="Permission:Leads.Manage")]
    public Task<IActionResult> Update(Guid id,UpdateLeadRequest r,CancellationToken ct)=>Execute(()=>leads.UpdateLeadAsync(id,r,ct));
    [HttpPost("{id:guid}/assign"),Authorize(Policy="Permission:Leads.Assign")]
    public Task<IActionResult> Assign(Guid id,LeadAssignmentRequest r,CancellationToken ct)=>Execute(()=>leads.AssignLeadAsync(id,r,ct));
    [HttpGet("{id:guid}/assignment"),Authorize(Policy="Permission:Leads.View")]
    public Task<IActionResult> Assignment(Guid id,CancellationToken ct)=>Execute(()=>leads.GetCurrentLeadAssignmentAsync(id,ct));
    [HttpGet("{id:guid}/assignment-history"),Authorize(Policy="Permission:Leads.View")]
    public Task<IActionResult> AssignmentHistory(Guid id,CancellationToken ct)=>Execute(()=>leads.GetLeadAssignmentHistoryAsync(id,ct));
    [HttpPost("{id:guid}/interactions"),Authorize(Policy="Permission:Leads.Manage")]
    public Task<IActionResult> Contact(Guid id,LeadInteractionRequest r,CancellationToken ct)=>Execute(()=>leads.AddLeadInteractionAsync(id,r,ct));
    [HttpGet("{id:guid}/interactions"),Authorize(Policy="Permission:Leads.View")]
    public Task<IActionResult> Contacts(Guid id,[FromQuery] int page=1,[FromQuery] int pageSize=25,CancellationToken ct=default)=>Execute(()=>leads.GetLeadInteractionsAsync(id,page,pageSize,ct));
    [HttpPatch("{id:guid}/status"),Authorize(Policy="Permission:Leads.Manage")]
    public Task<IActionResult> Status(Guid id,LeadStatusRequest r,CancellationToken ct)=>Execute(()=>leads.ChangeLeadStatusAsync(id,r,ct));
    [HttpPost("{id:guid}/convert"),Authorize(Policy="Permission:Leads.Convert")]
    public Task<IActionResult> Convert(Guid id,ConvertLeadRequest r,CancellationToken ct)=>Execute(()=>leads.ConvertLeadAsync(id,r,ct));
}
