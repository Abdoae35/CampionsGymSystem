using CampionsSystem.DAL.Enums;
namespace CampionsSystem.DAL.Entities;
// Immutable sold entitlement terms, including reservations awaiting payment/activation.
public sealed class SaleMembershipSnapshot
{
    public Guid SaleTransactionId { get; set; }
    public SaleTransaction SaleTransaction { get; set; } = null!;
    public Guid MembershipPlanId { get; set; }
    public MembershipPlan MembershipPlan { get; set; } = null!;
    public string PlanName { get; set; } = null!;
    public PlanCategory PlanCategory { get; set; }
    public Guid CityId { get; set; }
    public City City { get; set; } = null!;
    public Guid? FreelanceCoachEmployeeId { get; set; }
    public Employee? FreelanceCoachEmployee { get; set; }
    public AccessMode AccessMode { get; set; }
    public int DurationDays { get; set; }
    public int? SessionCount { get; set; }
    public int FreezeDays { get; set; }
    public ICollection<SaleBenefitSnapshot> Benefits { get; set; } = new List<SaleBenefitSnapshot>();
}
