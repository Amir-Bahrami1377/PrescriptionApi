using MediatR;
using Prescription.Modules.Orders.Domain;

namespace Prescription.Modules.Orders.Features.ListMyRenewalsInProgress;

/// <summary>Paid renewal requests waiting for this doctor to issue the new prescription.</summary>
public sealed record ListMyRenewalsInProgressQuery(Guid DoctorId) : IRequest<IReadOnlyList<InProgressRenewalDto>>;

public sealed record InProgressRenewalDto(
    Guid Id,
    Guid CustomerId,
    string CurrentPrescriptionReferenceNumber,
    string NationalCode,
    BasicInsuranceType BasicInsurance,
    long? PriceInRials,
    DateTimeOffset CreatedAtUtc);
