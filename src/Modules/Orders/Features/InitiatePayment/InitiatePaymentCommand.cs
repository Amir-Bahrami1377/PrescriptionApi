using MediatR;

namespace Prescription.Modules.Orders.Features.InitiatePayment;

public sealed record InitiatePaymentCommand(Guid OrderId, Guid CustomerId, string CallbackUrl) : IRequest<InitiatePaymentResponse>;

public sealed record InitiatePaymentResponse(string PaymentRedirectUrl);
