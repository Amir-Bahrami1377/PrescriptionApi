using MediatR;

namespace Prescription.Modules.Ticketing.Features.CreateTicket;

public sealed record CreateTicketCommand(Guid CustomerId, string Subject, string Message) : IRequest<CreateTicketResponse>;

public sealed record CreateTicketResponse(Guid TicketId);
