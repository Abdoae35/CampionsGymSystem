using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class PrivateCoachAssignmentConfiguration : IEntityTypeConfiguration<PrivateCoachAssignment>
{
    public void Configure(EntityTypeBuilder<PrivateCoachAssignment> builder)
    {
        builder.ToTable("PrivateCoachAssignments", table =>
        {
            table.HasCheckConstraint("CK_PrivateCoachAssignments_SessionsAllocated", "\"SessionsAllocated\" >= 0");
            table.HasCheckConstraint("CK_PrivateCoachAssignments_SessionsConsumed", "\"SessionsConsumed\" >= 0");
            table.HasCheckConstraint("CK_PrivateCoachAssignments_CommissionBasisAmount", "\"CommissionBasisAmount\" >= 0");
            table.HasCheckConstraint("CK_PrivateCoachAssignments_AssignedAtEndedAt", "\"EndedAt\" >= \"AssignedAt\"");
            table.HasCheckConstraint("CK_PrivateCoachAssignments_SessionBounds", "\"SessionsConsumed\" <= \"SessionsAllocated\"");
        });
        builder.HasIndex(x => new { x.PrivateSubscriptionId, x.EndedAt });
        // Preserve legacy values until explicitly reconciled; new history rows use zero.
        builder.Property<int>("SessionsAllocated").HasDefaultValue(0);
        builder.Property<decimal>("CommissionBasisAmount").HasDefaultValue(0m);
        builder.HasIndex(x => x.OperationId).IsUnique();
        builder.Property(x => x.RequestHash).HasMaxLength(64);
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.PrivateSubscription).WithMany(x => x.CoachAssignments).HasForeignKey(x => x.PrivateSubscriptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CoachEmployee).WithMany().HasForeignKey(x => x.CoachEmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.AssignedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.EndedAt).HasColumnType("timestamp with time zone");
        builder.Property<decimal>("CommissionBasisAmount").HasPrecision(18, 2);
        builder.Property(x => x.TransferReason).HasColumnType("text");
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => x.PrivateSubscriptionId).IsUnique().HasFilter("\"EndedAt\" IS NULL");
        builder.Property<int>("SessionsConsumed").HasDefaultValue(0);
    }
}
