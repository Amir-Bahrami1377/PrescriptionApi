using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Ticketing.Domain;
using Prescription.Modules.Ticketing.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Ticketing.Features.QueueTicketClosure;

public sealed class QueueTicketClosureHandler(TicketingDbContext dbContext)
    : IRequestHandler<QueueTicketClosureCommand>
{
    public async Task Handle(QueueTicketClosureCommand request, CancellationToken cancellationToken)
    {
        var ticket = await dbContext.Tickets.FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

        ticket.QueueForClosure();
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
