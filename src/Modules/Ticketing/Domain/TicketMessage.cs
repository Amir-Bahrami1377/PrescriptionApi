using Prescription.SharedKernel.Entities;

namespace Prescription.Modules.Ticketing.Domain;

public sealed class TicketMessage : BaseEntity
{
    private TicketMessage() { }

    public Guid TicketId { get; private set; }
    public Guid SenderId { get; private set; }
    public string SenderRole { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; private set; }

    internal static TicketMessage Create(
        Guid ticketId,
        Guid senderId,
        string senderRole,
        string body,
        DateTimeOffset? createdAtUtc = null)
    {
        return new TicketMessage
        {
            TicketId = ticketId,
            SenderId = senderId,
            SenderRole = senderRole,
            Body = body,
            CreatedAtUtc = createdAtUtc ?? DateTimeOffset.UtcNow,
        };
    }
}
