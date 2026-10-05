using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class OfferMembershipPlan
{
    public Guid OfferId { get; set; }
    public Offer Offer { get; set; } = null!;
    public Guid MembershipPlanId { get; set; }
    public MembershipPlan MembershipPlan { get; set; } = null!;
}
