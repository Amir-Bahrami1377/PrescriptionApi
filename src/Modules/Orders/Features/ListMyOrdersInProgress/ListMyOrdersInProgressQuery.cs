using MediatR;

namespace Prescription.Modules.Orders.Features.ListMyOrdersInProgress;

/// <summary>This doctor's paid orders awaiting their sign-off — where the prescription tracking number is registered.</summary>
public sealed record ListMyOrdersInProgressQuery(Guid DoctorId) : IRequest<IReadOnlyList<InProgressOrderDto>>;

public sealed record InProgressOrderDto(
    Guid Id,
    Guid CustomerId,
    IReadOnlyCollection<Guid> LabTestIds,
    bool HasResult,
    // Already paid by this point, so never null — exposed so the list doesn't render it as "not set yet".
    long? PriceInRials,
    // Null until the doctor registers it; the customer needs it to visit the lab.
    string? PrescriptionReferenceNumber,
    // True means signing off moves the order to result-upload rather than finishing it.
    bool RequestsConsultation,
    DateTimeOffset CreatedAtUtc);
