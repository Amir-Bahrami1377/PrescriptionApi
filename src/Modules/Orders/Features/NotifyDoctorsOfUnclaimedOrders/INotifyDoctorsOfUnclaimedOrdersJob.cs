namespace Prescription.Modules.Orders.Features.NotifyDoctorsOfUnclaimedOrders;

/// <summary>The unit Hangfire's recurring-job scheduler actually invokes; kept as an interface so Hangfire can serialize the call by type.</summary>
public interface INotifyDoctorsOfUnclaimedOrdersJob
{
    Task RunAsync(CancellationToken cancellationToken);
}
