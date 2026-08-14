namespace Prescription.Modules.Ticketing.Features.CloseExpiredTickets;

public interface ICloseExpiredTicketsJob
{
    Task RunAsync(CancellationToken cancellationToken);
}
