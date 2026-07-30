using MediatR;

namespace Prescription.Modules.Orders.Features.ClaimRenewalForReview;

public sealed record ClaimRenewalForReviewCommand(Guid RenewalId, Guid DoctorId) : IRequest<ClaimRenewalForReviewResponse>;

public sealed record ClaimRenewalForReviewResponse(DateTimeOffset ClaimExpiresAtUtc);
