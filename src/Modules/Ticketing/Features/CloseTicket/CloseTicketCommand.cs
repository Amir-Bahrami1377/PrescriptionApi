using MediatR;

namespace Prescription.Modules.Ticketing.Features.CloseTicket;

public sealed record CloseTicketCommand(Guid TicketId, Guid RequestingUserId, string RequestingUserRole) : IRequest;
