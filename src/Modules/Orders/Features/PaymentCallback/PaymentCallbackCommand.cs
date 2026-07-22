using MediatR;

namespace Prescription.Modules.Orders.Features.PaymentCallback;

public sealed record PaymentCallbackCommand(Guid OrderId, string Authority, string Status) : IRequest<PaymentCallbackResponse>;

public sealed record PaymentCallbackResponse(bool Success, string? ReferenceId);
