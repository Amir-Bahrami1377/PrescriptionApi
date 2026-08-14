using MediatR;

namespace Prescription.Modules.Ticketing.Features.ReopenTicket;

public sealed record ReopenTicketCommand(Guid TicketId, Guid CustomerId) : IRequest;
