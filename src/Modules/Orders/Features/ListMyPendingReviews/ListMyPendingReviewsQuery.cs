using MediatR;
using Prescription.Modules.Orders.Domain;

namespace Prescription.Modules.Orders.Features.ListMyPendingReviews;

/// <summary>Orders the calling doctor has claimed and still has an active (unexpired) 30-minute review window for.</summary>
public sealed record ListMyPendingReviewsQuery(Guid DoctorId) : IRequest<IReadOnlyList<MyPendingReviewDto>>;

public sealed record MyPendingReviewDto(
    Guid Id,
    Guid CustomerId,
    IReadOnlyCollection<Guid> LabTestIds,
    string? CustomerNote,
    bool HasAttachment,
    BasicInsuranceType BasicInsurance,
    SupplementaryInsuranceType SupplementaryInsurance,
    bool IsForThirdParty,
    bool RequestsConsultation,
    DateTimeOffset ClaimExpiresAtUtc,
    DateTimeOffset CreatedAtUtc);
