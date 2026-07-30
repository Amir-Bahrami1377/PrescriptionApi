using MediatR;
using Prescription.Modules.Orders.Domain;

namespace Prescription.Modules.Orders.Features.ListMyPendingRenewalReviews;

/// <summary>Renewal requests the calling doctor has claimed and still holds an unexpired window on.</summary>
public sealed record ListMyPendingRenewalReviewsQuery(Guid DoctorId) : IRequest<IReadOnlyList<MyPendingRenewalReviewDto>>;

public sealed record MyPendingRenewalReviewDto(
    Guid Id,
    Guid CustomerId,
    string CurrentPrescriptionReferenceNumber,
    string NationalCode,
    BasicInsuranceType BasicInsurance,
    DateTimeOffset ClaimExpiresAtUtc,
    DateTimeOffset CreatedAtUtc);
