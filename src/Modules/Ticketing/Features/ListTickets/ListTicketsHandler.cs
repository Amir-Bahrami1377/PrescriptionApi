using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Ticketing.Infrastructure.Persistence;

namespace Prescription.Modules.Ticketing.Features.ListTickets;

public sealed class ListTicketsHandler(TicketingDbContext dbContext)
    : IRequestHandler<ListTicketsQuery, IReadOnlyList<AdminTicketSummaryDto>>
{
    public async Task<IReadOnlyList<AdminTicketSummaryDto>> Handle(
        ListTicketsQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Tickets.AsNoTracking();

        if (request.Status is { } status)
        {
            query = query.Where(t => t.Status == status);
        }

        return await query
            .OrderByDescending(t => t.UpdatedAtUtc ?? t.CreatedAtUtc)
            .Select(t => new AdminTicketSummaryDto(
                t.Id,
                t.CustomerId,
                t.Subject,
                t.Status.ToString(),
                t.Messages.Count,
                t.CreatedAtUtc,
                t.UpdatedAtUtc,
                t.AutoCloseAtUtc,
                t.ClosedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
