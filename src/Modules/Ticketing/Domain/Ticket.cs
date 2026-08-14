using Prescription.SharedKernel.Entities;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Ticketing.Domain;

public sealed class Ticket : AuditableEntity
{
    public static readonly TimeSpan ClosureGracePeriod = TimeSpan.FromHours(6);

    private readonly List<TicketMessage> _messages = [];

    private Ticket() { }

    public Guid CustomerId { get; private set; }
    public string Subject { get; private set; } = null!;
    public TicketStatus Status { get; private set; }
    public DateTimeOffset? QueuedForClosureAtUtc { get; private set; }
    public DateTimeOffset? AutoCloseAtUtc { get; private set; }
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

    public TicketMessage AddReply(Guid senderId, string senderRole, string body, DateTimeOffset? repliedAtUtc = null)
    {
        if (Status == TicketStatus.Closed)
        {
            throw new ConflictException("این تیکت بسته شده است. پیش از پاسخ، آن را دوباره باز کنید.");
        }

        var now = repliedAtUtc ?? DateTimeOffset.UtcNow;
        var message = TicketMessage.Create(Id, senderId, senderRole, body, now);
        _messages.Add(message);

        // A customer response means the conversation is active again and cancels the administrator's
        // six-hour close countdown. An administrator may still add context while the countdown runs.
        if (senderRole == "Customer" && Status == TicketStatus.PendingClosure)
        {
            Status = TicketStatus.Open;
            QueuedForClosureAtUtc = null;
            AutoCloseAtUtc = null;
        }

        UpdatedAtUtc = now;
        return message;
    }

    public void CloseByCustomer(DateTimeOffset? closedAtUtc = null)
    {
        if (Status == TicketStatus.Closed)
        {
            throw new ConflictException("این تیکت قبلاً بسته شده است.");
        }

        var now = closedAtUtc ?? DateTimeOffset.UtcNow;
        Status = TicketStatus.Closed;
        QueuedForClosureAtUtc = null;
        AutoCloseAtUtc = null;
        ClosedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public void Reopen(DateTimeOffset? reopenedAtUtc = null)
    {
        if (Status != TicketStatus.Closed)
        {
            throw new ConflictException("فقط تیکت بسته‌شده را می‌توان دوباره باز کرد.");
        }

        var now = reopenedAtUtc ?? DateTimeOffset.UtcNow;
        Status = TicketStatus.Open;
        QueuedForClosureAtUtc = null;
        AutoCloseAtUtc = null;
        ClosedAtUtc = null;
        UpdatedAtUtc = now;
    }

    public void QueueForClosure(DateTimeOffset? queuedAtUtc = null)
    {
        if (Status == TicketStatus.Closed)
        {
            throw new ConflictException("تیکت بسته‌شده را نمی‌توان دوباره در صف بستن قرار داد.");
        }

        if (Status == TicketStatus.PendingClosure)
        {
            throw new ConflictException("این تیکت هم‌اکنون در صف بستن است.");
        }

        var now = queuedAtUtc ?? DateTimeOffset.UtcNow;
        Status = TicketStatus.PendingClosure;
        QueuedForClosureAtUtc = now;
        AutoCloseAtUtc = now.Add(ClosureGracePeriod);
        UpdatedAtUtc = now;
    }

    public bool CloseIfClosureDeadlinePassed(DateTimeOffset now)
    {
        if (Status != TicketStatus.PendingClosure || AutoCloseAtUtc is null || AutoCloseAtUtc > now)
        {
            return false;
        }

        Status = TicketStatus.Closed;
        ClosedAtUtc = now;
        UpdatedAtUtc = now;
        QueuedForClosureAtUtc = null;
        AutoCloseAtUtc = null;
        return true;
    }
}
