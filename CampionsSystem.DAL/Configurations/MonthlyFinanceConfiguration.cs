using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class FinancialMonthConfiguration : IEntityTypeConfiguration<FinancialMonth>
{
    public void Configure(EntityTypeBuilder<FinancialMonth> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Name).HasMaxLength(256).IsRequired();
        b.Property(x => x.Status).HasMaxLength(16).IsRequired();
        b.Property(x => x.Version).IsConcurrencyToken();
        b.HasIndex(x => new { x.BranchId, x.StartDate }).IsUnique();
        b.HasIndex(x => x.BranchId).IsUnique().HasFilter("\"Status\" = 'Open'");
        b.ToTable("FinancialMonths", t =>
        {
            t.HasCheckConstraint("CK_FinancialMonths_Bounds", "\"EndDate\" >= \"StartDate\" AND \"EndDate\" - \"StartDate\" < 62");
            t.HasCheckConstraint("CK_FinancialMonths_Status", "\"Status\" IN ('Open','Closed','Reopened') AND \"Version\" >= 0 AND \"LastRevision\" >= 0 AND (\"Status\" = 'Open' AND \"FirstClosedAt\" IS NULL OR \"Status\" <> 'Open' AND \"FirstClosedAt\" IS NOT NULL)");
        });
    }
}
public sealed class FinancialMonthEventConfiguration : IEntityTypeConfiguration<FinancialMonthEvent>
{
    public void Configure(EntityTypeBuilder<FinancialMonthEvent> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.FinancialMonth).WithMany().HasForeignKey(x => x.FinancialMonthId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ActorUser).WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.OperationId).IsUnique();
        b.Property(x => x.RequestHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.Kind).HasMaxLength(16).IsRequired();
        b.Property(x => x.Reason).HasMaxLength(2000).IsRequired();
        b.ToTable("FinancialMonthEvents", t => t.HasCheckConstraint("CK_FinancialMonthEvents_Kind", "\"Kind\" IN ('Opened','Closed','Reopened') AND \"ResultVersion\" >= 0"));
    }
}
public sealed class FinancialClosingRevisionConfiguration : IEntityTypeConfiguration<FinancialClosingRevision>
{
    public void Configure(EntityTypeBuilder<FinancialClosingRevision> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.FinancialMonth).WithMany().HasForeignKey(x => x.FinancialMonthId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ActorUser).WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.FinancialMonthId, x.Revision }).IsUnique();
        b.Property(x => x.ReportJson).HasColumnType("jsonb").IsRequired();
        b.Property(x => x.Workbook).IsRequired();
        b.Property(x => x.WorkbookSha256).HasMaxLength(64).IsRequired();
        b.Property(x => x.Reason).HasMaxLength(2000).IsRequired();
        b.ToTable("FinancialClosingRevisions", t => t.HasCheckConstraint("CK_FinancialClosingRevisions_Version", "\"Revision\" > 0 AND \"SchemaVersion\" > 0"));
    }
}
public sealed class PayrollPaymentRecordConfiguration : IEntityTypeConfiguration<PayrollPaymentRecord>
{
    public void Configure(EntityTypeBuilder<PayrollPaymentRecord> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Payroll).WithMany().HasForeignKey(x => x.PayrollId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.FinancialMonth).WithMany().HasForeignKey(x => x.FinancialMonthId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ActorUser).WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.OperationId).IsUnique();
        b.HasIndex(x => new { x.PayrollId, x.RecordedAt });
        b.Property(x => x.RequestHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.Reason).HasMaxLength(2000).IsRequired();
        b.Property(x => x.Reference).HasMaxLength(512);
        foreach (var n in new[] { "Amount", "BalanceBefore", "BalanceAfter" }) b.Property<decimal>(n).HasPrecision(18, 2);
        b.ToTable("PayrollPaymentRecords", t => t.HasCheckConstraint("CK_PayrollPaymentRecords_Balance", "\"Amount\" > 0 AND \"BalanceBefore\" >= \"Amount\" AND \"BalanceAfter\" = \"BalanceBefore\" - \"Amount\" AND \"PaidAt\" <= \"RecordedAt\""));
    }
}
public sealed class MaintenancePlanRevisionConfiguration : IEntityTypeConfiguration<MaintenancePlanRevision>
{
    public void Configure(EntityTypeBuilder<MaintenancePlanRevision> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ActorUser).WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.OperationId).IsUnique();
        b.HasIndex(x => new { x.PlanId, x.Revision }).IsUnique();
        b.Property(x => x.RequestHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.Name).HasMaxLength(256).IsRequired();
        b.Property(x => x.ExpectedCost).HasPrecision(18, 2);
        b.ToTable("MaintenancePlanRevisions", t => t.HasCheckConstraint("CK_MaintenancePlanRevisions_Values", "\"ExpectedCost\" > 0 AND \"FrequencyMonths\" BETWEEN 1 AND 120 AND \"Revision\" > 0"));
    }
}
public sealed class FinancialOccurrenceEvidenceConfiguration : IEntityTypeConfiguration<FinancialOccurrenceEvidence>
{
    public void Configure(EntityTypeBuilder<FinancialOccurrenceEvidence> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.FinancialTransaction).WithMany().HasForeignKey(x => x.FinancialTransactionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ActorUser).WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.FinancialTransactionId).IsUnique();
        b.HasIndex(x => x.OperationId).IsUnique();
        b.Property(x => x.RequestHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.Reason).HasMaxLength(2000).IsRequired();
        b.ToTable("FinancialOccurrenceEvidence", t => t.HasCheckConstraint("CK_FinancialOccurrenceEvidence_Date", "\"OriginalOccurredAt\" <= \"RecordedAt\""));
    }
}
