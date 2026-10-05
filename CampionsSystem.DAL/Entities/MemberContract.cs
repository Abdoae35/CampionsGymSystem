using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class MemberContract
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public Guid SaleTransactionId { get; set; }
    public SaleTransaction SaleTransaction { get; set; } = null!;
    public Guid ContractTemplateId { get; set; }
    public ContractTemplate ContractTemplate { get; set; } = null!;
    public int TemplateVersion { get; set; }
    public string TemplateSnapshotText { get; set; } = null!;
    public DateTimeOffset GeneratedAt { get; set; }
    public Guid GeneratedByUserId { get; set; }
    public ApplicationUser GeneratedByUser { get; set; } = null!;
    public string? PhysicalArchiveReference { get; set; }
}
