namespace CampionsSystem.DAL.Entities;
public sealed class InstallmentPaymentAllocation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;
    public Guid InstallmentId { get; set; }
    public Installment Installment { get; set; } = null!;
    public decimal Amount { get; set; }
}
