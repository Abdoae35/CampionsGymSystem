namespace CampionsSystem.DAL.Entities;

public sealed class CashDrawer
{
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public Guid FinancialAccountId { get; set; }
    public FinancialAccount FinancialAccount { get; set; } = null!;
    public long MovementSequence { get; set; }
    public Guid OperationId { get; set; }
    public string RequestHash { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
}
