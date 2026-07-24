using MediatR;

namespace Prescription.Modules.Orders.Features.ClaimOrderForReview;

public sealed record ClaimOrderForReviewCommand(Guid OrderId, Guid DoctorId) : IRequest<ClaimOrderForReviewResponse>;

public sealed record ClaimOrderForReviewResponse(DateTimeOffset ClaimExpiresAtUtc);
