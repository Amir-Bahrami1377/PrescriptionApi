using MediatR;
using Prescription.Modules.Orders.Domain;

namespace Prescription.Modules.Orders.Features.ListMyRenewals;

/// <summary>Every renewal request the calling patient has submitted, newest first.</summary>
public sealed record ListMyRenewalsQuery(Guid CustomerId) : IRequest<IReadOnlyList<MyRenewalDto>>;

public sealed record MyRenewalDto(
    Guid Id,
    string CurrentPrescriptionReferenceNumber,
    BasicInsuranceType BasicInsurance,
    string Status,
    long? PriceInRials,
    string? RejectionReason,
    string? NewPrescriptionReferenceNumber,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc);
