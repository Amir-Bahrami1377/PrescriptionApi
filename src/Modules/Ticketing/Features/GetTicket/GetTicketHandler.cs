using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Ticketing.Domain;
using Prescription.Modules.Ticketing.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Ticketing.Features.GetTicket;

public sealed class GetTicketHandler(TicketingDbContext dbContext) : IRequestHandler<GetTicketQuery, TicketDetailDto>
{
    public async Task<TicketDetailDto> Handle(GetTicketQuery request, CancellationToken cancellationToken)
    {
        var ticket = await dbContext.Tickets
            .AsNoTracking()
            .Include(t => t.Messages)
            .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

        var isOwner = ticket.CustomerId == request.RequestingUserId;
        var isAdmin = request.RequestingUserRole == "Admin";

        if (!isOwner && !isAdmin)
        {
            throw new UnauthorizedDomainException("این تیکت متعلق به شما نیست.");
        }

        return new TicketDetailDto(
            ticket.Id,
            ticket.CustomerId,
            ticket.Subject,
            ticket.Status.ToString(),
            ticket.CreatedAtUtc,
            ticket.UpdatedAtUtc,
            ticket.QueuedForClosureAtUtc,
            ticket.AutoCloseAtUtc,
            ticket.ClosedAtUtc,
            ticket.Messages
                .OrderBy(m => m.CreatedAtUtc)
                .Select(m => new TicketMessageDto(m.SenderId, m.SenderRole, m.Body, m.CreatedAtUtc))
                .ToList());
    }
}
