using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class CheckInConfiguration : IEntityTypeConfiguration<CheckIn>
{
    public void Configure(EntityTypeBuilder<CheckIn> builder)
    {
        builder.ToTable("CheckIns", table =>
        {
            table.HasCheckConstraint("CK_CheckIns_SubscriptionType", "(\"CheckInType\" IN ('Gym', 'BorrowedGym') AND \"MemberSubscriptionId\" IS NOT NULL AND \"ActivitySubscriptionId\" IS NULL AND \"GroupClassId\" IS NULL) OR (\"CheckInType\" = 'Activity' AND \"ActivitySubscriptionId\" IS NOT NULL AND \"MemberSubscriptionId\" IS NULL AND \"GroupClassId\" IS NULL) OR (\"CheckInType\" = 'Class' AND \"GroupClassId\" IS NOT NULL AND \"MemberSubscriptionId\" IS NULL AND \"ActivitySubscriptionId\" IS NULL) OR (\"CheckInType\" = 'Trial' AND \"MemberSubscriptionId\" IS NULL AND \"ActivitySubscriptionId\" IS NULL AND \"GroupClassId\" IS NULL)");
            table.HasCheckConstraint("CK_CheckIns_CheckInType", "\"CheckInType\" IN ('Gym', 'BorrowedGym', 'Activity', 'Class', 'Trial')");
            table.HasCheckConstraint("CK_CheckIns_Member", "\"CheckInType\" = 'Trial' OR \"MemberId\" IS NOT NULL");
            table.HasCheckConstraint("CK_CheckIns_CheckInMethod", "\"CheckInMethod\" IN ('Scanner', 'Manual', 'Coach')");
        });
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.OperationId).IsUnique();
        builder.HasIndex(x => new { x.BranchId, x.BusinessDate, x.MemberId });
        builder.ToTable("CheckIns", t =>
        {
            t.HasCheckConstraint("CK_CheckIns_OperationalDate", "\"OperationId\" IS NULL OR \"BusinessDate\" IS NOT NULL");
            t.HasCheckConstraint("CK_CheckIns_SessionSnapshot", "(\"SessionsBefore\" IS NULL AND \"SessionsAfter\" IS NULL) OR (\"SessionsBefore\" IS NOT NULL AND \"SessionsAfter\" IS NOT NULL AND \"SessionsBefore\" > 0 AND \"SessionsAfter\" = \"SessionsBefore\" - 1 AND \"CheckInType\" = 'Gym' AND \"SessionConsumed\")");
        });
        builder.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CheckInType).HasConversion<string>().HasMaxLength(40);
        builder.HasOne(x => x.MemberSubscription).WithMany().HasForeignKey(x => x.MemberSubscriptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ActivitySubscription).WithMany().HasForeignKey(x => x.ActivitySubscriptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.GroupClass).WithMany().HasForeignKey(x => x.GroupClassId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CoachEmployee).WithMany().HasForeignKey(x => x.CoachEmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CheckInMethod).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.CheckedInAt).HasColumnType("timestamp with time zone");
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.Notes).HasColumnType("text");
        builder.HasIndex(x => new { x.MemberId, x.CheckedInAt });
        builder.HasIndex(x => new { x.BranchId, x.CheckedInAt });
        builder.HasOne(x => x.ClassSession).WithMany().HasForeignKey(x => x.ClassSessionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.ClassSessionId, x.MemberId }).IsUnique().HasFilter("\"ClassSessionId\" IS NOT NULL");
        builder.ToTable("CheckIns", t => t.HasCheckConstraint("CK_CheckIns_ClassSession", "\"ClassSessionId\" IS NULL OR \"CheckInType\" = 'Class'"));
        builder.HasOne(x => x.BusinessDay).WithMany().HasForeignKey(x => x.BusinessDayId).OnDelete(DeleteBehavior.Restrict);
    }
}
