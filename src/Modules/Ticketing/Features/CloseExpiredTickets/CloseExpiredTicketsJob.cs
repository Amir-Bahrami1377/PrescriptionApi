using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Ticketing.Domain;
using Prescription.Modules.Ticketing.Infrastructure.Persistence;

namespace Prescription.Modules.Ticketing.Features.CloseExpiredTickets;

public sealed class CloseExpiredTicketsJob(TicketingDbContext dbContext) : ICloseExpiredTicketsJob
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var expiredTickets = await dbContext.Tickets
            .Where(t => t.Status == TicketStatus.PendingClosure
                && t.AutoCloseAtUtc != null
                && t.AutoCloseAtUtc <= now)
            .ToListAsync(cancellationToken);

        foreach (var ticket in expiredTickets)
        {
            ticket.CloseIfClosureDeadlinePassed(now);
        }

        if (expiredTickets.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
