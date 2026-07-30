using MediatR;
using Prescription.Modules.Orders.Domain;

namespace Prescription.Modules.Orders.Features.ListPendingRenewals;

/// <summary>The shared pool: renewal requests no doctor currently holds an unexpired claim on.</summary>
public sealed record ListPendingRenewalsQuery : IRequest<IReadOnlyList<PendingRenewalDto>>;

public sealed record PendingRenewalDto(
    Guid Id,
    Guid CustomerId,
    string CurrentPrescriptionReferenceNumber,
    string NationalCode,
    BasicInsuranceType BasicInsurance,
    DateTimeOffset CreatedAtUtc);
