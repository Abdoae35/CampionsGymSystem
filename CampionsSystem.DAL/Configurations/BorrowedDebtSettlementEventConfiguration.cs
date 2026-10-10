using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CampionsSystem.DAL.Configurations;
public sealed class BorrowedDebtSettlementEventConfiguration : IEntityTypeConfiguration<BorrowedDebtSettlementEvent>
{
    public void Configure(EntityTypeBuilder<BorrowedDebtSettlementEvent> b)
    {
        b.ToTable("BorrowedDebtSettlementEvents",t=>t.HasCheckConstraint("CK_BorrowedDebtSettlementEvents_State","\"Sequence\" > 0 AND \"IsReversal\" = (\"RefundId\" IS NOT NULL)"));
        b.HasKey(x=>x.Id);b.HasIndex(x=>new{x.BorrowedGymSessionId,x.Sequence}).IsUnique();
        b.HasOne(x=>x.BorrowedGymSession).WithMany().HasForeignKey(x=>x.BorrowedGymSessionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x=>x.Grant).WithMany().HasForeignKey(x=>x.GrantId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x=>x.Refund).WithMany().HasForeignKey(x=>x.RefundId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x=>x.Actor).WithMany().HasForeignKey(x=>x.ActorId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x=>x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
