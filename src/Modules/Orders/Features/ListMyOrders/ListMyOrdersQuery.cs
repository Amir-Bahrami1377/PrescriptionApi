using MediatR;
using Prescription.Modules.Orders.Domain;

namespace Prescription.Modules.Orders.Features.ListMyOrders;

/// <summary>Every order the calling customer has placed, newest first, alongside how much of their
/// pending-review allowance is left.</summary>
public sealed record ListMyOrdersQuery(Guid CustomerId) : IRequest<MyOrdersResponse>;

public sealed record MyOrdersResponse(
    IReadOnlyList<MyOrderDto> Orders,
    PendingApprovalCapacityDto PendingApprovalCapacity);

/// <summary>Served from the backend so the client never has to hardcode the cap and drift out of sync
/// with the rule CreateOrder actually enforces.</summary>
public sealed record PendingApprovalCapacityDto(int Used, int Limit, int Remaining);

public sealed record MyOrderDto(
    Guid Id,
    IReadOnlyCollection<Guid> LabTestIds,
    BasicInsuranceType BasicInsurance,
    bool IsForThirdParty,
    bool RequestsConsultation,
    string Status,
    long? PriceInRials,
    string? RejectionReason,
    // The tracking number the customer takes to the lab.
    string? PrescriptionReferenceNumber,
    bool HasResult,
    string? ConsultationOpinion,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc);
