using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class MemberOfferEntitlementConfiguration : IEntityTypeConfiguration<MemberOfferEntitlement>
{
    public void Configure(EntityTypeBuilder<MemberOfferEntitlement> builder)
    {
        builder.ToTable("MemberOfferEntitlements", table =>
        {
            table.HasCheckConstraint("CK_MemberOfferEntitlements_StartDateEndDate", "\"EndDate\" >= \"StartDate\"");
        });
        builder.HasOne(x => x.RevokedByUser).WithMany().HasForeignKey(x => x.RevokedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.OfferId, x.MemberId, x.RevokedAt });
        builder.ToTable("MemberOfferEntitlements", t => t.HasCheckConstraint("CK_MemberOfferEntitlements_Revocation", "(\"RevokedAt\" IS NULL AND \"RevokedByUserId\" IS NULL) OR (\"RevokedAt\" IS NOT NULL AND \"RevokedAt\" >= \"CreatedAt\" AND \"RevokedByUserId\" IS NOT NULL)"));
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Offer).WithMany().HasForeignKey(x => x.OfferId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.GrantedByEmployee).WithMany().HasForeignKey(x => x.GrantedByEmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.StartDate).HasColumnType("date");
        builder.Property(x => x.EndDate).HasColumnType("date");
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
