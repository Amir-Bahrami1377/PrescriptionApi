using MediatR;
using Prescription.Modules.Ticketing.Domain;
using Prescription.Modules.Ticketing.Infrastructure.Notifications;
using Prescription.Modules.Ticketing.Infrastructure.Persistence;

namespace Prescription.Modules.Ticketing.Features.CreateTicket;

public sealed class CreateTicketHandler(TicketingDbContext dbContext, ITicketNotificationService notificationService)
    : IRequestHandler<CreateTicketCommand, CreateTicketResponse>
{
    public async Task<CreateTicketResponse> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = Ticket.Create(request.CustomerId, request.Subject, request.Message);

        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync(cancellationToken);
        await notificationService.NotifyCreatedAsync(ticket, cancellationToken);

        return new CreateTicketResponse(ticket.Id);
    }
}
