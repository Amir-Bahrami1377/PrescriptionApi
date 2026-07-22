using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Orders.Domain;
using Prescription.Modules.Orders.Infrastructure.Persistence;
using Prescription.SharedKernel.Abstractions;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Orders.Features.PaymentCallback;

public sealed class PaymentCallbackHandler(OrdersDbContext dbContext, IPaymentGateway paymentGateway)
    : IRequestHandler<PaymentCallbackCommand, PaymentCallbackResponse>
{
    public async Task<PaymentCallbackResponse> Handle(PaymentCallbackCommand request, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        // Idempotent: ZarinPal (or the customer's browser) may hit this callback more than once for the same payment.
        if (order.Status == OrderStatus.InProgress)
        {
            return new PaymentCallbackResponse(true, order.PaymentReferenceId);
        }

        if (order.PaymentAuthority != request.Authority)
        {
            throw new DomainException("شناسه تراکنش با سفارش مطابقت ندارد.");
        }

        if (!string.Equals(request.Status, "OK", StringComparison.OrdinalIgnoreCase))
        {
            return new PaymentCallbackResponse(false, null);
        }

        // Amount + Authority are re-verified server-side against ZarinPal before the order ever moves to InProgress.
        var verification = new PaymentVerification(request.Authority, order.PriceInRials);
        var result = await paymentGateway.VerifyPaymentAsync(verification, cancellationToken);

        if (!result.Success || result.ReferenceId is null)
        {
            return new PaymentCallbackResponse(false, null);
        }

        order.ConfirmPayment(result.ReferenceId);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PaymentCallbackResponse(true, result.ReferenceId);
    }
}
