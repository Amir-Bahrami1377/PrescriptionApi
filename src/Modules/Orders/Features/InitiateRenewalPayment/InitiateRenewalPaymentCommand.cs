using MediatR;

namespace Prescription.Modules.Orders.Features.InitiateRenewalPayment;

public sealed record InitiateRenewalPaymentCommand(Guid RenewalId, Guid CustomerId, string CallbackUrl) : IRequest<InitiateRenewalPaymentResponse>;

public sealed record InitiateRenewalPaymentResponse(string PaymentRedirectUrl);
