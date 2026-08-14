using MediatR;
using Prescription.Modules.Orders.Domain;

namespace Prescription.Modules.Orders.Features.ListOrdersByCustomer;

/// <summary>Every order a given customer has placed, newest first — the admin panel's per-user order history.</summary>
public sealed record ListOrdersByCustomerQuery(Guid CustomerId) : IRequest<IReadOnlyList<CustomerOrderDto>>;

public sealed record CustomerOrderDto(
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
