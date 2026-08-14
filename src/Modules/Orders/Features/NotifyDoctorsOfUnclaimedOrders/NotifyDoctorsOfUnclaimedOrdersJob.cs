using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;
using Prescription.SharedKernel.Abstractions;

namespace Prescription.Modules.Orders.Features.NotifyDoctorsOfUnclaimedOrders;

/// <summary>
/// Runs on a recurring schedule (see the RecurringJob registration in Program.cs). Texts every active
/// doctor once for each order that has sat in PendingDoctorApproval, with no doctor holding an active
/// review claim on it, for more than <see cref="UnclaimedThreshold"/> — then marks the order so a later
/// run never sends a second round of SMS for it.
/// </summary>
public sealed class NotifyDoctorsOfUnclaimedOrdersJob(OrdersDbContext dbContext, IIdentityLookup identityLookup, INotificationQueue notificationQueue)
    : INotifyDoctorsOfUnclaimedOrdersJob
{
    private static readonly TimeSpan UnclaimedThreshold = TimeSpan.FromMinutes(15);

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var threshold = now - UnclaimedThreshold;

        var unclaimedOrders = await dbContext.Orders
            .Where(o => o.Status == OrderStatus.PendingDoctorApproval
                && o.CreatedAtUtc <= threshold
                && o.UnclaimedNotificationSentAtUtc == null
                && (o.ClaimedByDoctorId == null || o.ClaimExpiresAtUtc <= now))
            .ToListAsync(cancellationToken);

        if (unclaimedOrders.Count == 0)
        {
            return;
        }

        var doctorPhoneNumbers = await identityLookup.GetActiveDoctorPhoneNumbersAsync(cancellationToken);

        foreach (var order in unclaimedOrders)
        {
            var message = ComposeMessage(order);
            foreach (var phoneNumber in doctorPhoneNumbers)
            {
                notificationQueue.EnqueueSms(phoneNumber, message);
            }

            order.MarkUnclaimedNotificationSent();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string ComposeMessage(Order order)
    {
        var reference = order.Id.ToString()[..8];
        return $"سامانه نسخه\nسفارش {reference} بیش از ۱۵ دقیقه در انتظار بررسی پزشک است. لطفاً به پنل پزشک مراجعه کنید.";
    }
}
