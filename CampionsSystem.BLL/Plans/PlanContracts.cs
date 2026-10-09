using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CampionsSystem.DAL.Enums;
namespace CampionsSystem.BLL.Plans;

public class CatalogListRequest
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
public sealed class MembershipPlanListRequest : CatalogListRequest
{
    public Guid? BranchId { get; set; }
    public Guid? CityId { get; set; }
    public PlanCategory? PlanType { get; set; }
    public Guid? AudienceCategoryId { get; set; }
    public AccessMode? Mode { get; set; }
    public Guid? FreelanceCoachEmployeeId { get; set; }
    // Employee products require explicit administrative opt-in and MembershipPlans.Manage.
    public bool IncludeInternal { get; set; }
}
public sealed record PlanReference(Guid Id, string Name);
public sealed record CatalogDetails(Guid Id, string? Code, string Name, string? Description, bool IsActive, DateTimeOffset CreatedAt);
public sealed record CatalogListResult(IReadOnlyList<CatalogDetails> Items, int TotalCount, int Page, int PageSize);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SaveCategoryRequest
{
    [Required,MaxLength(256)] public string Name { get; set; } = "";
    [MaxLength(4096)] public string? Description { get; set; }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SaveBenefitRequest
{
    [Required,MaxLength(128)] public string Code { get; set; } = "";
    [Required,MaxLength(256)] public string Name { get; set; } = "";
    [MaxLength(4096)] public string? Description { get; set; }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ChangePlanStatusRequest
{
    [Required] public bool? IsActive { get; set; }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class UpdateMembershipPlanRequest
{
    public Guid BranchId { get; set; }
    public Guid CityId { get; set; }
    [Required,MaxLength(256)] public string Name { get; set; } = "";
    [Required,JsonConverter(typeof(JsonStringEnumConverter))] public PlanCategory? PlanType { get; set; }
    public Guid? AudienceCategoryId { get; set; }
    [Required,JsonConverter(typeof(JsonStringEnumConverter))] public AccessMode? Mode { get; set; }
    public int DurationDays { get; set; }
    public int? Sessions { get; set; }
    public decimal Price { get; set; }
    public bool AllowFreeze { get; set; }
    public int FreezeDays { get; set; }
    public Guid? FreelanceCoachEmployeeId { get; set; }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CreateMembershipPlanRequest : UpdateMembershipPlanRequest;
public sealed record MembershipPlanSummary(Guid Id, string Name, string PlanType, PlanReference? Branch,
    PlanReference City, PlanReference? AudienceCategory, string Mode, decimal Price, int DurationDays,
    int? Sessions, bool AllowFreeze, int FreezeDays, bool IsActive, PlanReference? FreelanceCoach)
{
    public bool CustomerPurchasable => PlanType != nameof(PlanCategory.Employee) && Branch is not null && IsActive;
}
public sealed record PlanBenefitDetails(Guid Id, Guid BenefitId, string Code, string Name, bool IsActive, string GrantType, int? Quantity);
public sealed record MembershipPlanDetails(MembershipPlanSummary Plan, DateTimeOffset CreatedAt,
    IReadOnlyList<PlanBenefitDetails> Benefits, int EligibleMemberCount);
public sealed record MembershipPlanListResult(IReadOnlyList<MembershipPlanSummary> Items, int TotalCount, int Page, int PageSize);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class PlanBenefitRequest
{
    public Guid BenefitId { get; set; }
    [Required,JsonConverter(typeof(JsonStringEnumConverter))] public MembershipBenefitGrantType? GrantType { get; set; }
    public int? Quantity { get; set; }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SetPlanBenefitsRequest
{
    public List<PlanBenefitRequest> Benefits { get; set; } = [];
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EligibleMemberRequest
{
    public Guid MemberId { get; set; }
}
public sealed record EligibleMemberDetails(Guid MemberId, string FullName, string PrimaryPhone, Guid RegistrationBranchId,
    long? ClientCode, string MemberCode, DateTimeOffset? AddedAt);
public sealed record EligibleMemberListResult(IReadOnlyList<EligibleMemberDetails> Items, int TotalCount, int Page, int PageSize);
