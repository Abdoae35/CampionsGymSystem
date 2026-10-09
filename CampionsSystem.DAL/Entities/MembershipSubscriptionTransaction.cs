using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class MembershipSubscriptionTransaction
{
    // Null snapshots identify legacy history requiring explicit reconciliation; never infer from today's plan.
    public Guid? OperationId { get; set; }
    public Guid? RenewedFromSubscriptionId { get; set; }
    public MemberSubscription? RenewedFromSubscription { get; set; }
    public string? PlanNameSnapshot { get; set; }
    public Guid? PlanBranchIdSnapshot { get; set; }
    public GymBranch? PlanBranchSnapshot { get; set; }
    public Guid? CityIdSnapshot { get; set; }
    public City? CitySnapshot { get; set; }
    public AccessMode? AccessModeSnapshot { get; set; }
    public int? FreezeDaysSnapshot { get; set; }
    public Guid? BusinessDayId { get; set; }
    public BusinessDay? BusinessDay { get; set; }
    public DateOnly? StartDateSnapshot { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }
    public int? PreviousRemainingSessions { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MemberSubscriptionId { get; set; }
    public MemberSubscription MemberSubscription { get; set; } = null!;
    public Guid? SaleTransactionId { get; set; }
    public SaleTransaction? SaleTransaction { get; set; } = null!;
    public Guid MembershipPlanId { get; set; }
    public MembershipPlan MembershipPlan { get; set; } = null!;
    public SubscriptionTransactionType TransactionType { get; set; }
    public int PurchasedDurationDays { get; set; }
    public int? PurchasedSessions { get; set; }
    public PlanCategory PlanCategorySnapshot { get; set; }
    public Guid? FreelanceCoachEmployeeIdSnapshot { get; set; }
    public Employee? FreelanceCoachEmployeeSnapshot { get; set; }
    public decimal PlanPriceSnapshot { get; set; }
    public decimal NetPriceSnapshot { get; set; }
    public DateOnly? PreviousEndDate { get; set; }
    public DateOnly? NewEndDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
