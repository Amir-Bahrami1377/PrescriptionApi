using MediatR;

namespace Prescription.Modules.Orders.Features.ListMyOrdersInProgress;

/// <summary>This doctor's paid orders that still need a result uploaded / to be completed.</summary>
public sealed record ListMyOrdersInProgressQuery(Guid DoctorId) : IRequest<IReadOnlyList<InProgressOrderDto>>;

public sealed record InProgressOrderDto(
    Guid Id,
    Guid CustomerId,
    IReadOnlyCollection<Guid> LabTestIds,
    bool HasResult,
    DateTimeOffset CreatedAtUtc);
