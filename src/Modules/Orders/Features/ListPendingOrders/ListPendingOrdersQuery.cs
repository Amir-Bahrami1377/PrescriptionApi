using MediatR;

namespace Prescription.Modules.Orders.Features.ListPendingOrders;

/// <summary>Lists orders awaiting a doctor's review — any doctor may claim one by calling DoctorReviewOrder.</summary>
public sealed record ListPendingOrdersQuery : IRequest<IReadOnlyList<PendingOrderDto>>;

public sealed record PendingOrderDto(
    Guid Id,
    Guid CustomerId,
    IReadOnlyCollection<Guid> LabTestIds,
    string? CustomerNote,
    bool HasAttachment,
    DateTimeOffset CreatedAtUtc);
