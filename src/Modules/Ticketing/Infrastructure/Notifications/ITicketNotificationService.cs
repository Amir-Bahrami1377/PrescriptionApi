using Prescription.Modules.Ticketing.Domain;

namespace Prescription.Modules.Ticketing.Infrastructure.Notifications;

public interface ITicketNotificationService
{
    Task NotifyCreatedAsync(Ticket ticket, CancellationToken cancellationToken = default);
    Task NotifyAdminReplyAsync(Ticket ticket, CancellationToken cancellationToken = default);
}
