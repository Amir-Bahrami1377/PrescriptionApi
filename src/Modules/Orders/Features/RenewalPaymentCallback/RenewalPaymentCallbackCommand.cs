using MediatR;

namespace Prescription.Modules.Orders.Features.RenewalPaymentCallback;

public sealed record RenewalPaymentCallbackCommand(Guid RenewalId, string Authority, string Status) : IRequest<RenewalPaymentCallbackResponse>;

public sealed record RenewalPaymentCallbackResponse(bool Success, string? ReferenceId);
