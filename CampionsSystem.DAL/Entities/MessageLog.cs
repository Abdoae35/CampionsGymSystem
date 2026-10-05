using CampionsSystem.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampionsSystem.DAL.Entities;

public sealed class MessageLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? MemberId { get; set; }
    public Member? Member { get; set; }
    public MessageType MessageType { get; set; }
    public string? TemplateKey { get; set; }
    public string DestinationPhone { get; set; } = null!;
    public MessageStatus Status { get; set; }
    public string? ProviderMessageId { get; set; }
    public string? FailureReason { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
