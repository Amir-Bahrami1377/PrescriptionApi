using Prescription.Modules.Ticketing.Domain;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Ticketing.Infrastructure.Notifications;

internal sealed class TicketNotificationService(
    IIdentityLookup identityLookup,
    INotificationQueue notificationQueue)
    : ITicketNotificationService
{
    public Task NotifyCreatedAsync(Ticket ticket, CancellationToken cancellationToken = default) =>
        NotifyAsync(ticket, TicketSmsComposer.ComposeCreated(ticket), cancellationToken);

    public Task NotifyAdminReplyAsync(Ticket ticket, CancellationToken cancellationToken = default) =>
        NotifyAsync(ticket, TicketSmsComposer.ComposeAdminReply(ticket), cancellationToken);

    private async Task NotifyAsync(Ticket ticket, string message, CancellationToken cancellationToken)
    {
        var phoneNumber = await identityLookup.GetPhoneNumberAsync(ticket.CustomerId, cancellationToken);
        if (phoneNumber is not null)
        {
            notificationQueue.EnqueueSms(phoneNumber, message);
        }
    }
}
