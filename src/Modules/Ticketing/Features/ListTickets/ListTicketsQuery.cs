using MediatR;
using Prescription.Modules.Ticketing.Domain;

namespace Prescription.Modules.Ticketing.Features.ListTickets;

public sealed record ListTicketsQuery(TicketStatus? Status) : IRequest<IReadOnlyList<AdminTicketSummaryDto>>;

public sealed record AdminTicketSummaryDto(
    Guid Id,
    Guid CustomerId,
    string Subject,
    string Status,
    int MessageCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    DateTimeOffset? AutoCloseAtUtc,
    DateTimeOffset? ClosedAtUtc);
