using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampionsSystem.DAL.Configurations;

public sealed class MessageLogConfiguration : IEntityTypeConfiguration<MessageLog>
{
    public void Configure(EntityTypeBuilder<MessageLog> builder)
    {
        builder.ToTable("MessageLogs", table =>
        {
            table.HasCheckConstraint("CK_MessageLogs_MessageType", "\"MessageType\" IN ('WhatsApp', 'SMS', 'OTP')");
            table.HasCheckConstraint("CK_MessageLogs_Status", "\"Status\" IN ('Pending', 'Sent', 'Failed')");
        });
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.MessageType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.TemplateKey).HasMaxLength(128);
        builder.Property(x => x.DestinationPhone).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.ProviderMessageId).HasMaxLength(256);
        builder.Property(x => x.FailureReason).HasColumnType("text");
        builder.Property(x => x.SentAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
