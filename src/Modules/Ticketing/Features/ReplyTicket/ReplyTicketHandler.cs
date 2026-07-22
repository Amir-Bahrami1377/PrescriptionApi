using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Ticketing.Domain;
using Prescription.Modules.Ticketing.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Ticketing.Features.ReplyTicket;

public sealed class ReplyTicketHandler(TicketingDbContext dbContext) : IRequestHandler<ReplyTicketCommand>
{
    public async Task Handle(ReplyTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await dbContext.Tickets.Include(t => t.Messages).FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

        var isOwner = ticket.CustomerId == request.SenderId;
        var isStaff = request.SenderRole is "Doctor" or "Admin";

        if (!isOwner && !isStaff)
        {
            throw new UnauthorizedDomainException("این تیکت متعلق به شما نیست.");
        }

        ticket.AddReply(request.SenderId, request.SenderRole, request.Message);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
