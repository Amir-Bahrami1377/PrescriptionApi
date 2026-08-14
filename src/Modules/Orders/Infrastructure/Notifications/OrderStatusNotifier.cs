using Prescription.Modules.Orders.Domain;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Infrastructure.Notifications;

internal sealed class OrderStatusNotifier(IIdentityLookup identityLookup, INotificationQueue notificationQueue)
    : IOrderStatusNotifier
{
    public async Task NotifyCustomerAsync(Order order, CancellationToken cancellationToken = default)
    {
        var phoneNumber = await identityLookup.GetPhoneNumberAsync(order.CustomerId, cancellationToken);
        if (phoneNumber is null)
        {
            return;
        }

        notificationQueue.EnqueueSms(phoneNumber, OrderStatusSmsComposer.Compose(order));
    }
}
