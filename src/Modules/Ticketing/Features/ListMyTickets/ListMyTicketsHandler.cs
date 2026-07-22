using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Ticketing.Infrastructure.Persistence;

namespace Prescription.Modules.Ticketing.Features.ListMyTickets;

public sealed class ListMyTicketsHandler(TicketingDbContext dbContext)
    : IRequestHandler<ListMyTicketsQuery, IReadOnlyList<TicketSummaryDto>>
{
    public async Task<IReadOnlyList<TicketSummaryDto>> Handle(ListMyTicketsQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Tickets
            .AsNoTracking()
            .Where(t => t.CustomerId == request.CustomerId)
            .OrderByDescending(t => t.CreatedAtUtc)
            .Select(t => new TicketSummaryDto(t.Id, t.Subject, t.Status.ToString(), t.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
