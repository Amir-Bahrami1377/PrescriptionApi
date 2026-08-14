using MediatR;

namespace Prescription.Modules.Ticketing.Features.ListMyTickets;

public sealed record ListMyTicketsQuery(Guid CustomerId) : IRequest<IReadOnlyList<TicketSummaryDto>>;

public sealed record TicketSummaryDto(
    Guid Id,
    string Subject,
    string Status,
    int MessageCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    DateTimeOffset? AutoCloseAtUtc,
    DateTimeOffset? ClosedAtUtc);
