using MediatR;

namespace Prescription.Modules.Ticketing.Features.GetTicket;

public sealed record GetTicketQuery(Guid TicketId, Guid RequestingUserId, string RequestingUserRole) : IRequest<TicketDetailDto>;

public sealed record TicketDetailDto(
    Guid Id,
    Guid CustomerId,
    string Subject,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    DateTimeOffset? QueuedForClosureAtUtc,
    DateTimeOffset? AutoCloseAtUtc,
    DateTimeOffset? ClosedAtUtc,
    IReadOnlyList<TicketMessageDto> Messages);

public sealed record TicketMessageDto(Guid SenderId, string SenderRole, string Body, DateTimeOffset CreatedAtUtc);
