using Prescription.Modules.Orders.Domain;

namespace Prescription.Modules.Orders.Infrastructure.Notifications;

/// <summary>Tells the customer their order's status just changed. Kept as one seam so every status-changing
/// handler composes and sends the message the same way instead of duplicating the lookup + compose + enqueue steps.</summary>
public interface IOrderStatusNotifier
{
    Task NotifyCustomerAsync(Order order, CancellationToken cancellationToken = default);
}
