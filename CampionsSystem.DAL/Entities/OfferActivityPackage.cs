using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class OfferActivityPackage
{
    public Guid OfferId { get; set; }
    public Offer Offer { get; set; } = null!;
    public Guid ActivityPackageId { get; set; }
    public ActivityPackage ActivityPackage { get; set; } = null!;
}
