using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Ticketing.Domain;
using Prescription.Modules.Ticketing.Infrastructure.Notifications;
using Prescription.Modules.Ticketing.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Ticketing.Features.ReplyTicket;

public sealed class ReplyTicketHandler(
    TicketingDbContext dbContext,
    ITicketNotificationService notificationService)
    : IRequestHandler<ReplyTicketCommand>
{
    public async Task Handle(ReplyTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await dbContext.Tickets
            .Include(t => t.Messages)
            .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

        var isOwner = ticket.CustomerId == request.SenderId && request.SenderRole == "Customer";
        var isAdmin = request.SenderRole == "Admin";

        if (!isOwner && !isAdmin)
        {
            throw new UnauthorizedDomainException("دسترسی پاسخ به این تیکت را ندارید.");
        }

        var reply = ticket.AddReply(request.SenderId, request.SenderRole, request.Message);

        // TicketMessage is an owned entity with a client-generated Guid. Without an explicit state,
        // EF interprets that non-default key as an existing row and issues an UPDATE instead of INSERT.
        dbContext.Entry(reply).State = EntityState.Added;
        await dbContext.SaveChangesAsync(cancellationToken);

        if (isAdmin)
        {
            await notificationService.NotifyAdminReplyAsync(ticket, cancellationToken);
        }
    }
}
