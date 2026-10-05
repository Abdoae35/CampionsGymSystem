using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class BorrowedGymSessionConfiguration : IEntityTypeConfiguration<BorrowedGymSession>
{
    public void Configure(EntityTypeBuilder<BorrowedGymSession> builder)
    {
        builder.ToTable("BorrowedGymSessions", table =>
        {
            table.HasCheckConstraint("CK_BorrowedGymSessions_Status", "\"Status\" IN ('Outstanding', 'Settled')");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ExpiredSubscription).WithMany().HasForeignKey(x => x.ExpiredSubscriptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CheckIn).WithMany().HasForeignKey(x => x.CheckInId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.BorrowedAt).HasColumnType("timestamp with time zone");
        builder.HasOne(x => x.SettledBySubscription).WithMany().HasForeignKey(x => x.SettledBySubscriptionId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.SettledAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
    }
}
