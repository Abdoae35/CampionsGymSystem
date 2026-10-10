using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class RefundPart
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RefundId { get; set; }
    public Refund Refund { get; set; } = null!;
    public Guid FinancialAccountId { get; set; }
    public FinancialAccount FinancialAccount { get; set; } = null!;
    public decimal Amount { get; set; }
    public string? ProviderReference { get; set; }
}
