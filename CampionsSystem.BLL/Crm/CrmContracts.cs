using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CampionsSystem.DAL.Enums;
using CampionsSystem.BLL.Members;
namespace CampionsSystem.BLL.Crm;

[JsonConverter(typeof(JsonStringEnumConverter<FollowUpWindow>))]
public enum FollowUpWindow { Overdue, Today, Upcoming }
public sealed class LeadListRequest
{
    [MaxLength(256)] public string? Search { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? SalesEmployeeId { get; set; }
    public bool? UnassignedOnly { get; set; }
    public LeadStatus? Status { get; set; }
    public FollowUpWindow? FollowUp { get; set; }
    [Range(1,int.MaxValue)] public int Page { get; set; } = 1;
    [Range(1,100)] public int PageSize { get; set; } = 25;
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CreateLeadRequest
{
    public Guid BranchId { get; set; }
    [MaxLength(256)] public string? FullName { get; set; }
    [Required,MaxLength(64)] public string PrimaryPhone { get; set; } = "";
    [MaxLength(64)] public string? SecondaryPhone { get; set; }
    [EmailAddress,MaxLength(320)] public string? Email { get; set; }
    [MaxLength(256)] public string? Source { get; set; }
    [MaxLength(4096)] public string? Notes { get; set; }
}
public sealed record LeadAssignmentDetails(Guid Id, Guid SalesEmployeeId, string SalesEmployeeName,
    DateTimeOffset AssignedAt, DateTimeOffset? EndedAt, Guid AssignedByUserId, string? Reason);
public sealed record LeadDetails(Guid Id, Guid BranchId, string BranchName, string? FullName,
    string PrimaryPhone, string? PrimaryPhoneNormalized, string? SecondaryPhone, string? Email,
    string? Source, string? Notes, string Status, DateTimeOffset CreatedAt,
    DateTimeOffset? NextFollowUpAt, LeadAssignmentDetails? CurrentAssignment,
    Guid? ConvertedMemberId, DateTimeOffset? ConvertedAt, DateTimeOffset? LostAt, string? LostReason);
public sealed record LeadListResult(IReadOnlyList<LeadDetails> Items, int TotalCount, int Page, int PageSize);
// Duplicate IDs/details are returned only when independently visible to the caller.
public sealed record LeadCreationResult(string Result, LeadDetails? Lead = null, Guid? ExistingMemberId = null, string? Error = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class LeadAssignmentRequest
{
    public Guid SalesEmployeeId { get; set; }
    [MaxLength(2048)] public string? Reason { get; set; }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class LeadInteractionRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter<InteractionType>))]
    public InteractionType InteractionType { get; set; }
    [Required,MaxLength(4096)] public string Notes { get; set; } = "";
    [MaxLength(256)] public string? Outcome { get; set; }
    public DateTimeOffset? ContactedAt { get; set; }
    public DateTimeOffset? NextFollowUpAt { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter<LeadStatus>))]
    public LeadStatus Status { get; set; } = LeadStatus.Contacted;
}
public sealed record LeadInteractionDetails(Guid Id, Guid EmployeeId, Guid? CreatedByUserId,
    string InteractionType, string Notes, string? Outcome, DateTimeOffset ContactedAt,
    DateTimeOffset? NextFollowUpAt, DateTimeOffset CreatedAt);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class LeadStatusRequest
{
    [Required, JsonConverter(typeof(JsonStringEnumConverter<LeadStatus>))]
    public LeadStatus? Status { get; set; }
    [MaxLength(2048)] public string? Reason { get; set; }
    public DateTimeOffset? NextFollowUpAt { get; set; }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ConvertLeadRequest : UpdateMemberRequest
{
    public Guid TargetRegistrationBranchId { get; set; }
    // Explicit exception; never inferred from missing owner.
    public Guid? FreelancePlanId { get; set; }
}
public sealed class FollowUpListRequest
{
    public FollowUpWindow? Window { get; set; }
    public Guid? BranchId { get; set; }
    [Range(1,int.MaxValue)] public int Page { get; set; } = 1;
    [Range(1,100)] public int PageSize { get; set; } = 25;
}
public sealed record FollowUpItem(Guid Id, string SubjectType, Guid? LeadId, Guid? MemberId,
    string Name, string Phone, Guid BranchId, string BranchName, string Reason, DateTimeOffset DueAt,
    bool CrossBranch);
public sealed record FollowUpListResult(IReadOnlyList<FollowUpItem> Items, int TotalCount, int Page, int PageSize);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class MemberFollowUpRequest
{
    [Required] public string Reason { get; set; } = "Reactivation";
    public DateTimeOffset DueAt { get; set; }
    [MaxLength(4096)] public string? Notes { get; set; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class UpdateLeadRequest
{
    [MaxLength(256)] public string? FullName { get; set; }
    [Required,MaxLength(64)] public string PrimaryPhone { get; set; } = "";
    [MaxLength(64)] public string? SecondaryPhone { get; set; }
    [EmailAddress,MaxLength(320)] public string? Email { get; set; }
    [MaxLength(256)] public string? Source { get; set; }
    [MaxLength(4096)] public string? Notes { get; set; }
}
