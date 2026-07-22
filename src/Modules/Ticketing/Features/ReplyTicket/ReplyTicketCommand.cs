using MediatR;

namespace Prescription.Modules.Ticketing.Features.ReplyTicket;

public sealed record ReplyTicketCommand(Guid TicketId, Guid SenderId, string SenderRole, string Message) : IRequest;
