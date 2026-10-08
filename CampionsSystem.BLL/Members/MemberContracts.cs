using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CampionsSystem.BLL.Authentication;

namespace CampionsSystem.BLL.Members;

public sealed class MemberListRequest
{
    [MaxLength(256)] public string? Search { get; set; }
    public Guid? RegistrationBranchId { get; set; }
    public long? ClientCode { get; set; }
    public bool? IsActive { get; set; }
    public Guid? SalesEmployeeId { get; set; }
    public bool? HouseOnly { get; set; }
    [Range(1, int.MaxValue)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}
public sealed record SalesAssignmentDetails(Guid Id, Guid SalesEmployeeId, string SalesEmployeeName,
    DateTimeOffset AssignedAt, DateTimeOffset? EndedAt, string? Reason);
public sealed record SalesAssignmentHistoryItem(Guid Id, Guid? SalesEmployeeId, string? SalesEmployeeName,
    string AssignmentType, DateTimeOffset AssignedAt, DateTimeOffset? EndedAt, string? Reason);
public sealed record MemberSummary(Guid Id, long? ClientCode, string MemberCode, string FullName,
    string PrimaryPhone, bool IsActive, BranchContext RegistrationBranch, SalesAssignmentDetails? CurrentSalesOwner)
{
    public bool IsHouse => CurrentSalesOwner is null;
}
public sealed record EligibleFreelancePlan(Guid PlanId, string PlanName, string CoachName);
public sealed record MemberEmployeeLink(Guid Id, string FullName);
public sealed record MemberDetails(Guid Id, long? ClientCode, string MemberCode, string FullName,
    string PrimaryPhone, string? PrimaryPhoneNormalized, string? SecondaryPhone, bool IsPrimaryPhoneVerified,
    DateOnly? DateOfBirth, string? Gender, string? Notes, bool IsActive, BranchContext RegistrationBranch,
    DateTimeOffset CreatedAt, SalesAssignmentDetails? CurrentSalesOwner, MemberEmployeeLink? Employee,
    IReadOnlyList<EligibleFreelancePlan> EligibleFreelancePlans)
{
    public bool IsHouse => CurrentSalesOwner is null;
}
public sealed record MemberListResult(IReadOnlyList<MemberSummary> Items, int TotalCount, int Page, int PageSize);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class UpdateMemberRequest
{
    [Required, MaxLength(256)] public string FullName { get; set; } = string.Empty;
    [Required, MaxLength(64)] public string PrimaryPhone { get; set; } = string.Empty;
    [MaxLength(64)] public string? SecondaryPhone { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    [MaxLength(32)] public string? Gender { get; set; }
    [MaxLength(4096)] public string? Notes { get; set; }
}
[JsonConverter(typeof(JsonStringEnumConverter<MemberCreationType>))]
public enum MemberCreationType { NormalSales, FreelanceHouse, ManagementCreated }

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CreateMemberRequest : UpdateMemberRequest
{
    public Guid TargetRegistrationBranchId { get; set; }
    [Required] public MemberCreationType? CreationType { get; set; }
    // ManagementCreated only, requires Members.AssignSales. NormalSales resolves the actor.
    public Guid? SalesEmployeeId { get; set; }
    // FreelanceHouse only; eligibility requires FreelancePlans.ManageEligibleMembers.
    public Guid? FreelancePlanId { get; set; }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ChangeMemberStatusRequest
{
    [Required] public bool? IsActive { get; set; }
    [MaxLength(2048)] public string? Reason { get; set; }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SalesAssignmentRequest
{
    public Guid SalesEmployeeId { get; set; }
    [MaxLength(2048)] public string? Reason { get; set; }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ReturnToHouseRequest
{
    [MaxLength(2048)] public string? Reason { get; set; }
}
