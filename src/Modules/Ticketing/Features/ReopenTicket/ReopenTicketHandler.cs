using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Ticketing.Domain;
using Prescription.Modules.Ticketing.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Ticketing.Features.ReopenTicket;

public sealed class ReopenTicketHandler(TicketingDbContext dbContext) : IRequestHandler<ReopenTicketCommand>
{
    public async Task Handle(ReopenTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await dbContext.Tickets.FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

        if (ticket.CustomerId != request.CustomerId)
        {
            throw new UnauthorizedDomainException("این تیکت متعلق به شما نیست.");
        }

        ticket.Reopen();
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
