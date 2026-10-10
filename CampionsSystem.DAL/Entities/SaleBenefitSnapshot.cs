using CampionsSystem.DAL.Enums;
namespace CampionsSystem.DAL.Entities;
public sealed class SaleBenefitSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SaleTransactionId { get; set; }
    public SaleMembershipSnapshot SaleMembershipSnapshot { get; set; } = null!;
    public Guid MembershipBenefitId { get; set; }
    public MembershipBenefit MembershipBenefit { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public MembershipBenefitGrantType GrantType { get; set; }
    public int? Quantity { get; set; }
}
