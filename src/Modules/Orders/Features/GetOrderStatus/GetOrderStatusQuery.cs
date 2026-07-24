using MediatR;

namespace Prescription.Modules.Orders.Features.GetOrderStatus;

public sealed record GetOrderStatusQuery(Guid OrderId, Guid RequestingUserId, string RequestingUserRole) : IRequest<OrderStatusDto>;

public sealed record OrderStatusDto(
    Guid Id,
    Guid LabTestId,
    long? PriceInRials,
    string Status,
    string? RejectionReason,
    string? PrescriptionReferenceNumber,
    string? PaymentReferenceId,
    bool HasResult,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc);
