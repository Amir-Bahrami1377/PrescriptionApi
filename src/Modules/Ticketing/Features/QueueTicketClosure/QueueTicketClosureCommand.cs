using MediatR;

namespace Prescription.Modules.Ticketing.Features.QueueTicketClosure;

public sealed record QueueTicketClosureCommand(Guid TicketId) : IRequest;
