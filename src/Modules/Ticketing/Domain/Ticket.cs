using Prescription.SharedKernel.Entities;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Ticketing.Domain;

public sealed class Ticket : AuditableEntity
{
    private readonly List<TicketMessage> _messages = [];

    private Ticket() { }

    public Guid CustomerId { get; private set; }
    public string Subject { get; private set; } = null!;
    public TicketStatus Status { get; private set; }
    public DateTimeOffset? ClosedAtUtc { get; private set; }

    public IReadOnlyCollection<TicketMessage> Messages => _messages.AsReadOnly();

    public static Ticket Create(Guid customerId, string subject, string initialMessage)
    {
        var ticket = new Ticket
        {
            CustomerId = customerId,
            Subject = subject,
            Status = TicketStatus.Open,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };

        ticket._messages.Add(TicketMessage.Create(ticket.Id, customerId, "Customer", initialMessage));

        return ticket;
    }

    public void AddReply(Guid senderId, string senderRole, string body)
    {
        if (Status != TicketStatus.Open)
        {
            throw new ConflictException("این تیکت بسته شده و امکان پاسخ‌دهی به آن وجود ندارد.");
        }

        _messages.Add(TicketMessage.Create(Id, senderId, senderRole, body));
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Close()
    {
        if (Status != TicketStatus.Open)
        {
            throw new ConflictException("این تیکت قبلاً بسته شده است.");
        }

        Status = TicketStatus.Closed;
        ClosedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
