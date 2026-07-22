using MediatR;

namespace Prescription.Modules.Orders.Features.DoctorReviewOrder;

public sealed record DoctorReviewOrderCommand(Guid OrderId, Guid DoctorId, bool Approve, string? RejectionReason) : IRequest;
