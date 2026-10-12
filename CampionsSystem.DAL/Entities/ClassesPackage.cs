namespace CampionsSystem.DAL.Entities;

public sealed class ClassesPackage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int SessionCount { get; set; }
    public int DurationDays { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
}

// One purchase, one shared balance usable at every class type in its branch.
public sealed class ClassesSubscription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public Guid BranchId { get; set; }
    public GymBranch Branch { get; set; } = null!;
    public Guid ClassesPackageId { get; set; }
    public ClassesPackage ClassesPackage { get; set; } = null!;
    public Guid SaleTransactionId { get; set; }
    public SaleTransaction SaleTransaction { get; set; } = null!;
    public string PackageName { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int PurchasedSessions { get; set; }
    public int RemainingSessions { get; set; }
    public decimal Price { get; set; }
    public decimal UnitSessionValue { get; set; }
    public decimal RemainderValue { get; set; }
    public int Version { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
