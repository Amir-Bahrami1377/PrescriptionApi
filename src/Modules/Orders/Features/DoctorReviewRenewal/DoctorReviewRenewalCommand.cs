using MediatR;

namespace Prescription.Modules.Orders.Features.DoctorReviewRenewal;

public sealed record DoctorReviewRenewalCommand(Guid RenewalId, Guid DoctorId, bool Approve, string? RejectionReason) : IRequest;
