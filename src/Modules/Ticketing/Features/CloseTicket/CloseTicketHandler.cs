using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Ticketing.Domain;
using Prescription.Modules.Ticketing.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Ticketing.Features.CloseTicket;

public sealed class CloseTicketHandler(TicketingDbContext dbContext) : IRequestHandler<CloseTicketCommand>
{
    public async Task Handle(CloseTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await dbContext.Tickets.FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

        var isOwner = ticket.CustomerId == request.RequestingUserId;
        var isStaff = request.RequestingUserRole is "Doctor" or "Admin";

        if (!isOwner && !isStaff)
        {
            throw new UnauthorizedDomainException("این تیکت متعلق به شما نیست.");
        }

        ticket.Close();

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
